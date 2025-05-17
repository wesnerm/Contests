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


    const int NN = 1000 + 5;

    List<int>[] v = new List<int>[NN];
    int n;
    int[] s = new int[NN], ans1 = new int[NN], ans2 = new int[NN];

    static int Gx, Gy;

    static int[] Primes = new int[]
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
        31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
        73, 79, 83, 89, 97, 101, 103, 107, 109, 113,
        127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
        179, 181, 191, 193, 197, 199, 211, 223, 227, 229,
    };

    struct point
    {
        public int x, y, id;
    };

    List<point> p = new List<point>();

    class MyComparer : IComparer<point>
    {
        public int Compare(point a, point b)
        {
            return ((a.x - Gx) * 1L * (b.y - Gy)).CompareTo(1L * (b.x - Gx) * (a.y - Gy));
        }
    }

    IComparer<point> mycomp = new MyComparer();

    void dfs1(int g, int fr)
    {
        s[g] = 1;
        foreach (int t in v[g])
        {
            if (t != fr)
            {
                dfs1(t, g);
                s[g] += s[t];
            }
        }
    }


    void dfs2(int g, int fr, int l, int r)
    {

        for (int i = l + 1; i < r; i++)
        {
            if (p[l].x > p[i].x || (p[l].x == p[i].x && p[l].y > p[i].y))
            {
                var tmp = p[l];
                p[l] = p[i];
                p[i] = tmp;
            }
        }

        ans1[g] = p[l].x;
        ans2[g] = p[l].y;

        Gx = p[l].x;
        Gy = p[l].y;

        l++;

        p.Sort(l, r - l, mycomp);

        foreach (int t in v[g])
        {
            if (t != fr)
            {
                dfs2(t, g, l, l + s[t]);
                l += s[t];
            }
        }
    }



    void Solve()
    {
        n = Ni();
        int maxlen = 1;

        for (int i = 0; i < v.Length; i++)
            v[i] = new List<int>();

        for (int i = 1; i < n; i++)
        {
            int x = Ni(), y = Ni(), len = Ni();
            v[x].Add(y);
            v[y].Add(x);
            if (len > maxlen) maxlen = len;
        }

        maxlen += 300;

        var s = (int)Math.Ceiling(maxlen * Math.Sqrt(3) / 2);
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                // Make each number prime
                point po;
                po.x = i * maxlen + Primes[(i + j) % Primes.Length] | 1;
                po.y = j * maxlen + (i % 2 == 1 ? (maxlen + 1) / 2 : 0) - Primes[i + 1] & ~1;
                po.id = i;
                p.Add(po);
            }
        }

        dfs1(1, 1);
        dfs2(1, 1, 0, p.Count);
        for (int i = 1; i <= n; i++)
            WriteLine($"{ans1[i]} {ans2[i]}");
    }

    static IEnumerable<point> FareySequence(int n)
    {
        int a = 0, b = 1, c = 1, d = n;
        while (d > 1)
        {
            int k = (n + b) / d;
            int aa = a, bb = b;
            a = c;
            b = d;
            c = k * c - aa;
            d = k * d - bb;
            yield return new point { x = a, y = b };
        }
    }

    public static double Score(int[][] pts)
    {
        var score = 0.0;
        for (int i = 0; i < pts.Length; i++)
            for (int j = 0; j < i; j++)
            {
                var dx = pts[i][0] - pts[j][0];
                var dy = pts[i][1] - pts[j][1];
                score += Sqrt(1.0 * dx * dx + 1.0 * dy * dy);
            }
        return score;
    }

    #region Primes
    public static int[] PrimesListUpTo1e6()
    {
        List<int> primes = new List<int>(1000000);
        int max = 2000 * 1000 + 1;
        var factors = PrimeFactorsUpTo(max);

        for (int i = 2; i < max; i++)
        {
            if (factors[i] == i)
                primes.Add(i);
        }

        return primes.ToArray();
    }

    public static int[] PrimeFactorsUpTo(int n)
    {
        var factors = new int[n + 1];

        for (int i = 2; i <= n; i += 2)
            factors[i] = 2;

        var sqrt = (int)Math.Sqrt(n);
        for (int i = 3; i <= sqrt; i += 2)
        {
            if (factors[i] != 0) continue;
            for (int j = i * i; j <= n; j += i + i)
            {
                if (factors[j] == 0)
                    factors[j] = i;
            }
        }

        for (int i = 3; i <= n; i += 2)
        {
            if (factors[i] == 0)
                factors[i] = i;
        }

        return factors;
    }
    #endregion

    #region Heap
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
