// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import * as coreHttp from '@azure/core-http'
import * as fs from 'fs'
import { fileURLToPath } from 'url'

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

async function localModelFetcherRecursive (dtmi: string, endpoint: string): Promise<{[dtmi: string]: JSON }> {
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

	await fetchFromEndpoint(dtmi, endpoint, result)
	return result

}


// NOTE: Currently there is no support for getting dependencies
async function localModelFetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{ [dtmi: string]: any }> {
	const targetPath = dtmiConventions.dtmiToQualifiedPath(dtmi, directory, tryFromExpanded)
	const fileBuffer = fs.readFileSync(targetPath, 'utf8');
	const dtdlAsJson = JSON.parse(fileBuffer)
	const result = { [dtmi]: dtdlAsJson }
	return result
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

function flattenedExpandedResult(jsonMapping: any, baseDtmi: string): {[dtmi: string]: JSON } {
	if (jsonMapping[baseDtmi].length === 1) {
		return { [baseDtmi]: jsonMapping[baseDtmi][0] }
	} else {
		let newResult = { [baseDtmi]: jsonMapping[baseDtmi][0] }
		jsonMapping[baseDtmi].forEach((element: any) => {
			newResult[element['@id']] = element
		})
		return newResult
	}
}

export async function modelFetcher(dtmi: string, endpoint: string, resolveDependencies: boolean, tryFromExpanded: boolean): Promise<{ [dtmi: string]: JSON | Array<JSON> }> {
	const isLocal = isLocalPath(endpoint)
	if (isLocal) {
		const formattedEndpoint = endpoint.includes('file://') ? fileURLToPath(endpoint) : endpoint
		if (tryFromExpanded) {
			try {
				const dtmiMapping = await localModelFetcher(dtmi, formattedEndpoint, true)
				return flattenedExpandedResult(dtmiMapping, dtmi)
			} catch (reason) {
				console.log('resolving from expanded.json failed. Falling back on psuedo-parsing resolution.')
				console.error(reason)
				return await localModelFetcherRecursive(dtmi, formattedEndpoint)
			}
		}
		return localModelFetcher(dtmi, formattedEndpoint, false)
	} else {
		if (tryFromExpanded) {
			try {
				const dtmiMapping = await remoteModelFetcher(dtmi, endpoint, true)
				return flattenedExpandedResult(dtmiMapping, dtmi)
			} catch (reason) {
				console.log('resolving from expanded.json failed. Falling back on psuedo-parsing resolution.')
				console.error(reason)
				return await remoteModelFetcherRecursive(dtmi, endpoint)
			}
		} else if (resolveDependencies) {
			return remoteModelFetcherRecursive(dtmi, endpoint)
		}
		return remoteModelFetcher(dtmi, endpoint, false)
	}
}