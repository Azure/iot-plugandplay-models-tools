# Microsoft IoT Models Repository Command Line

This solution includes a command line project `Microsoft.IoT.ModelsRepository.CommandLine` intended to be used as a `dotnet tool` to manage and interact with models repositories implemented with Azure IoT conventions.

> Note: `Microsoft.IoT.ModelsRepository.CommandLine` is in preview and may contain breaking changes between preview versions until a GA release.

## Install the dmr-client command line tool

The Device Models Repository command line tool (aka `dmr-client`) is published on [NuGet](https://www.nuget.org/packages/Microsoft.IoT.ModelsRepository.CommandLine) and requires `dotnet sdk `6.0.x` or `7.0.x`.

You can use the `dotnet` command line via the `dotnet tool install` command to install `dmr-client`. The following is an example to install `dmr-client` as a global tool:

`dotnet tool install -g Microsoft.IoT.ModelsRepository.CommandLine --version 1.0.0-beta.8`

To learn how to install `dmr-client` in a local context, please see [this guide](https://docs.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use).

## Usage of `dmr-client`

```text
dmr-client
  Microsoft IoT Models Repository CommandLine v1.0.0-beta.8

Usage:
  dmr-client [options] [command]

Options:
  --debug          Shows additional logs for debugging. [default: False]
  --silent         Silences command output on standard out. [default: False]
  --version        Show version information
  --max-dtdl-version Sets Max DTDL Version for import and validate
  -?, -h, --help   Show help and usage information

Commands:
  export    Exports a model producing the model and its dependency chain in an expanded format.
            The target repository is used for model resolution.
  validate  Validates the DTDL model contained in a file. When validating a single model object the target repository
            is used for model resolution. When validating an array of models only the array contents is used for resolution.
  import    Imports models from a model file into the local repository. The local repository is used for model resolution.
            Target model files for import will first be validated to ensure adherence to IoT Models Repository conventions.
  index     Builds a model index file from the state of a target local models repository.
  expand    For each model in a local repository, generate expanded model files and insert them in-place.
            The expanded version of a model includes the model with its full model dependency chain.
```

## Examples

### dmr-client export

```bash
# Retrieves a model definition by DTMI from the global repository https://devicemodels.azure.com.

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1"

# This form will pipe the command output to the desired file specified in the -o argument.
> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" -o thermostat.json
```

> Note: Parsing of symbols by the shell is executed before input is passed to the command line tool. In this case, the quotes are used to avoid the shell splitting symbols around the semi-colon `;` character in the `--dtmi` argument `dtmi:com:example:Thermostat;1`

```bash
# Retrieves a model definition by DTMI from a custom models repository

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" --repo https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main
```

### dmr-client import

```bash
# Adds an external model to the target models repository (in this case the current working directory) following the DTMI to path convention.

> dmr-client import --model-file "MyExampleThermostat1.json" --local-repo . --max-dtdl-version 2

# Creates the path `./dtmi/com/example/thermostat-1.json`
```

### dmr-client validate

```bash
# Validates a DTDL v2 model using the Digital Twins Parser and global models repository https://devicemodels.azure.com for model dependency resolution.

> dmr-client validate --model-file "/path/to/model/file.json" --max-dtdl-version 2
```

```bash
# Validates a DTDL v2 model using the Digital Twins Parser and custom models repository https://devicemodels.azure.com for model dependency resolution.

> dmr-client validate --model-file "/path/to/model/file.json" --repo "https://mycustom.domain"
```

### dmr-client index

```bash
# Builds a model index for the repository. If models exceed the page limit new page files will be created relative to the root index.

> dmr-client index --local-repo .
```

```bash
# Build a model index with a custom page limit indicating max models per page.

> dmr-client index --local-repo . --page-limit 2048
```

### dmr-client expand

```bash
# Expand all models of a target local models repository following Azure IoT conventions. Expanded model definitions are inserted in-place.

> dmr-client expand --local-repo "/path/to/models/repository"
```

```bash
# The default --local-repo value is the current directory. Be sure to specifiy the root path of the repository for --local-repo.

> dmr-client expand
```
