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
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace VDF.Core.Utils {
	
	/// <summary>
	/// Represents a perceptual hash for fast duplicate detection
	/// </summary>
	public struct PerceptualHash {
		private readonly ulong hash1;
		private readonly ulong hash2;
		private readonly ulong hash3;
		private readonly ulong hash4;

		public ulong Hash1 => hash1;
		public ulong Hash2 => hash2;
		public ulong Hash3 => hash3;
		public ulong Hash4 => hash4;

		public PerceptualHash(ulong h1, ulong h2, ulong h3, ulong h4) {
			hash1 = h1;
			hash2 = h2;
			hash3 = h3;
			hash4 = h4;
		}

		public bool IsEmpty => hash1 == 0 && hash2 == 0 && hash3 == 0 && hash4 == 0;

		/// <summary>
		/// Calculate Hamming distance between two perceptual hashes
		/// Returns a value between 0 (identical) and 256 (completely different)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int HammingDistance(PerceptualHash other) {
			return PopCount(hash1 ^ other.hash1) +
				   PopCount(hash2 ^ other.hash2) +
				   PopCount(hash3 ^ other.hash3) +
				   PopCount(hash4 ^ other.hash4);
		}

		/// <summary>
		/// Calculate similarity percentage (0-100, where 100 is identical)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float SimilarityPercentage(PerceptualHash other) {
			int distance = HammingDistance(other);
			return (256f - distance) / 256f * 100f;
		}

		/// <summary>
		/// Population count (number of set bits) using hardware acceleration when available
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int PopCount(ulong value) {
			if (Popcnt.X64.IsSupported) {
				return (int)Popcnt.X64.PopCount(value);
			}
			
			// Software fallback
			int count = 0;
			while (value != 0) {
				count++;
				value &= value - 1; // Clear lowest set bit
			}
			return count;
		}

		public override bool Equals(object? obj) {
			return obj is PerceptualHash hash &&
				   hash1 == hash.hash1 &&
				   hash2 == hash.hash2 &&
				   hash3 == hash.hash3 &&
				   hash4 == hash.hash4;
		}

		public override int GetHashCode() {
			return HashCode.Combine(hash1, hash2, hash3, hash4);
		}

		public static bool operator ==(PerceptualHash left, PerceptualHash right) {
			return left.Equals(right);
		}

		public static bool operator !=(PerceptualHash left, PerceptualHash right) {
			return !(left == right);
		}

		/// <summary>
		/// Get hash bytes for serialization
		/// </summary>
		public byte[] ToBytes() {
			byte[] bytes = new byte[32];
			BitConverter.GetBytes(hash1).CopyTo(bytes, 0);
			BitConverter.GetBytes(hash2).CopyTo(bytes, 8);
			BitConverter.GetBytes(hash3).CopyTo(bytes, 16);
			BitConverter.GetBytes(hash4).CopyTo(bytes, 24);
			return bytes;
		}

		/// <summary>
		/// Create hash from bytes for deserialization
		/// </summary>
		public static PerceptualHash FromBytes(byte[] bytes) {
			if (bytes.Length != 32) 
				throw new ArgumentException("Hash bytes must be 32 bytes long");
			
			return new PerceptualHash(
				BitConverter.ToUInt64(bytes, 0),
				BitConverter.ToUInt64(bytes, 8),
				BitConverter.ToUInt64(bytes, 16),
				BitConverter.ToUInt64(bytes, 24)
			);
		}
	}
}
