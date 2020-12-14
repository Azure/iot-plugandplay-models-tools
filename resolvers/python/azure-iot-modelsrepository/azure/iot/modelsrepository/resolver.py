# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import requests
import logging
import requests_unixsocket
import re

requests_unixsocket.monkeypatch()
logger = logging.getLogger(__name__)


class ResolverError(Exception):
    pass


def resolve(dtmi, endpoint, fully_resolve=False, expanded=False):
    """Retrieve and return the DTDL model(s) corresponding to the given DTMI

    :returns: Dictionary mapping DTMI to a resolved DTDL (or list of DTDLs)
    :rtype: dict
    """
    # Check value of endpoint to determine if URL or local filesystem directory
    if endpoint.startswith("http"):
        return _resolve_remote_url(dtmi, endpoint, expanded)
    else:
        pass


def _resolve_remote_url(dtmi, endpoint, expanded):
    """Resolve a DTMI from a remote URL endpoint"""
    if not endpoint.endswith("/"):
        endpoint += "/"
    url = endpoint + _convert_dtmi_to_path(dtmi)

    if expanded:
        url.replace(".json", ".expanded.json")

    logger.debug("Making GET request to {}".format(url))
    response = requests.get(url)
    logger.debug("Received GET response: {}".format(response.status_code))
    if response.status_code == 200:
        # probably need to expand this logic so it has consistent return values
        return {dtmi : response.json()}
    else:
        raise ResolverError("Failed to resolve DTMI. Status Code: {}".format(response.status_code))


def _resolve_local_filesystem_dir():
    pass


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