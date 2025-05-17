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
    const int MOD = 1000000007;
    const int FactCache = 1000;
    const long BIG = long.MaxValue << 15;

    #endregion

    public void Solve()
    {
        int n = Ni();
        var a = Ni(n);
        var g = ReadGraph(n);
        var t = new TreeGraph(g);

        var dp = t.NodeAgg(x => a[x], (x, y) => x | y);
        var dp2 = new long[n];
        for (int i = 1; i < n; i++)
            dp2[i] = a[t.Parents[i]];

        int count = 0;

        t.MinimizeParentStatistics(dp, dp2);
        
        for (int i = 1; i < n; i++)
        {
            var p = t.Parents[i];
            if (dp[i] == dp2[i]) count++;

        }

        WriteLine(count);

    }

    /*PLACEHOLDER*/

    #region Graph Construction

    public static List<int>[] ReadGraph(int n)
    {
        var g = NewGraph(n);

        for (int i = 1; i < n; i++)
        {
            var u = Ni()-1;
            var v = Ni()-1;
            g[u].Add(v);
            g[v].Add(u);
        }

        return g;
    }

    public static List<int>[] NewGraph(int n)
    {
        var g = new List<int>[n];
        for (int i = 0; i < n; i++)
            g[i] = new List<int>();
        return g;
    }

    #endregion

    #region TreeGraph
    public class TreeGraph
    {
        #region Variables
        public int[] Parents;
        public int[] Queue;
        public int[] Depths;
        public int[] Sizes;
        public IList<int>[] Graph;
        public int Root;
        public int TreeSize;
        public int Separator;

        bool sizesInited;
        #endregion

        #region Constructor
        public TreeGraph(IList<int>[] g, int root = 0, int avoid = -1)
        {
            Graph = g;
            if (root >= 0)
                Init(root, avoid);
        }
        #endregion

        #region Methods
        public void Init(int root, int avoid = -1)
        {
            var g = Graph;
            int n = g.Length;
            Root = root;
            Separator = avoid;

            Queue = new int[n];
            Parents = new int[n];
            Depths = new int[n];

            for (int i = 0; i < Parents.Length; i++)
                Parents[i] = -1;

            Queue[0] = root;

            int treeSize = 1;
            for (int p = 0; p < treeSize; p++)
            {
                int cur = Queue[p];
                var par = Parents[cur];
                foreach (var child in g[cur])
                {
                    if (child != par && child != avoid)
                    {
                        Queue[treeSize++] = child;
                        Parents[child] = cur;
                        Depths[child] = Depths[cur] + 1;
                    }
                }
            }

            TreeSize = treeSize;
        }

        public void InitSizes()
        {
            if (sizesInited)
                return;

            if (Sizes == null)
                Sizes = new int[Graph.Length];
            sizesInited = true;

            if (Separator >= 0)
                Sizes[Separator] = 0;
            for (int i = TreeSize - 1; i >= 0; i--)
            {
                int current = Queue[i];
                Sizes[current] = 1;
                foreach (int e in Graph[current])
                    if (Parents[current] != e)
                        Sizes[current] += Sizes[e];
            }
        }
        #endregion

        #region Dynamic Programming

        public long[] NodeSum(Func<int, long> func, long[] reuseBuffer = null)
        {
            var g = Graph;
            var result = reuseBuffer ?? new long[g.Length];
            var queue = Queue;

            for (int i = TreeSize - 1; i >= 0; i--)
            {
                var u = queue[i];
                var p = Parents[u];

                var sum = func(u);
                foreach (var v in g[u])
                {
                    if (v == p) continue;
                    sum += result[v];
                }
                result[u] = sum;
            }

            return result;
        }

        public long[] NodeAgg(Func<int, long> func, Func<long, long, long> combine,
            long[] reuseBuffer = null)
        {
            var g = Graph;
            var result = reuseBuffer ?? new long[g.Length];
            var queue = Queue;

            for (int i = TreeSize - 1; i >= 0; i--)
            {
                var u = queue[i];
                var p = Parents[u];

                var sum = func(u);
                foreach (var v in g[u])
                {
                    if (v == p) continue;
                    sum = combine(sum, result[v]);
                }
                result[u] = sum;
            }

            return result;
        }


        public long[] NodeMin(Func<int, long> func, bool minimize = true, long[] reuseBuffer = null)
        {
            var g = Graph;
            var result = reuseBuffer ?? new long[g.Length];
            var queue = Queue;

            for (int i = TreeSize - 1; i >= 0; i--)
            {
                var u = queue[i];
                var p = Parents[u];

                var min = func(u);
                foreach (var v in g[u])
                {
                    if (v == p) continue;
                    min = minimize
                        ? Min(min, result[v])
                        : Max(min, result[v]);
                }
                result[u] = min;
            }

            return result;
        }

        public long[] NodeSet(Func<int, long> func, long[] reuseBuffer = null)
        {
            var g = Graph;
            var result = reuseBuffer ?? new long[g.Length];
            var queue = Queue;

            for (int i = TreeSize - 1; i >= 0; i--)
            {
                var u = queue[i];
                result[u] = func(u);
            }

            return result;
        }

        public void MinimizeNodeStatistics(long[] childStats, bool minimize = true)
        {
            var g = Graph;
            var parents = Parents;
            var queue = Queue;
            for (int iu = TreeSize - 1; iu >= 0; iu--)
            {
                var u = queue[iu];
                var p = parents[u];
                var min = childStats[u];
                foreach (var v in g[u])
                {
                    if (v != p)
                    {
                        min = minimize
                            ? Min(min, childStats[v])
                            : Max(min, childStats[v]);
                    }
                }
                childStats[u] = min;
            }
        }

        public class ExcludedMask
        {
            public long Mask;
            public long Mask2;

            public ExcludedMask(long v)
            {
                Mask = v;
            }

            public void Add(long v)
            {
                Mask2 |= Mask & v;
                Mask |= v;
            }

            public long Exclude(long exclude) => (Mask & ~exclude) | Mask2;
        }


        public void MinimizeParentStatistics(long[] childStats, long[] parentStats,
            bool merge = true)
        {
            int treeSize = TreeSize;
            var g = Graph;
            var queue = Queue;
            var empty = new ExcludedMask(0);

            for (int iu = 0; iu < treeSize; iu++)
            {
                var u = queue[iu];
                var extrema = empty;
                extrema.Add(parentStats[u]);

                var p = Parents[u];
                foreach (var v in g[u])
                {
                    if (v != p)
                        extrema.Add(childStats[v]);
                }

                foreach (var v in g[u])
                {
                    if (v != p)
                        parentStats[v] = parentStats[v] | extrema.Exclude(childStats[v]);
                }

                //if (merge)
                //    parentStats[u] = extrema.Mask;
            }

            // var offBulbs = NodeSum(u => lit[u] ? 0 : 1);
            // var totals = NodeSum(x => 1);
            // var odd = NodeSet(v => (offBulbs[v] & 1) != 0 ? totals[v] : int.MaxValue);
            // var iodd = NodeSet(v => (offBulbs[0] - offBulbs[v] & 1) == 1 ? totals[0] - totals[v] : int.MaxValue);
            // MinimizeNodeStatistics(odd);
            // MinimizeParentStatistics(odd, iodd, true);
            // for (int u = 0; u < g.Length; u++)
            //     WriteLine(Max(0, g.Length - iodd[u]));
        }
        #endregion
    }

    #endregion

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
        var thread = new Thread(()=>new Solution().Solve());
        var f = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = typeof(Thread).GetField("internal_thread", f).GetValue(thread);
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
