// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import https from 'https'



export function remoteModelFetcher (dtmi: string, endpoint: string, expanded: boolean): Promise<{ [dtmi: string]: string}> {
    const formattedPath = expanded ? dtmiConventions.dtmiToPath(dtmi).replace('.json','.expanded.json') : dtmiConventions.dtmiToPath(dtmi);
    const options = {
        hostname: endpoint,
        port: 443,
        path: formattedPath,
        method: 'GET'
    }
    return new Promise((resolve, reject) => {
        let body: Buffer[] = [];
        const req = https.request(options, res => {
            console.log('statusCode: ', res.statusCode);
            if (res.statusCode && res.statusCode >= 400) {
               reject(res);
            } else {
                res.on('data', (chunk: Buffer) => {
                    body.push(chunk)
                    console.log(chunk);
                });
                res.on('end', () => {
                    console.log('all data received');
                    let stringBody = Buffer.concat(body).toString();
                    resolve({ [dtmi] : stringBody });
                });
            }
        });
        req.on('error', error => {
            console.log(`DTMI not valid for endpoint ${endpoint}`);
            console.error(error);
            reject(error);
        });

        req.end();
    })
}