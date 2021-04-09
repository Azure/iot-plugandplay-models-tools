#!/bin/bash

# Copyright (c) Microsoft Corporation MIT license

dmr_client_ver="1.0.0-beta.2"

source_archive_dir=dmr-tools-$(date +"%s")
framework_version=$(dotnet --version)
framework_moniker=""

if [[ "$framework_version" = "3.1"* ]]; then
    framework_moniker="netcoreapp3.1"
    pack_target="-p:NoWarn=NU5128 -p:TargetFrameworks=$framework_moniker"
    framework_target="--framework $framework_moniker"
elif [[ "$framework_version" = "5"* ]]; then
    framework_moniker="net5.0"
else
    echo "dmr-client requires dotnetcore 3.1 SDK or dotnet 5.0 SDK. Detected '$framework_version'. " && exit 1
fi

echo "Executing dmr-client install script for $framework_moniker..."
mkdir "$source_archive_dir"
curl -L -# -o "$source_archive_dir/snapshot" "https://github.com/digimaun/iot-plugandplay-models-tools/archive/refs/tags/$dmr_client_ver.tar.gz"
cd "$source_archive_dir" && { tar -xf snapshot ; cd -; }
root_cli_path="$source_archive_dir/iot-plugandplay-models-tools-$dmr_client_ver/clients/dotnet/Microsoft.IoT.ModelsRepository.CommandLine"

dotnet build -c Release --nologo $framework_target "$root_cli_path"
dotnet pack -c Release --no-build --nologo $pack_target "$root_cli_path"
dotnet tool install -g Microsoft.IoT.ModelsRepository.CommandLine $framework_target --add-source "$root_cli_path/bin/Release" --version $dmr_client_ver
