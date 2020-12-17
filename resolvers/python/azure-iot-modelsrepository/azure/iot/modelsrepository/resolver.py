# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import requests
import logging
import requests_unixsocket
import re
import os
import json
from six.moves import urllib

requests_unixsocket.monkeypatch()
logger = logging.getLogger(__name__)


class ResolverError(Exception):
    pass


def resolve(dtmi, endpoint, fully_resolve=False, expanded=False):
    """Retrieve and return the DTDL model(s) corresponding to the given DTMI

    :param str dtmi: DTMI for the desired DTDL
    :param str endpoint: Either a URL or a local filesystem directory where the desired DTDL can
        be found according to the specified DTMI
    :param bool fully_resolve: If True, will recursively resolve any addtional DTMIs referenced
        from within the DTDL. (Default - False) <----- THIS IS NOT YET IMPLEMENTED!
    :param bool expanded: If True, will retrieve the expanded DTDL instead of the regular one
        (Default - False)

    :raises: ValueError if DTMI is invalid
    :raises: :class:`azure.iot.modelsrepository.resolver.ResolverError` if resolution of the DTMI
        at the given endpoint is unsuccessful

    :returns: Dictionary mapping DTMI to a resolved DTDL (or list of DTDLs)
    :rtype: dict
    """
    if not endpoint.endswith("/"):
        endpoint += "/"
    dtmi_location = endpoint + _convert_dtmi_to_path(dtmi)

    if expanded:
        dtmi_location = dtmi_location.replace(".json", ".expanded.json")

    # Check value of endpoint to determine if URL or local filesystem directory
    parse_result = urllib.parse.urlparse(dtmi_location)
    if parse_result.scheme == "http" or parse_result.scheme == "https":
        # HTTP URL
        json =  _resolve_from_remote_url(dtmi_location)
    elif parse_result.scheme == "file":
        # File URI
        # TODO: do we need to support files from localhost?
        dtmi_location = dtmi_location[len("file://"):]
        json = _resolve_from_local_file(dtmi_location)
    else:
        # File location
        json = _resolve_from_local_file(dtmi_location)

    # JSON dict will sometimes come wrapped in a list. If so, remove
    if isinstance(json, list):
        if len(json) == 1:
            json = json[0]
        else:
            # This shouldn't occur. JSON returned that is a list should only be single-element
            raise ResolverError("Unexpected format of DTDL")

    # TODO: full resolution

    return {dtmi : json}


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
