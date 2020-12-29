# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
from azure.iot.modelsrepository import resolver


# expected = "https://devicemodels.azure.com/dtmi/com/example/thermostat-1.expanded.json"

dtdl = resolver.resolve(dtmi="dtmi:com:example:Thermostat;1", endpoint="https://devicemodels.azure.com/", expanded=True)
#dtdl = resolver.resolve(dtmi="dtmi:com:example:Thermostat;1", endpoint="C:/")
print(dtdl)