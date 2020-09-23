
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


// Instantiate a parser as usual
ModelParser parser = new ModelParser
{
    Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
};

// Make a resolver client using the desired registry
ResolverClient client = ResolverClient.FromRemoteRegistry("https://iotmodels.github.io/registry/");

// Assign the ResolverClient.ParserDtmiResolver delegate
parser.DtmiResolver = client.ParserDtmiResolver;

// Use ParseAsync() as normal - when the parser needs dtmi content it will invoke the resolver
var parserResult = await parser.ParseAsync(...);
```

## Federated Model Resolution

- Coming soon!

## Configuration

Out of the box, the `ResolverClient` has a default configuration with common settings so it is not strictly necessary for to provide one.

### General Settings

- Dependency resolution

### Caching Settings

- Coming soon!

## Resolver Client CLI

This solution includes a CLI project `Azure.DigitalTwins.Resolver.CLI` to jumpstart scenarios. You are able to invoke commands via `dotnet run` or as the compiled executable `resolverclient`.

```bash
resolverclient:
  Microsoft IoT Plug and Play Model Resolution CLI

Usage:
  resolverclient [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  show        Retrieve a model and its dependencies by dtmi using the target registry for model resolution.
  validate    Validates a model using the Digital Twins parser and target registry for model resolution.
```

**Examples**

```bash
# Retrieves the target model and its dependencies by dtmi using the default model registry.

> resolverclient show --dtmi "dtmi:com:example:Thermostat;1"
```

```bash
# Retrieves the target model and its dependencies by dtmi using a custom registry endpoint.

> resolverclient show --dtmi "dtmi:com:example:Thermostat;1" --registry "https://mycustom.domain/models/"
```

```bash
# Retrieves the target model and its dependencies by dtmi using a custom local registry.

> resolverclient show --dtmi "dtmi:com:example:Thermostat;1" --registry "/my/models/"
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and default model registry for resolution.

> resolverclient validate --model-file ./my/model/file.json
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and custom registry endpoint for resolution.

> resolverclient validate --model-file ./my/model/file.json --registry "https://mycustom.domain/models/"
```

## Common Issues

- Coming soon!
