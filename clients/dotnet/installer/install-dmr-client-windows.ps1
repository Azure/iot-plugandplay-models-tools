# Copyright (c) Microsoft Corporation MIT license

$dmr_client_ver="1.0.0-beta.1"
$snapshot_ver="dev"

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
mkdir $source_archive_dir
Invoke-WebRequest -Uri "https://codeload.github.com/Azure/iot-plugandplay-models-tools/tar.gz/$snapshot_ver" -OutFile "$source_archive_dir/snapshot-$snapshot_ver"
Push-Location
Set-Location -Path "$source_archive_dir"
tar -xf "snapshot-$snapshot_ver"
Pop-Location
$root_cli_path = "$source_archive_dir/iot-plugandplay-models-tools-$snapshot_ver/clients/dotnet/Azure.Iot.ModelsRepository.CLI"

dotnet build -c Release --nologo $framework_target $root_cli_path
dotnet pack -c Release --no-build --nologo $pack_target "$root_cli_path"
dotnet tool install -g dmr-client $framework_target --add-source "$root_cli_path/bin/Release" --version $dmr_client_ver
