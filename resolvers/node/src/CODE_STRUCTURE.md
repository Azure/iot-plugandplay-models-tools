## resolverClient.ts

This file contains the logic for defining instantiation and the main API surface for the client library. This is extremely simple right now because all we are doing is instantiating with a single endpoint, and then resolving with a single DTMI. The other work, like manipulating the DTMI into it's URL format, all that other work happens in the next stage of the client.

## dtmiConventions.ts

Contains methods for checking that the DTMI is valid, and to convert the DTMI to a string.

## modelFetchers.ts

Given a DTMI, this will fetch the model and return it to the user.