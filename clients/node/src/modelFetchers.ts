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

	return fetchFromEndpoint(dtmi, endpoint, result)
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

function localModelFetcherRecursive (dtmi: string, endpoint: string): Promise<{[dtmi: string]: JSON }> {
	let result: {[dtmi: string]: JSON } = {}

	const fetchFromEndpoint = async function (fnDtmi: string, fnEndpoint: string, result: any): Promise<void> {
		console.log(`in fetchFromEndpoint ${fnDtmi} | ${fnEndpoint}`)
		const targetPath = dtmiConventions.dtmiToQualifiedPath(fnDtmi, fnEndpoint, false)
		const data = fs.readFileSync(targetPath, 'utf8')
		const dtdlAsJson = JSON.parse(data)
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

	return fetchFromEndpoint(dtmi, endpoint, result)
	.then(() => {
		return result
	}).catch(e => {
		return e
	})
}


// NOTE: Currently there is no support for getting dependencies
function localModelFetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{ [dtmi: string]: any }> {
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
		const formattedEndpoint = endpoint.includes('file://') ? fileURLToPath(endpoint) : endpoint
		if (tryFromExpanded) {
			return localModelFetcher(dtmi, formattedEndpoint, true)
			.then((result) => {
					if (result[dtmi].length === 1) {
						return { [dtmi]: result[dtmi][0] }
					} else {
						let newResult = { [dtmi]: result[dtmi][0] }
						result[dtmi].forEach((element: any) => {
							newResult[element['@id']] = element
						})
						console.log(result)
						return newResult
					}})
			.catch((reason) => {
					console.log('resolving from expanded.json failed. Falling back on psuedo-parsing resolution.')
					console.error(reason)
					return localModelFetcherRecursive(dtmi, formattedEndpoint)
				}
			)
		}
		return localModelFetcher(dtmi, formattedEndpoint, false)
	} else {
		if (tryFromExpanded) {
			return remoteModelFetcher(dtmi, endpoint, true)
			.then((result) => {
					if (result[dtmi].length === 1) {
						return { [dtmi]: result[dtmi][0] }
					} else {
						let newResult = { [dtmi]: result[dtmi][0] }
						result[dtmi].forEach((element: any) => {
							newResult[element['@id']] = element
						})
						console.log(result)
						return newResult
					}})
			.catch((reason) => {
					console.log('resolving from expanded.json failed. Falling back on psuedo-parsing resolution.')
					console.error(reason)
					return remoteModelFetcherRecursive(dtmi, endpoint)
				}
			)
		} else if (resolveDependencies) {
			return remoteModelFetcherRecursive(dtmi, endpoint)
		}
		return remoteModelFetcher(dtmi, endpoint, false)
	}
}