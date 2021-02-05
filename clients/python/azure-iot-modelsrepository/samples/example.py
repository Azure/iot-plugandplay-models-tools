# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------

from azure.iot.modelsrepository import resolver
import pprint

repository_endpoint = "https://devicemodels.azure.com"
dtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'

a = resolver.resolve(dtmi, repository_endpoint)
pprint.pprint(a)