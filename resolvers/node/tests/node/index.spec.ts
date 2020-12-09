// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as lib from '../../src/index'
import * as modelFetchers from '../../src/modelFetchers'

import { assert } from 'chai'
import * as sinon from 'sinon'

describe('resolver - node', () => {
  afterEach(() => {
    sinon.restore()
  })

  it('given successful execution, should return a promise', (done) => {
    sinon.stub(modelFetchers, 'remoteModelFetcher').returns(Promise.resolve({ fakeDTMI: 'fakeJSON' }))
    const result = lib.resolve('fakeDTMI', 'fakeEndpoint')
    assert(result instanceof Promise)
    done()
  })

  it('given successful execution, should return map of DTMI to DTDL', (done) => {
    const validFakeDTMI = 'dtmi:foo:BarDevice:FakeDeviceInformation;1'
    const validFakeEndpoint = 'contoso.com'
    // sinon.stub(modelFetchers, 'remoteModelFetcher').returns(Promise.resolve({ fakeDTMI: 'fakeJSON' }))
    lib.resolve('fakeDTMI', 'fakeEndpoint')
      .then((result) => {
        assert.deepStrictEqual(result, { fakeDTMI: 'fakeJSON' })
        done()
      }).catch((err) => {
        console.log(err)
        done(err)
      })
  })
})
