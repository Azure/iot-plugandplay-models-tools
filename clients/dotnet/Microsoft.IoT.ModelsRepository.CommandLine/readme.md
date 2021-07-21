
# Microsoft IoT Models Repository Tools

## :exclamation: WARNING: This project is under heavy active development - breaking changes may occur between versions

## Microsoft IoT Models Repository CommandLine

This solution includes a command line project `Microsoft.IoT.ModelsRepository.CommandLine` intended to be used as a `dotnet tool` to manage and interact with repositories implemented with Azure IoT conventions.

### Install the dmr-client command line tool

The Device Models Repository command line tool (aka `dmr-client`) is published on [NuGet](https://www.nuget.org/packages/Microsoft.IoT.ModelsRepository.CommandLine) and requires `dotnet sdk 3.1` or `dotnet sdk 5.0`.
> Note .NET 6 is not yet supported.

You can use the `dotnet` command line via the `dotnet tool install` command to install `dmr-client`. The following is an example to install `dmr-client` as a global tool:

`dotnet tool install -g Microsoft.IoT.ModelsRepository.CommandLine --version 1.0.0-beta.4`

To learn how to install `dmr-client` in a local context, please see [this guide](https://docs.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use).

### Usage of `dmr-client`

```text
dmr-client:
  Microsoft IoT Models Repository CommandLine v1.0.0-beta.4

Usage:
  dmr-client [options] [command]

Options:
  --debug           Shows additional logs for debugging.
  --silent          Silences command output on standard out.
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  export      Exports a model producing the model and its dependency chain in an expanded format. The target repository is used for model resolution.
  validate    Validates the DTDL model contained in a file. When validating a single model object the target repository is used for model resolution. When
              validating an array of models only the array contents is used for resolution.
  import      Imports models from a model file into the local repository. The local repository is used for model resolution.
  index       Builds a model index file from the state of a target local models repository.
  expand      For each model in a local repository, generate expanded model files and insert them in-place. The expanded version of a model includes the model
              with its full model dependency chain.
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
# Retrieves an interface from a custom repo by DTMI

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" --repo https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main
```

### dmr-client import

```bash
# Adds an external file to the `dtmi` folder structure in the current working directory

> dmr-client import --model-file "MyThermostat.json" --local-repo .

# Creates the path `./dtmi/com/example/thermostat-1.json`
```

### dmr-client validate

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and default model repository for resolution.

> dmr-client validate --model-file file.json
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and custom repository endpoint for resolution.

> dmr-client validate --model-file ./my/model/file.json --repo "https://mycustom.domain"
```

### dmr-client index

```bash
# Builds a model index for the repository. If models exceed the page limit new page files will be created relative to the root index.

> dmr-client index --local-repo .
```

```bash
# Build a model index with a custom page limit indicating max models per page.

> dmr-client index --local-repo . --page-limit 100
```

### dmr-client expand

```bash
# Expand all models from the root directory of a local models repository following Azure IoT conventions.
# Expanded models are inserted in-place.

> dmr-client expand --local-repo .
```

```bash
# The default --local-repo value is the current directory. Be sure to specifiy the root for --local-repo.

> dmr-client expand
```
