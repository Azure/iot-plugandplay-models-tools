// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as lib from '../../src/dtmiConventions'
// import * as modelFetchers from '../../src/modelFetchers'

import * as sinon from 'sinon'
import { assert, expect } from 'chai'

// var should = require('chai').should()

describe('dtmiConventions', function () {
  afterEach(() => {
    sinon.restore()
  })
  describe('isValidDtmi', function () {
    it('should validate a correctly formatted dtmi', function () {
      const validDtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
      const result = lib.isValidDtmi(validDtmi)
      assert(result, 'valid dtmi not found as valid')
    })

    it('should return a false boolean if the dtmi is invalid', function () {
      const invalidDtmi = 'dtmiazure:DeviceManagement:DeviceInformation;1'
      const result = lib.isValidDtmi(invalidDtmi)
      assert(!result, 'invalid dtmi incorrectly labelled as valid')
    })
  })

  describe('dtmiToPath', function () {
    it('should fail if the dtmi is not formatted correctly', function () {
      expect(() => {
        const invalidDtmi = 'dtmiazure:DeviceManagement:DeviceInformation;1'
        lib.dtmiToPath(invalidDtmi, false)
      }).to.throw()
    })

    it('should reformat a DTMI to a URL Path', function () {
      const validDtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
      const result = lib.dtmiToPath(validDtmi, false)
      assert.deepEqual(result, '/dtmi/azure/devicemanagement/deviceinformation-1.json')
    })

    it('should add expanded to the path if specified', function () {
      const validDtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
      const result = lib.dtmiToPath(validDtmi, true)
      assert.deepEqual(result, '/dtmi/azure/devicemanagement/deviceinformation-1.expanded.json')
    })
  })
})
