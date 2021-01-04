// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import logger from '@azure/logger';
logger.setLogLevel('info');
import { modelFetcher } from './modelFetchers';


/**
 * resolve - get interfaces (dtdls) associated to a given dtmi
 *
 * @param dtmi code used to label and organize dtdl
 * @param endpoint URL or local path for dtdl repository
 * @param options object containing optional parameters
 *
 * @returns Promise that resolves to mapping of dtmi(s) to JSON dtdl(s)
 */
function resolve(dtmi: string, endpoint: string): Promise<{ [dtmi: string]: JSON}>;
function resolve(dtmi: string, endpoint: string, options: any): Promise<{ [dtmi: string]: JSON}>;
function resolve(dtmi: string, endpoint : string, options ?: any): Promise<{ [dtmi: string]: JSON}> {
    const isExpanded: boolean = options.expanded ?? false;
    const resolveDependencies: boolean = options.resolveDependencies ?? false;

    return modelFetcher(dtmi, endpoint, isExpanded, resolveDependencies);
}


export { resolve }