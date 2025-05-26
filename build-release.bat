@echo off
set VERSION=1.1.0
set APP_NAME=VideoDuplicateFinder

echo Building Video Duplicate Finder v%VERSION%...
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean VDF.GUI/VDF.GUI.csproj -c Release
if exist "Release" rmdir /s /q "Release"
mkdir "Release"

echo.
echo Building Windows x64 (self-contained)...
dotnet publish VDF.GUI/VDF.GUI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "Release/win-x64"

echo.
echo Building Windows x86 (self-contained)...
dotnet publish VDF.GUI/VDF.GUI.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "Release/win-x86"

echo.
echo Building Windows ARM64 (self-contained)...
dotnet publish VDF.GUI/VDF.GUI.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "Release/win-arm64"

echo.
echo Creating distribution packages...

REM Create ZIP packages
powershell -Command "Compress-Archive -Path 'Release/win-x64/VDF.GUI.exe' -DestinationPath 'Release/%APP_NAME%-v%VERSION%-win-x64.zip' -Force"
powershell -Command "Compress-Archive -Path 'Release/win-x86/VDF.GUI.exe' -DestinationPath 'Release/%APP_NAME%-v%VERSION%-win-x86.zip' -Force"
powershell -Command "Compress-Archive -Path 'Release/win-arm64/VDF.GUI.exe' -DestinationPath 'Release/%APP_NAME%-v%VERSION%-win-arm64.zip' -Force"

echo.
echo Build completed successfully!
echo Release files created in Release folder:
dir Release\*.zip
echo.
pause
