// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

import { DTDL } from "./DTDL";

export function flattenDtdlResponse(input: DTDL[]) {
	let newResult: {[x: string]: DTDL} = {};
	input.forEach((element: DTDL) => {
		newResult[element['@id']] = element
	})
	return newResult
}