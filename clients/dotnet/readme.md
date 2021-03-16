
# Microsoft IoT Plug and Play Models Repository Tools

## :exclamation: WARNING: This project is under heavy active development and should not be depended on until further notice

## Microsoft IoT Models Repository CLI

This solution includes a CLI project `Azure.Iot.ModelsRepository.CLI` to interact with local and remote repositories. 

### Install dmr-client

The tool is distributed as source code and requires `dotnet sdk 3.1` to build and install.

#### Linux/Bash

```bash
curl -L https://aka.ms/install-dmr-client-linux | bash
```

#### Windows/Powershell

```powershell
iwr https://aka.ms/install-dmr-client-windows -UseBasicParsing | iex
```

### dmr-client Usage

```text
dmr-client:
  Microsoft IoT Plug and Play Device Models Repository CLI v0.0.17.0

Usage:
  dmr-client [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  export      Retrieve a model and its dependencies by dtmi or model file using the target repository for model
              resolution.
  validate    Validates a model using the DTDL model parser & resolver. The target repository is used for model
              resolution.
  import      Validates a local model file then adds it to the local repository.

  
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

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" --repo https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main
```

### dmr-client import

```bash
# Adds an external file to the `dtmi` folder structure in the current working directory

> dmr-client import --model-file "MyThermostat.json" --local-repo .

# Creates the path `.dtmi/com/example/thermostat-1.json`
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
