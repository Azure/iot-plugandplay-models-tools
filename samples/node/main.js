// dtmi2path sample
const https = require('https')

/**
 * @description Converts DTMI to dtmi/com/example/device-1.json path.
 *   Validates DTMI with RegEx from https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
 * @param {string} dtmi
 * @returns {string}
 */
const dtmi2path = dtmi => {
  if (RegExp('^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$').test(dtmi)) {
    const idAndVersion = dtmi.toLowerCase().split(';')
    const ids = idAndVersion[0].split(':')
    const fileName = `${ids.pop()}-${idAndVersion[1]}.json`
    const modelFolder = ids.join('/')
    return `/${modelFolder}/${fileName}`
  } else return 'NOT-VALID-DTMI'
}

const repo = 'devicemodels.azure.com'
const dtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
const path = dtmi2path(dtmi)
console.log(repo, path)

const options = {
  hostname: repo,
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
