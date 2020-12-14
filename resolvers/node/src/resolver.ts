// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import logger = require('@azure/logger');
logger.setLogLevel('info');
import { modelFetcher } from './modelFetchers';


/**
 * resolve
 */
function resolve(dtmi: string, endpoint: string): Promise<{ [dtmi: string]: JSON}>;
function resolve(dtmi: string, endpoint: string, expanded: boolean): Promise<{ [dtmi: string]: JSON}>;
function resolve(dtmi: string, endpoint : string, expanded ?: boolean): Promise<{ [dtmi: string]: JSON}> {
    let isExpanded: boolean;
    if (expanded) {
        isExpanded = expanded;
    } else {
        isExpanded = false;
    }
    return modelFetcher(dtmi, endpoint, isExpanded);
}


export { resolve }