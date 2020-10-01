# Dotnet Resolution Sample

This example project shows a minimum implementation of the [DMR resolution convention](https://github.com/Azure/device-models-tools/wiki/Resolution-Convention) for `dotnet` using `C#`.

The sample achieves the following points:

- Takes a `DTMI` argument or uses a default for resolution.
- Validates the `DTMI` format using RegEx predefined in the [DTMI specification document](https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions).
- Transforms the `DTMI` to a path using an implementation of the [DMR resolution convention](https://github.com/Azure/device-models-tools/wiki/Resolution-Convention).
- Retrieves string content via http request to a fully qualified path combining the DMR endpoint and transformed `DTMI`.
- Implements a resolver callback for the  [.NET DTDL Parser](https://www.nuget.org/packages/Microsoft.Azure.DigitalTwins.Parser)

## Quick Start

Open the project or solution file and start debugging!

Alternatively you can execute `dotnet run` and use the default `DTMI` arg of `dtmi:com:example:TemperatureController;1` or pass in your own `DTMI` arg via `dotnet run -- "<dtmiString>"`.

> :exclamation: Note be aware of shell rules for argument input. For example in powershell quote the dtmi input i.e. `dotnet run -- "<dtmiString>"`

This sample uses the DMR endpoint `https://devicemodeltest.azureedge.net` by default.

## Code walktrhough

To convert a DTMI to an absolute path we use the `DtmiToPath` function, with `IsValidDtmi`:

```cs
static string DtmiToPath(string dtmi)
{
    if (!IsValidDtmi(dtmi))
    {
        return null;
    }
    // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
    return $"/{dtmi.ToLowerInvariant().Replace(":", "/").Replace(";", "-")}.json";
}

static bool IsValidDtmi(string dtmi)
{
    // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
    Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
    return rx.IsMatch(dtmi);
}
```

With the resulting path and the base URL for the repository we can obtain the interface:

```cs
const string _repositoryEndpoint = "https://devicemodeltest.azureedge.net";

string dtmiPath = DtmiToPath(dtmi.ToString());
string fullyQualifiedPath = $"{_repositoryEndpoint}{dtmiPath}";
string modelContent = await _httpClient.GetStringAsync(fullyQualifiedPath);
```

To integrate with the .NET DTDL Parser, we provide a callback to resolve any interface requested during parsing.

Add a reference to the parser:

```dotnetcli
dotnet add package Microsoft.Azure.DigitalTwins.Parser
```

```cs
 static async Task<IEnumerable<string>> ResolveCallback(IReadOnlyCollection<Dtmi> dtmis)
{
    Console.WriteLine("ResolveCallback invoked!");
    List<string> result = new List<string>();

    foreach (Dtmi dtmi in dtmis)
    {
        string content = await Resolve(dtmi.ToString());
        result.Add(content);
    }

    return result;
}
```

To parse the DTDL document using the callback:

```cs
 // Assign the callback
ModelParser parser = new ModelParser
{
    DtmiResolver = ResolveCallback
};
await parser.ParseAsync(new List<string> { dtmiContent });
```
