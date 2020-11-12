# Copyright (c) Microsoft Corporation MIT license

$source_archive_dir = "dmr-tools-" + [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$snapshot_ver = "0.4-preview"

Write-Host "Running dmr-client install script..."
mkdir $source_archive_dir
Invoke-WebRequest -Uri "https://codeload.github.com/Azure/iot-plugandplay-models-tools/tar.gz/$snapshot_ver" -OutFile "$source_archive_dir/snapshot-$snapshot_ver"
Push-Location
Set-Location -Path "$source_archive_dir"
tar -xf "snapshot-$snapshot_ver"
Pop-Location
dotnet pack "$source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet" -v q -c Release
dotnet tool install -g dmr-client --add-source "$source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet/Azure.Iot.ModelsRepository.CLI/bin/Release"
