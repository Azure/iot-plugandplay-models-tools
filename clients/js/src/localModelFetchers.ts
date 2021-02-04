// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import { DTDL } from './DTDL'
import * as fs from 'fs'
import * as path from 'path'


// TODO: CHANGE THIS TO FIX NEW DATA STRUCTURES
function isFlat(x: {}) {
	const keysOfX = Object.keys(x);
	for (let i=0; i < keysOfX.length; i++) {
		if (typeof(i) !== 'object') {
			return false;
		}
	}
	return true;
}

function flattenDtdlResponse(input: DTDL[]) {
	let newResult: {[x: string]: DTDL} = {};
	input.forEach((element: DTDL) => {
		newResult[element['@id']] = element
	})
	return newResult
}

async function localModelFetcherRecursive (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{[x:string]: DTDL}> {
	let dependencyModels: {[x:string]: DTDL} = {};
	let fetchedModels: {[x: string]: DTDL };
	try {
		fetchedModels = await localModelFetcher(dtmi, directory, tryFromExpanded);
	} catch (error) {
		if (tryFromExpanded) {
			console.log(`ERROR ! ${error}`)
			console.log('TryFromExpanded Failed on current DTMI. Attempting Non-expanded.');
			fetchedModels = await localModelFetcher(dtmi, directory, false);
		}
		throw error
	}
	const dtmis = Object.keys(fetchedModels) 
	for (let i=0; i<dtmis.length; i++) {
		const currentDtdl = fetchedModels[dtmis[i]];
		const deps = modelMetadata.getModelMetadata(currentDtdl)['componentSchemas']
		if (deps && deps.length > 0) {
			for (let j=0; j<deps.length; j++) {
				const fetchedDependencies = await localModelFetcherRecursive(deps[i], directory, tryFromExpanded);
				dependencyModels = {...dependencyModels, ...fetchedDependencies};
			}
		}
	}
	if (Object.keys(dependencyModels).length > 0) {
		fetchedModels = {...fetchedModels, ...dependencyModels};
	}
	return fetchedModels
}

async function localModelFetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{ [dtmi: string]: DTDL }> {
	const dtmiPath = dtmiConventions.dtmiToPath(dtmi);
	const dtmiPathFormatted = tryFromExpanded ? dtmiPath.replace('.json', '.expanded.json') : dtmiPath 
	const targetPath = path.join(directory, dtmiPathFormatted);
	const dtdlFile = fs.readFileSync(targetPath, 'utf8');
	let parsedDtdl: DTDL | DTDL[] = JSON.parse(dtdlFile);
	if (!isFlat(parsedDtdl)) {
		const result = flattenDtdlResponse(parsedDtdl as DTDL[])
		return result
	} else {
		const result = {[dtmi]: parsedDtdl as DTDL}
		return result
	}
}

export { localModelFetcher, localModelFetcherRecursive }