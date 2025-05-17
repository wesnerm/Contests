using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static System.Array;
using static System.Math;

class Solution
{
    public void Solve()
    {
        int n = Ni();
        var strs = Ns(n);

        var indices = new int[n];
        for (int i = 0; i < n; i++)
            indices[i] = i;

        var sortedStrs = strs.ToArray();
        Sort(sortedStrs, indices, StringComparer.Ordinal);

        var indices2 = new int[n];
        for (int i = 0; i < n; i++)
            indices2[indices[i]] = i;

        int queryCount = Ni();
        var queries = new Query[queryCount];
        long[] answers = new long[queryCount];
        for (int i = 0; i < queryCount; i++)
        {
            string s;
            queries[i] = new Query {I = i, L = Ni() - 1, R = Ni() - 1, S = s = Ns()};
            queries[i].StringIndex = Bound(sortedStrs, s, true);
        }

        var queries2 = queries.ToArray();
        Sort(queries);
        Sort(queries2, (a,b)=> a.R.CompareTo(b.R));

        var ft = new FenwickTree(n);

        int left = 0;
        int right = 0;
        for (int i = 0; i < n; i++)
        {
            while (left < queryCount && queries[left].L <= i)
            {
                var q = queries[left++];
                var count = ft.SumInclusive(q.StringIndex - 1);
                answers[q.I] -= count;
            }

            ft.Add(indices2[i], 1);

            while (right < queryCount && queries2[right].R <= i)
            {
                var q = queries2[right++];
                var count = ft.SumInclusive(q.StringIndex-1);
                answers[q.I] += count;
            }
        }

        foreach (var answer in answers)
            WriteLine(answer);
    }

    public class Query : IComparable<Query>
    {
        public int I;
        public int L;
        public int R;
        public string S;
        public int StringIndex;

        public int CompareTo(Query other)
        {
            var cmp = L.CompareTo(other.L);
            if (cmp != 0) return cmp;
            cmp = R.CompareTo(other.R);
            if (cmp != 0) return cmp;
            return 0;
        }
    }

    public class FenwickTree
    {
        #region Variables
        public readonly long[] A;
        #endregion

        #region Constructor
        /*public Fenwick(int[] a) : this(a.Length)
        {
            for (int i = 0; i < a.Length; i++)
                Add(i, a[i]);
        }*/

        public FenwickTree(long[] a) : this(a.Length)
        {
            int n = a.Length;
            System.Array.Copy(a, 0, A, 1, n);
            for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
            {
                for (int i = k; i <= n; i += k)
                    A[i] += A[i - h];
            }

            //for (int i = 0; i < a.Length; i++)
            //	Add(i, a[i]);
        }

        public FenwickTree(long size)
        {
            A = new long[size + 1];
        }
        #endregion

        #region Properties
        public long this[int index] => AltRangeUpdatePointQueryMode ? SumInclusive(index) : SumInclusive(index, index);

        public int Length => A.Length - 1;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public long[] Table
        {
            get
            {
                int n = A.Length - 1;
                long[] ret = new long[n];
                for (int i = 0; i < n; i++)
                    ret[i] = SumInclusive(i);
                if (!AltRangeUpdatePointQueryMode)
                    for (int i = n - 1; i >= 1; i--)
                        ret[i] -= ret[i - 1];
                return ret;
            }
        }
        #endregion


        #region Methods
        // Increments value		
        /// <summary>
        /// Adds val to the value at i
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="val">The value.</param>
        public void Add(int i, long val)
        {
            if (val == 0) return;
            for (i++; i < A.Length; i += (i & -i))
                A[i] += val;
        }

        // Sum from [0 ... i]
        public long SumInclusive(int i)
        {
            long sum = 0;
            for (i++; i > 0; i -= (i & -i))
                sum += A[i];
            return sum;
        }

        public long SumInclusive(int i, int j)
        {
            return SumInclusive(j) - SumInclusive(i - 1);
        }

        // get largest value with cumulative sum less than x;
        // for smallest, pass x-1 and add 1 to result
        public int GetIndexGreater(long x)
        {
            int i = 0, n = A.Length - 1;
            for (int bit = (int)HighestOneBit(n); bit != 0; bit >>= 1)
            {
                int t = i | bit;

                // if (t <= n && Array[t] < x) for greater or equal 
                if (t <= n && A[t] <= x)
                {
                    i = t;
                    x -= A[t];
                }
            }
            return i;
        }

        #endregion

        #region Alternative Range Update Point Query Mode  ( cf Standard Point Update Range Query )

        public bool AltRangeUpdatePointQueryMode;

        /// <summary>
        /// Inclusive update of the range [left, right] by value
        /// The default operation of the fenwick tree is point update - range query.
        /// We use this if we want alternative range update - point query.
        /// SumInclusive becomes te point query function.
        /// </summary>
        /// <returns></returns>
        public void AltAddInclusive(int left, int right, long delta)
        {
            Add(left, delta);
            Add(right + 1, -delta);
        }

        public long AltQuery(int index)
        {
            return SumInclusive(index);
        }


        #endregion
    }


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

