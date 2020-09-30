# Resolution Samples for Device Model Repository

DTDL models stored on a compatible Device Model Repository can be located at a known location from the DTMI.

This sample shows how to convert any given DTMI to a relative path that can be used to retrieve the required DTDL interface.

## dtmi2path

Based on the [DMR convention](https://github.com/Azure/device-models-tools/wiki/Resolution-Convention) a DTMI can be translated to a relative path by using the next rules:

- Convert all characters to lower case
- Replace `:` with `/`
- The file name is the last DTMI segment with the version and the `.json` extension

```bash
dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
```

The next JavaScript function implements these rules and validates the DTMI using the RegEx provided in the [DTMI spec](https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions)

```JS
/**
 * @description Converts DTMI to dtmi/com/example/device-1.json path.
 *   Validates DTMI with RegEx from https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
 * @param {string} dtmi
 * @returns {string)}
 */
const dtmi2path = dtmi => {
  if (RegExp('^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$').test(dtmi)) {
    return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
  } else return 'NOT-VALID-DTMI'
}
```

To use this function from a Node.js console app:

```js
const repo = 'devicemodels.azure.com'
const dtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
const path = dtmi2path(dtmi)
console.log(repo, path)
```

Full sample is available in the `main.js` script located in the folder.
