#!/bin/bash
# Shell Script to Restore NuGet Packages
# This script resolves the NETSDK1004 error by restoring packages

echo "========================================"
echo "MB Business - NuGet Package Restore"
echo "========================================"
echo ""

echo "Checking .NET SDK installation..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found!"
    echo "Please install .NET 9.0 SDK or later from:"
    echo "https://dotnet.microsoft.com/download"
    exit 1
fi

dotnet --version
echo ""

echo "Navigating to MbBusinessApp directory..."
if [ ! -d "MbBusinessApp" ]; then
    echo "ERROR: MbBusinessApp directory not found!"
    echo "Make sure you run this script from the repository root."
    exit 1
fi

cd MbBusinessApp
echo ""

echo "Restoring NuGet packages..."
if ! dotnet restore; then
    echo "ERROR: Package restore failed!"
    echo "Try clearing NuGet cache: dotnet nuget locals all --clear"
    exit 1
fi
echo ""

echo "========================================"
echo "SUCCESS: Packages restored successfully!"
echo "========================================"
echo ""
echo "Next steps:"
echo "1. Build the project: dotnet build"
echo "2. Run the application: dotnet run"
echo ""
echo "Or open MbBusinessApp.csproj in your IDE"
echo ""

cd ..
