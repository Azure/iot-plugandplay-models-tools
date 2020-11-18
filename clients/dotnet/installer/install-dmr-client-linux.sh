#!/bin/bash

# Copyright (c) Microsoft Corporation MIT license

source_archive_dir=dmr-tools-$(date +"%s")
snapshot_ver="1.0.0-beta.1"

echo "Running dmr-client install script..."
mkdir $source_archive_dir
curl -# -o $source_archive_dir/snapshot-$snapshot_ver https://codeload.github.com/Azure/iot-plugandplay-models-tools/tar.gz/$snapshot_ver
cd $source_archive_dir && { tar -xf snapshot-$snapshot_ver ; cd -; }
dotnet pack $source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet -v q -c Release
dotnet tool install -g dmr-client --add-source $source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet/Azure.Iot.ModelsRepository.CLI/bin/Release
