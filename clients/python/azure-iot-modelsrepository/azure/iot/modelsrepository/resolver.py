# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import requests
import logging
import re
import os
import json
import urllib

logger = logging.getLogger(__name__)


class ResolverError(Exception):
    pass


def resolve(dtmi, endpoint, expanded=False, resolve_dependencies=False):
    """Retrieve and return the DTDL model(s) corresponding to the given DTMI

    :param str dtmi: DTMI for the desired DTDL
    :param str endpoint: Either a URL or a local filesystem directory where the desired DTDL can
        be found according to the specified DTMI
    :param bool expanded: If True, will retrieve the expanded DTDL instead of the regular one
        (Default - False)
    :param bool resolve_dependencies: If True, will recursively resolve any addtional DTMIs
        referenced from within the DTDL. (Default - False) <----- THIS IS NOT YET IMPLEMENTED!

    :raises: ValueError if DTMI is invalid
    :raises: :class:`azure.iot.modelsrepository.resolver.ResolverError` if resolution of the DTMI
        at the given endpoint is unsuccessful

    :returns: Dictionary mapping DTMIs to a resolved DTDL (multiple DTMIs possible if using
        full resolution mode)
    :rtype: dict
    """
    fully_qualified_dtmi = get_fully_qualified_dtmi(dtmi, endpoint)

    if expanded:
        fully_qualified_dtmi = fully_qualified_dtmi.replace(".json", ".expanded.json")

    # Check value of endpoint to determine if URL or local filesystem directory
    parse_result = urllib.parse.urlparse(fully_qualified_dtmi)
    if parse_result.scheme == "http" or parse_result.scheme == "https":
        # HTTP URL
        json = _resolve_from_remote_url(fully_qualified_dtmi)
    elif parse_result.scheme == "file":
        # File URI
        # TODO: do we need to support files from localhost?
        fully_qualified_dtmi = fully_qualified_dtmi[len("file://"):]
        json = _resolve_from_local_file(fully_qualified_dtmi)
    else:
        # File location
        json = _resolve_from_local_file(fully_qualified_dtmi)

    # Expanded JSON will be in the form of a list of DTDLs
    dtdl_map = {}
    if expanded:
        for dtdl in json:
            dtdl_map[dtdl["@id"]] = dtdl
    # Otherwise, the JSON is a singular DTDL
    else:
        dtdl_map[dtmi] = json

    # TODO: full resolution

    return dtdl_map

def get_fully_qualified_dtmi(dtmi, endpoint):
    """ Return a fully-qualified path for a DTMI at an endpoint

    :param str dtmi: DTMI to be make fully-qualified
    :param str endpoint: Either a URL or a local filesystem directory that corresponds to the DTMI
    """
    # NOTE: does this belong in this library?
    if not endpoint.endswith("/"):
        endpoint += "/"
    fully_qualified_dtmi = endpoint + _convert_dtmi_to_path(dtmi)
    return fully_qualified_dtmi


def _resolve_from_remote_url(url):
    """Return JSON from a specified remote URL"""
    logger.debug("Making GET request to {}".format(url))
    response = requests.get(url)
    logger.debug("Received GET response: {}".format(response.status_code))
    if response.status_code == 200:
        return response.json()
    else:
        raise ResolverError("Failed to resolve DTMI from URL. Status Code: {}".format(response.status_code))


def _resolve_from_local_file(file):
    """Return JSON from specified local file"""
    logger.debug("Opening local file {}".format(file))
    with open(file) as f:
        file_str = f.read()
    return json.loads(file_str)


def _convert_dtmi_to_path(dtmi):
    """Converts a DTMI into a DTMI path

    E.g:
    dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
    """
    pattern = re.compile("^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$")
    if not pattern.match(dtmi):
        raise ValueError("Invalid DTMI")
    else:
        return dtmi.lower().replace(":", "/").replace(";", "-") + ".json"
