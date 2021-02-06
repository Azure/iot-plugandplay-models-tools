// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Demonstrates resolving/obtaining a particular model definition from a remote model repository
 */

let resolver = require('../../out/src/index.js')

const repositoryEndpoint = 'https://devicemodels.azure.com/'
const dtmi = process.argv[2] || 'dtmi:com:example:TemperatureController;1'

console.log(repositoryEndpoint, dtmi)

async function main () {
  const result = await resolver.resolve(dtmi, repositoryEndpoint, { resolveDependencies: 'enabled' })
  console.log(result)
  Object.keys(result).forEach((fetchedDtmi) => {
    console.log(`DTMI is: ${fetchedDtmi}`)
    console.log(`DTDL Display Name is: ${result[fetchedDtmi]['displayName']}`);
    console.log(`DTDL Description is: ${result[fetchedDtmi]['description']}`);
    console.log('------------------------------------------------');
    console.log(JSON.stringify(result[fetchedDtmi]));
    console.log('------------------------------------------------');
  })
}

main().catch((err) => {
  console.error('The sample encountered an error:', err)
})
