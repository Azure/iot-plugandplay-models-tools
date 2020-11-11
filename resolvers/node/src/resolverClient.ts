// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import logger = require('@azure/logger');
logger.setLogLevel('info');
import { fetchers } from 'modelFetchers';


const defaultEndpoint = 'devicemodels.azure.com';



function dtmiToPath (dtmi) {
    if (!isDtmi(dtmi)) {
        return null
    }
    // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
    return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
}


class ResolverClient {
    endpoint: string;
    // ResolverClient will take as input one endpoint.
    constructor (endpoint: string) {
        this.endpoint = endpoint;
    }

    async resolve (dtmi: string): Promise<void> {
        fetchers.fetchModel(dtmi);
    }
}

export { ResolverClient }