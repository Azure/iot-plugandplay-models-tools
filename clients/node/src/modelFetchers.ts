// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import * as dtmiConventions from './dtmiConventions'
import * as coreHttp from '@azure/core-http'
import fs from 'fs'
import { fileURLToPath } from 'url'


/**
 * @private
 * remoteModelFetcher - for remote file paths
 *
 * @param dtmi string corresponding with specific device model.
 * @param targetUrl url endpoint where model repository is located.
 *
 * @returns Promise that resolves a mapping of dtmi strings to JSON dtdls.
 */
function remoteModelFetcher (dtmi: string, targetUrl: string): Promise<{[dtmi: string]: JSON }> {
    const client = new coreHttp.ServiceClient();

    return new Promise((resolve, reject) => {
        const req: coreHttp.RequestPrepareOptions = {
            url: targetUrl,
            method: "GET"
        };
        client.sendRequest(req)
        .then((res: coreHttp.HttpOperationResponse) => {
            const dtdlAsString = res.bodyAsText ? res.bodyAsText : ''
            const dtdlAsJson = JSON.parse(dtdlAsString)
            resolve({[dtmi]: dtdlAsJson});
        })
        .catch((err) => {
            reject(err)
        });
    })
}

/**
 * @private
 * localModelFetcher - for local file paths
 *
 * @param dtmi string corresponding with specific device model
 * @param targetPath local folder where model repository is located
 *
 * @returns Promise that resolves a mapping of dtmi strings to JSON dtdls.
 */
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

/**
 * @private
 * isLocalPath - helper function for validating if a string is a local folder path.
 *
 * @param p string to check if corresponds to path
 */
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

/**
 *
 * @param dtmi string representing the digital twin modeling identifier
 * @param endpoint local or remote URL or path where the model repository being used is located
 * @param expanded boolean
 * @param resolveDependencies
 */
export function modelFetcher(dtmi: string, endpoint: string, expanded: boolean, resolveDependencies: boolean): Promise<{ [dtmi: string]: JSON}> {
    const isLocal = isLocalPath(endpoint)
    const formattedPath = dtmiConventions.dtmiToPath(dtmi)

    if (expanded) {
        throw new Error('expanded has not been implemented yet')
    } else if (resolveDependencies) {
        throw new Error('resolveDependencies has not been implemented yet')
    }

    if (isLocal) {
        let formattedEndpoint = endpoint
        if (endpoint.includes('file://')) {
            formattedEndpoint = fileURLToPath(endpoint)
        }
        return localModelFetcher(dtmi, `${formattedEndpoint}${formattedPath}`)
    }

    return remoteModelFetcher(dtmi, `${endpoint}${formattedPath}`)
}
