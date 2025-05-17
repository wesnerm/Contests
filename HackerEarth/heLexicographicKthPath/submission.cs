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
    const int BIG = int.MaxValue >> 4;
    const long BIGPATH = 1000L * 1000 * 1000 * 1000 * 1000 * 1000;
    int n, m;
    long k;
    List<Edge>[] g;

    #endregion

    public void Solve()
    {
        n = Ni();
        m = Ni();
        k = Nl();

        g = ReadGraph(n, m);

        // Shortest dists
        var dist = new int[n + 1];
        var paths = new long[n + 1];
        for (int i = 0; i < dist.Length; i++)
            dist[i] = BIG;

        var queue = new Queue<int>();
        dist[n] = 0;
        paths[n] = 1;
        queue.Enqueue(n);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            int depth = dist[p] + 1;
            foreach (var e in g[p])
            {
                var v = e.Other(p);
                if (depth > dist[v]) continue;
                if (dist[v] == BIG) queue.Enqueue(v);

                paths[v] += paths[p];
                if (paths[v] > BIGPATH) paths[v] = BIGPATH + 1;

                dist[v] = depth;
            }
        }

        // Lexicographic shortest path
        var dict = new State[26];
        var heap = new SimpleHeap<State>((a, b) => a.S.CompareTo(b.S));

        var shortestPath = dist[1];

        var state = new State { Us = { new Bucket { V = 1, Count = 1 } } };
        heap.Enqueue(state);
        int stringLength = 0;
        StringBuilder sb = new StringBuilder();
        while (heap.Count > 0)
        {
            state = heap.Dequeue();
            state.Compress();

            long numPaths = 0;

            var check = shortestPath - stringLength - 1;

            foreach (var pb in state.Us)
            {
                var p = pb.V;
                if (p == n)
                {
                    k -= pb.Count;
                    if (k > 0) continue;

                    sb.Append(state.S);
                    WriteLine(sb);
                    Thread.Sleep(500);
                    return;
                }

                foreach (var e in g[p])
                {
                    var v = e.Other(p);
                    if (dist[v] != check) continue;

                    if (paths[v] > BIGPATH / pb.Count)
                        numPaths = BIGPATH + 1;
                    else
                        numPaths += paths[v] * pb.Count;
                    if (numPaths > BIGPATH) numPaths = BIGPATH + 1;
                }
            }

            if (numPaths < k)
            {
                k -= numPaths;
                continue;
            }

            heap.Clear();
            Array.Clear(dict, 0, dict.Length);
            if (state.S != 0)
                sb.Append(state.S);
            stringLength++;

            foreach (var pb in state.Us)
            {
                var p = pb.V;
                foreach (var e in g[p])
                {
                    var v = e.Other(p);
                    var sd = stringLength + dist[v];
                    if (sd > shortestPath) continue;

                    var s2 = (char)e.W;
                    State state2 = dict[s2 - 'a'];
                    if (state2 == null)
                    {
                        state2 = dict[s2 - 'a'] = new State { S = s2 };
                        heap.Enqueue(state2);
                    }
                    state2.Us.Add(new Bucket { V = v, Count = pb.Count });
                }
            }
        }

        WriteLine("-1");
    }

    public class State : IComparable<State>
    {
        public char S;
        public List<Bucket> Us = new List<Bucket>();

        public int CompareTo(State other)
        {
            return S.CompareTo(other.S);
        }

        public void Compress()
        {
            Us.Sort();
            int write = 0;
            for (int i = 0; i < Us.Count; i++)
            {
                if (write == 0 || Us[write - 1].V != Us[i].V)
                    Us[write++] = Us[i];
                else
                {
                    var count = Us[write - 1].Count + Us[i].Count;
                    if (count > BIGPATH) count = BIGPATH + 1;
                    Us[write - 1] = new Bucket
                    {
                        V = Us[i].V,
                        Count = count
                    };
                }
            }
            Us.RemoveRange(write, Us.Count - write);
        }

        public override string ToString() => $"{S} (" + string.Join(" ", Us) + ")";
    }

    public struct Bucket : IEquatable<Bucket>, IComparable<Bucket>
    {
        public int V;
        public long Count;
        public bool Equals(Bucket other)
        {
            return V == other.V && Count == other.Count;
        }

        public int CompareTo(Bucket other)
        {
            return V.CompareTo(other.V);
        }
    }

    #region Features
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
        var g = NewGraph(n + 1);

        for (int i = 0; i < m; i++)
        {
            var u = Ni();
            var v = Ni();
            var w = SkipSpaces();
            var edge = new Edge { U = u, V = v, I = i, W = w };
            g[u].Add(edge);
            g[v].Add(edge);
        }

        return g;
    }

    public static List<Edge>[] NewGraph(int n)
    {
        var g = new List<Edge>[n + 1];
        for (int i = 0; i <= n; i++)
            g[i] = new List<Edge>();
        return g;
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

        public void Clear()
        {
            list.Clear();
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
        outputBuffer = new byte[4096];
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
        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());

        new Solution().Solve();

        Flush();
    }
    #endregion
    #endregion
}
class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}
