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
        int t = Ni();
        while (t-- > 0)
        {
            int n = Ni();

            var lit = new bool[n];
            for (int i = 0; i < n; i++)
                lit[i] = Ni() == 1;

            var g = NewGraph(n);
            for (int i = 1; i < n; i++)
            {
                int u = Ni() - 1;
                int v = Ni() - 1;
                g[u].Add(v);
                g[v].Add(u);
            }


            int bulbsOff = 0;
            int leafWithBulbOff = -1;
            int leavesWithBulbOn = -1;

            for (int i = 0; i < n; i++)
            {
                if (!lit[i])
                {
                    bulbsOff++;
                    if (leafWithBulbOff != -2 && g[i].Count == 1)
                        leafWithBulbOff = leafWithBulbOff == -1 ? i : -2;
                }
            }

            // Trivial Case
            bool evenBulbsOff = (bulbsOff & 1) == 0;
            if (evenBulbsOff)
            {
                for (int i = 0; i < n; i++)
                    WriteLine(n);
                continue;
            }

            if (leafWithBulbOff == -2)
            {
                for (int i = 0; i < n; i++)
                    WriteLine((n - 1));
                continue;
            }

            var tree = new TreeGraph(g);
            var treeSize = tree.TreeSize;
            var queue = tree.Queue;

            int[] odd = new int[n];
            int[] even = new int[n];
            int[] ieven = new int[n];
            int[] iodd = new int[n];
            int[] totals = new int[n];
            int[] offBulbs = new int[n];

            for (int i = treeSize - 1; i >= 0; i--)
            {
                var u = queue[i];
                var bulbOn = lit[u];

                var p = tree.Parents[u];

                offBulbs[u] = bulbOn ? 0 : 1;
                totals[u] = 1;
                int minEven = int.MaxValue;
                int minOdd = int.MaxValue;
                foreach (var v in g[u])
                {
                    if (v == p) continue;
                    offBulbs[u] += offBulbs[v];
                    totals[u] += totals[v];
                    minEven = Min(minEven, even[v]);
                    minOdd = Min(minOdd, odd[v]);
                }

                even[u] = Min(minEven, (offBulbs[u] & 1) == 0 ? totals[u] : int.MaxValue);
                odd[u] = Min(minOdd, (offBulbs[u] & 1) != 0 ? totals[u] : int.MaxValue);
            }

            ieven[queue[0]] = 0;
            iodd[queue[0]] = int.MaxValue;

            for (int i = 0; i < treeSize; i++)
            {
                var u = queue[i];
                int minOdd1 = iodd[u];
                int minOdd2 = int.MaxValue;

                var p = tree.Parents[u];
                
                foreach (var v in g[u])
                {
                    if (v == p) continue;
                    if (odd[v] < minOdd2)
                    {
                        minOdd2 = odd[v];
                        if (minOdd2 < minOdd1) Swap(ref minOdd2, ref minOdd1);
                    }
                }

                foreach (var v in g[u])
                {
                    if (v == p) continue;

                    var alt = (offBulbs[0] - offBulbs[v] & 1) == 1 
                        ? totals[0] - totals[v]
                        : int.MaxValue;

                    iodd[v] = Min(alt, odd[v] != minOdd1 ? minOdd1 : minOdd2);
                }
            }


            for (int u = 0; u < n; u++)
            {
                var answer = (long)0;

                if (leafWithBulbOff != -1 && lit[u])
                {
                    WriteLine((n - 1));
                    continue;
                }

                var p = tree.Parents[u];
                foreach (var v in g[u])
                {
                    long subtract = v == p ? iodd[u] : odd[v];

                    var newAnswer = 0L + n - subtract;
                    if (answer < newAnswer) answer = newAnswer;
                }

                WriteLine(answer);
            }
        }
    }

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


    }

    public static List<int>[] NewGraph(int n)
    {
        var g = new List<int>[n];
        for (int i = 0; i < n; i++)
            g[i] = new List<int>();
        return g;
    }
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
        WriteLine((object)string.Format(format, args));
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

