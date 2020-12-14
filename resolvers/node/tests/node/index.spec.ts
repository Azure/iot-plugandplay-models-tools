// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as lib from '../../src/index'
import * as coreHttp from '@azure/core-http'

import { assert } from 'chai'
import * as sinon from 'sinon'

import fs from 'fs'
import path from 'path'

describe('resolver - node', () => {
  afterEach(() => {
    sinon.restore()
  })

  describe('single resolution (no pseudo-parsing)', () => {
    describe('given remote URL resolution', () => {
      it('given successful execution, should return a promise that resolves to a mapping from a DTMI to a JSON object', function (done) {
        const fakeDtmi: string = 'dtmi:contoso:FakeDeviceManagement:DeviceInformation;1'
        const fakeData = JSON.stringify({
          fakeDtdl: 'fakeBodyAsText'
        })
        sinon.stub(coreHttp, 'ServiceClient')
          .returns({
            sendRequest: function () {
              return Promise.resolve({ bodyAsText: fakeData })
            }
          })
        const fakeEndpoint = 'devicemodels.contoso.com'
        const resolveResult = lib.resolve(fakeDtmi, fakeEndpoint)
        assert(resolveResult instanceof Promise)
        resolveResult.then((actualOutput) => {
          assert.deepStrictEqual({ [fakeDtmi]: JSON.parse(fakeData) }, actualOutput)
          done()
        }).catch(err => done(err))
      })
    })

    describe('given local file resolution', () => {
      it('given successful execution, should return a promise that resolves to a mapping from a DTMI to a JSON object', function (done) {
        const fakeDtmi: string = 'dtmi:contoso:DeleteMeDevice;1'
        const fakeData = JSON.stringify({
          "fakeKey": "fakeValue"
        })
        const localDirectory = path.resolve('./')
        sinon.stub(fs, 'readFile').callsArgWith(2, null, fakeData)
        const resolveResult = lib.resolve(fakeDtmi, localDirectory)
        assert(resolveResult instanceof Promise)
        resolveResult.then((actualOutput) => {
          assert.deepStrictEqual({ [fakeDtmi]: JSON.parse(fakeData) }, actualOutput)
          done()
        }).catch(err => done(err))
      })    })
  })

  describe('full resolution (using pseudo-parsing)', () => {
    describe('given remote URL resolution', () => {

    })

    describe('give local file resolution', () => {

    })
  })
})
