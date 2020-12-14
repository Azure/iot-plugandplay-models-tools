// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as coreHttp from '@azure/core-http'
import fs from 'fs'
import { fileURLToPath } from 'url'

function remoteModelFetcher (dtmi: string, targetUrl: string): Promise<{ [dtmi: string]: JSON }> {
    const req: coreHttp.RequestPrepareOptions = {
        url: targetUrl,
        method: "GET"
    };

    return new Promise((resolve, reject) => {
        const client = new coreHttp.ServiceClient();
        client.sendRequest(req)
        .then((res: coreHttp.HttpOperationResponse) => {
            const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
            const dtdlAsJson = JSON.parse(dtdlAsString)
            const result = { [dtmi] : dtdlAsJson }
            resolve(result);
        })
        .catch((err) => {
            reject(err)
        });
    })
}

function localModelFetcher (dtmi:string, targetPath: string): Promise<{ [dtmi: string]: JSON }> {
    return new Promise((resolve, reject) => {
        fs.readFile(targetPath, 'utf8', function (err, data) {
            if (err) {
                reject(err)
            } else {
                const dtdlAsJson = JSON.parse(data)
                const result = { [dtmi]: dtdlAsJson }
                resolve(result)
            }
        })
    })
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

export function modelFetcher(dtmi: string, endpoint: string, expanded: boolean): Promise<{ [dtmi: string]: JSON}> {
    const isLocal = isLocalPath(endpoint)
    const formattedPath = dtmiConventions.dtmiToPath(dtmi, expanded);
    if (isLocal) {
        let formattedEndpoint = endpoint
        if (endpoint.includes('file://')) {
            formattedEndpoint = fileURLToPath(endpoint)
        }
        return localModelFetcher(dtmi, `${formattedEndpoint}${formattedPath}`);
    }
    return remoteModelFetcher(dtmi, `${endpoint}${formattedPath}`, )
}