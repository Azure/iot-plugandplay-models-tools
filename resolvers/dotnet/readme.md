
# Microsoft IoT Plug and Play Model Resolution Client

## :exclamation: WARNING: This project is under heavy active development and should not be depended on until further notice

## Overview

The model resolution client `ResolverClient` provides functionality for retrieving Digital Twin Definition Language (`DTDL`) models from a device model repository such as the *IoT Plug and Play Device Model Repository* [https://github.com/Azure/iot-plugandplay-models](https://github.com/Azure/iot-plugandplay-models).

## Usage

The client is available in the NuGet package `Azure.IoT.DeviceModelsRepository.Resolver` as `netstandard2.0`.

> Note. The package is not yet available on NuGet.org.

### Default settings

The following code block shows the basic usage of the `ResolverClient` using default parameters:

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;

ResolverClient client = new ResolverClient();
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

The resolver can be customized to use a different repository, local or remote:

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;

ResolverClient client = new ResolverClient("https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

To configure the repository from a local folder use an absolute path:

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;

ResolverClient client = new ResolverClient("/LocalModelRepo/");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

### DependencyResolutionOption

If the root interface has dependencies with external interfaces, via `expand` or `@component` the client can be configured with the next `DependencyResolutionOption`:

|DependencyResolutionOption|Description|
|--------------------------|-----------|
|Disabled|Do not process external dependencies|
|Enabled|Enable external dependencies|
|TryFromExpanded|Try to get external dependencies using [.expanded.json](https://github.com/Azure/iot-plugandplay-models-tools/wiki/Resolution-Convention#expanded-dependencies)|

The next code block shows how to configure the resolver with a custom `DependencyResolutionOption`

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;

ResolverClient rc = new ResolverClient(new ResolverClientOptions(DependencyResolutionOption.Enabled));
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:TemperatureController;1");
```

### Logging

To support traceability and diagnostics, the `ResolverClient` supports an optional `ILogger` parameter to pass in during initialization.

The following shows an example of how to pass in an `ILogger` instance.

```csharp
ILogger logger = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace)).CreateLogger<Program>();
ResolverClient rc = new ResolverClient(logger);
```

Logging configuration is done with the standard `Microsoft.Extensions.Hosting` pattern using configuration appsettings or via environment variables.

When using env vars, it's recommended using the cross-platform environment variable syntax (double underscore `__` delimiters).

```powershell
# powershell example for setting the default logger min log level to "Trace" in the current session.
$env:Logging__LogLevel__Default="Trace"
```

```bash
# bash example for setting the default logger min log level to "Warning"
export Logging__LogLevel__Default="Trace"
```

## Integration with the DigitalTwins Model Parser

The `ResolverClient` is designed to work independently of the Digital Twins `ModelParser`.

There are two options to integrate with the parser:

### Resolve before parsing

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

string dtmi = "dtmi:com:example:TemperatureController;1";
ILogger logger = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace)).CreateLogger<Program>();
ResolverClient rc = new ResolverClient(logger);
var models = await rc.ResolveAsync(dtmi);
ModelParser parser = new ModelParser();
var parseResult = await parser.ParseAsync(models.Values.ToArray());
Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
```

### Resolve while parsing

The parser call a `DtmiResolverCallback` when it founds an unknown `@Id`, to configure the callback to be used from the parser, you can use the sister package
`Azure.IoT.DeviceModelsRepository.Resolver.Extensions` to support this  integration:

```csharp
using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

string dtmi = "dtmi:com:example:TemperatureController;1";
ILogger logger = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace)).CreateLogger<Program>();
ResolverClient rc = new ResolverClient(new ResolverClientOptions(DependencyResolutionOption.Enabled), logger);
var models = await rc.ResolveAsync(dtmi);
ModelParser parser = new ModelParser();
parser.DtmiResolver = rc.ParserDtmiResolver;
var parseResult = await parser.ParseAsync(models.Values.Take(1).ToArray());
Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
```

## Error Handling

When the `ResolverClient` hits an issue resolving `DTMI`'s a `ResolverException` will be thrown which summarizes the issue. The `ResolverException` may contain an inner exception with additional details as to why the exception occured.

This snippet from the `CLI` shows a way to use `ResolverException`.

```csharp
try
{
    result = await new ResolverClient().ResolveAsync(dtmi);
}
catch (ResolverException resolverEx)
{
    logger.LogError(resolverEx.Message);
}
```

## Device Model Repository Client

This solution includes a CLI project `Azure.IoT.DeviceModelsRepository.CLI` to jumpstart scenarios. You are able to invoke commands via `dotnet run` or as the compiled executable `dmr-client`.

```text
dmr-client:
  Microsoft IoT Plug and Play Device Model Repository CLI

Usage:
  dmr-client [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  export      Retrieve a model and its dependencies by dtmi or model file using the target repository for model
              resolution.
  validate    Validates a model using the Digital Twins model parser. Uses the target repository for model resolution.
  import      Adds a model to the repo. Validates ids, dependencies and set the right folder/file name
  
```

## Examples

### dmr-client export

```bash
# Retrieves an interface from the default repo by DTMI

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1"
> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" -o thermostat.json
```

>Note: The quotes are required to avoid the shell to split the param in the `;`

```bash
# Retrieves an interface from a custom  repo by DTMI

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" --repository https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main
```



### dmr-client import

```bash
# Adds an external file to the `dtmi` folder structure in the current working directory CWD

> dmr-client import --dtmi "dtmi:com:example:Thermostat;1"

# Creates the path `dtmi/com/example/thermostat-1.json`
```

### dmr-client validate

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and default model repository for resolution.

> dmr-client validate --model-file file.json
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and custom repository endpoint for resolution.

> dmr-client validate --model-file ./my/model/file.json --repository "https://mycustom.domain"
```
