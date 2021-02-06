// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as modelMetadata from './modelMetadata'
import { DTDL } from './DTDL'
import fs from 'fs';
import * as path from 'path'
import { flattenDtdlResponse } from './modelFetcherHelper';

async function recursiveFetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{[x:string]: DTDL}> {
	let dependencyModels: {[x:string]: DTDL} = {};
	let fetchedModels: {[x: string]: DTDL };
	try {
		fetchedModels = await fetcher(dtmi, directory, tryFromExpanded);
	} catch (error) {
		if (tryFromExpanded && error.code === 'ENOENT') {
			console.log(`ERROR ! ${error}`)
			console.log('TryFromExpanded Failed on current DTMI. Attempting Non-expanded.');
			fetchedModels = await fetcher(dtmi, directory, false);
		} else {
			throw error
		}
	}
	const dtmis = Object.keys(fetchedModels) 
	for (let i=0; i<dtmis.length; i++) {
		const currentDtdl = fetchedModels[dtmis[i]];
		const deps = modelMetadata.getModelMetadata(currentDtdl)['componentSchemas']
		if (deps && deps.length > 0) {
			for (let j=0; j<deps.length; j++) {
				if (Object.keys(dependencyModels).includes(deps[j]) || Object.keys(fetchedModels).includes(deps[j])) {
					console.log(`${deps[j]} already fetched`)
				} else {
					const fetchedDependencies = await recursiveFetcher(deps[j], directory, tryFromExpanded);
					dependencyModels = {...dependencyModels, ...fetchedDependencies};
				}
			}
		}
	}
	if (Object.keys(dependencyModels).length > 0) {
		fetchedModels = {...fetchedModels, ...dependencyModels};
	}
	return fetchedModels
}

async function fetcher (dtmi: string, directory: string, tryFromExpanded: boolean): Promise<{ [dtmi: string]: DTDL }> {
	const dtmiPath = dtmiConventions.dtmiToPath(dtmi);
	const dtmiPathFormatted = tryFromExpanded ? dtmiPath.replace('.json', '.expanded.json') : dtmiPath 
	const targetPath = path.join(directory, dtmiPathFormatted);
	const dtdlFile = fs.readFileSync(targetPath, 'utf8');
	let parsedDtdl: DTDL | DTDL[] = JSON.parse(dtdlFile);
	if (Array.isArray(parsedDtdl)) {
		const result = flattenDtdlResponse(parsedDtdl as DTDL[])
		return result
	} else {
		const result = {[dtmi]: parsedDtdl as DTDL}
		return result
	}
}

export { fetcher, recursiveFetcher }