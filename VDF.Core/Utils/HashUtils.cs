// /*
//     Copyright (C) 2021 0x90d
//     This file is part of VideoDuplicateFinder
//     VideoDuplicateFinder is free software: you can redistribute it and/or modify
//     it under the terms of the GPLv3 as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     VideoDuplicateFinder is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License
//     along with VideoDuplicateFinder.  If not, see <http://www.gnu.org/licenses/>.
// */
//

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace VDF.Core.Utils {
	
	/// <summary>
	/// Utilities for computing and comparing perceptual hashes for fast duplicate detection
	/// </summary>
	public static class HashUtils {
		
		/// <summary>
		/// Compute a perceptual hash from multiple grayscale images (video frames or single image)
		/// Uses DCT-based perceptual hashing similar to pHash algorithm
		/// </summary>
		public static PerceptualHash ComputePerceptualHash(Dictionary<double, byte[]?> grayBytes, bool isImage = false) {
			if (grayBytes == null || grayBytes.Count == 0) {
				return new PerceptualHash(0, 0, 0, 0);
			}

			// For images, use the single frame
			if (isImage && grayBytes.ContainsKey(0)) {
				return ComputeSingleFrameHash(grayBytes[0]!);
			}

			// For videos, combine hashes from multiple frames
			List<ulong> frameHashes = new();
			foreach (var kvp in grayBytes.OrderBy(x => x.Key)) {
				if (kvp.Value != null) {
					var frameHash = ComputeSingleFrameHash(kvp.Value);
					frameHashes.Add(frameHash.hash1);
					frameHashes.Add(frameHash.hash2);
					frameHashes.Add(frameHash.hash3);
					frameHashes.Add(frameHash.hash4);
				}
			}

			// Combine frame hashes into a single hash
			return CombineFrameHashes(frameHashes);
		}

		/// <summary>
		/// Compute perceptual hash for a single frame
		/// </summary>
		private static PerceptualHash ComputeSingleFrameHash(byte[] grayBytes) {
			// Assuming the gray bytes represent a 16x16 image (256 bytes total)
			if (grayBytes.Length != 256) {
				// If not 16x16, create a temporary image and resize
				int size = (int)Math.Sqrt(grayBytes.Length);
				if (size * size != grayBytes.Length) {
					throw new ArgumentException("Gray bytes must represent a square image");
				}
				
				// Convert to 16x16 for consistent hashing
				grayBytes = ResizeGrayBytes(grayBytes, size, 16);
			}

			// Convert to DCT hash using simplified DCT
			return ComputeDCTHash(grayBytes, 16);
		}

		/// <summary>
		/// Resize grayscale bytes from one size to another
		/// </summary>
		private static byte[] ResizeGrayBytes(byte[] source, int sourceSize, int targetSize) {
			byte[] target = new byte[targetSize * targetSize];
			float scale = (float)sourceSize / targetSize;
			
			for (int y = 0; y < targetSize; y++) {
				for (int x = 0; x < targetSize; x++) {
					int srcX = (int)(x * scale);
					int srcY = (int)(y * scale);
					target[y * targetSize + x] = source[srcY * sourceSize + srcX];
				}
			}
			
			return target;
		}

		/// <summary>
		/// Compute DCT-based hash from 16x16 grayscale data
		/// </summary>
		private static PerceptualHash ComputeDCTHash(byte[] grayBytes, int size) {
			// Simplified DCT calculation for performance
			// This is a reduced version of the full DCT for speed
			
			float[,] dct = new float[8, 8];
			float total = 0;
			
			// Compute 8x8 DCT from the 16x16 input (downsampled)
			for (int v = 0; v < 8; v++) {
				for (int u = 0; u < 8; u++) {
					float sum = 0;
					for (int y = 0; y < 8; y++) {
						for (int x = 0; x < 8; x++) {
							// Sample from 16x16 grid
							int srcX = x * 2;
							int srcY = y * 2;
							float pixel = grayBytes[srcY * size + srcX] / 255.0f;
							
							float cosU = (float)Math.Cos((2 * x + 1) * u * Math.PI / 16);
							float cosV = (float)Math.Cos((2 * y + 1) * v * Math.PI / 16);
							sum += pixel * cosU * cosV;
						}
					}
					
					float cu = u == 0 ? 1.0f / (float)Math.Sqrt(2) : 1.0f;
					float cv = v == 0 ? 1.0f / (float)Math.Sqrt(2) : 1.0f;
					dct[v, u] = 0.25f * cu * cv * sum;
					
					// Skip DC component (0,0) for median calculation
					if (!(u == 0 && v == 0)) {
						total += dct[v, u];
					}
				}
			}
			
			// Calculate median of AC components
			float median = total / 63.0f; // 64 - 1 (excluding DC)
			
			// Generate hash bits by comparing against median
			ulong hash1 = 0, hash2 = 0, hash3 = 0, hash4 = 0;
			int bitIndex = 0;
			
			for (int v = 0; v < 8; v++) {
				for (int u = 0; u < 8; u++) {
					if (u == 0 && v == 0) continue; // Skip DC component
					
					if (dct[v, u] > median) {
						int quadrant = bitIndex / 16;
						int bitPos = bitIndex % 16;
						
						switch (quadrant) {
							case 0: hash1 |= 1UL << bitPos; break;
							case 1: hash2 |= 1UL << bitPos; break;
							case 2: hash3 |= 1UL << bitPos; break;
							case 3: hash4 |= 1UL << bitPos; break;
						}
					}
					bitIndex++;
				}
			}
			
			return new PerceptualHash(hash1, hash2, hash3, hash4);
		}

		/// <summary>
		/// Combine multiple frame hashes into a single video hash
		/// </summary>
		private static PerceptualHash CombineFrameHashes(List<ulong> frameHashes) {
			if (frameHashes.Count == 0) {
				return new PerceptualHash(0, 0, 0, 0);
			}

			// Use XOR combination with rotation for better distribution
			ulong h1 = 0, h2 = 0, h3 = 0, h4 = 0;
			
			for (int i = 0; i < frameHashes.Count; i += 4) {
				h1 ^= RotateLeft(frameHashes.ElementAtOrDefault(i), i);
				h2 ^= RotateLeft(frameHashes.ElementAtOrDefault(i + 1), i + 1);
				h3 ^= RotateLeft(frameHashes.ElementAtOrDefault(i + 2), i + 2);
				h4 ^= RotateLeft(frameHashes.ElementAtOrDefault(i + 3), i + 3);
			}
			
			return new PerceptualHash(h1, h2, h3, h4);
		}

		/// <summary>
		/// Rotate bits left for better hash distribution
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong RotateLeft(ulong value, int shift) {
			shift &= 63; // Ensure shift is in valid range
			return (value << shift) | (value >> (64 - shift));
		}

		/// <summary>
		/// Fast comparison using perceptual hashes
		/// Returns similarity percentage (0-100)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CompareHashes(PerceptualHash hash1, PerceptualHash hash2) {
			if (hash1.IsEmpty || hash2.IsEmpty) {
				return 0f; // Can't compare empty hashes
			}
			
			return hash1.SimilarityPercentage(hash2);
		}

		/// <summary>
		/// Check if two hashes are similar within a threshold
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreHashesSimilar(PerceptualHash hash1, PerceptualHash hash2, float thresholdPercent) {
			return CompareHashes(hash1, hash2) >= thresholdPercent;
		}
	}
}
