# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import requests
import logging
import re
import json
import urllib

logger = logging.getLogger(__name__)


REMOTE_PROTOCOLS = ["http", "https", "ftp"]


class ResolverError(Exception):
    pass


def resolve(dtmi, endpoint, expanded=False, resolve_dependencies=False):
    """Retrieve and return the DTDL model(s) corresponding to the given DTMI

    :param str dtmi: DTMI for the desired DTDL model
    :param str endpoint: Either a URL or a local filesystem directory where the desired DTDL model
        can be found according to the specified DTMI
    :param bool expanded: If True, will retrieve the model from the expanded DTDL instead of the
        regular one (Default - False)
    :param bool resolve_dependencies: If True, will recursively resolve any addtional DTMIs
        for interfaces or components referenced from within the DTDL model (Default - False)

    :raises: ValueError if DTMI is invalid
    :raises: :class:`azure.iot.modelsrepository.resolver.ResolverError` if resolution of the DTMI
        at the given endpoint is unsuccessful

    :returns: Dictionary mapping DTMIs to corresponding DTDL models (multiple DTMIs possible when
        resolving dependencies or using expanded DTDL documents)
    :rtype: dict
    """
    fully_qualified_dtmi = get_fully_qualified_dtmi(dtmi, endpoint)

    if expanded:
        fully_qualified_dtmi = fully_qualified_dtmi.replace(".json", ".expanded.json")

    # If fetching an expanded DTDL, this DTDL will be a list of models.
    # Otherwise, it will just be a single model.
    dtdl = _fetch_dtdl(fully_qualified_dtmi)

    model_map = {}
    # If using expanded DTDL, add an entry to the model map for each model
    if expanded:
        for model in dtdl:
            model_map[model["@id"]] = model
    # If resolving dependencies, will need to fetch component models
    # NOTE: This (should) be unnecessary if using expanded DTDL because
    # expanded DTDL (should) already have them
    elif resolve_dependencies:
        model_map[dtmi] = dtdl
        _resolve_model_dependencies(dtdl, model_map, endpoint)
    # Otherwise, just return a one-entry map of the returned DTDL (single model)
    else:
        model_map[dtmi] = dtdl

    return model_map


def get_fully_qualified_dtmi(dtmi, endpoint):
    """Return a fully-qualified path for a DTMI at an endpoint

    E.g:
    dtmi:com:example:Thermostat;1, https://somedomain.com
        -> https://somedomain.com/dtmi/com/example/thermostat-1.json

    :param str dtmi: DTMI to be make fully-qualified
    :param str endpoint: Either a URL or a local filesystem directory that corresponds to the DTMI

    :returns: The fully qualified path for the specified DTMI at the specified endpoint
    :rtype: str
    """
    # NOTE: does this belong in this library (resolver.py) as opposed to another library within
    # the same package?
    # NOTE: does this have the correct name? Is this really a DTMI path, or is it a DTDL path?
    if not endpoint.endswith("/"):
        endpoint += "/"
    fully_qualified_dtmi = endpoint + _convert_dtmi_to_path(dtmi)
    return fully_qualified_dtmi


def _resolve_model_dependencies(model, model_map, endpoint):
    """Retrieve all components and extended interfaces of the provided DTDL from the provided
    endpoint, and add them to the provided DTDL map.
    This recursively operates on the retrieved dependencies as well"""
    if "contents" in model:
        components = [item["schema"] for item in model["contents"] if item["@type"] == "Component"]
    else:
        components = []

    if "extends" in model:
        # Models defined in a DTDL can implement extensions of up to two interfaces
        if isinstance(model["extends"], list):
            interfaces = model["extends"]
        else:
            interfaces = [model["extends"]]
    else:
        interfaces = []

    dependencies = components + interfaces
    for dependency_dtmi in dependencies:
        if dependency_dtmi not in model_map:
            fq_dependency_dtmi = get_fully_qualified_dtmi(dependency_dtmi, endpoint)
            # The fetched DTDL will be a single model
            dependency_model = _fetch_dtdl(fq_dependency_dtmi)
            model_map[dependency_dtmi] = dependency_model
            _resolve_model_dependencies(dependency_model, model_map, endpoint)


def _fetch_dtdl(resource_location):
    """Return JSON format of a DTDL, fetched from a specified resource location"""
    # Check value of endpoint to determine if URL or local filesystem directory
    parse_result = urllib.parse.urlparse(resource_location)

    if parse_result.scheme in REMOTE_PROTOCOLS:
        # HTTP/HTTPS URL
        json = _fetch_from_remote_url(resource_location)
    elif parse_result.scheme == "file":
        # Filesystem URI
        resource_location = resource_location[len("file://") :]
        json = _fetch_from_local_file(resource_location)
    elif parse_result.scheme == "" and (resource_location.startswith("/")):
        # POSIX filesystem path
        json = _fetch_from_local_file(resource_location)
    elif parse_result.scheme == "" and re.search(
        r"\.[a-zA-z]{2,63}$", resource_location[: resource_location.find("/")]
    ):
        # Web URL with protocol unspecified - default to HTTPS
        resource_location = "https://" + resource_location
        json = _fetch_from_remote_url(resource_location)
    elif (
        parse_result.scheme != ""
        and len(parse_result.scheme) == 1
        and parse_result.scheme.isalpha()
    ):
        # Filesystem path using drive letters (e.g. scheme == "C" or "F" or something)
        json = _fetch_from_local_file(resource_location)
    else:
        raise ValueError("Unable to identify resource location: {}".format(resource_location))

    return json


def _fetch_from_remote_url(url):
    """Return JSON from a specified remote URL"""
    logger.debug("Making GET request to {}".format(url))
    response = requests.get(url)
    logger.debug("Received GET response: {}".format(response.status_code))
    if response.status_code == 200:
        return response.json()
    else:
        raise ResolverError(
            "Failed to resolve DTMI from URL. Status Code: {}".format(response.status_code)
        )


def _fetch_from_local_file(file):
    """Return JSON from specified local file"""
    logger.debug("Opening local file {}".format(file))
    try:
        with open(file) as f:
            file_str = f.read()
    except Exception as e:
        raise ResolverError("Failed to resolve DTMI from Filesystem") from e
    return json.loads(file_str)


def _convert_dtmi_to_path(dtmi):
    """Converts a DTMI into a DTMI path

    E.g:
    dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
    """
    pattern = re.compile(
        "^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$"
    )
    if not pattern.match(dtmi):
        raise ValueError("Invalid DTMI")
    else:
        return dtmi.lower().replace(":", "/").replace(";", "-") + ".json"
