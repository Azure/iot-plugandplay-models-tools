
# Microsoft IoT Plug and Play Model Resolution Client

## :exclamation: WARNING: This project is under heavy active development and should not be depended on until further notice

## Overview

The model resolution client `ResolverClient` provides functionality for retrieving Digital Twin Definition Language (`DTDL`) models and related dependencies via a configured model registry.  

## Usage

The following code block shows initializing a `ResolverClient` with a **remote endpoint** model registry and retrieving a desired model (specified by `DTMI`) and its dependencies.

```csharp
using Azure.DigitalTwins.Resolver;

ResolverClient client = ResolverClient.FromRemoteRegistry("https://iotmodels.github.io/registry/");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:thermostat;1");
```

You are also able to initialize the `ResolverClient` with a **local directory** model registry.

```csharp
using Azure.DigitalTwins.Resolver;

ResolverClient client = ResolverClient.FromLocalRegistry(@"C:\Me\MyLocalRegistry");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:thermostat;1");
```

The client `ResolveAsync()` function has overloads to look up multiple models at once. This is achieved by passing in comma delimited `DTMI`'s **or** passing in an `IEnumerable<string>` of `DTMI`'s.

```csharp
using Azure.DigitalTwins.Resolver;

ResolverClient client = ResolverClient.FromRemoteRegistry("https://iotmodels.github.io/registry/");

// Id's for reuse
string dtmiToResolve1 = "dtmi:com:example:thermostat;1";
string dtmiToResolve2 = "dtmi:com:example:sensor;1";

// Multi resolution path 1
Dictionary<string, string> models = await client.ResolveAsync(dtmiToResolve1, dtmiToResolve2);

// Multi resolution path 2
string[] targetDtmis = new string[] {dtmiToResolve1, dtmiToResolve2};
Dictionary<string, string> models = await client.ResolveAsync(targetDtmis);
```

## Integration with the DigitalTwins Model Parser

The `ResolverClient` is designed to work independently of the Digital Twins `ModelParser`. However this solution includes a sister package
`Azure.DigitalTwins.Resolver.Extensions` to support integration.

Here is an example to show how this works.

```csharp
using Microsoft.Azure.DigitalTwins.Parser;
using Azure.DigitalTwins.Resolver;
using Azure.DigitalTwins.Resolver.Extensions;


// Instantiate the parser as usual
ModelParser parser = new ModelParser()

// Make a resolver client using the desired registry
ResolverClient client = ResolverClient.FromRemoteRegistry("https://iotmodels.github.io/registry/");

// Assign the ResolverClient.ParserDtmiResolver delegate
parser.DtmiResolver = client.ParserDtmiResolver;

// Use ParseAsync() as normal - when the parser needs dtmi content it will invoke the resolver
var parserResult = await parser.ParseAsync(...);
```

## Logging

To support traceability and diagnostics, the `ResolverClient` supports an optional `ILogger` parameter to pass in during initialization.

The following shows an example of how to pass in an `ILogger` instance.

```csharp
IServiceProvider serviceProvider = host.Services;
ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
ILogger logger = loggerFactory.CreateLogger(typeof(Program));

logger.LogInformation($"Using registry location {registry}");

// ResolverClient will use the logger.
client = ResolverClient.FromRemoteRegistry(registry, logger);
```

Logging configuration is done with the standard `Microsoft.Extensions.Hosting` pattern using configuration appsettings or via environment variables.

When using env vars, we recommend using the cross-platform environment variable syntax (double underscore `__` delimiters).

```powershell
# powershell example for setting the default logger min log level to "Trace" in the current session.
$env:Logging__LogLevel__Default="Trace"
```

```bash
# bash example for setting the default logger min log level to "Warning"
export Logging__LogLevel__Default="Trace"
```

## Error Handling

In general when the `ResolverClient` hits an issue resolving `DTMI`'s a `ResolverException` will be thrown which summarizes the issue. The `ResolverException` may contain an inner exception with additional details as to why the exception occured.

This snippet from the `CLI` shows a way to use `ResolverException`.

```csharp
try
{
    logger.LogInformation($"Using registry location {registry}");
    result = await InitializeClient(registry, logger).ResolveAsync(dtmi);
}
catch (ResolverException resolverEx)
{
    logger.LogError(resolverEx.Message);
    return ReturnCodes.ResolutionError;
}
```

## Settings

Out of the box, the `ResolverClient` has default settings with common options so it is not strictly necessary for you to provide one.

### General Settings

- Dependency resolution

### Caching Settings

- Coming soon!

## Device Model Repository CLI

This solution includes a CLI project `Azure.DigitalTwins.Resolver.CLI` to jumpstart scenarios. You are able to invoke commands via `dotnet run` or as the compiled executable `dmr-client`.

```bash
dmrclient:
  Microsoft IoT Plug and Play Device Model Repository CLI

Usage:
  dmr-client [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  show        Shows the fully qualified path of an input dtmi. Does not evaluate existance of content.
  resolve     Retrieve a model and its dependencies by dtmi using the target repository for model resolution.
  validate    Validates a model using the Digital Twins model parser. Uses the target repository for model resolution.
```

## Examples

### dmr-client show

```bash
# Show the fully qualified path of dtmi:com:example:Thermostat;1 with respect to the default repository.

> dmr-client show --dtmi "dtmi:com:example:Thermostat;1"
```

```bash
# Show the fully qualified path of dtmi:com:example:Thermostat;1 with respect to a custom local repository.

> dmr-client show --dtmi "dtmi:com:example:Thermostat;1" --repository "/my/model/repo"
```

### dmr-client resolve

```bash
# Retrieves the target model and its dependencies by dtmi using the default model repository.

> dmr-client resolve --dtmi "dtmi:com:example:Thermostat;1"
```

```bash
# Retrieves the target model and its dependencies by dtmi using a custom repository endpoint.

> dmr-client resolve --dtmi "dtmi:com:example:Thermostat;1" --repository "https://mycustom.domain/models/"
```

```bash
# Retrieves the target model and its dependencies by dtmi using the default model repository and save contents to a new file with the path /my/model/result.json.

> dmr-client resolve --dtmi "dtmi:com:example:Thermostat;1" -o "/my/model/result.json"
```

```bash
# Retrieves the target model and its dependencies by dtmi using a custom local repository.

> dmr-client resolve --dtmi "dtmi:com:example:Thermostat;1" --repository "/my/models/"
```

### dmr-client validate

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and default model repository for resolution.

> dmr-client validate --model-file ./my/model/file.json
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and custom repository endpoint for resolution.

> dmr-client validate --model-file ./my/model/file.json --repository "https://mycustom.domain/models/"
```
