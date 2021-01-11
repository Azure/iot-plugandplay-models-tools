// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import * as coreHttp from '@azure/core-http'
import * as fs from 'fs'
import { fileURLToPath } from 'url'

function remoteModelFetcherRecursive (dtmi: string, targetUrl: string): Promise<{[dtmi: string]: JSON }> {
	const client = new coreHttp.ServiceClient()
	let result: {[dtmi: string]: JSON } = {}
	function fetchFromEndpoint(dtmi1: any, targetUrl1: any): any {
		return new Promise((resolve, reject) => {
			const req: coreHttp.RequestPrepareOptions = {
				url: targetUrl1,
				method: "GET"
			}
			client.sendRequest(req)
			.then((res: coreHttp.HttpOperationResponse) => {
				const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
				const dtdlAsJson = JSON.parse(dtdlAsString)
				result[dtmi1] = dtdlAsJson
				const deps = modelMetadata.getModelMetadata(dtdlAsJson)
				if (deps) {
					console.log(deps)
					// deps.forEach(depDtmi => {
					// 	return fetchFromEndpoint(depDtmi, targetUrl1)
					// })
				}

				resolve(result)
			})
			.catch((err) => {
				reject(err)
			})
		})
	}

	return fetchFromEndpoint(dtmi, targetUrl)
}

function remoteModelFetcher (dtmi: string, targetUrl: string): Promise<{[dtmi: string]: any }> {
	const client = new coreHttp.ServiceClient()

	return new Promise((resolve, reject) => {
		const req: coreHttp.RequestPrepareOptions = {
			url: targetUrl,
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

function localModelFetcher (dtmi:string, targetPath: string): Promise<{ [dtmi: string]: JSON }> {
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

async function remoteModelFetcherFromExpanded(dtmi: string, targetUrl: string): Promise<{ [dtmi: string]: any }> {
	try {
		const result = await remoteModelFetcher(dtmi, targetUrl)
		let newResult = { [dtmi]: result[dtmi][0] }
		result[dtmi].forEach((element: any) => {
			newResult[element['@id']] = element
		})
		console.log(result)
		return newResult
	} catch {
		return await remoteModelFetcherRecursive(dtmi, targetUrl.replace('.expanded.json', '.json'))
	}
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
		return localModelFetcher(dtmi, dtmiConventions.dtmiToQualifiedPath(dtmi, formattedEndpoint, tryFromExpanded))
	}
	else if (tryFromExpanded) {
		return remoteModelFetcherFromExpanded(dtmi, dtmiConventions.dtmiToQualifiedPath(dtmi, endpoint, true))
	} else if (resolveDependencies) {
		return remoteModelFetcherRecursive(dtmi, dtmiConventions.dtmiToQualifiedPath(dtmi, endpoint, tryFromExpanded))
	}
	return remoteModelFetcher(dtmi, dtmiConventions.dtmiToQualifiedPath(dtmi, endpoint, false))
}