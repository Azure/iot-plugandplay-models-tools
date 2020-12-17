// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

// TODO: Once Implemented replace this
// import * as lib from '../../src/index'
import * as coreHttp from '@azure/core-http'

import { assert } from 'chai'
import * as sinon from 'sinon'

import fs from 'fs'
import path from 'path'

// fake class while lib not implemented
class lib {
  // @ts-ignore
  static resolve (a: any, b: any): any {
    return null
  }
}

describe('resolver - node', () => {
  afterEach(() => {
    sinon.restore()
  })

  describe('single resolution (no pseudo-parsing)', () => {
    describe('given remote URL resolution', () => {
      it('given successful execution, should return a promise that resolves to a mapping from a DTMI to a JSON object', function (done) {
        const fakeDtmi: string = 'dtmi:contoso:FakeDeviceManagement:DeviceInformation;1'
        const fakeEndpoint = 'devicemodels.contoso.com'
        const expectedUri = 'https://devicemodels.contoso.com/dtmi/contoso/fakedevicemanagement/deviceinformation-1.json'
        const fakeData = JSON.stringify({
          fakeDtdl: 'fakeBodyAsText'
        })
        sinon.stub(coreHttp, 'ServiceClient')
          .returns({
            sendRequest: function (req: any) {
              assert.deepEqual(req.url, expectedUri, 'URL not formatted for request correctly.')
              return Promise.resolve({ bodyAsText: fakeData })
            }
          })

        const resolveResult = lib.resolve(fakeDtmi, fakeEndpoint)
        assert(resolveResult instanceof Promise, 'resolve method did not return a promise')
        resolveResult.then((actualOutput: any) => {
          assert.deepStrictEqual({ [fakeDtmi]: JSON.parse(fakeData) }, actualOutput)
          done()
        }).catch((err: any) => done(err))
      })
    })

    describe('given local file resolution', () => {
      it('given successful execution, should return a promise that resolves to a mapping from a DTMI to a JSON object', function (done) {
        const fakeDtmi: string = 'dtmi:contoso:DeleteMeDevice;1'
        const fakeData = JSON.stringify({
          "fakeKey": "fakeValue"
        })
        const localDirectory = path.resolve('./')
        const expectedFilePath = path.join(localDirectory, './dtmi/contoso/deletemedevice-1.json')
        // @ts-ignore
        sinon.stub(fs, 'readFile').callsFake((path, opts, cb) => {
          assert.deepEqual(path, expectedFilePath, 'path to dtdl incorrectly formatted.')
        })
        const resolveResult = lib.resolve(fakeDtmi, localDirectory)
        assert(resolveResult instanceof Promise, 'resolve method did not return a promise')
        resolveResult.then((actualOutput: any) => {
          assert.deepStrictEqual({ [fakeDtmi]: JSON.parse(fakeData) }, actualOutput)
          done()
        }).catch((err: any) => done(err))
      })    })
  })

  describe('full resolution (using pseudo-parsing)', () => {
    describe('given remote URL resolution', () => {

    })

    describe('given local file resolution', () => {

    })
  })
})
