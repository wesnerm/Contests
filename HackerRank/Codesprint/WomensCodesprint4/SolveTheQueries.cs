#define ST
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Array;
using static System.Math;
using STType = System.Int32;

class Solution
{
    private int n;
    private int[] ai;
#if ST
    private DynamicSegmentTree2 st;
#endif

    public void Solve()
    {
        n = Ni();
        ai = Ni(n);

        var fts = new DynamicSegmentTree[Primes100.Length];
        for (int i = 0; i < fts.Length; i++)
            fts[i] = new DynamicSegmentTree(n);

#if ST
        st = new DynamicSegmentTree2(n);
#endif
        for (int i = 0; i < n; i++)
        {
            var mask = 0;
            var a = ai[i];
            for (int ip = 0; ip < Primes100.Length; ip++)
            {
                var p = Primes100[ip];
                if (p > a) break;

                int c = 0;
                for (; a % p == 0; a /= p)
                    c++;

                if (c > 0)
                {
                    mask |= 1 << ip;
                    fts[ip].Add(i, 1, c);
                }
            }

#if ST
            st.Cover(i, 1, mask);
#endif
        }

        int queryCount = Ni();

        while (queryCount-- > 0)
        {
            int cmd = Ni();
            int left = Ni() - 1;
            int right = Ni() - 1;

            var work = new long[Primes100.Length];
            var check1 = st.GetMask(left, right - left + 1);

            switch (cmd)
            {
                case 1:
                    int x = Ni();
                    var mask = 0;

                    var a = x;
                    for (int ip = 0; ip < Primes100.Length; ip++)
                    {
                        var p = Primes100[ip];
                        var bit = 1 << ip;
                        if (p > x && bit > check1) break;

                        int c = 0;
                        for (; a % p == 0; a /= p)
                            c++;

                        if (c == 0 && (check1 & bit) == 0) continue;

                        fts[ip].Cover(left, right - left + 1, c);
                        if (c++ > 0)
                            mask |= 1 << ip;
                    }

#if ST
                    st.Cover(left, right-left+1, mask);
#endif
                    break;

                case 2:
                    int k = Ni() - 1;
                    int L = Ni() - 1;
                    int m = Ni();

#if ST
                    var check2 = st.GetMask(k, L - k + 1);
                    if ((~check1 & check2) != 0)
                    {
                        WriteLine(-1);
                        continue;
                    }
#endif

                    var result = 1L;
                    int iip = 0;

                    for (; iip < Primes100.Length; iip++)
                    {
                        var bit = 1 << iip;
                        if (bit > check1) break;

                        var sum1 = (check1&bit)!=0 ? fts[iip].GetSum(left, right - left + 1) : 0;
                        var sum2 = (check2&bit)!=0 ? fts[iip].GetSum(k, L - k + 1) : 0;

                        var diff = sum1 - sum2;
                        if (diff < 0)
                        {
                            result = -1;
                            break;
                        }

                        work[iip] = diff;
                    }

                    if (result == 1)
                    {
                        if (work[0] != 0)
                        {
                            var diff = work[0];
                            if (diff < 31)
                                result <<= (int)diff;
                            else
                                result *= ModPow(2, diff, m);
                            if (result >= m) result %= m;
                        }

                        for (int ip = 1; ip < iip; ip++)
                        {
                            var diff = work[ip];
                            if (diff > 0)
                            {
                                var p = Primes100[ip];
                                if (diff == 1)
                                    result *= p;
                                else
                                    result *= ModPow(p, diff, m);
                                if (result >= m) result %= m;
                            }
                        }
                    }

                    WriteLine(result);
                    break;
            }
        }
    }


    private static int[] Primes100 = new int[]
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
        31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
        73, 79, 83, 89, 97,
    };

    public class DynamicSegmentTree
    {
        public STType Min;
        public STType Sum;
        public STType LazyAdd;
        public int Count;
        public DynamicSegmentTree Left;
        public DynamicSegmentTree Right;
        public bool Covering;

        // public int Start;
        // public int End => Start + Count;

        public DynamicSegmentTree(STType[] array)
            : this(array, 0, array.Length)
        {
        }

        public DynamicSegmentTree(int n)
            : this(null, 0, n)
        {

        }

        DynamicSegmentTree(STType[] array, int start, int count)
        {
            // Start = start;
            Count = count;

            if (count >= 2)
            {
                int mid = count / 2;
                Left = new DynamicSegmentTree(array, start, mid);
                Right = new DynamicSegmentTree(array, start + mid, count - mid);
                UpdateNode();
            }
            else
            {
                var v = array == null ? 0 : array[start];
                Min = v;
                Sum = v;
            }
        }


        public long GetMin(int start, int count)
        {
            int end = start + count;
            if (start <= 0 && Count <= end)
                return Min;
            if (start >= Count || end <= 0)
                return long.MaxValue;

            LazyPropagate();
            return Min(Left.GetMin(start, count),
                Right.GetMin(start - Left.Count, count));
        }


        public long GetSum(int start, int count)
        {
            int end = start + count;
            if (start <= 0 && Count <= end)
                return Sum;
            if (start >= Count || end <= 0)
                return 0;

            LazyPropagate();
            return Left.GetSum(start, count)
                + Right.GetSum(start - Left.Count, count);
        }

        public void Add(int start, int count, STType value)
        {
            int end = start + count;
            if (start >= Count || end <= 0)
                return;

            if (start <= 0 && Count <= end)
            {
                Add(value);
                return;
            }

            LazyPropagate();
            Left.Add(start, count, value);
            Right.Add(start - Left.Count, count, value);
            UpdateNode();
        }

        void Add(STType value)
        {
            Sum += value * Count;
            LazyAdd += value;
        }


        public void Cover(int start, int count, STType value)
        {
            int end = start + count;
            if (start >= Count || end <= 0)
                return;

            if (start <= 0 && Count <= end)
            {
                Cover(value);
                return;
            }

            LazyPropagate();
            Left.Cover(start, count, value);
            Right.Cover(start - Left.Count, count, value);
            UpdateNode();
        }

        void Cover(STType value)
        {
            Min = value;
            LazyAdd = 0;
            Sum = value * Count;
            Covering = true;
        }

        void LazyPropagate()
        {
            if (Count <= 1)
                return;

            if (Covering)
            {
                Left.Cover(Min);
                Right.Cover(Min);
                LazyAdd = 0;
                Covering = false;
                return;
            }

            if (LazyAdd != 0)
            {
                var value = LazyAdd;
                LazyAdd = 0;
                Left.Add(value);
                Right.Add(value);
            }
        }

        void UpdateNode()
        {
            var left = Left;
            var right = Right;
            Sum = left.Sum + right.Sum;
            //Min = Min(left.Min, right.Min);
        }


        public STType[] Table
        {
            get
            {
                var table = new STType[Count];
                for (int i = 0; i < table.Length; i++)
                    table[i] = (STType)GetSum(i, 1);
                return table;
            }
        }

        public override string ToString()
        {
            return string.Join(",", Table);
        }

    }

    #region MaskST
#if ST
    public class DynamicSegmentTree2
    {
        public STType Mask;
        public int Count;
        public DynamicSegmentTree2 Left;
        public DynamicSegmentTree2 Right;
        public bool Covering;

        // public int Start;
        // public int End => Start + Count;

        public DynamicSegmentTree2(STType[] array)
            : this(array, 0, array.Length)
        {
        }

        public DynamicSegmentTree2(int n)
            : this(null, 0, n)
        {

        }

        DynamicSegmentTree2(STType[] array, int start, int count)
        {
            // Start = start;
            Count = count;

            if (count >= 2)
            {
                int mid = count / 2;
                Left = new DynamicSegmentTree2(array, start, mid);
                Right = new DynamicSegmentTree2(array, start + mid, count - mid);
                UpdateNode();
            }
            else
            {
                var v = array == null ? 0 : array[start];
                Mask = v;
            }
        }


        public long GetMask(int start, int count)
        {
            int end = start + count;
            if (start <= 0 && Count <= end)
                return Mask;
            if (start >= Count || end <= 0)
                return 0;

            LazyPropagate();
            return Left.GetMask(start, count) |
                Right.GetMask(start - Left.Count, count);
        }


        public void Cover(int start, int count, STType value)
        {
            int end = start + count;
            if (start >= Count || end <= 0)
                return;

            if (start <= 0 && Count <= end)
            {
                Cover(value);
                return;
            }

            LazyPropagate();
            Left.Cover(start, count, value);
            Right.Cover(start - Left.Count, count, value);
            UpdateNode();
        }

        void Cover(STType value)
        {
            Mask = value;
            Covering = true;
        }

        void LazyPropagate()
        {
            if (Count <= 1)
                return;

            if (Covering)
            {
                Left.Cover(Mask);
                Right.Cover(Mask);
                Covering = false;
                return;
            }
        }

        void UpdateNode()
        {
            var left = Left;
            var right = Right;
            Mask = left.Mask | right.Mask;
        }


        public STType[] Table
        {
            get
            {
                var table = new STType[Count];
                for (int i = 0; i < table.Length; i++)
                    table[i] = (STType)GetMask(i, 1);
                return table;
            }
        }

        public override string ToString()
        {
            return string.Join(",", Table);
        }

    }
#endif
    #endregion

    #region Mod Math
    public const int MOD = 1000000007;

    static int[] _inverse;
    public static long Inverse(long n)
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

    public static long Mult(long left, long right)
    {
        return (left * right) % MOD;
    }

    public static long Div(long left, long divisor)
    {
        return left % divisor == 0
            ? left / divisor
            : Mult(left, Inverse(divisor));
    }

    public static long Add(long x, long y)
    {
        return (x += y) >= MOD ? x - MOD : x;
    }

    public static long Subtract(long left, long right)
    {
        long result = left - right;
        if (result < 0) result += MOD;
        return result;
    }

    public static long Fix(long n)
    {
        return ((n % MOD) + MOD) % MOD;
    }

    public static long ModPow(long n, long p, long mod = MOD)
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
    static List<long> _ifact;

    public static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(100) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    public static long InverseFact(int n)
    {
        if (_ifact == null) _ifact = new List<long>(100) { 1 };
        for (int i = _ifact.Count; i <= n; i++)
            _ifact.Add(Div(_ifact[i - 1], i));
        return _ifact[n];
    }

    public static long Fact(int n, int m)
    {
        var fact = Fact(n);
        if (m < n)
            fact = Mult(fact, InverseFact(n - m));
        return fact;
    }

    public static long Comb(int n, int k)
    {
        if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
        if (k + k > n) return Comb(n, n - k);
        return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
    }
#endregion

#region Common
    public static void Swap<T>(ref T a, ref T b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    public static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    public static int Bound<T>(T[] array, T value, bool upper = false)
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

    public static long IntPow(long n, long p)
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

    public static int Log2(long value)
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

    public static int BitCount(long x)
    {
        int count;
        var y = unchecked((ulong)x);
        for (count = 0; y != 0; count++)
            y &= y - 1;
        return count;
    }

    public static int HighestOneBit(int n)
    {
        return n != 0 ? 1 << Log2(n) : 0;
    }

    public static long HighestOneBit(long n)
    {
        return n != 0 ? 1L << Log2(n) : 0;
    }
#endregion

#region Fast IO
#region  Input
    static System.IO.Stream inputStream;
    static int inputIndex, bytesRead;
    static byte[] inputBuffer;
    static System.Text.StringBuilder builder;
    const int MonoBufferSize = 4096;

    public static void InitInput(System.IO.Stream input = null, int stringCapacity = 16)
    {
        builder = new System.Text.StringBuilder(stringCapacity);
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
        inputBuffer[0] = (byte)'\n';
    }

    public static int Read()
    {
        if (inputIndex >= bytesRead) ReadMore();
        return inputBuffer[inputIndex++];
    }

    public static T[] N<T>(int n, Func<T> func)
    {
        var list = new T[n];
        for (int i = 0; i < n; i++) list[i] = func();
        return list;
    }

    public static int[] Ni(int n)
    {
        var list = new int[n];
        for (int i = 0; i < n; i++) list[i] = Ni();
        return list;
    }

    public static long[] Nl(int n)
    {
        var list = new long[n];
        for (int i = 0; i < n; i++) list[i] = Nl();
        return list;
    }

    public static string[] Ns(int n)
    {
        var list = new string[n];
        for (int i = 0; i < n; i++) list[i] = Ns();
        return list;
    }

    public static int Ni()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg) { c = Read(); }

        int number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
            if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg) { c = Read(); }

        long number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
            if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static char[] Nc(int n)
    {
        var list = new char[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
        return list;
    }

    public static byte[] Nb(int n)
    {
        var list = new byte[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte)c;
        return list;
    }

    public static string Ns()
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

    public static int SkipSpaces()
    {
        int c;
        do c = Read(); while (unchecked((uint)c - 33 >= (127 - 33)));
        return c;
    }

    public static string ReadLine()
    {
        builder.Clear();
        while (true)
        {
            int c = Read();
            if (c == 10 || c == 13 && (c = Read()) == 10 || c <= 0) break;
            builder.Append((char)c);
        }
        return builder.ToString();
    }
#endregion

#region Output
    static System.IO.Stream outputStream;
    static byte[] outputBuffer;
    static int outputIndex;

    public static void InitOutput(System.IO.Stream output = null)
    {
        outputStream = output ?? Console.OpenStandardOutput();
        outputIndex = 0;
        outputBuffer = new byte[65535];
    }

    public static void Write(string format, params object[] args)
    {
        Write(string.Format(format, args));
    }

    public static void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }

    public static void WriteLine(object obj = null)
    {
        Write(obj);
        Write('\n');
    }

    public static void WriteLine(long number)
    {
        Write(number);
        Write('\n');
    }

    public static void Write(long signedNumber)
    {
        ulong number = unchecked((ulong)signedNumber);
        if (signedNumber < 0)
        {
            Write('-');
            number = unchecked((ulong)(-signedNumber));
        }

        Reserve(20 + 1); // 20 digits + 1 extra for sign
        int left = outputIndex;
        do
        {
            outputBuffer[outputIndex++] = (byte)('0' + number % 10);
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

    public static void Write(object obj)
    {
        if (obj == null) return;

        var s = obj.ToString();
        Reserve(s.Length);
        for (int i = 0; i < s.Length; i++)
            outputBuffer[outputIndex++] = (byte)s[i];
    }

    public static void Write(char c)
    {
        Reserve(1);
        outputBuffer[outputIndex++] = (byte)c;
    }

    public static void Write(byte[] array, int count)
    {
        Reserve(count);
        Array.Copy(array, 0, outputBuffer, outputIndex, count);
        outputIndex += count;
    }

    static void Reserve(int n)
    {
        if (outputIndex + n <= outputBuffer.Length)
            return;

        Dump();
        if (n > outputBuffer.Length)
            Array.Resize(ref outputBuffer, Math.Max(outputBuffer.Length * 2, n));
    }

    static void Dump()
    {
        outputStream.Write(outputBuffer, 0, outputIndex);
        outputIndex = 0;
    }

    public static void Flush()
    {
        Dump();
        outputStream.Flush();
    }

#endregion
#endregion

#region Main
    public static int Main()
    {
        try
        {
            InitInput(Console.OpenStandardInput());
            InitOutput(Console.OpenStandardOutput());
            new Solution().Solve();
            Flush();
            // Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            var line = new StackTrace(e, true).GetFrames()
                .Select(x => x.GetFileLineNumber()).FirstOrDefault(x => x != 0);
            var wait = line % 300 * 10 + 5;
            var process = Process.GetCurrentProcess();
            while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 3000) wait += 1000;
            while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(wait, 3000)) ;
            return 1;
        }
    }
#endregion
}

