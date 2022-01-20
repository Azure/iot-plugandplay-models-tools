# Release History

## 1.0.0-beta.6 (2022-01-20)

- Supports dotnet 6
- Adds SourceLink configuration to package
- Upgrade Azure.IoT.ModelsRepository dependency to 1.0.0-preview.5

## 1.0.0-beta.5 (2021-09-02)

- The `validate` command supports validation of directory model files (recursive) with file search pattern.
- The `import` command supports the import of directory model files (recursive) with file search pattern.
- Removed the `--strict` flag from the `import` command. When importing model files to an IoT Models Repository strict validation
  is implicit.
- Upgrade Azure.IoT.ModelsRepository dependency to 1.0.0-preview.4
- Upgrade System.CommandLine dependency to 2.0.0-beta1.21308.1

## 1.0.0-beta.4 (2021-07-22)

- Initial NuGet package publish of Microsoft.IoT.ModelsRepository.CommandLine
