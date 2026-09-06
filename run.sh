#!/bin/bash

set -euo pipefail

dotnet build
clear
dotnet run --no-build --no-restore --project Aurora/Aurora.csproj -- "$1"
