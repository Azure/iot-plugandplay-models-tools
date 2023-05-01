# Release History

## 1.0.0-beta.8 (2023-03)

- Switch to latest DTDLParser (1.0.52). Please note: no version and major.minor version forms in DTMIs are not supported

## 1.0.0-beta.8 (2023-03)

- Update target frameworks to net6 and net7
- Removes net31 and net5 support
- Support DTDL v3 (behind the argument `max-dtdl-version`)
- Switch to latest DTDLParser (1.0.*-preview)
- Adds `--force` option to `import` #190
- Removes `Newtonsoft.Json` dependency

## 1.0.0-beta.7 (2022-08-08)

- Add explicit `Newtonsoft.Json` reference for `13.0.1`. This is a dependency for the DTDL parser however this change will constrain the installable version.

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
