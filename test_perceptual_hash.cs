using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

// Standalone test for perceptual hashing without external dependencies
namespace VDF.Core.Tests
{
    // Simplified PerceptualHash struct for testing
    public struct PerceptualHash
    {
        public ulong Hash1, Hash2, Hash3, Hash4;

        public PerceptualHash(ulong hash1, ulong hash2, ulong hash3, ulong hash4)
        {
            Hash1 = hash1;
            Hash2 = hash2;
            Hash3 = hash3;
            Hash4 = hash4;
        }

        public int HammingDistance(PerceptualHash other)
        {
            ulong xor1 = Hash1 ^ other.Hash1;
            ulong xor2 = Hash2 ^ other.Hash2;
            ulong xor3 = Hash3 ^ other.Hash3;
            ulong xor4 = Hash4 ^ other.Hash4;

            if (Popcnt.IsSupported)
            {
                return (int)(Popcnt.PopCount(xor1) + Popcnt.PopCount(xor2) + 
                           Popcnt.PopCount(xor3) + Popcnt.PopCount(xor4));
            }
            else
            {
                return PopCountSoftware(xor1) + PopCountSoftware(xor2) + 
                       PopCountSoftware(xor3) + PopCountSoftware(xor4);
            }
        }

        private static int PopCountSoftware(ulong value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }

        public float SimilarityPercentage(PerceptualHash other)
        {
            int distance = HammingDistance(other);
            return 100f * (256f - distance) / 256f;
        }

        public override string ToString()
        {
            return $"{Hash1:X16}{Hash2:X16}{Hash3:X16}{Hash4:X16}";
        }
    }

    // Simple DCT implementation for testing
    public static class SimpleDCT
    {
        public static double[,] ComputeDCT(byte[,] input)
        {
            int N = input.GetLength(0);
            int M = input.GetLength(1);
            double[,] output = new double[N, M];

            for (int u = 0; u < N; u++)
            {
                for (int v = 0; v < M; v++)
                {
                    double sum = 0.0;
                    for (int x = 0; x < N; x++)
                    {
                        for (int y = 0; y < M; y++)
                        {
                            sum += input[x, y] * 
                                   Math.Cos((2 * x + 1) * u * Math.PI / (2 * N)) *
                                   Math.Cos((2 * y + 1) * v * Math.PI / (2 * M));
                        }
                    }
                    
                    double cu = (u == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                    double cv = (v == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                    output[u, v] = 0.25 * cu * cv * sum;
                }
            }
            return output;
        }

        public static PerceptualHash ComputeHash(byte[,] grayImage)
        {
            // Resize to 32x32 for DCT
            byte[,] resized = ResizeImage(grayImage, 32, 32);
            
            // Compute DCT
            double[,] dct = ComputeDCT(resized);
            
            // Extract low-frequency 16x16 region (skip DC component)
            double[] lowFreq = new double[255]; // 16x16 - 1 (skip DC)
            int index = 0;
            for (int u = 0; u < 16; u++)
            {
                for (int v = 0; v < 16; v++)
                {
                    if (u == 0 && v == 0) continue; // Skip DC component
                    lowFreq[index++] = dct[u, v];
                }
            }
            
            // Compute median
            Array.Sort(lowFreq);
            double median = lowFreq[127]; // Middle value
            
            // Create hash based on values above/below median
            ulong hash1 = 0, hash2 = 0, hash3 = 0, hash4 = 0;
            index = 0;
            
            for (int u = 0; u < 16; u++)
            {
                for (int v = 0; v < 16; v++)
                {
                    if (u == 0 && v == 0) continue;
                    
                    if (dct[u, v] > median)
                    {
                        int bitIndex = index % 64;
                        if (index < 64)
                            hash1 |= (1UL << bitIndex);
                        else if (index < 128)
                            hash2 |= (1UL << bitIndex);
                        else if (index < 192)
                            hash3 |= (1UL << bitIndex);
                        else
                            hash4 |= (1UL << bitIndex);
                    }
                    index++;
                }
            }
            
            return new PerceptualHash(hash1, hash2, hash3, hash4);
        }

        private static byte[,] ResizeImage(byte[,] image, int newWidth, int newHeight)
        {
            int oldWidth = image.GetLength(0);
            int oldHeight = image.GetLength(1);
            byte[,] resized = new byte[newWidth, newHeight];

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    int srcX = x * oldWidth / newWidth;
                    int srcY = y * oldHeight / newHeight;
                    resized[x, y] = image[srcX, srcY];
                }
            }
            return resized;
        }
    }

    // Test program
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Perceptual Hash Testing ===");
            
            // Test 1: Basic hash functionality
            TestBasicHashFunctionality();
            
            // Test 2: Hash similarity
            TestHashSimilarity();
            
            // Test 3: DCT computation
            TestDCTComputation();
            
            // Test 4: Performance test
            TestPerformance();
            
            Console.WriteLine("\n=== All Tests Completed ===");
        }

        static void TestBasicHashFunctionality()
        {
            Console.WriteLine("\n--- Test 1: Basic Hash Functionality ---");
            
            // Create identical hashes
            var hash1 = new PerceptualHash(0x1234567890ABCDEF, 0xFEDCBA0987654321, 
                                         0x1111222233334444, 0x5555666677778888);
            var hash2 = new PerceptualHash(0x1234567890ABCDEF, 0xFEDCBA0987654321, 
                                         0x1111222233334444, 0x5555666677778888);
            
            Console.WriteLine($"Hash1: {hash1}");
            Console.WriteLine($"Hash2: {hash2}");
            Console.WriteLine($"Hamming Distance: {hash1.HammingDistance(hash2)}");
            Console.WriteLine($"Similarity: {hash1.SimilarityPercentage(hash2):F2}%");
            
            // Test with different hashes
            var hash3 = new PerceptualHash(0x1234567890ABCDEF, 0xFEDCBA0987654320, 
                                         0x1111222233334444, 0x5555666677778888);
            Console.WriteLine($"Hash3: {hash3}");
            Console.WriteLine($"Distance 1->3: {hash1.HammingDistance(hash3)}");
            Console.WriteLine($"Similarity 1->3: {hash1.SimilarityPercentage(hash3):F2}%");
        }

        static void TestHashSimilarity()
        {
            Console.WriteLine("\n--- Test 2: Hash Similarity Ranges ---");
            
            var baseHash = new PerceptualHash(0x0000000000000000, 0x0000000000000000, 
                                            0x0000000000000000, 0x0000000000000000);
            
            // Test different bit differences
            var tests = new[]
            {
                new PerceptualHash(0x0000000000000001, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000), // 1 bit
                new PerceptualHash(0x0000000000000003, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000), // 2 bits
                new PerceptualHash(0x000000000000000F, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000), // 4 bits
                new PerceptualHash(0x00000000000000FF, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000), // 8 bits
                new PerceptualHash(0x000000000000FFFF, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000), // 16 bits
                new PerceptualHash(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF), // All bits
            };
            
            for (int i = 0; i < tests.Length; i++)
            {
                int distance = baseHash.HammingDistance(tests[i]);
                float similarity = baseHash.SimilarityPercentage(tests[i]);
                Console.WriteLine($"Test {i + 1}: Distance={distance}, Similarity={similarity:F2}%");
            }
        }

        static void TestDCTComputation()
        {
            Console.WriteLine("\n--- Test 3: DCT Computation ---");
            
            // Create test patterns
            byte[,] solidBlack = new byte[16, 16];
            byte[,] solidWhite = new byte[16, 16];
            byte[,] checkerboard = new byte[16, 16];
            
            // Fill patterns
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    solidBlack[x, y] = 0;
                    solidWhite[x, y] = 255;
                    checkerboard[x, y] = (byte)((x + y) % 2 == 0 ? 255 : 0);
                }
            }
            
            var blackHash = SimpleDCT.ComputeHash(solidBlack);
            var whiteHash = SimpleDCT.ComputeHash(solidWhite);
            var checkerHash = SimpleDCT.ComputeHash(checkerboard);
            
            Console.WriteLine($"Black Hash: {blackHash}");
            Console.WriteLine($"White Hash: {whiteHash}");
            Console.WriteLine($"Checker Hash: {checkerHash}");
            
            Console.WriteLine($"Black vs White: {blackHash.SimilarityPercentage(whiteHash):F2}%");
            Console.WriteLine($"Black vs Checker: {blackHash.SimilarityPercentage(checkerHash):F2}%");
            Console.WriteLine($"White vs Checker: {whiteHash.SimilarityPercentage(checkerHash):F2}%");
        }

        static void TestPerformance()
        {
            Console.WriteLine("\n--- Test 4: Performance Test ---");
            
            const int iterations = 10000;
            var random = new Random(42);
            var hashes = new PerceptualHash[100];
            
            // Generate random hashes
            for (int i = 0; i < hashes.Length; i++)
            {
                hashes[i] = new PerceptualHash(
                    (ulong)random.Next() << 32 | (ulong)random.Next(),
                    (ulong)random.Next() << 32 | (ulong)random.Next(),
                    (ulong)random.Next() << 32 | (ulong)random.Next(),
                    (ulong)random.Next() << 32 | (ulong)random.Next()
                );
            }
            
            var start = DateTime.UtcNow;
            int totalComparisons = 0;
            
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < hashes.Length; i++)
                {
                    for (int j = i + 1; j < hashes.Length; j++)
                    {
                        var similarity = hashes[i].SimilarityPercentage(hashes[j]);
                        totalComparisons++;
                    }
                }
            }
            
            var elapsed = DateTime.UtcNow - start;
            double comparisonsPerSecond = totalComparisons / elapsed.TotalSeconds;
            
            Console.WriteLine($"Total comparisons: {totalComparisons:N0}");
            Console.WriteLine($"Time elapsed: {elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Comparisons per second: {comparisonsPerSecond:N0}");
            Console.WriteLine($"Hardware POPCNT support: {System.Runtime.Intrinsics.X86.Popcnt.IsSupported}");
        }
    }
}
