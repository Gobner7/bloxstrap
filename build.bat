@echo off
echo ========================================
echo  Bloxstrap Enhanced - Build Script
echo ========================================
echo.

REM Check for .NET 6 SDK
echo Checking for .NET 6 SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 6 SDK not found!
    echo Please install .NET 6 SDK from: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

echo .NET 6 SDK found!
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "Bloxstrap\bin" rmdir /s /q "Bloxstrap\bin"
if exist "Bloxstrap\obj" rmdir /s /q "Bloxstrap\obj"

echo.
echo Building Bloxstrap Enhanced...
echo ========================================

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore NuGet packages!
    pause
    exit /b 1
)

REM Build in Release mode
echo.
echo Building in Release mode...
dotnet build --configuration Release --no-restore
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

REM Publish self-contained executable
echo.
echo Publishing self-contained executable...
dotnet publish Bloxstrap\Bloxstrap.csproj --configuration Release --runtime win-x64 --self-contained true --output "build\Release" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Build completed successfully!
echo ========================================
echo.
echo Output location: build\Release\Bloxstrap.exe
echo.

REM Create bypass DLL directory
echo Creating bypass DLL directory...
if not exist "build\Release\bypass" mkdir "build\Release\bypass"

REM Copy additional files
echo Copying additional files...
if exist "README.md" copy "README.md" "build\Release\"
if exist "LICENSE" copy "LICENSE" "build\Release\"

echo.
echo ========================================
echo  IMPORTANT SECURITY NOTICE
echo ========================================
echo.
echo This enhanced version contains:
echo - Advanced anti-cheat bypass techniques
echo - Memory patching capabilities  
echo - Process injection mechanisms
echo - Performance optimization systems
echo.
echo WARNING: This software may trigger antivirus
echo software due to its advanced capabilities.
echo Use at your own risk and ensure you understand
echo the legal implications in your jurisdiction.
echo.
echo Features included:
echo + Byfron/Hyperion bypass
echo + Unlimited FPS optimization
echo + Memory and CPU optimization
echo + Real-time performance monitoring
echo + Process hollowing capabilities
echo + Advanced stealth techniques
echo.

pause