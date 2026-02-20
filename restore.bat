@echo off
REM Windows Batch Script to Restore NuGet Packages
REM This script resolves the NETSDK1004 error by restoring packages

echo ========================================
echo MB Business - NuGet Package Restore
echo ========================================
echo.

echo Checking .NET SDK installation...
dotnet --version
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 9.0 SDK or later from:
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo.

echo Navigating to MbBusinessApp directory...
cd MbBusinessApp
if %errorlevel% neq 0 (
    echo ERROR: MbBusinessApp directory not found!
    echo Make sure you run this script from the repository root.
    pause
    exit /b 1
)
echo.

echo Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Package restore failed!
    echo Try clearing NuGet cache: dotnet nuget locals all --clear
    pause
    exit /b 1
)
echo.

echo ========================================
echo SUCCESS: Packages restored successfully!
echo ========================================
echo.
echo Next steps:
echo 1. Build the project: dotnet build
echo 2. Run the application: dotnet run
echo.
echo Or open MbBusinessApp.csproj in Visual Studio 2022
echo.

cd ..
pause
