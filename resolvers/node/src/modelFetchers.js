// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"



function remoteModelFetcher (remoteEndpoint) {
    const options = {
        hostname: repositoryEndpoint,
        port: 443,
        path: path,
        method: 'GET'
    }

    const req = https.request(options, res => {
        debug('statusCode: ', res.statusCode);
        res.on('data', d => {
            debug(d);
        });
        res.on('end', () => {
            debug('all data received');
            break;
        });
    });

    req.on('error', error => {
        debug(`DTMI not valid for endpoint ${repositoryEndpoint}`);
        debug(error);
    });

    req.end();
}

function localModelFetcher(localPathEndpoint) {

}

module.exports = function fetchModel(repositoryEndpoint) {
    // method
}