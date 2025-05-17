#region Usings
#define USEMINMAX
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
    const long Big = long.MaxValue >> 20;

    long RaceAgainstTime(int mason_height)
    {
        var hashset = new HashSet<int>();
        var hashset2 = new HashSet<int>();
        var costs = new long[n];
        var costs2 = new long[n];

        hashset.Add(mason_height);

        for (int i = 0; i < n - 1; i++)
        {
            var newHeight = heights[i];
            var exchangePrice = prices[i];
            foreach (var h in hashset)
            {
                var cost = costs[h];

                if (hashset2.Add(newHeight)) costs2[newHeight] = Big;
                costs2[newHeight] = Min(cost
                                          + Abs(heightValues[h] - heightValues[newHeight])
                                          + exchangePrice,
                    costs2[newHeight]);

                if (h >= newHeight)
                {
                    if (hashset2.Add(h)) costs2[h] = Big;
                    costs2[h] = Min(cost, costs2[h]);

                    // Doesm't work because height differences can affect costs
                    //if (costs2[h] <= costs2[newHeight])
                    //    hashset2.Remove(newHeight);
                }
            }

            Swap(ref hashset, ref hashset2);
            Swap(ref costs, ref costs2);
            hashset2.Clear();
        }

        long mincost = hashset.Min(x => costs[x]);
        return mincost;
    }

    long RaceAgainstTime2(int mason_height)
    {
        int m = heightValues.Length;
        var costsUp = new LazySegmentTree(m);
        costsUp.CoverInclusive(0, m, Big);

        var costsDown = /* new LazySegmentTree(m); */ costsUp.Clone();

        costsUp[mason_height] = heightValues[mason_height];
        costsDown[mason_height] = -heightValues[mason_height];

        //Console.Error.WriteLine("CostUp: " + string.Join(" ", costsUp.Table.Select((x, j) => x - heightValues[j])));

        for (int i = 0; i < n - 1; i++)
        {
            var newHeight = heights[i];
            var exchangePrice = prices[i];
            var heightValue = heightValues[newHeight];

            var costsUp2 = costsUp.Clone();
            var costsDown2 = costsDown.Clone();

            costsUp2.CoverInclusive(0, newHeight - 1, Big);
            costsDown2.CoverInclusive(0, newHeight - 1, Big);

            long costDown = costsDown.GetMinInclusive(0, newHeight); // costs[h] - heightValues[h]
            long costUp = costsUp.GetMinInclusive(newHeight + 1, m - 1); // costs[h] + heightValues[h]

            var cost = costsUp[newHeight] - heightValue;
            cost = Min(cost, Min(costUp - heightValue, costDown + heightValue) + exchangePrice);
            costsUp2[newHeight] = cost + heightValue;
            costsDown2[newHeight] = cost - heightValue;

            costsUp = costsUp2;
            costsDown = costsDown2;

            //Console.Error.WriteLine("CostUp: " + string.Join(" ", costsUp.Table.Select((x, j) => x - heightValues[j])));
        }

        long minCost = long.MaxValue;
        var table = costsUp.Table;
        for (int i = 0; i < m; i++)
            minCost = Min(minCost, table[i] - heightValues[i]);

        return minCost;
    }

    long RaceAgainstTime3(int mason_height)
    {
        int m = heightValues.Length;
        var costs = new long[m];
        var costs2 = new long[m];

        for (int i = 0; i < m; i++) costs[i] = Big;
        costs[mason_height] = 0;

        //Console.Error.WriteLine("Cost: " + string.Join(" ", costs));

        for (int i = 0; i < n - 1; i++)
        {
            var newHeight = heights[i];

            for (int h = 0; h < newHeight; h++)
                costs2[h] = Big;
            for (int h = newHeight; h < m; h++)
                costs2[h] = costs[h];

            var exchangePrice = prices[i];
            var heightValue = heightValues[newHeight];

            long cost0 = Big, cost1 = Big;
            for (int h = 0; h <= newHeight; h++)
                cost0 = Min(cost0, costs[h] - heightValues[h]);
            for (int h = newHeight + 1; h < m; h++)
                cost1 = Min(cost1, costs[h] + heightValues[h]);

            costs2[newHeight] =
                Min(costs2[newHeight],
                    Min(cost0 + heightValue, cost1 - heightValue) + exchangePrice);

            Swap(ref costs, ref costs2);

            //Console.Error.WriteLine("Cost: " + string.Join(" ", costs));
        }

        return costs.Min();
    }

    public static int[] CompressX(int[] a)
    {
        int n = a.Length;
        var ret = (int[])a.Clone();
        int[] ind = new int[n];
        for (int i = 0; i < n; i++)
            ind[i] = i;

        Sort(ret, ind);

        int p = 0;
        for (int i = 0; i < n; i++)
        {
            if (i == 0 || ret[i] != ret[i - 1]) ret[p++] = ret[i];
            a[ind[i]] = p - 1;
        }

        Resize(ref ret, p);
        return ret;
    }

    int n;
    int mason_height;
    int[] heights, prices;
    int[] heightValues;

    void Solve()
    {
        n = Ni();
        mason_height = Ni();
        heights = new List<int>(Ni(n - 1)) { mason_height }.ToArray();
        prices = Ni(n - 1);
        heightValues = CompressX(heights);

        long result = RaceAgainstTime2(heights[heights.Length - 1]);
        WriteLine(result + n);
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

        public long[] Table
        {
            get
            {
                var result = new long[Length];
                Root.FillTable(result);
                return result;
            }
        }
        #endregion

        #region Methods

        public void Add(int start, int length, STType v) => Root = Root.Add(start, length, v);

        public void AddInclusive(int start, int end, STType v) => Root = Root.Add(start, end - start + 1, v);

        public void CoverInclusive(int start, int end, STType v) => Root = Root.Cover(start, end - start + 1, v);

        public long GetSum(int start, int length) => Root.Query(start, length);

        public void GetSumInclusive(int start, int end, STType v) => Root.Query(start, end - start + 1);

        public int FindGreater(STType sum) => Root.FindGreater(sum);

        public int Next(STType k) => Root.Next(k);

        public int Previous(STType k) => Root.Previous(k);

#if USEMINMAX
        public STType GetMin(int start, int length) => Root.GetMin(start, length);

        public STType GetMinInclusive(int start, int end) => Root.GetMin(start, end - start + 1);

        public STType GetMax(int start, int length) => Root.GetMax(start, length);

        public STType GetMaxInclusive(int start, int end) => Root.GetMax(start, end - start + 1);
#endif

        #endregion

        public class Node
        {
            #region Variables

            public long Sum;
            public long LazyValue;
            public Node Left;
            public Node Right;
            public byte Covering;
            public bool ReadOnly;
            public int Length;


#if USEMINMAX
            public STType Min = STType.MaxValue;
            public STType Max = STType.MinValue;
#endif

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
                var cover = Covering>0? " " + LazyValue : "";
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
#if USEMINMAX
                Min = v;
                Max = v;
#endif
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
            static long Combine(long left, long right) => left + right;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static long CombineLength(long v, long length) => v * length;

            public long Query(int start, int count)
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
                node.Sum += CombineLength(value, node.Length);

#if USEMINMAX
                node.Min = (STType)Combine(node.Min, value);
                node.Max = (STType)Combine(node.Max, value);
#endif
                return node;
            }

            Node Cover(STType value)
            {
                var node = this;

                if (ReadOnly)
                {
                    if ( (Covering&1) != 0 && LazyValue == value) return this;
                    node = MutableNode();
                }

                node.LazyValue = value;
                node.Sum = CombineLength(value, Length);
                node.Covering |= 1;
#if USEMINMAX
                node.Min = value;
                node.Max = value;
#endif
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
                            ? Right = Right.Cover((STType) value)
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
                    Debug.Assert(!ReadOnly);
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
                // Sum = left.Sum + right.Sum;

#if USEMINMAX
                Min = Min(left.Min, right.Min);
                Max = Max(left.Max, right.Max);
#endif
            }

            #endregion

            #region Other Operations

#if USEMINMAX
            public STType GetMin(int start, int count)
            {
                int end = start + count;
                if (start <= 0 && Length <= end)
                    return Min;
                if (start >= Length || end <= 0)
                    return STType.MaxValue >> 20;

                LazyPropagate();
                return Min(Left.GetMin(start, count),
                    Right.GetMin(start - Left.Length, count));
            }

            public STType GetMax(int start, int count)
            {
                int end = start + count;
                if (start <= 0 && Length <= end)
                    return Max;
                if (start >= Length || end <= 0)
                    return STType.MinValue >> 20;

                LazyPropagate();
                return Max(Left.GetMax(start, count),
                    Right.GetMax(start - Left.Length, count));
            }
#endif

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

            public void FillTable(long[] table, int start = 0)
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

            #region Search

            public int FindGreaterOrEqual(int k) => FindGreater(k - 1);

            public int FindGreater(long k, int start = 0)
            {
                if (k >= Sum) return -1;
                if (Length == 1) return start;

                LazyPropagate();
                var leftSum = Left.Sum;
                return k < leftSum
                    ? Left.FindGreater(k, start)
                    : Right.FindGreater(k - leftSum, start + Left.Length);
            }

            public int Next(STType k, int start = 0)
            {
                if (k >= start + Length - 1 || Sum <= 0) return -1;
                if (Length == 1) return start;

                LazyPropagate();
                int result = Left.Next(k);
                if (result < 0) result = Right.Next(k, start + Left.Length);
                return result;
            }

            public int Previous(STType k, int start = 0)
            {
                if (k <= start || Sum <= 0) return -1;
                if (Length == 1) return start;

                LazyPropagate();
                int result = Right.Previous(k, start + Left.Length);
                if (result < 0) result = Left.Previous(k);
                return result;
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


    #region Mod Math

    public const int MOD = 1000000007;
    const int FactCache = 1000;

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

    public static long Mult(long left, long right) =>
        (left * right) % MOD;

    public static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    public static long Add(long x, long y) =>
        (x += y) >= MOD ? x - MOD : x;

    public static long Subtract(long x, long y) => (x -= y) < 0 ? x + MOD : x;

    public static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

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

    static List<long> _fact, _ifact;

    public static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(FactCache) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    public static long InverseFact(int n)
    {
        if (_ifact == null) _ifact = new List<long>(FactCache) { 1 };
        for (int i = _ifact.Count; i <= n; i++)
            _ifact.Add(Div(_ifact[i - 1], i));
        return _ifact[n];
    }

    public static long Fact(int n, int m)
    {
        var fact = Fact(n);
        if (m < n) fact = fact * InverseFact(n - m) % MOD;
        return fact;
    }

    public static long Comb(int n, int k)
    {
        if (k <= 1)
            return k == 1 ? n :
                k == 0 ? 1 : 0;
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

    public static V Get<K, V>(Dictionary<K, V> dict, K key) where V : new()
    {
        V result;
        if (dict.TryGetValue(key, out result) == false)
            result = dict[key] = new V();
        return result;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BitCount(long x)
    {
        int count;
        var y = unchecked((ulong)x);
        for (count = 0; y != 0; count++)
            y &= y - 1;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HighestOneBit(int n) => n != 0 ? 1 << Log2(n) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long HighestOneBit(long n) => n != 0 ? 1L << Log2(n) : 0;

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

    public static int[] Ni(int n) => N(n, Ni);

    public static long[] Nl(int n) => N(n, Nl);

    public static string[] Ns(int n) => N(n, Ns);

    public static int Ni() => checked((int)Nl());

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg)
        {
            c = Read();
        }

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
            if (c < 32)
            {
                if (c == 10 || c <= 0) break;
                continue;
            }

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
        } while (number > 0);

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

    public static int TestCase()
    {
        return Directory.EnumerateFiles(".", "input*.in").Max(f => int.Parse(f.Substring(f.Length - 8, 5)));
    }

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
            while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(wait, 3000)) ;
            Environment.Exit(1);
        };

        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());
#if __MonoCS__
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
}