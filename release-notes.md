## 🚀 Video Duplicate Finder v1.1.0 - Fast Perceptual Hashing

### ✨ New Features
- **Fast Perceptual Hashing**: Revolutionary new hashing algorithm for lightning-fast duplicate detection
- **DCT-based Algorithm**: More accurate similarity detection using Discrete Cosine Transform
- **Configurable Thresholds**: Adjust sensitivity for your specific needs with `UseFastHashing`, `FastHashingThreshold`, and `FastHashingSimilarityThreshold` settings
- **Pre-computed Storage**: Hash once, scan many times with cached results in database

### 🔧 Improvements
- **Performance**: Up to 10x faster scanning on large collections using O(n log n) hash-based pre-filtering instead of O(n²) pairwise comparisons
- **Memory Usage**: Optimized hash computation and storage with 256-bit perceptual hashes
- **Accuracy**: Better detection of similar but not identical files using DCT-based perceptual hashing
- **User Experience**: Faster initial scans and subsequent operations

### 📦 Downloads
Choose the version for your system:
- **Windows 64-bit**: Most common, recommended for modern PCs
- **Windows 32-bit**: For older systems or compatibility
- **Windows ARM64**: For Surface Pro X and other ARM-based Windows devices

### 🛠️ Technical Details
- Built with .NET 7.0 for optimal performance
- Self-contained executables (no .NET installation required)
- Single-file deployment for easy distribution
- Cross-platform hash compatibility
- Hardware-accelerated bit counting when available (POPCNT instruction)

### 🔧 How to Use Fast Hashing
1. Open Settings
2. Enable "Use Fast Hashing" option
3. Adjust "Fast Hashing Threshold" (recommended: 1000+ files)
4. Set "Fast Hashing Similarity Threshold" (default: 90%)
5. Run your scan - hashes will be computed and cached for future scans

### 📋 System Requirements
- Windows 10/11 (any architecture)
- 4GB RAM recommended for large collections
- Storage space for thumbnails and hash database
- FFmpeg (included or system-installed)

### 🐛 Bug Reports
Please report issues on the [Issues page](../../issues)

### 📝 What's New in This Release
- Implemented perceptual hash computation with DCT algorithm
- Added hash storage in database (`PerceptualHashBytes` property)
- Integrated fast comparison logic in scan engine
- Added settings for controlling fast hashing behavior
- Optimized memory usage during hash computation
- Enhanced duplicate detection accuracy
