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

    int n, m;
    int[][] groups;
    long[] eff;
    long[] masks;

    const int Shift = 21;

    void Solve()
    {
        n = Ni();
        m = Ni();

        eff = new long[m];
        groups = new int[m][];

        for (int i = 0; i < m; i++)
        {
            var k = Ni();
            eff[i] = Nl();
            groups[i] = Na(k, () => Ni() - 1);

        }

        Sort(eff, groups, new Comparer<long>((a, b) => -a.CompareTo(b)));
        masks = new long[m];
        for (int i = 0; i < m; i++)
        {
            var mask = 0L;
            foreach (var v in groups[i])
                mask |= 1L << v;
            masks[i] = mask;
        }

        long result1 = BruteForce();
        long result2 = MinCutApproach();
        WriteLine(Max(result1,result2));
    }


    long BruteForce()
    {
        var list = new List<int>(m);
        for (int i = 0; i < m; i++)
            list.Add(i);

        list.Sort((a, b) =>
        {
            var ma = this.masks[a];
            var mb = this.masks[b];
            return (ma & -ma).CompareTo(mb & -mb);
        });

        long half = Min(1L << Shift, 1L << n) - 1;
        var masks = list.Select(x=>this.masks[x]).ToArray();
        var eff = list.Select(x=>this.eff[x]).ToArray();
        var totalScore = this.eff.Sum();
        long result = 0;
        int limit = 0;
        for (int i = 1; i < half; i++)
        {
            while (limit < masks.Length && (masks[limit] & i) != 0)
            {
                totalScore -= eff[limit];
                limit++;
            }

            var score = totalScore;

            for (int j = 0; j < limit; j++)
            {
                var mask = masks[j];
                if ((mask & i) == mask || (mask & i) == 0)
                    score += eff[j];
            }

            if (result < score) result = score;
        }

        return result;
    }

    long MinCutApproach()
    {
        var g = new long[n, n];
        long total = 0;

        for (var ig = 0; ig < groups.Length; ig++)
        {
            var group = groups[ig];
            var e = eff[ig];
            total += e;

            for (int j = 0; j < group.Length; j++)
            {
                var x = group[j];
                var y = group[(j + 1) % group.Length];

                g[x, y] += e;
                g[y, x] += e;
            }
        }

        var v = new int[n];
        var a = new bool[n];
        var w = new long[n];
        var best = long.MaxValue;

        for (int i = 0; i < n; i++)
            v[i] = i;

        while (n > 1)
        {
            a[v[0]] = true;
            for (int i = 1; i < n; i++)
            {
                a[v[i]] = false;
                w[i] = g[v[0], v[i]];
            }

            for (int i = 1, prev = v[0]; i < n; i++)
            {
                int k = -1;
                for (int j = 1; j < n; j++)
                {
                    if (a[v[j]]) continue;
                    if (k < 0 || w[j] > w[k])
                        k = j;
                }

                a[v[k]] = true;

                if (i + 1 == n)
                {
                    best = Min(w[k], best);

                    for (int j = 0; j < n; j++)
                    {
                        g[prev, v[j]] += g[v[k], v[j]];
                        g[v[j], prev] = g[prev, v[j]];
                    }

                    n--;
                    v[k] = v[n];
                    break;
                }

                prev = v[k];

                for (int j = 1; j < n; j++)
                {
                    if (a[v[j]]) continue;
                    w[j] += g[v[k], v[j]];
                }
            }
        }

        return total - best / 2;
    }

    public class Comparer<T> : IComparer<T>
    {
        readonly Comparison<T> _comparison;

        public Comparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T a, T b) => _comparison(a, b);
    }

    #region Library

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
    }

    #endregion
    #endregion
}
