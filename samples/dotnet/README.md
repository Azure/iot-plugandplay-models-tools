# Dotnet Resolution Sample

This example project shows a minimum implementation of the [DMR resolution convention](https://github.com/Azure/device-models-tools/wiki/Resolution-Convention) for `dotnet` using `C#`.

The sample achieves the following points:

- Takes a `DTMI` argument or uses a default for resolution.
- Validates the `DTMI` format using RegEx predefined the [DTMI specification document](https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions).
- Transforms the `DTMI` to a relative path using an implementation of the DMR resolution convention [DMR resolution convention](https://github.com/Azure/device-models-tools/wiki/Resolution-Convention).
- Makes an http request to a fully qualified address combining the DMR endpoint and transformed `DTMI`.

## Quick Start

Open the project or solution file and start debugging!

Alternatively you can execute `dotnet run` and use the default `DTMI` arg of `dtmi:azure:DeviceManagement:DeviceInformation;1` or pass in your own `DTMI` arg via `dotnet run -- "<dtmiString>"`.

This sample uses the `DMR` endpoint `https://devicemodels.azure.com`.
