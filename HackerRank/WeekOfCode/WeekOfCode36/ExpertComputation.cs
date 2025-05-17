#define USEMINMAX

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static System.Array;
using static System.Math;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0675
#endregion

using T = System.Double;
using STType = Solution.LineType;

partial class Solution
{
    #region Variables
    const int MOD = 1000000007;
    const int FactCache = 1000;
    const long BIG = long.MaxValue << 15;
    int n;
    int[] x, y, z;

    #endregion

    void Solve()
    {
        n = Ni();
        x = Ni(n);
        y = Ni(n);
        z = Ni(n);
        int g = 0;
        var cvo = new ECSegmentTree(n + 1);
        var bestm = new long[n + 1];
        var bestb = new long[n + 1];
        var bestm2 = new long[n + 1];
        var bestb2 = new long[n + 1];
        var bestm3 = new long[n + 1];
        var bestb3 = new long[n + 1];
        bestb[0] = int.MinValue;
        bestb2[0] = int.MinValue;
        bestm3[0] = int.MinValue;
        bestb3[0] = 1;
        for (int i = 1; i <= n; i++)
        {
            var h = x[i - 1] ^ g;
            var c = y[i - 1] ^ g;
            var L = z[i - 1] ^ g;

            //var f = long.MinValue;
            // H[j]/C[j] * C[i] - C[j]

            var m = bestm[i - 1];
            var b = bestb[i - 1];
            if (h >= m && (h > m || -c < bestb[i - 1]))
            {
                m = h;
                b = -c;
            }
            bestm[i] = m;
            bestb[i] = b;

            var m2 = bestm2[i - 1];
            var b2 = bestb2[i - 1];
            if (-c >= b2 && (-c > b2 || h > m2))
            {
                m2 = h;
                b2 = -c;
            }
            bestm2[i] = m2;
            bestb2[i] = b2;

            var m3 = bestm3[i - 1];
            var b3 = bestb3[i - 1];
            if (-h * b3 > m3 * c)
            {
                m3 = h;
                b3 = -c;
            }
            bestm3[i] = m3;
            bestb3[i] = b3;

            ECSegmentTree.Cover(cvo, i, h, -c);
            int limit = i - L;
            var f = ECSegmentTree.GetMax(cvo, 1, i - L, h, c,
                                         Max(Max(bestm[limit] * c + bestb[limit] * h,
                                         bestm2[limit] * c + bestb2[limit] * h),
                                            bestm3[limit] * c + bestb3[limit] * h));

            // C[i] * 1.0 / H[i]
            //var d = Round(H[i] * query);
            //var f = (long)d;

            var G = (g + f) % MOD;
            if (G < 0) G += MOD;
            g = (int)G; //(int)G;
            //Console.Error.WriteLine($"#{i} H={H[i]} C={C[i]} L={L[i]} F={F[i]} G={G[i]} ");
        }

        WriteLine(g);
    }

    #region Seg Tree

    public struct LineType
    {
        public int M;
        public int C;

        public override string ToString()
        {
            return $"{M}x + {C}";
        }
    }


    public class ECSegmentTree
    {
        #region Variables

        public ECSegmentTree Left;
        public ECSegmentTree Right;
        public int Length;
        public int MaxC = int.MinValue;
        public int MaxM = int.MinValue;
        public int MaxMC = int.MinValue;
        public int MaxCM = int.MinValue;
        public int MaxM2 = int.MinValue;
        public int MaxC2 = 1;
        #endregion

        #region Constructor

        public ECSegmentTree(int length)
        {
            Length = length;
            if (length >= 2)
            {
                int half = length + 1 >> 1;
                Left = new ECSegmentTree(half);
                Right = new ECSegmentTree(length - half);
            }
        }


        #endregion

        #region Core Operations

        public static void Cover(ECSegmentTree node, int start, int m, int c)
        {
            while (node.Length > 1)
            {
                if (c > node.MaxC)
                {
                    node.MaxC = c;
                    node.MaxCM = m;
                }
                if (m > node.MaxM || m == node.MaxM && c < node.MaxC)
                {
                    node.MaxM = m;
                    node.MaxMC = c;
                }

                if (c * 1L * node.MaxM2 > m * 1L * node.MaxC2)
                {
                    node.MaxM2 = m;
                    node.MaxC2 = c;
                }

                if (start < node.Left.Length)
                {
                    node = node.Left;
                }
                else
                {
                    start -= node.Left.Length;
                    node = node.Right;
                }

            }
            node.MaxM = node.MaxCM = m;
            node.MaxC = node.MaxMC = c;
            node.MaxM2 = m;
            node.MaxC2 = c;
        }

        #endregion

        #region Other Operations

        public static long GetMax(ECSegmentTree node, int start, int count, int hi, int ci, long best = long.MinValue)
        {
            while (start < node.Length && start + count > 0)
            {
                var check = (long)hi * node.MaxC + (long)ci * node.MaxM;
                if (check <= best) return best;
                if (node.Length == 1) return check;

                if (start <= 0 && start + count >= node.Length)
                    return GetMaxInner(node, start, count, hi, ci, best);

                best = GetMax(node.Left, start, count, hi, ci, best);
                start -= node.Left.Length;
                node = node.Right;
            }

            return best;
        }

        public static long GetMaxInner(ECSegmentTree node, int start, int count, int hi, int ci, long best = long.MinValue)
        {
            while (start < node.Length && start + count > 0)
            {
                var check = (long)hi * node.MaxC + (long)ci * node.MaxM;
                if (check <= best) return best;
                if (node.Length == 1) return check;

                best = Max(best, (long)hi * node.MaxMC + (long)ci * node.MaxM);
                best = Max(best, (long)hi * node.MaxC + (long)ci * node.MaxCM);
                best = Max(best, (long)hi * node.MaxC2 + (long)ci * node.MaxM2);
                best = GetMax(node.Left, start, count, hi, ci, best);
                start -= node.Left.Length;
                node = node.Right;
            }

            return best;
        }


        #endregion

        #region Misc

        public override string ToString() => $" Length={Length}";

        #endregion

    }

    #endregion


    #region Library

    #region Common
    partial void TestData();

    static void Swap<T>(ref T a, ref T b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    static V Get<K, V>(Dictionary<K, V> dict, K key) where V : new()
    {
        V result;
        if (dict.TryGetValue(key, out result) == false)
            result = dict[key] = new V();
        return result;
    }

    static int Bound<T>(T[] array, T value, bool upper = false)
        where T : IComparable<T>
    {
        int left = 0;
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int cmp = value.CompareTo(array[mid]);
            if (cmp > 0 || cmp == 0 && upper)
                left = mid + 1;
            else
                right = mid - 1;
        }
        return left;
    }

    static long IntPow(long n, long p)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = (result * b);
            p >>= 1;
            b = (b * b);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Log2(long value)
    {
        if (value <= 0)
            return value == 0 ? -1 : 63;

        var log = 0;
        if (value >= 0x100000000L)
        {
            log += 32;
            value >>= 32;
        }
        if (value >= 0x10000)
        {
            log += 16;
            value >>= 16;
        }
        if (value >= 0x100)
        {
            log += 8;
            value >>= 8;
        }
        if (value >= 0x10)
        {
            log += 4;
            value >>= 4;
        }
        if (value >= 0x4)
        {
            log += 2;
            value >>= 2;
        }
        if (value >= 0x2)
        {
            log += 1;
        }
        return log;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int BitCount(long y)
    {
        var x = unchecked((ulong)y);
        x -= (x >> 1) & 0x5555555555555555;
        x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
        x = (x + (x >> 4)) & 0x0f0f0f0f0f0f0f0f;
        return unchecked((int)((x * 0x0101010101010101) >> 56));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int HighestOneBit(int n) => n != 0 ? 1 << Log2(n) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static long HighestOneBit(long n) => n != 0 ? 1L << Log2(n) : 0;
    #endregion

    #region Fast IO
    #region  Input
    static Stream inputStream;
    static int inputIndex, bytesRead;
    static byte[] inputBuffer;
    static StringBuilder builder;
    const int MonoBufferSize = 4096;
    const char NL = (char)10, DASH = (char)45, ZERO = (char)48;

    static void InitInput(Stream input = null, int stringCapacity = 16)
    {
        builder = new StringBuilder(stringCapacity);
        inputStream = input ?? Console.OpenStandardInput();
        inputIndex = bytesRead = 0;
        inputBuffer = new byte[MonoBufferSize];
    }

    static void ReadMore()
    {
        if (bytesRead < 0) throw new FormatException();
        inputIndex = 0;
        bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
        if (bytesRead > 0) return;
        bytesRead = -1;
        inputBuffer[0] = (byte)NL;
    }

    static int Read()
    {
        if (inputIndex >= bytesRead) ReadMore();
        return inputBuffer[inputIndex++];
    }

    static T[] N<T>(int n, Func<T> func)
    {
        var list = new T[n];
        for (int i = 0; i < n; i++) list[i] = func();
        return list;
    }

    static int[] Ni(int n) => N(n, Ni);

    static long[] Nl(int n) => N(n, Nl);

    static string[] Ns(int n) => N(n, Ns);

    static int Ni() => checked((int)Nl());

    static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c == DASH;
        if (neg) { c = Read(); }

        long number = c - ZERO;
        while (true)
        {
            var d = Read() - ZERO;
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
            if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    static char[] Nc(int n)
    {
        var list = new char[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
        return list;
    }

    static string Ns()
    {
        var c = SkipSpaces();
        builder.Clear();
        while (true)
        {
            if (unchecked((uint)c - 33 >= (127 - 33))) break;
            builder.Append((char)c);
            c = Read();
        }
        return builder.ToString();
    }

    static int SkipSpaces()
    {
        int c;
        do c = Read(); while (unchecked((uint)c - 33 >= (127 - 33)));
        return c;
    }

    static string ReadLine()
    {
        builder.Clear();
        while (true)
        {
            int c = Read();
            if (c < 32) { if (c == 10 || c <= 0) break; continue; }
            builder.Append((char)c);
        }
        return builder.ToString();
    }
    #endregion

    #region Output
    static Stream outputStream;
    static byte[] outputBuffer;
    static int outputIndex;

    static void InitOutput(Stream output = null)
    {
        outputStream = output ?? Console.OpenStandardOutput();
        outputIndex = 0;
        outputBuffer = new byte[65535];
    }

    static void WriteLine(object obj = null)
    {
        Write(obj);
        Write(NL);
    }

    static void WriteLine(long number)
    {
        Write(number);
        Write(NL);
    }

    static void Write(long signedNumber)
    {
        ulong number = unchecked((ulong)signedNumber);
        if (signedNumber < 0)
        {
            Write(DASH);
            number = unchecked((ulong)(-signedNumber));
        }

        Reserve(20 + 1); // 20 digits + 1 extra for sign
        int left = outputIndex;
        do
        {
            outputBuffer[outputIndex++] = (byte)(ZERO + number % 10);
            number /= 10;
        }
        while (number > 0);

        int right = outputIndex - 1;
        while (left < right)
        {
            byte tmp = outputBuffer[left];
            outputBuffer[left++] = outputBuffer[right];
            outputBuffer[right--] = tmp;
        }
    }

    static void Write(object obj)
    {
        if (obj == null) return;

        var s = obj.ToString();
        Reserve(s.Length);
        for (int i = 0; i < s.Length; i++)
            outputBuffer[outputIndex++] = (byte)s[i];
    }

    static void Write(char c)
    {
        Reserve(1);
        outputBuffer[outputIndex++] = (byte)c;
    }

    static void Write(byte[] array, int count)
    {
        Reserve(count);
        Copy(array, 0, outputBuffer, outputIndex, count);
        outputIndex += count;
    }

    static void Reserve(int n)
    {
        if (outputIndex + n <= outputBuffer.Length)
            return;

        Dump();
        if (n > outputBuffer.Length)
            Resize(ref outputBuffer, Max(outputBuffer.Length * 2, n));
    }

    static void Dump()
    {
        outputStream.Write(outputBuffer, 0, outputIndex);
        outputIndex = 0;
    }

    static void Flush()
    {
        Dump();
        outputStream.Flush();
    }

    #endregion
    #endregion

    #region Main

    public static void Main()
    {
        {
            InitInput(Console.OpenStandardInput());
            InitOutput(Console.OpenStandardOutput());
            new Solution().Solve();
            Flush();
            Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
        }
    }

    #endregion
    #endregion
}

