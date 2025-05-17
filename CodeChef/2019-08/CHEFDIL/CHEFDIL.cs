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
		int T = Ni();
		for (int t=1; t<=T; t++)
		{
			var s = Ns();

			bool flipped = false;
			for (int i=0; i<s.Length; i++)
			{
				bool up = s[i] == '1';

				if (flipped) up = !up;

				if (up)
				{
					flipped = true;
				}
				else
				{
					flipped = false;
				}
			}

			WriteLine(flipped ? "WIN" : "LOSE");
		}
    }

    /*PLACEHOLDER*/

    #region Library


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

    static List<int>[] NewGraph(int n, int m = 0, int off = 0)
    {
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
