# Node Resolution Sample

This example project shows a minimum implementation of the [DMR resolution convention](https://github.com/Azure/iot-plugandplay-models-tools/wiki/Resolution-Convention) for `node` using `ES6 JavaScript`.

The sample achieves the following points:

- Takes a `DTMI` argument or uses a default for resolution.
- Validates the `DTMI` format using RegEx predefined in the [DTMI specification document](https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions).
- Transforms the `DTMI` to a path using an implementation of the [DMR resolution convention](https://github.com/Azure/iot-plugandplay-models-tools/wiki/Resolution-Convention).
- Retrieves string content via http request to a fully qualified path combining the DMR endpoint and transformed `DTMI`.

## Quick Start

Open the folder from the command line to run:

```bash
node main.js
```

> :exclamation: Note there are no external dependencies, so there is no need to `npm install` any additional packages.

This sample uses the DMR endpoint `https://devicemodels.azure.com` by default.

## Code Walktrough

To convert a DTMI to an absolute path we use the `dtmiToPath` function, with `isDtmi`:

```javascript
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
  if (isDtmi(dtmi)) {
    return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
  } else return null
}
```

With the resulting path and the base URL for the repository we can obtain the interface:

```javascript
const https = require('https')
const repositoryEndpoint = 'devicemodels.azure.com'
const dtmi = process.argv[2] || 'dtmi:azure:DeviceManagement:DeviceInformation;1'
const path = dtmiToPath(dtmi)
console.log(repositoryEndpoint, path)

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
```
