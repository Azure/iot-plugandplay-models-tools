// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

const { debug } = require('debug')('resolver');
const https = require('https');
var validUrl = require('valid-url');
const fetchers = require('modelFetchers');
const { url } = require('inspector');

const defaultEndpoint = 'devicemodels.azure.com';

function endpointToURIObject (endpoint) {
    if (validUrl.isUri(endpoint)) {
        // endpoint is URL
        return { 'uri' : endpoint }
    } else {
        // endpoint is file
        return { 'file' : endpoint }
    }
}

function dtmiToPath (dtmi) {
    if (!isDtmi(dtmi)) {
        return null
    }
    // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
    return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
}


class ResolverClient {
    // ResolverClient will take as input one endpoint. If the endpoint is more than
    // one value it should error.
    constructor (endpoint, localOrRemote) {
        if (Array.isArray(endpoint)) { return new Error('on instantiation can only pass in one endpoint. Cannot pass in a list.'); }
        this.endpointList = [endpoint]
        this.location = localOrRemote
    }

    get endpoints() {
        return this.endpointList;
    }

    static fromRemoteRepository (uri) {
        let location;
        if (!uri) {
            location = defaultEndpoint;
        } else {
            location = uri;
        }
        const resolver = new ResolverClient(location);
        return resolver
    }

    static fromLocalRepository (path) {
        // takes in a fully resolved local path and gets the JSON and returns.
        let location;
        if (!path) {
            location = url.pathToFileURL(__dirname)
        } else {
            location = url.pathToFileURL(path)
        }
        const resolver = new ResolverClient(location);
        return resolver;
    }

    addEndpoints (endpoints) {
        if (!Array.isArray(endpoints)) {
            endpoints = [endpoints]
        }

        endpoints.forEach((endpoint) => {
            const endpointObject = endpointToURIObject(endpoint);
            this.endpointList.push(endpointObject)
        });
    }


    resolve (dtmis) {
        let output;
        // takes a list of DTMIs and parses them
        let dtmiList;
        if (!Array.isArray(dtmiList)) {
            dtmiList = [dtmis]
        } else {
            dtmiList = dtmis
        }

        dtmiList.forEach((dtmi) => {
            // iterate over the DTMIs and then go through the endpoints list to get
            // the JSON
            let model;
            this.endpointList.forEach((repositoryEndpoint) => {
                // TODO: Implement fetchModel API in ModelFetcher
                const result = fetchers.fetchModel(repositoryEndpoint);
                if (result) {
                    model = result;
                    break;
                }
                // if there is a result, break the loop
                // this should break the endpointList loop to move onto the next dtmi.
            });

            if (!model) {
                throw new Error(`for dtmi ${dtmi}, no model found in any of the endpoints.`);
            }
            output.push({ [dtmi] : model })
        });
    }
}

module.exports = ResolverClient
module.exports.ResolverClient = ResolverClient