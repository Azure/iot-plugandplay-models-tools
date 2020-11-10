// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as debug from 'debug'
import https from 'https'



export function remoteModelFetcher (dtmi, endpoint) {

    const options = {
        hostname: endpoint,
        port: 443,
        path: dtmiConventions.dtmiToPath(dtmi),
        method: 'GET'
    }

    const req = https.request(options, res => {
        debug('statusCode: ', res.statusCode);
        res.on('data', d => {
            debug(d);
        });
        res.on('end', () => {
            debug('all data received');
            return;
        });
    });

    req.on('error', error => {
        debug(`DTMI not valid for endpoint ${endpoint}`);
        debug(error);
    });

    req.end();
}