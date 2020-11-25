// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

'use strict'

export function isValidDtmi (dtmi: string) {
    if (dtmi) {
        const re = new RegExp("^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
        return re.test(dtmi); // true if dtmi matches regular expression, false otherwise
    }
    return false; // if not a string return false.
}

export function dtmiToPath (dtmi: string) {
    // presently this dtmi to path function does not return the path with a
    // file format at the end, i.e. does not append .json or .expanded.json.
    // that happens in the dtmiToQualifiedPath function

    if (isValidDtmi(dtmi)) {
        return `/${dtmi.toLowerCase().replace(/:/gm, '/').replace(/;/gm, '-')}.json`
    } else {
        throw new Error('dtmi not provided as type string');
    }
}

export function dtmiToQualifiedPath (dtmi: string, basePath: string, expanded ?: boolean) {
    const dtmiPath = dtmiToPath(dtmi);
    const intermediatePath = basePath.endsWith('/') ? basePath + '/' : basePath;
    const fullyQualifiedPath = intermediatePath + dtmiPath;
    if (expanded) {
        return fullyQualifiedPath.replace('.json', '.expanded.json');
    }

    return fullyQualifiedPath;
}

