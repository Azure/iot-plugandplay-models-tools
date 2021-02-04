// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as fs from 'fs'
import { fileURLToPath } from 'url'

import { localModelFetcher, localModelFetcherRecursive } from './localModelFetchers'
// import { remoteModelFetcher, remoteModelFetcherRecursive } from './remoteModelFetchers'

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

export async function modelFetcher(dtmi: string, endpoint: string, resolveDependencies: boolean, tryFromExpanded: boolean): Promise<{ [dtmi: string]: JSON | Array<JSON> }> {
	const isLocal = isLocalPath(endpoint)
	if (isLocal) {
		const formattedDirectory = endpoint.includes('file://') ? fileURLToPath(endpoint) : endpoint
		if (tryFromExpanded || resolveDependencies) {
			return localModelFetcherRecursive(dtmi, formattedDirectory, tryFromExpanded);
		} else {
			return localModelFetcher(dtmi, formattedDirectory, false)
		}
	}
	throw new Error('LOCAL!')
	// else {
	// 	if (tryFromExpanded) {
	// 		try {
	// 			const dtmiMapping = await remoteModelFetcher(dtmi, endpoint, true)
	// 			if (typeof(dtmiMapping[dtmi]) !== 'object') {
	// 				return flattenedExpandedResult(dtmiMapping, dtmi)
	// 			} else {
	// 				return dtmiMapping
	// 			}
	// 		} 
	// 		catch (reason) {
	// 			console.log('resolving from expanded.json failed. Falling back on psuedo-parsing resolution.')
	// 			console.error(reason)
	// 			return await remoteModelFetcherRecursive(dtmi, endpoint)
	// 		}
	// 	} else if (resolveDependencies) {
	// 		return remoteModelFetcherRecursive(dtmi, endpoint)
	// 	}
	// 	return remoteModelFetcher(dtmi, endpoint, false)
	// }
}