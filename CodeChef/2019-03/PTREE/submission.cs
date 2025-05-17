#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    const int MOD = 1000000007;
    const int FactCache = 1000;
    const long BIG = long.MaxValue >> 15;

    #endregion

    public void Solve()
    {

    }

    /*PLACEHOLDER*/

    #region Library
    #region Mod Math

    static int[] _inverse;
    static long Inverse(long n)
    {
        long result;

        if (_inverse == null)
            _inverse = new int[1000];

        if (n >= 0 && n < _inverse.Length && (result = _inverse[n]) != 0)
            return result - 1;

        result = InverseDirect((int)n);
        if (n >= 0 && n < _inverse.Length)
            _inverse[n] = (int)(result + 1);
        return result;
    }

    public static int InverseDirect(int a)
    {
        if (a < 0) return -InverseDirect(-a);
        int t = 0, r = MOD, t2 = 1, r2 = a;
        while (r2 != 0)
        {
            var q = r / r2;
            t -= q * t2;
            r -= q * r2;

            if (r != 0)
            {
                q = r2 / r;
                t2 -= q * t;
                r2 -= q * r;
            }
            else
            {
                r = r2;
                t = t2;
                break;
            }
        }
        return r <= 1 ? (t >= 0 ? t : t + MOD) : -1;
    }

    static long Mult(long left, long right) =>
        (left * right) % MOD;

    static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    static long Add(long x, long y) =>
        (x += y) >= MOD ? x - MOD : x;

    static long Subtract(long x, long y) => (x -= y) < 0 ? x + MOD : x;

    static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

    static long ModPow(long n, long p, long mod = MOD)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = (result * b) % mod;
            p >>= 1;
            b = (b * b) % mod;
        }
        return result;
    }

    static List<long> _fact;

    static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(FactCache) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    static long[] _ifact = new long[0];
    static long InverseFact(int n)
    {
        long result;
        if (n < _ifact.Length && (result = _ifact[n]) != 0)
            return result;

        var inv = Inverse(Fact(n));
        if (n >= _ifact.Length) Resize(ref _ifact, _fact.Capacity);
        _ifact[n] = inv;
        return inv;
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
	
    public static long Combinations(long n, int k)
    {
        if (k <= 0) return k == 0 ? 1 : 0;  // Note: n<0 -> 0 unless k=0
        if (k + k > n) return Combinations(n, (int)(n - k));

        var result = InverseFact(k);
        for (long i = n - k + 1; i <= n; i++) result = result * i % MOD;
        return result;
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

	public static int Gcd(int n, int m)
	{
		while (true)
		{
			if (m == 0) return n >= 0 ? n : -n;
			n %= m;
			if (n == 0) return m >= 0 ? m : -m;
			m %= n;
		}
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe int Log2(long value)
    {
        double f = unchecked((ulong)value); // +.5 -> -1 for zero
        return (((int*)&f)[1] >> 20) - 1023;
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
    /*static unsafe int HighestOneBit(int x) // sometimes doesn't work
	{
		double f = unchecked((uint)x);
        return unchecked(1 << (((int*)&f)[1] >> 20) - 1023 & ~((x - 1 & -x - 1) >> 31));
	}*/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe long HighestOneBit(long x) 
	{
	    double f = unchecked((ulong)x);
		return unchecked(1L << (((int*)&f)[1] >> 20) - 1023 & ~(x - 1 >> 63 & -x - 1 >> 63));
	}
	#endregion

    #region Fast IO
    #region  Input
    static Stream inputStream;
    static int inputIndex, bytesRead;
    static byte[] inputBuffer;
    static StringBuilder builder;
    const int MonoBufferSize = 4096;
    const char EOL = (char)10, DASH = (char)45, ZERO = (char)48; 

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
        inputBuffer[0] = (byte)EOL;
    }

    static int Read()
    {
        if (inputIndex >= bytesRead) ReadMore();
        return inputBuffer[inputIndex++];
    }

    static T[] Na<T>(int n, Func<T> func, int z = 0)
    {
        n += z;
        var list = new T[n];
        for (int i = z; i < n; i++) list[i] = func();
        return list;
    }

    static int[] Ni(int n, int z = 0) => Na(n, Ni, z);

    static long[] Nl(int n, int z = 0) => Na(n, Nl, z);

    static string[] Ns(int n, int z = 0) => Na(n, Ns, z);

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
        Write(EOL);
    }

    static void WriteLine(long number)
    {
        Write(number);
        Write(EOL);
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
#if __MonoCS__ && !C7
        var thread = new System.Threading.Thread(()=>new Solution().Solve());
        var f = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = thread.GetType().GetField("internal_thread", f).GetValue(thread);
        t.GetType().GetField("stack_size", f).SetValue(t, 32 * 1024 * 1024);
        thread.Start();
        thread.Join();
#else
        new Solution().Solve();
#endif
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
