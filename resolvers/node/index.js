// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This example provides use for the Node.js Device Model Repo Code

import { ResolverClient } from "./src/resolverClient"

const repositoryEndpoint = 'devicemodels.azure.com'
const dtmi = process.argv[2] || 'dtmi:azure:DeviceManagement:DeviceInformation;1'

console.log(repositoryEndpoint, path)

dmrClient = new ResolverClient(repositoryEndpoint);
dmrClient.resolve(dtmi);