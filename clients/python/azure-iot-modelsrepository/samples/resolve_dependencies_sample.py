# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------

from azure.iot.modelsrepository import resolver
import pprint

repository_endpoint = "https://devicemodels.azure.com"
dtmi = "dtmi:com:example:TemperatureController;1"

# This API call will return a dictionary mapping DTMI to its corresponding DTDL from
# a .json file at the specified endpoint

# This API call will return a dictionary mapping the specified DTMI to it's corresponding DTDL,
# from a .json file at the specified endpoint, as well as the DTMIs and DTDLs for all dependencies
# on components and subcomponents mentioned in the document
# i.e. https://devicemodels.azure.com/dtmi/com/example/temperaturecontroller-1.json
a = resolver.resolve(dtmi, repository_endpoint, resolve_dependencies=True)
pprint.pprint(a)
