
# Microsoft IoT Plug and Play Model Resolution Client

## :exclamation: WARNING: This project is under heavy active development and should not be depended on in anyway until future notice

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

## Common Errors

- Coming soon!

## Contributing

- Coming soon!
