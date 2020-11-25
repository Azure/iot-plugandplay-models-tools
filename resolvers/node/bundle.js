'use strict';

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
Object.defineProperty(exports, "__esModule", { value: true });
exports.createEventEmitter = void 0;
const tslib_1 = require("tslib");
// This node built-in must be shimmed for the browser.
const events_1 = tslib_1.__importDefault(require("events"));
// this is a utility function from a library that should be external
// for both node and web
const core_http_1 = require("@azure/core-http");
tslib_1.__exportStar(require("./resolver"), exports);
function createEventEmitter() {
    // use event emitter
    const e = new events_1.default();
    // Dynamic Node and browser-specific code
    if (core_http_1.isNode) {
        console.log("Node 👊");
    }
    else {
        console.log("Browser ❤");
    }
    return e;
}
exports.createEventEmitter = createEventEmitter;
