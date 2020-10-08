// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const https = require('https')

/**
 * @description Validates DTMI with RegEx from https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
 * @param {string} dtmi
 */
const isDtmi = dtmi => {
  return RegExp('^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$').test(dtmi)
}

/**
 * @description Converts DTMI to /dtmi/com/example/device-1.json path.
 * @param {string} dtmi
 * @returns {string}
 */
const dtmiToPath = dtmi => {
  if (!isDtmi(dtmi)) {
    return null
  }
  // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
  return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
}

const repositoryEndpoint = 'devicemodels.azure.com'
const dtmi = process.argv[2] || 'dtmi:azure:DeviceManagement:DeviceInformation;1'
const path = dtmiToPath(dtmi)
console.log(repositoryEndpoint, path)

if (path) {
  const options = {
    hostname: repositoryEndpoint,
    port: 443,
    path: path,
    method: 'GET'
  }

  const req = https.request(options, res => {
    console.log(`statusCode: ${res.statusCode}`)
    res.on('data', d => {
      process.stdout.write(d)
    })
  })

  req.on('error', error => {
    console.error(error)
  })

  req.end()
} else console.log(`Invalid DTMI ${dtmi}`)
