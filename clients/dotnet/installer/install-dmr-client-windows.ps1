# Copyright (c) Microsoft Corporation MIT license

$dmr_client_ver="1.0.0-beta.2"

$source_archive_dir="dmr-tools-" + [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$framework_version=dotnet --version

if ($framework_version.StartsWith("3.1")){
    $framework_moniker="netcoreapp3.1"
    $pack_target=@("-p:NoWarn=NU5128", "-p:TargetFrameworks=$framework_moniker")
    $framework_target="--framework=$framework_moniker"
}
elseif ($framework_version.StartsWith("5")){
    $framework_moniker="net5.0"
}
else {
    Write-Host "dmr-client requires dotnetcore 3.1 SDK or dotnet 5.0 SDK. Detected '$framework_version'. "
    exit 1
}

Write-Host "Executing dmr-client install script for $framework_moniker..."
mkdir "$source_archive_dir"
Invoke-WebRequest -Uri "https://github.com/Azure/iot-plugandplay-models-tools/archive/refs/tags/$dmr_client_ver.tar.gz" -OutFile "$source_archive_dir/snapshot"
Push-Location
Set-Location -Path "$source_archive_dir"
tar -xf snapshot
Pop-Location
$root_cli_path = "$source_archive_dir/iot-plugandplay-models-tools-$dmr_client_ver/clients/dotnet/Microsoft.IoT.ModelsRepository.CommandLine"

dotnet build -c Release --nologo $framework_target "$root_cli_path"
dotnet pack -c Release --no-build --nologo $pack_target "$root_cli_path"
dotnet tool install -g Microsoft.IoT.ModelsRepository.CommandLine $framework_target --add-source "$root_cli_path/bin/Release" --version $dmr_client_ver
