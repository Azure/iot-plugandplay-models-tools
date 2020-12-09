// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as coreHttp from "@azure/core-http"


export function remoteModelFetcher (dtmi: string, endpoint: string, expanded: boolean): Promise<{ [dtmi: string]: string}> {
    const formattedPath = dtmiConventions.dtmiToPath(dtmi, expanded)
    const req: coreHttp.RequestPrepareOptions = {
        url: `${endpoint}${formattedPath}`,
        method: "GET"
    };

    return new Promise((resolve, reject) => {
        const client = new coreHttp.ServiceClient();
        client.sendRequest(req)
        .then((res: coreHttp.HttpOperationResponse) => {
            if (res.bodyAsText) {
                console.log(res.bodyAsText.substr(0,1000));
            } else {
                console.log('res is undefined or null');
            }
            const result = { [dtmi] : res.bodyAsText ? res.bodyAsText : '' };
            resolve(result);
        })
        .catch((err) => {
            console.error(err);
            reject(err);
        });
    })
}