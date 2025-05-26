#!/bin/bash

# Code Analysis Script for Perceptual Hashing Implementation
# This script performs static analysis without requiring compilation

echo "=== VDF Perceptual Hashing Code Analysis ==="
echo "Date: $(date)"
echo

# Function to check if a file exists and report
check_file() {
    local file="$1"
    local description="$2"
    
    if [ -f "$file" ]; then
        echo "✓ $description: $file ($(wc -l < "$file") lines)"
        return 0
    else
        echo "✗ $description: $file (NOT FOUND)"
        return 1
    fi
}

# Function to search for patterns in code
check_pattern() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    
    if [ -f "$file" ]; then
        local count=$(grep -c "$pattern" "$file" 2>/dev/null || echo "0")
        if [ "$count" -gt 0 ]; then
            echo "  ✓ $description: Found $count occurrence(s)"
        else
            echo "  ✗ $description: Not found"
        fi
    fi
}

# Function to analyze file complexity
analyze_complexity() {
    local file="$1"
    
    if [ -f "$file" ]; then
        local lines=$(wc -l < "$file")
        local methods=$(grep -c "public\|private\|internal\|protected.*(" "$file" 2>/dev/null || echo "0")
        local classes=$(grep -c "class\|struct\|interface" "$file" 2>/dev/null || echo "0")
        
        echo "  Lines: $lines, Methods: $methods, Types: $classes"
    fi
}

echo "--- 1. Core Implementation Files ---"
check_file "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "PerceptualHash struct"
check_file "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" "HashUtils class"
check_file "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" "FileEntry modifications"
check_file "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" "Settings modifications"
check_file "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" "ScanEngine modifications"

echo
echo "--- 2. GUI Integration Files ---"
check_file "/home/leo/videoduplicatefinder/VDF.GUI/Data/SettingsFile.cs" "GUI Settings"
check_file "/home/leo/videoduplicatefinder/VDF.GUI/Views/MainWindow.xaml" "MainWindow XAML"
check_file "/home/leo/videoduplicatefinder/VDF.GUI/ViewModels/MainWindowVM.cs" "MainWindow ViewModel"

echo
echo "--- 3. PerceptualHash.cs Analysis ---"
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" ]; then
    analyze_complexity "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "struct PerceptualHash" "Struct declaration"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "HammingDistance" "Hamming distance method"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "SimilarityPercentage" "Similarity calculation"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "Popcnt" "Hardware acceleration"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" "ProtoMember" "Serialization support"
fi

echo
echo "--- 4. HashUtils.cs Analysis ---"
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" ]; then
    analyze_complexity "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" "ComputeImageHash" "Image hash computation"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" "ComputeVideoHash" "Video hash computation"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" "DCT" "DCT implementation"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" "median" "Median calculation"
fi

echo
echo "--- 5. FileEntry.cs Analysis ---"
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" ]; then
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" "PerceptualHashBytes" "Hash storage property"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" "PerceptualHash.*get.*set" "Hash accessor property"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" "ProtoMember.*9" "Protobuf serialization"
fi

echo
echo "--- 6. Settings.cs Analysis ---"
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" ]; then
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" "UseFastHashing" "Fast hashing enable flag"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" "FastHashingThreshold" "File count threshold"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" "FastHashingSimilarityThreshold" "Similarity threshold"
fi

echo
echo "--- 7. ScanEngine.cs Analysis ---"
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" ]; then
    analyze_complexity "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" "CheckIfDuplicateWithFastHashing" "Fast hashing method"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" "HashUtils.ComputeImageHash" "Image hash integration"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" "HashUtils.ComputeVideoHash" "Video hash integration"
    check_pattern "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" "useFastHashing" "Fast hashing logic"
fi

echo
echo "--- 8. Error Pattern Analysis ---"
echo "Checking for potential issues..."

# Check for common C# issues
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" ]; then
    if grep -q "GetSimilarityPercentage" "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs"; then
        echo "  ⚠ Found old method name 'GetSimilarityPercentage' - should be 'SimilarityPercentage'"
    fi
fi

# Check for missing null checks
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" ]; then
    local null_checks=$(grep -c "!= null\|== null" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" 2>/dev/null || echo "0")
    echo "  Null checks in ScanEngine: $null_checks"
fi

# Check for exception handling
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" ]; then
    local try_blocks=$(grep -c "try\s*{" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" 2>/dev/null || echo "0")
    local catch_blocks=$(grep -c "catch\s*(" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" 2>/dev/null || echo "0")
    echo "  Exception handling: $try_blocks try blocks, $catch_blocks catch blocks"
fi

echo
echo "--- 9. Integration Points Analysis ---"
echo "Verifying integration between components..."

# Check if ScanEngine uses the new hash computation
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" ]; then
    if grep -q "Settings.UseFastHashing" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs"; then
        echo "  ✓ ScanEngine checks fast hashing setting"
    else
        echo "  ✗ ScanEngine missing fast hashing setting check"
    fi
    
    if grep -q "entry.PerceptualHash" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs"; then
        echo "  ✓ ScanEngine accesses perceptual hash property"
    else
        echo "  ✗ ScanEngine missing perceptual hash access"
    fi
fi

# Check GUI integration
if [ -f "/home/leo/videoduplicatefinder/VDF.GUI/ViewModels/MainWindowVM.cs" ]; then
    if grep -q "UseFastHashing" "/home/leo/videoduplicatefinder/VDF.GUI/ViewModels/MainWindowVM.cs"; then
        echo "  ✓ GUI ViewModel handles fast hashing settings"
    else
        echo "  ✗ GUI ViewModel missing fast hashing settings"
    fi
fi

echo
echo "--- 10. Performance Considerations ---"
echo "Analyzing performance-related code..."

if [ -f "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" ]; then
    if grep -q "Popcnt.IsSupported" "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs"; then
        echo "  ✓ Hardware acceleration detection implemented"
    fi
    
    if grep -q "PopCountSoftware" "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs"; then
        echo "  ✓ Software fallback for bit counting implemented"
    fi
fi

# Check for potential O(n²) complexity optimization
if [ -f "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" ]; then
    if grep -q "FastHashingThreshold" "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs"; then
        echo "  ✓ Threshold-based fast hashing activation"
    fi
fi

echo
echo "--- Summary ---"
total_files=8
found_files=0

for file in \
    "/home/leo/videoduplicatefinder/VDF.Core/Utils/PerceptualHash.cs" \
    "/home/leo/videoduplicatefinder/VDF.Core/Utils/HashUtils.cs" \
    "/home/leo/videoduplicatefinder/VDF.Core/FileEntry.cs" \
    "/home/leo/videoduplicatefinder/VDF.Core/Settings.cs" \
    "/home/leo/videoduplicatefinder/VDF.Core/ScanEngine.cs" \
    "/home/leo/videoduplicatefinder/VDF.GUI/Data/SettingsFile.cs" \
    "/home/leo/videoduplicatefinder/VDF.GUI/Views/MainWindow.xaml" \
    "/home/leo/videoduplicatefinder/VDF.GUI/ViewModels/MainWindowVM.cs"
do
    if [ -f "$file" ]; then
        ((found_files++))
    fi
done

echo "Implementation completeness: $found_files/$total_files files present"

if [ $found_files -eq $total_files ]; then
    echo "✓ All implementation files are present"
else
    echo "⚠ Some implementation files are missing"
fi

echo
echo "=== Analysis Complete ==="
