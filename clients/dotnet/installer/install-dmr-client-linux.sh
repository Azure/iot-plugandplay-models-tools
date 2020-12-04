#!/bin/bash

# Copyright (c) Microsoft Corporation MIT license

dmr_client_ver="1.0.0-beta.1"
snapshot_ver="dev"

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
curl -# -o "$source_archive_dir/snapshot-$snapshot_ver" "https://codeload.github.com/Azure/iot-plugandplay-models-tools/tar.gz/$snapshot_ver"
cd "$source_archive_dir" && { tar -xf "snapshot-$snapshot_ver" ; cd -; }
root_cli_path="$source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet/Azure.Iot.ModelsRepository.CLI"

dotnet build -c Release --nologo $framework_target "$root_cli_path"
dotnet pack -c Release --no-build --nologo $pack_target "$root_cli_path"
dotnet tool install -g dmr-client $framework_target --add-source "$root_cli_path/bin/Release" --version $dmr_client_ver
