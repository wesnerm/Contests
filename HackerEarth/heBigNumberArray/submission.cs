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
using STType = System.Int64;
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


    const int MAX = 100002;
    public void Solve()
    {
        int tests = Ni();

        var queries = new List<int[]>();
        var rand = new Random();
        var values = new int[MAX*4];
        var hashes = new long[MAX*4];

        for (int i = 0; i < hashes.Length; i++)
            hashes[i] = ((long) rand.Next()) << 31 | rand.Next();

        var sum = 0L;
        for (int i = 0; i < hashes.Length; i++)
        {
            var tmp = hashes[i];
            hashes[i] = sum;
            sum ^= tmp;
        }

        //var st = new LazySegmentTree(MAX);
        var st = new LazyXorSegmentTree(MAX);
        for (int t = 1; t <= tests; t++)
        {
            int n = Ni(), qc = Ni();

            st.CoverInclusive(0,Max(n,MAX-1),0);

            queries.Clear();

            var pos = 0;

            for (int q = 0; q < qc; q++)
            {
                int c = Ni();
                var array = c == 1 ? Ni(4) : Ni(2);
                queries.Add(array);
                if (c == 1)
                {
                    values[pos++] = array[2];
                    values[pos++] = array[2] + 1;
                    values[pos++] = array[3];
                    values[pos++] = array[3] + 1;
                }
            }

            Sort(values, 0, pos);

            int pos2 = pos;
            pos = 0;
            for (int i = 0; i < pos2; i++)
            {
                if (i == 0 || values[i] != values[i - 1])
                    values[pos++] = values[i];
            }
            
            foreach (var array in queries)
            {
                var x = array[0];
                var y = array[1];
                if (array.Length == 4)
                {
                    var lb = Bound(values, array[2], 0, pos - 1, false);
                    var rb = Bound(values, array[3], 0, pos - 1, true);
                    st.AddInclusive(x, y, hashes[lb] ^ hashes[rb]);
                }
                else
                {
                    bool good = st[x] == st[y];
                    WriteLine(good ? "YES" : "NO");
                }
            }
        }
    }

    static Random rand = new Random();
    public long Rand()
    {
        return  ((long)rand.Next()) << 31 ^ rand.Next();
    }


    [DebuggerDisplay("Length = {Length}")]
    public class LazySegmentTree
    {
        #region Variables
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Node Root;
        public int Length;
        #endregion

        #region LazySegmentTree

        public LazySegmentTree(int length)
        {
            Root = new Node(Max(1, length));
            Length = length;
        }

        public LazySegmentTree(STType[] array, int start = 0, int length = int.MaxValue)
        {
            if (length > array.Length - start)
                length = array.Length - start;
            Root = new Node(array, start, length);
            Length = length;
        }

        public LazySegmentTree Clone()
        {
            var clone = (LazySegmentTree)MemberwiseClone();
            clone.Root = clone.Root.Clone();
            return clone;
        }
        #endregion

        #region Properties
        public STType this[int index]
        {
            get { return (STType)Root.Query(index, 1); }
            set { Root = Root.Cover(index, 1, value); }
        }

        public STType[] Table
        {
            get
            {
                var result = new STType[Length];
                Root.FillTable(result);
                return result;
            }
        }
        #endregion

        #region Methods

        public void Add(int start, int length, STType v) => Root = Root.Add(start, length, v);

        public void AddInclusive(int start, int end, STType v) => Root = Root.Add(start, end - start + 1, v);

        public void CoverInclusive(int start, int end, STType v) => Root = Root.Cover(start, end - start + 1, v);

        public STType GetSum(int start, int length) => Root.Query(start, length);

        public void GetSumInclusive(int start, int end, STType v) => Root.Query(start, end - start + 1);


        #endregion

        public class Node
        {
            #region Variables

            public STType Sum;
            public STType LazyValue;
            public Node Left;
            public Node Right;
            public byte Covering;
            public bool ReadOnly;
            public int Length;


#if DEBUG
            static int counter;
            int ID = counter++;
            public string Dump()
            {
                var sb = new StringBuilder();
                Dump(sb, " ");
                return sb.ToString();
            }

            void Dump(StringBuilder sb, string indent, int start = 0)
            {
                var readOnly = ReadOnly ? " readonly" : "";
                var cover = Covering > 0 ? " " + LazyValue : "";
                Debug.WriteLine($"{indent} #{ID} {start}:{start + Length} ={Sum} {cover}{readOnly}");
                Left?.Dump(sb, indent + " ", start);
                Right?.Dump(sb, indent + " ", start + Left.Length);
            }
#endif

            #endregion

            #region Constructor

            public Node(int length, int defaultValue = 0)
            {
                Length = length;
                if (length >= 2)
                {
                    int half = length + 1 >> 1;
                    Left = new Node(half);
                    Right = (length & 1) == 0 ? Left.Clone() : new Node(length - half);
                    UpdateNode();
                }
                else
                {
                    InitSingleton(defaultValue);
                }
            }

            public Node(STType[] array)
                : this(array, 0, array.Length)
            {
            }

            public Node(STType[] array, int start, int length)
            {
                Length = length;
                if (length >= 2)
                {
                    int half = (length + 1 >> 1);
                    Left = new Node(array, start, half);
                    Right = new Node(array, start + half, length - half);
                    UpdateNode();
                }
                else
                {
                    InitSingleton(array[start]);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void InitSingleton(STType v)
            {
                Sum = v;
            }

            Node(Node left, Node right)
            {
                Length = left.Length + right.Length;
                Left = left;
                Right = right;
                UpdateNode();
            }

            public Node Clone()
            {
                if (!ReadOnly)
                {
                    LazyPropagate(); // Propagate before setting readonly for O(1)
                    ReadOnly = true;
                    Covering |= 2;
                }
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node MutableNode()
            {
                if (!ReadOnly) return this;
                var node = (Node)MemberwiseClone();
                node.ReadOnly = false;
#if DEBUG
                node.ID = counter++;
#endif
                return node;
            }

            #endregion

            #region Core Operations

            const int DefaultValue = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static STType Combine(STType left, STType right) => left ^ right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static STType CombineLength(STType v, STType length) => (length & 1)==1 ? v : 0;

            public STType Query(int start, int count)
            {
                int end = start + count;
                if (start <= 0 && Length <= end)
                    return Sum;
                if (start >= Length || end <= 0)
                    return DefaultValue;

                LazyPropagate();
                var left = Left.Query(start, count);
                var right = Right.Query(start - Left.Length, count);
                return Combine(left, right);
            }

            public Node Add(int start, int count, STType value)
            {
                int end = start + count;
                if (start >= Length || end <= 0)
                    return this;

                if (start <= 0 && Length <= end)
                    return Add(value);

                LazyPropagate();
                return UpdateNode(
                    Left.Add(start, count, value),
                    Right.Add(start - Left.Length, count, value));
            }

            public Node Cover(int start, int count, STType value)
            {
                int end = start + count;
                if (start >= Length || end <= 0)
                    return this;

                if (start <= 0 && Length <= end)
                    return Cover(value);

                LazyPropagate();
                return UpdateNode(
                    Left.Cover(start, count, value),
                    Right.Cover(start - Left.Length, count, value));
            }


            Node Add(STType value)
            {
                var node = MutableNode();
                node.LazyValue = (STType)Combine(node.LazyValue, value);
                node.Sum = Combine(node.Sum,
                    CombineLength(value, node.Length));


                return node;
            }

            Node Cover(STType value)
            {
                var node = this;

                if (ReadOnly)
                {
                    if ((Covering & 1) != 0 && LazyValue == value) return this;
                    node = MutableNode();
                }

                node.LazyValue = value;
                node.Sum = CombineLength(value, Length);
                node.Covering |= 1;

                return node;
            }

            void LazyPropagate()
            {
                if (Length <= 1)
                    return;

                var value = (STType)LazyValue;
                if (Covering != 0)
                {
                    if ((Covering & 1) != 0)
                    {
                        Left = Left.Cover(value);
                        Right = !ReadOnly || Left.Length != Right.Length
                            ? Right = Right.Cover((STType)value)
                            : Left.Clone();
                    }
                    LazyValue = DefaultValue;
                    Covering = 0;
                    if (ReadOnly)
                    {
                        Left = Left.Clone();
                        Right = Right.Clone();
                    }
                }
                else if (LazyValue != DefaultValue)
                {
                    var left = Left;
                    Left = left.Add(value);
                    Right = left != Right ? Right.Add(value) : Left.Clone();
                    LazyValue = DefaultValue;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void UpdateNode()
            {
                var left = Left;
                var right = Right;

                Sum = Combine(left.Sum, right.Sum);

            }

            #endregion

            #region Other Operations


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            Node UpdateNode(Node left, Node right)
            {
                if (ReadOnly)
                    return new Node(left, right);
                Left = left;
                Right = right;
                UpdateNode();
                return this;
            }

            #endregion

            #region Misc

            public override string ToString() => $"Sum={Sum} Length={Length}";

            public void FillTable(STType[] table, int start = 0)
            {
                if (Length == 1)
                {
                    table[start] = Sum;
                    return;
                }
                LazyPropagate();
                Left.FillTable(table, start);
                var rightStart = start + Left.Length;
                if (rightStart < table.Length) Right.FillTable(table, rightStart);
            }

            #endregion


        }

        #region Debug
        // Optimized setoperations -- union, set, delete
#if DEBUG
        public DebugNode DebugRoot => new DebugNode(Root, 0);

        [DebuggerDisplay("{Start}..{End} ({Root.Length}) -> {Root}")]
        public struct DebugNode
        {
            Node Root;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public int Start;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public int End => Start + (Root?.Length ?? 1) - 1;

            public DebugNode Left => new DebugNode(Root?.Left, Start);
            public DebugNode Right => new DebugNode(Root?.Right, Min(End, (Start + End >> 1) + 1));

            public DebugNode(Node node, int start)
            {
                Start = start;
                Root = node;
            }
        }
#endif
        #endregion
    }

    public class LazyXorSegmentTree
    {
        public long Xor = 0;
        public long LazyXor = 0;
        public int Start;
        public int End;
        public LazyXorSegmentTree Left;
        public LazyXorSegmentTree Right;
        public bool Covering;

        public LazyXorSegmentTree(int n)
            : this(null, 0, n - 1)
        {
        }


        public LazyXorSegmentTree(long[] array)
            : this(array, 0, array.Length - 1)
        {
        }

        LazyXorSegmentTree(long[] array, int start, int end)
        {
            Start = start;
            End = end;

            if (end > start)
            {
                int mid = (start + end) >> 1;
                Left = new LazyXorSegmentTree(array, start, mid);
                Right = new LazyXorSegmentTree(array, mid + 1, end);
                UpdateNode();
            }
            else
            {
                var v = array?[start] ?? 0;
                Xor = v;
            }
        }

        public long this[int index] => QueryInclusive(index, index);
        public int Length => End - Start + 1;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public long[] Table
        {
            get
            {
                var result = new long[Length];
                FillTable(result);
                return result;
            }
        }

        public void FillTable(long[] table)
        {
            if (Start == End)
            {
                table[Start] = Xor;
                return;
            }
            LazyPropagate();
            Left.FillTable(table);
            if (Right.Start < table.Length) Right.FillTable(table);
        }


        public long QueryInclusive(int start, int end)
        {
            if (Start >= start && End <= end)
                return Xor;
            if (start > End || end < Start)
                return 0;

            LazyPropagate();
            return (Left.QueryInclusive(start, end) ^ Right.QueryInclusive(start, end));
        }


        public void AddInclusive(int start, int end, long value)
        {
            if (start > End || end < Start)
                return;

            if (Start >= start && End <= end)
            {
                Add(value);
                return;
            }

            LazyPropagate();
            Left.AddInclusive(start, end, value);
            Right.AddInclusive(start, end, value);
            UpdateNode();
        }

        void Add(long value)
        {
            Xor = value ^ Xor;
            LazyXor = LazyXor ^ value;
        }

        void LazyPropagate()
        {
            if (Start == End)
                return;

            if (Covering)
            {
                Left.Cover(Xor);
                Right.Cover(Xor);
                LazyXor = 0;
                Covering = false;
            }

            if (LazyXor != 0 )
            {
                var value = LazyXor;
                LazyXor = 0;
                Left.Add(value);
                Right.Add(value);
            }
        }

        void UpdateNode()
        {
            var left = Left;
            var right = Right;
            Xor = left.Xor ^ right.Xor;
        }

        public void CoverInclusive(int start, int end, long value)
        {
            if (start > End || end < Start)
                return;

            if (Start >= start && End <= end)
            {
                Cover(value);
                return;
            }

            LazyPropagate();
            Left.CoverInclusive(start, end, value);
            Right.CoverInclusive(start, end, value);
            UpdateNode();
        }

        void Cover(long value)
        {
            Xor = value;
            LazyXor = 0;
            Covering = true;
        }
    }


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

    static int Bound(int[] array, int value, int left, int right, bool upper = false)
    {
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

        new Solution().Solve();
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
