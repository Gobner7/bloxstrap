#!/bin/bash

echo "========================================"
echo " Bloxstrap Enhanced - Build Script"
echo "========================================"
echo

# Check for .NET 6 SDK
echo "Checking for .NET 6 SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET 6 SDK not found!"
    echo "Please install .NET 6 SDK from: https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
fi

echo ".NET 6 SDK found!"
echo

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin obj Bloxstrap/bin Bloxstrap/obj

echo
echo "Building Bloxstrap Enhanced..."
echo "========================================"

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore NuGet packages!"
    exit 1
fi

# Build in Release mode
echo
echo "Building in Release mode..."
dotnet build --configuration Release --no-restore
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed!"
    exit 1
fi

# Publish self-contained executable
echo
echo "Publishing self-contained executable..."
dotnet publish Bloxstrap/Bloxstrap.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output "build/Release" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true

if [ $? -ne 0 ]; then
    echo "ERROR: Publish failed!"
    exit 1
fi

echo
echo "========================================"
echo " Build completed successfully!"
echo "========================================"
echo
echo "Output location: build/Release/Bloxstrap.exe"
echo

# Create bypass DLL directory
echo "Creating bypass DLL directory..."
mkdir -p "build/Release/bypass"

# Copy additional files
echo "Copying additional files..."
[ -f "README.md" ] && cp "README.md" "build/Release/"
[ -f "LICENSE" ] && cp "LICENSE" "build/Release/"

echo
echo "========================================"
echo " IMPORTANT SECURITY NOTICE"
echo "========================================"
echo
echo "This enhanced version contains:"
echo "- Advanced anti-cheat bypass techniques"
echo "- Memory patching capabilities"
echo "- Process injection mechanisms"
echo "- Performance optimization systems"
echo
echo "WARNING: This software may trigger antivirus"
echo "software due to its advanced capabilities."
echo "Use at your own risk and ensure you understand"
echo "the legal implications in your jurisdiction."
echo
echo "Features included:"
echo "+ Byfron/Hyperion bypass"
echo "+ Unlimited FPS optimization"
echo "+ Memory and CPU optimization"
echo "+ Real-time performance monitoring"
echo "+ Process hollowing capabilities"
echo "+ Advanced stealth techniques"
echo