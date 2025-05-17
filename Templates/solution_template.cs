#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Array;
using static System.Math;
#endregion

partial class Solution
{
    public void Solve()
    {

    }

    #region Library
    #region Mod Math
    const int MOD = 1000000007;
    static int[] _inverse;

    static long Inverse(long n)
    {
        long result;
        if (_inverse == null) _inverse = new int[100001];
        if (unchecked((ulong) n < (ulong)_inverse.Length) && (result = _inverse[n]) != 0)
            return result - 1;

        result = InverseDirect((int)n);
        if (unchecked((ulong)n < (ulong)_inverse.Length))
            _inverse[n] = (int)(result + 1);
        return result;
    }

    public static int InverseDirect(int a, int mod = MOD)
    {
        int b = mod, p = 1, q = 0;
        while (b > 0)
        {
            int c = a / b, d = a;
            a = b;
            b = d % b;
            d = p;
            p = q;
            q = d - c * q;
        }
        return p < 0 ? p + mod : p;
    }

    static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

    static long ModPow(long n, long p, long mod = MOD)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = result * b % mod;
            p >>= 1;
            b = b * b % mod;
        }
        return result;
    }

    static int[] _fact = new int[] { 1, 1, 2, 6 };
    static int[] _ifact = new int[] { 1 };

    static long Fact(int n)
    {
        if (n >= _fact.Length)
        {
            int prev = _fact.Length;
			Ensure(ref _fact, n);
            for (long i = prev; i < _fact.Length; i++)
                _fact[i] = (int)(_fact[i - 1] * i % MOD);
        }
        return _fact[n];
    }

    static long InvFact(int n)
    {
        if (n >= _ifact.Length)
        {
            int prev = _ifact.Length;
            Ensure(ref _ifact, n);
            _ifact[_ifact.Length - 1] = InverseDirect((int)Fact(_ifact.Length - 1));
            for (long i = _ifact.Length - 1; i > prev; i--)
                _ifact[i - 1] = (int)(_ifact[i] * i % MOD);
        }
        return _ifact[n];
    }

    static long Fact(int n, int m)
        => m < n
            ? Fact(n) * InvFact(n - m) % MOD
            : m == n ? Fact(n) : 0;

    static long Comb(int n, int k)
        => k > 1
            ? Fact(n) * InvFact(k) % MOD * InvFact(n - k) % MOD
            : k == 1 ? n : k == 0 ? 1 : 0;
    #endregion

    #region Common
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Ensure<T>(ref T[] array, int n)
    {
        if (n >= array.Length)
            Resize(ref array, Max(n + 1, array.Length * 2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    static T[] Na<T>(int n, Func<int, T> func, int z = 0)
    {
        n += z;
        var list = new T[n];
        for (int i = z; i < n; i++) list[i] = func(i);
        return list;
    }

    static int[] Ni(int n, int z = 0) => Na(n, i=>Ni(), z);
    static long[] Nl(int n, int z = 0) => Na(n, i=>Nl(), z);
    static string[] Ns(int n, int z = 0) => Na(n, i=>Ns(), z);
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
	
    static List<int>[] NewGraph(int n, int m = 0, bool oneIndexed = true)
    {
		int off = oneIndexed ? 0 : -1;
        n += 1 + off;
        var g = new List<int>[n];
        for (int i = 0; i < n; i++)
            g[i] = new List<int>();

        for (int i = 0; i < m; i++)
        {
            int u = Ni() + off, v = Ni() + off;
            g[u].Add(v);
            g[v].Add(u);
        }
        return g;
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
        outputBuffer = new byte[16384];
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
        if (outputIndex + n <= outputBuffer.Length) return;
        Flush(false);
        if (n > outputBuffer.Length)
            Resize(ref outputBuffer, Max(outputBuffer.Length * 2, n));
    }

    static void Flush(bool flush=true)
    {
        outputStream.Write(outputBuffer, 0, outputIndex);
        outputIndex = 0;
        if (flush) outputStream.Flush();
    }

    #endregion
    #endregion

    #region Main

    public static void Main()
    {
        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());
#if __MonoCS__ && !C7
        var thread = new System.Threading.Thread(()=>new Solution().Solve());
        var f = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var t = thread.GetType().GetField("internal_thread", f).GetValue(thread);
        t.GetType().GetField("stack_size", f).SetValue(t, 32 * 1024 * 1024);
        thread.Start();
        thread.Join();
#else
        new Solution().Solve();
#endif
        Flush();
    }
    #endregion
    #endregion
}
