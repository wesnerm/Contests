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
    List<Edge>[] g;
    int n, m;
    long k;
    long[] Distances;
    int[] Parents;

    #endregion

    void Solve()
	{
		n = Ni();
		k = Nl();
		m = Ni();
        g = ReadGraph(n,m);
        
        foreach(var list in g)
            list.Sort((a,b)=>a.W-b.W);
        Djikstra(g, 1, n);
    }



    public void Djikstra(List<Edge>[] graph, int start, int end = -1)
    {
        int n = graph.Length;
        Distances = new long[n];
        //Parents = new int[n];
        for (int i = 0; i < n; i++)
        {
            Distances[i] = long.MaxValue >> 4;
            //Parents[i] = -1;
        }

        var q = new SortedSet<Pair>();
        q.Add(new Pair(start, 0));
        Distances[start] = 0;
        while (q.Count > 0)
        {
            var tup = q.Min;
            q.Remove(tup);
            var u = tup.X;
            if (u == end) break;
            if (tup.Cost != Distances[u]) continue;

            // Round up
            var d = Distances[u];

            var neighbors = graph[u];
            foreach (var e in neighbors)
            {
                var v = e.Other(u);
                var d2 = d + e.W;
                if (v != end && ((d2 / k) & 1) == 1)
                    d2 += k - d2 % k;
                if (d2 >= Distances[v]) continue;
                //q.Remove(new Pair(v, Distances[v]));
                //Parents[v] = Parents[u];
                Distances[v] = d2;
                q.Add(new Pair(v, d2));
            }
        }

        WriteLine(Distances[end]);
    }

    struct Pair : IComparable<Pair>
    {
        public readonly int X;
        public readonly long Cost;

        public Pair(int x, long cost)
        {
            X = x;
            Cost = cost;
        }

        public int CompareTo(Pair pair)
        {
            int cmp = Cost.CompareTo(pair.Cost);
            return cmp != 0 ? cmp : X.CompareTo(pair.X);
        }
    }

    public class SimpleHeap<T>
    {
        Comparison<T> comparison;
        List<T> list = new List<T>();

        public SimpleHeap(Comparison<T> comparison = null)
        {
            this.comparison = comparison ?? Comparer<T>.Default.Compare;
        }

        public int Count => list.Count;

        public T FindMin() => list[0];

        public T Dequeue()
        {
            var pop = list[0];
            var elem = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            if (list.Count > 0) ReplaceTop(elem);
            return pop;
        }

        public void ReplaceTop(T elem)
        {
            int i = 0;
            list[i] = elem;
            while (true)
            {
                int child = 2 * i + 1;
                if (child >= list.Count)
                    break;

                if (child + 1 < list.Count && comparison(list[child], list[child + 1]) > 0)
                    child++;

                if (comparison(list[child], elem) >= 0)
                    break;

                Swap(i, child);
                i = child;
            }
        }

        public void Enqueue(T push)
        {
            list.Add(push);
            int i = list.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (comparison(list[parent], push) <= 0)
                    break;
                Swap(i, parent);
                i = parent;
            }
        }

        void Swap(int t1, int t2)
        {
            T tmp = list[t1];
            list[t1] = list[t2];
            list[t2] = tmp;
        }
    }

    #region Helpers

    public class Edge
    {
        public int I;
        public int U;
        public int V;
        public int W;

        public int Other(int u) => U == u ? V : U;

        public override string ToString()
        {
            return $"#{I} ({U + 1},{V + 1})  {W}";
        }
    }

    public static List<Edge>[] ReadGraph(int n, int m)
    {
        var g = NewGraph(n);

        for (int i = 1; i <= m; i++)
        {
            var u = Ni();
            var v = Ni();
            var w = Ni();
            if (u==v) continue;
            var edge = new Edge { U = u, V = v, I = i, W = w };
            g[u].Add(edge);
            g[v].Add(edge);
        }

        return g;
    }

    public static List<Edge>[] NewGraph(int n)
    {
        var g = new List<Edge>[n+1];
        for (int i = 0; i <= n; i++)
            g[i] = new List<Edge>();
        return g;
    }

    #endregion





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

    static T[] Na<T>(int n, Func<T> func)
    {
        var list = new T[n];
        for (int i = 0; i < n; i++) list[i] = func();
        return list;
    }

    static int[] Ni(int n) => Na(n, Ni);

    static long[] Nl(int n) => Na(n, Nl);

    static string[] Ns(int n) => Na(n, Ns);

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
