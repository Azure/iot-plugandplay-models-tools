#!/bin/sh -l

cd /App

dotnet restore
dotnet build
dotnet run $TOKEN $REPO_ID $PULL_REQUEST_ID $FORMAT