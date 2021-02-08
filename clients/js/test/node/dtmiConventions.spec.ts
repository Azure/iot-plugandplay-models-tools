// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

// TODO: once implemented replace this
// import * as lib from '../../src/dtmiConventions'

// fake class while lib not implemented
import * as sinon from 'sinon'
import { assert, expect } from 'chai'

class lib {
  // @ts-ignore
  static isValidDtmi (a: any): any {
    return null
  }

  // @ts-ignore
  static dtmiToPath (a: any): any {
    return null
  }

  // @ts-ignore
  static dtmiToQualifiedPath (a: any, b: any, c: any): any {
    return null
  }
}

describe('dtmiConventions', function () {
  afterEach(() => {
    sinon.restore()
  })
  describe('isValidDtmi', function () {
    // TODO: Will implement more rigorous testing of DTMI validation over multiple different valid DTMIs.
    it('should validate a correctly formatted dtmi', function () {
      const validDtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
      const result = lib.isValidDtmi(validDtmi)
      assert(result, 'valid dtmi not found as valid')
    })

    // TODO: Will implement more rigorous testing of DTMI validation over multiple different invalid DTMIs.
    it('should invalidate an incorrectly formatted dtmi', function () {
      const invalidDtmi = 'dtmiazure:DeviceManagement:DeviceInformation;1'
      const result = lib.isValidDtmi(invalidDtmi)
      assert(!result, 'invalid dtmi incorrectly labelled as valid')
    })
  })

  describe('dtmiToPath', function () {
    it('should fail if the dtmi is not formatted correctly', function () {
      expect(() => {
        const invalidDtmi = 'dtmiazure:DeviceManagement:DeviceInformation;1'
        lib.dtmiToPath(invalidDtmi)
      }).to.throw('DTMI is incorrectly formatted. Ensure DTMI follows conventions.')
    })

    it('should reformat a DTMI to a generic path', function () {
      const validDtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
      const result = lib.dtmiToPath(validDtmi)
      assert.deepEqual(result, '/dtmi/azure/devicemanagement/deviceinformation-1.json')
    })
  })

  describe('dtmiToFullyQualifiedPath', function () {
    it('should fail if the dtmi is not formatted correctly', function () {
      expect(() => {
        const invalidDtmi = 'dtmiazure:DeviceManagement:DeviceInformation;1'
        const fakeBasePath = 'https://contoso.com'
        lib.dtmiToQualifiedPath(invalidDtmi, fakeBasePath, false)
      }).to.throw('DTMI is incorrectly formatted. Ensure DTMI follows conventions.')
    })

    it('should reformat a DTMI to a qualified URL path', function () {
      const validDtmi = 'dtmi:foobar:DeviceInformation;1'
      const fakeBasePath = 'https://contoso.com'
      const result = lib.dtmiToQualifiedPath(validDtmi, fakeBasePath, false)
      assert.deepEqual(result, 'https://contoso.com/dtmi/foobar/deviceinformation-1.json')
    })

    it('should add expanded to the path if specified', function () {
      const validDtmi = 'dtmi:foobar:DeviceInformation;1'
      const fakeBasePath = 'https://contoso.com'
      const result = lib.dtmiToQualifiedPath(validDtmi, fakeBasePath, true)
      assert.deepEqual(result, 'https://contoso.com/dtmi/foobar/deviceinformation-1.expanded.json')
    })
  })
})
