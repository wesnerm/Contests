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

partial class Solution
{
    #region Variables
    static int MOD = 1000000007;
    const int FactCache = 7654321;
    const long BIG = long.MaxValue << 15;

    #endregion

    public void Solve()
    {

        //BreakOutValues();
        //return;

        int t = Ni();
        MOD = Ni();

        long maxn = 0;
        long maxk = 0;

        var list = new long[t][];
        for (int tt = 0; tt < t; tt++)
        {
            long n = Nl();
            long k = Nl();
            maxn = Max(n, maxn);
            maxk = Max(k, maxk);
            list[tt] = new[] { n, k };
        }

        //var table = CombinationsWithReplacementAtMostK((int)maxn + 1, (int)maxk + 1, MOD - 1);

        for (int tt = 0; tt < t; tt++)
        {
            long n = list[tt][0];
            long k = list[tt][1];

            var digits = Max(3, (int)Log(Max(n, k), MOD) + 1);
            var nlist = BreakOut(n+k-1, MOD, digits);
            var klist = BreakOut(k, MOD, digits);
            var n2list = BreakOut(n, MOD, digits);

            //var good = true;
            //for (int i = 0; i < digits; i++)
            //    if (nlist[i] < klist[i])
            //    {
            //        good = false;
            //        break;
            //    }

            //if (!good)
            //{
            //    WriteLine(0);
            //    continue;
            //}

            long result = F(nlist[0], klist[0]);
            for (int i = 1; i < digits; i++)
            {
                if (result == 0) result = 1;
                result = result * CombLong(nlist[i], klist[i]) % MOD;
            }

            result = (result + MOD) % MOD;

            //long result;
            //if (MOD == 2)
            //    result = 1;
            //else if (n == 1)
            //    result = (k < MOD) ? 0 : 1;
            //else if (k == 1)
            //    result = n % MOD;
            //else 
            //    result = F(n,k);

            WriteLine(Fix(result));
        }
    }

    void GenerateNumbers()
    {
        foreach (var p in new[] { 2, 3, 5, 7, 11, 1000000007 })
        {
            var size = 60;

            var dp = CombinationsWithReplacementAtMostK(size, size, p - 1, p);
            Console.Error.WriteLine($"\n\nPrime {p}");
            Console.Error.Write(-1);
            for (int n = 0; n <= size; n++)
            {
                Console.Error.Write("," + n);
            }

            Console.Error.WriteLine();

            for (int k = 0; k <= size; k++)
            {
                Console.Error.Write(k);
                for (int n = 0; n <= size; n++)
                {
                    Console.Error.Write($",{dp[k, n]}");
                }
                Console.Error.WriteLine();
            }
        }
    }

    void BreakOutValues()
    {
        var p = 3;
        MOD = p;
        var size = 1000;
        var dp = CombinationsWithReplacementAtMostK(size, size, p - 1, p);

        var dp2 = new long[50][];

        for (int n=1; n<50; n++)
            dp2[n] = CombinationsWithReplacementAtMostK(n,p-1);

        var error = Console.Error;
       
            for (int k = 1; k < 50; k++)
            for (int n = 1; n < 50; n++)
            {
                var digits = Max(3, (int) Log(Max(n,k), p) + 1);
                var klist = BreakOut(k, p, digits);
                var nlist = BreakOut(n, p, digits);
                var qlist = BreakOut(n + k - 1, p, 3);

                var np = string.Join("", nlist);
                var kp = string.Join("", klist);
                var qp = string.Join("", qlist);
                var ans = F(n,k);

                var nint = Intermediate(nlist,p);
                var kint = Intermediate(klist,p);

                var nip = ToBase(nint);
                var kip = ToBase(kint);

                var inter = new List<long>();
                var inter2 = new List<long>();
                long prev = 1;
                for (int i = 0; i < nint.Count; i++)
                {
                    var next = F(nint[i], kint[i]);
                    inter.Add(next);
                    inter2.Add(prev == 0 ? 0 : Div(next,prev));
                    prev = next;
                }

                var inters = string.Join(" ", inter);
                var inter2s = string.Join("*", inter2);

                Debug.Assert(F(n,k)==dp[k,n]);
                Debug.Assert(F3(n,k)==dp[k,n]);
                error.WriteLine($"F({n,2},{k,2})={inter2s} {ans}, {np}, {kp}, {qp}, {nip}, {kip}, {inters}");
            }
    }

    List<int> Intermediate(List<int> list, int p)
    {
        var result = new List<int>();
        var b = 1;
        var prev = 0;
        foreach (var v in list)
        {
            prev += b * v;
            result.Add(prev);
            b *= p;
        }
        return result;
    }

    List<int> BreakOut(long n, int p, int minSize = 0)
    {
        var list = new List<int>();
        return BreakOut(list, n, p, minSize);
    }

    List<int> BreakOut(List<int> list, long n, int p, int minSize = 0)
    {
        list.Clear();
        long N = n;
        do
        {
            list.Add((int)(N % p));
            N /= p;
        } while (N > 0 || list.Count < minSize);

        return list;
    }


    string ToBase(List<int> list)
    {
        var str = string.Join(" ", list);
        return str;
    }


    public long F(long n, long k)
    {
        long nk1 = n + k - 1;
        if (nk1 < k) return k!=0 ? 0 : 1;

        long result = CombLong(nk1, k);

        if (k >= MOD)
        {
            long sign = -1;
            long s = 1;

            var nk1_2 = nk1;
            while (true)
            {
                nk1_2 -= MOD;
                if (nk1_2 < n - 1) break;
                long tmp1 = CombLong(n, s);
                long tmp2 = CombLong(nk1_2, n - 1);
                result += (sign * tmp1 * tmp2) % MOD;
                sign = -sign;
                s++;
            }

            result %= MOD;
            if (result < 0) result += MOD;

        }

        return result;
    }

    public long F2(long n, long k)
    {
        long nk1 = n + k - 1;
        long result = 0;
        long sign = 1;
        long s = 0;
        while (nk1 >= n-1)
        {
            long tmp1 = CombLong(n, s);
            long tmp2 = CombLong(nk1, n - 1);
            result += (sign * tmp1 * tmp2) % MOD;
            sign = -sign;
            nk1 -= MOD;
            s++;
        }
        result %= MOD;
        if (result < 0) result += MOD;
        return result;
    }


    public long F3(long n, long k)
    {
        return F3Recursive(n + k - 1, n, 0);
    }

    public long F3Recursive(long nk1, long n, long s)
    {
        if (nk1 < n - 1) return 0;
        long tmp1 = CombLong(n, s);
        long tmp2 = CombLong(nk1, n - 1);
        long result = tmp1 * tmp2 % MOD - F3Recursive(nk1-MOD, n, s+1);
        if (result < 0) result += MOD;
        return result;
    }


    public long FLong(long n, long r)
    {
        long result = 1;
        while (r != 0)
        {
            long N = n % MOD;
            long R = r % MOD;
            if (N < R) return 0;
            n /= MOD;
            r /= MOD;
            var tmp = r != 0
                ? CombLong(N + R - 1, R)
                : F((int)N, (int)R);
            result = result * tmp % MOD;
        }
        return Fix(result);
    }

    public long CombLong(long n, long r)
    {
        long result = 1;
        while (r != 0)
        {
            long N = n % MOD;
            long R = r % MOD;
            if (N < R) return 0;
            result = result * Comb((int)N, (int)R) % MOD;
            n /= MOD;
            r /= MOD;
        }
        return Fix(result);
    }

    public static long[,] CombinationsWithReplacementAtMostK(int k, int n, int kmax, int MOD)
    {
        var dp = new long[k + 1, n + 1];

        for (int j = 0; j <= n; j++)
        {
            dp[0, j] = 1;
            dp[1, j] = 1 + j;
        }

        for (int i = 2; i <= k; i++)
        {
            dp[i, 1] = dp[i - 1, 1] + (i <= kmax ? 1 : 0);

            for (int j = 2; j <= n; j++)
            {
                int prev = i - kmax - 1;
                var tmp = dp[i, j - 1] - (prev < 0 ? 0 : dp[prev, j - 1]);

                long result = tmp + dp[i - 1, j];
                dp[i, j] = result % MOD;
            }
        }

        for (int j = 0; j <= n; j++)
            for (int i = k; i > 0; i--)
                dp[i, j] = ((dp[i, j] - dp[i - 1, j]) % MOD + MOD) % MOD;

        return dp;
    }

    public static long[] MultiplyPolynomialsMod(long[] a, long[] b, int mod, int size = 0)
    {
        if (size == 0) size = a.Length + b.Length - 1;
        size = Min(a.Length + b.Length - 1, size);
        var result = new long[size];
        for (int i = 0; i < a.Length; i++)
        for (int j = Min(size - i, b.Length) - 1; j >= 0; j--)
        {
            var r = result[i + j] + a[i] * b[j] % mod;
            if (r >= mod) r -= mod;
            result[i + j] = r;
        }

        return result;
    }


    public static long[] PolyPow(long[] x, long n, Func<long[], long[], long[]> multiply)
    {
        if (n <= 1) return n == 1 ? x : new long[] { 1 };
        var t = PolyPow(x, n >> 1, multiply);
        var sq = multiply(t, t);
        return (n & 1) == 0 ? sq : multiply(x, sq);
    }

    public static long[] CombinationsWithReplacementAtMostK(int m, int k)
    {
        var poly = new long[k + 1];

        for (int i = 0; i <= k; i++)
            poly[i] = 1;

        return PolyPow(poly, m, (a,b)=>MultiplyPolynomialsMod(a,b,MOD, 100));
    }

    #region Library
    #region Mod Math

    static int[] _inverse;
    static long Inverse(long n)
    {
        long result;

        if (_inverse == null)
            _inverse = new int[3000];

        if (n < _inverse.Length && (result = _inverse[n]) != 0)
            return result - 1;

        result = ModPow(n, MOD - 2);
        if (n < _inverse.Length)
            _inverse[n] = (int)(result + 1);
        return result;
    }

    static long Mult(long left, long right) =>
        (left * right) % MOD;

    static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    static long Add(long x, long y) =>
        (x += y) >= MOD ? x - MOD : x;

    static long Subtract(long x, long y) => (x -= y) < 0 ? x + MOD : x;

    static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

    static long ModPow(long n, long p)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = (result * b) % MOD;
            p >>= 1;
            b = (b * b) % MOD;
        }
        return result;
    }

    static List<long> _fact, _ifact;

    static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(FactCache) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    static long InverseFact(int n)
    {
        if (_ifact == null) _ifact = new List<long>(FactCache) { 1 };
        for (int i = _ifact.Count; i <= n; i++)
            _ifact.Add(Div(_ifact[i - 1], i));
        return _ifact[n];
    }

    static long Fact(int n, int m)
    {
        var fact = Fact(n);
        if (m < n) fact = fact * InverseFact(n - m) % MOD;
        return fact;
    }

    static long Comb(int n, int k)
    {
        if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
        return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
    }
    #endregion

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
            int mid = left + (right - left >> 1);
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
        AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
        {
            Flush();
            var e = (Exception)arg.ExceptionObject;
            Console.Error.WriteLine(e);
            var line = new StackTrace(e, true).GetFrames()
                .Select(x => x.GetFileLineNumber()).FirstOrDefault(x => x != 0);
            var wait = line % 300 * 10 + 5;
            var process = Process.GetCurrentProcess();
            while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 3000) wait += 1000;
            while (process.TotalProcessorTime.TotalMilliseconds < Min(wait, 3000)) ;
            Environment.Exit(1);
        };

        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());

        new Solution().Solve();

        Flush();
        Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
    }
    #endregion
    #endregion
}

class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}
