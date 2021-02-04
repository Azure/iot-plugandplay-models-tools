// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import * as coreHttp from '@azure/core-http'

async function remoteModelFetcherRecursive (dtmi: string, endpoint: string): Promise<{[dtmi: string]: JSON }> {
	const client = new coreHttp.ServiceClient()
	let result: {[dtmi: string]: JSON } = {}

	const fetchFromEndpoint = async function (fnDtmi: string, fnEndpoint: string, result: any): Promise<void> {
		console.log(`in fetchFromEndpoint ${fnDtmi} | ${fnEndpoint}`)
		const req: coreHttp.RequestPrepareOptions = {
			url: dtmiConventions.dtmiToQualifiedPath(fnDtmi, fnEndpoint, false),
			method: "GET"
		}
		const res: coreHttp.HttpOperationResponse = await client.sendRequest(req)
		console.log(`${fnDtmi}: response received`)
		const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
		const dtdlAsJson = JSON.parse(dtdlAsString)
		result[fnDtmi] = dtdlAsJson
		const dtdlMetaData = modelMetadata.getModelMetadata(dtdlAsJson)
		const deps = dtdlMetaData['componentSchemas']
		if (deps && deps.length > 0) {
			console.log(deps)
			for (let i=0; i < deps.length; i++) {
				await fetchFromEndpoint(deps[i], fnEndpoint, result)
			}
		}
	}

	await fetchFromEndpoint(dtmi, endpoint, result)
	return result
}

async function remoteModelFetcher (dtmi: string, endpoint: string, tryFromExpanded: boolean): Promise<{[dtmi: string]: any }> {
	const client = new coreHttp.ServiceClient()
	const req: coreHttp.RequestPrepareOptions = {
		url: dtmiConventions.dtmiToQualifiedPath(dtmi, endpoint, tryFromExpanded),
		method: "GET"
	}
	const res: coreHttp.HttpOperationResponse = await client.sendRequest(req);
	if (res.status >= 200 && res.status < 400) {
		const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
		const dtdlAsJson = JSON.parse(dtdlAsString)
		return {[dtmi]: dtdlAsJson}
	} else {
		const respError = `${res.parsedBody}${res.status}`
		throw new Error(`Error on HTTP Request: ${respError}`)
	}
}

export { remoteModelFetcher, remoteModelFetcherRecursive }