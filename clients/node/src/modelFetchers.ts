// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import * as coreHttp from '@azure/core-http'
import * as fs from 'fs'
import { fileURLToPath } from 'url'

function remoteModelFetcherRecursive (dtmi: string, endpoint: string): Promise<{[dtmi: string]: JSON }> {
	const client = new coreHttp.ServiceClient()
	let result: {[dtmi: string]: JSON } = {}

	const fetchFromEndpoint = async function (fnDtmi: any, fnEndpoint: any): Promise<void> {
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
			await Promise.all(deps.map(async (depDtmi) => {
				console.log('calling fetchFromEndpoint')
				try {
					await fetchFromEndpoint(depDtmi, fnEndpoint)
					console.log(result)
				} catch (e) {
					return e
				}
			}))
		}

	}

	return fetchFromEndpoint(dtmi, endpoint)
	.then(() => {
		return result
	}).catch(e => {
		return e
	})
}

function remoteModelFetcher (dtmi: string, endpoint: string, tryFromExpanded: boolean): Promise<{[dtmi: string]: any }> {
	const client = new coreHttp.ServiceClient()

	return new Promise((resolve, reject) => {
		const req: coreHttp.RequestPrepareOptions = {
			url: dtmiConventions.dtmiToQualifiedPath(dtmi, endpoint, tryFromExpanded),
			method: "GET"
		}
		client.sendRequest(req)
		.then((res: coreHttp.HttpOperationResponse) => {
			if (res.status >= 200 && res.status < 400) {
				const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
				const dtdlAsJson = JSON.parse(dtdlAsString)
				resolve({[dtmi]: dtdlAsJson})
			} else {
				const respError = `${res.parsedBody}${res.status}`
				reject(new Error(respError))
			}
		})
		.catch((err) => {
			reject(err)
		})
	})
}

async function remoteModelFetcherFromExpanded(dtmi: string, endpoint: string): Promise<{ [dtmi: string]: any }> {
	try {
		const result = await remoteModelFetcher(dtmi, endpoint, true)
		let newResult = { [dtmi]: result[dtmi][0] }
		result[dtmi].forEach((element: any) => {
			newResult[element['@id']] = element
		})
		console.log(result)
		return newResult
	} catch {
		return await remoteModelFetcherRecursive(dtmi, endpoint)
	}
}

// NOTE: Currently there is no support for getting dependencies
function localModelFetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{ [dtmi: string]: JSON }> {
	const targetPath = dtmiConventions.dtmiToQualifiedPath(dtmi, directory, tryFromExpanded)
	return new Promise((resolve, reject) => {
		fs.readFile(targetPath, 'utf8', function (err, data) {
			if (err) {
				reject(err)
			} else {
				const dtdlAsJson = JSON.parse(data)
				const result = { [dtmi]: dtdlAsJson }
				resolve(result)
			}
		})
	})
}

function isLocalPath (p: string): boolean {
	if (p.startsWith('https://') || p.startsWith('http://')) {
		return false
	} else if (p.startsWith('file://')) {
		return true
	} else {
		try {
			fs.accessSync(p)
			return true
		} catch {
			return false
		}
	}
}

export function modelFetcher(dtmi: string, endpoint: string, resolveDependencies: boolean, tryFromExpanded: boolean): Promise<{ [dtmi: string]: JSON | Array<JSON> }> {
	const isLocal = isLocalPath(endpoint)

	if (isLocal) {
		if (resolveDependencies) {
			return Promise.reject('Local Dependency Resolution is not supported. Disable resolution or use \'tryFromExpanded\'.')
		}
		const formattedEndpoint = endpoint.includes('file://') ? fileURLToPath(endpoint) : endpoint
		return localModelFetcher(dtmi, formattedEndpoint, tryFromExpanded)
	}
	else if (tryFromExpanded) {
		return remoteModelFetcherFromExpanded(dtmi, endpoint)
	} else if (resolveDependencies) {
		return remoteModelFetcherRecursive(dtmi, endpoint)
	}
	return remoteModelFetcher(dtmi, endpoint, false)
}