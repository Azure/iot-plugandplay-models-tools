// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

"use strict"

function getId(content: any): string {
    const idElement = content['id']
    return idElement
}

function getExtends(content: any): string {
    const extendElement = content['extends']
    return extendElement
}

function getComponentSchemas(content: any): string[] {
    const componentSchemas: string[] = []
    if (content['contents']) {
        const contents: Array<string> = content['contents']
        contents.forEach((element: any) => {
            if (element['@type']
            && (typeof element['@type'] === 'string') &&
            element['@type'] === 'component') {
                if (element['schema'] && typeof element['schema'] === 'string') {
                    componentSchemas.push(element['schema'])
                }
            }
        })
    }
    return componentSchemas
}

export  function getModelMetadata(content: JSON) {
    const idElement: string = getId(content)
    const extendsElement: string = getExtends(content)
    const componentSchemas: string[] = getComponentSchemas(content)
    return {
        id: idElement,
        extends: extendsElement,
        componentSchemas: componentSchemas
    }

}