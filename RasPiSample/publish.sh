#!/usr/bin/env bash
dotnet publish RasPiSample.csproj -c Debug -r linux-arm /p:PublishSingleFile=true
