// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


/**
 * Demonstrates something
 */

import { ResolverClient } from "../../src/resolverClient"

const repositoryEndpoint = 'devicemodels.azure.com'
const dtmi = process.argv[2] || 'dtmi:azure:DeviceManagement:DeviceInformation;1'

console.log(repositoryEndpoint, path)

dmrClient = new ResolverClient(repositoryEndpoint);
dmrClient.resolve(dtmi);

async function main() {
  dmrClient = new ResolverClient(repositoryEndpoint);
  dmrClient.resolve(dtmi);

}

main().catch((err) => {
    console.error("The sample encountered an error:", err);
  });