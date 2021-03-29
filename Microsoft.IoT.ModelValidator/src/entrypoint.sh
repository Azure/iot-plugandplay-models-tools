#!/bin/sh -l

cd /App

dotnet restore
dotnet build
dotnet run $TOKEN $PULL_REQUEST_ID $FORMAT