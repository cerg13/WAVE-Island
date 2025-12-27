#!/bin/bash

# WAVE Island Build Script
# Usage: ./build.sh [android|ios|all] [dev|prod]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"
BUILD_METHOD="BuildScript"

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check Unity installation
check_unity() {
    if [ ! -f "$UNITY_PATH" ]; then
        print_error "Unity not found at $UNITY_PATH"
        print_info "Please update UNITY_PATH in build.sh"
        exit 1
    fi
    print_info "Unity found: $UNITY_PATH"
}

# Function to build Android
build_android() {
    print_info "Building Android..."

    "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "WaveIsland.Editor.BuildScript.BuildAndroid" \
        -logFile "Builds/Android/build.log"

    if [ $? -eq 0 ]; then
        print_info "Android build completed successfully!"
    else
        print_error "Android build failed. Check Builds/Android/build.log"
        exit 1
    fi
}

# Function to build Android App Bundle
build_android_bundle() {
    print_info "Building Android App Bundle..."

    "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "WaveIsland.Editor.BuildScript.BuildAndroidAppBundle" \
        -logFile "Builds/Android/bundle.log"

    if [ $? -eq 0 ]; then
        print_info "Android App Bundle completed successfully!"
    else
        print_error "Android App Bundle build failed. Check Builds/Android/bundle.log"
        exit 1
    fi
}

# Function to build iOS
build_ios() {
    print_info "Building iOS..."

    "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -projectPath "$PROJECT_PATH" \
        -executeMethod "WaveIsland.Editor.BuildScript.BuildIOS" \
        -logFile "Builds/iOS/build.log"

    if [ $? -eq 0 ]; then
        print_info "iOS build completed successfully!"
        print_info "Open Xcode project in Builds/iOS/"
    else
        print_error "iOS build failed. Check Builds/iOS/build.log"
        exit 1
    fi
}

# Function to set build mode
set_build_mode() {
    local mode=$1

    if [ "$mode" = "dev" ]; then
        print_info "Setting development build mode..."
        "$UNITY_PATH" \
            -quit \
            -batchmode \
            -nographics \
            -projectPath "$PROJECT_PATH" \
            -executeMethod "WaveIsland.Editor.BuildScript.SetDevelopmentBuild"
    elif [ "$mode" = "prod" ]; then
        print_info "Setting production build mode..."
        "$UNITY_PATH" \
            -quit \
            -batchmode \
            -nographics \
            -projectPath "$PROJECT_PATH" \
            -executeMethod "WaveIsland.Editor.BuildScript.SetProductionBuild"
    fi
}

# Main script
main() {
    local platform=${1:-all}
    local mode=${2:-prod}

    print_info "=== WAVE Island Build Script ==="
    print_info "Platform: $platform"
    print_info "Mode: $mode"
    echo ""

    # Check Unity
    check_unity

    # Set build mode
    set_build_mode "$mode"

    # Build based on platform
    case $platform in
        android)
            build_android
            ;;
        android-bundle)
            build_android_bundle
            ;;
        ios)
            build_ios
            ;;
        all)
            build_android
            build_ios
            ;;
        *)
            print_error "Unknown platform: $platform"
            print_info "Usage: ./build.sh [android|ios|android-bundle|all] [dev|prod]"
            exit 1
            ;;
    esac

    print_info "=== Build Complete ==="
}

# Run main
main "$@"
