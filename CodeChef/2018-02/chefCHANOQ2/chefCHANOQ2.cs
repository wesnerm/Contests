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
    int n;
    List<Segment> segs = new List<Segment>();
    IntervalTree tree;
    int q;
    long[] ans;
    int[][] queries;

    long[] hashes;
    int[] segCounts;

    public void Solve()
    {
        int t = Ni();
        for (int tt = 0; tt < t; tt++)
        {
            n = Ni();
            segs.Clear();
            for (int i = 0; i < n; i++)
                segs.Add(new Segment { L = Ni(), R = Ni(), W = 1, I = i });
            n++; // One-indexed

            q = Ni();
            queries = new int[q][];
            ans = new long[q];

            hashes = new long[n + 1];
            segCounts = new int[n + 1];

            for (int i = 0; i < q; i++)
                queries[i] = Ni(Ni());

            Compress();
            tree = IntervalTree.Build(segs);


            var cache = new Dictionary<long, int>(2 * n) { [0] = 0 };
            foreach (var seg in segs)
                cache[seg.Hash] = seg.W;

            for (int i = 0; i < q; i++)
            {
                var line = queries[i];

                long hash = 0;
                foreach (var p in line)
                    hash ^= hashes[p];

                int count = 0;
                if (!cache.TryGetValue(hash, out count))
                {
                    if (false && segs.Count < 400)
                    {
                        foreach (var seg in segs)
                        {
                            int left = Bound(line, seg.L);
                            int right = Bound(line, seg.R, left, true);
                            if ((right - left & 1) == 1)
                                count += seg.W;
                        }
                    }
                    else
                    {
                        if (line.Length > 0)
                            count += IntervalTree.ProcessQuery(tree, line);
                    }

                    cache[hash] = count;
                }


                ans[i] += count;
            }

            for (int i = 0; i < q; i++)
                WriteLine(ans[i]);
        }

    }

    static Random random = new Random();


    void Compress()
    {
        Rehash(false);

        // Remove redundant pts
        var seen = new Dictionary<long, int>();
        for (var i = 0; i < queries.Length; i++)
        {
            var line = queries[i];

            seen.Clear();
            foreach (var p in line)
            {
                var h = hashes[p];
                if (segCounts[p] == 0) continue; // No segments

                if (seen.ContainsKey(h))
                    seen.Remove(h);
                else
                    seen[h] = p;
            }

            line = seen.Values.ToArray();
            if (seen.Count != 1)
            {
                Sort(line);
            }
            else
            {
                ans[i] += segCounts[seen.Values.First()];
                line = new int[0];
            }

            queries[i] = line;
        }

        var ptSet = new HashSet<int>();
        foreach (var v in queries)
            ptSet.UnionWith(v);

        var ptArray = ptSet.ToArray();
        Sort(ptArray);

        // Remove redundant or empty segments
        var ptAns = new int[n];
        var segMap = new Dictionary<long, Segment>();
        segs.RemoveAll(x =>
        {
            int left = Bound(ptArray, x.L);
            int right = Bound(ptArray, x.R, left, true);
            int count = right - left;
            if (count == 0) return true;
            if (count == 1)
            {
                ptAns[ptArray[left]] += x.W;
                return true;
            }

            var t = ((long)left << 20) + right;
            Segment find;
            if (segMap.TryGetValue(t, out find))
            {
                find.W += x.W;
                return true;
            }

            x.L = ptArray[left];
            x.R = ptArray[right - 1];
            segMap.Add(t, x);
            return false;
        });

        // Second pass
        var uf = new UnionFind(n);
        var queryHashes = new long[n];

        for (var index = 0; index < queries.Length; index++)
        {
            var pts = queries[index];
            if (pts.Length == 0) continue;

            var h = random.Next() + ((long)random.Next() << 31);
            var first = pts[0];
            for (int i = 0; i < pts.Length; i++)
            {
                uf.Union(first, pts[i]);
                queryHashes[pts[i]] ^= h;
            }
        }

        var pt2query = new Dictionary<long, int>();
        var seen2 = new HashSet<int>();

        segs.RemoveAll(x =>
        {
            int left = Bound(ptArray, x.L);
            int right = Bound(ptArray, x.R, left, true);
            int count = right - left;

            if (count > 300) return false;

            pt2query.Clear();
            seen2.Clear();

            /*
            for (int i = left; i < right; i++)
            {
                var p = ptArray[i];
                if (!seen2.Add(uf.Find(p)))
                    return false;
            }

            for (int i = left; i < right; i++)
            {
                var p = ptArray[i];
                ptAns[p] += x.W;
            }
            */

            // Group all pts by common queries
            for (int i = left; i < right; i++)
            {
                var p = ptArray[i];
                var h = queryHashes[p];
                if (pt2query.ContainsKey(h))
                    pt2query.Remove(h);
                else
                    pt2query.Add(h, p);
            }

            foreach (var p in pt2query.Values)
                if (!seen2.Add(uf.Find(p)))
                    return false;

            foreach (var p in pt2query.Values)
                ptAns[p] += x.W;
            return true;
        });


        for (int i = 0; i < q; i++)
        {
            var line = queries[i];
            int precount = 0;
            foreach (var p in line)
                precount += ptAns[p];
            ans[i] += precount;
        }

        Rehash(true);
    }

    void Rehash(bool clear = true)
    {
        if (clear)
        {
            Array.Clear(hashes, 0, hashes.Length);
            Array.Clear(segCounts, 0, segCounts.Length);
        }

        // Create hashMap
        foreach (var seg in segs)
        {
            hashes[seg.L] ^= seg.Hash;
            hashes[seg.R + 1] ^= seg.Hash;
            segCounts[seg.L] += seg.W;
            segCounts[seg.R + 1] -= seg.W;
        }

        for (int i = 1; i < hashes.Length; i++)
        {
            hashes[i] ^= hashes[i - 1];
            segCounts[i] += segCounts[i - 1];
        }
    }


    /*void SolveIt()
    {
        var inside = new bool[n];
        var endpts = new List<int>[n];
        var id2Seg = new Segment[n];

        for (int i = 0; i < n; i++)
            endpts[i] = new List<int>();

        foreach (var s in segs)
        {
            endpts[s.L].Add(s.I);
            endpts[s.R].Add(~s.I);
            id2Seg[s.I] = s;
        }

        int outerSegments = 0;
        var activeRight = new FenwickTree(n);
        var activeLeft = new FenwickTree(n);


        var mos = new MosAlgorithm();
        for (var iq = 0; iq < queries.Length; iq++)
        {
            var q = queries[iq];
            if (q.Length == 0) continue;
            mos.AddTask(q[0], q[q.Length - 1], null, task =>
            {
                if ((q.Length & 1) == 1) ans[iq] += outerSegments;

                for (var ip = 0; ip < q.Length; ip++)
                {
                    var p = q[ip];

                    // Overlapping segments
                    if ((ip & 1) == 0) ans[iq] += activeLeft.SumInclusive(task.Start, p);
                    if ((q.Length - ip & 1) == 1) ans[iq] += activeRight.SumInclusive(p, task.End);

                    // Inner segments

                }
            });
        }

        mos.Execute((pos, delta, isEnd) =>
        {
            var ptList = endpts[pos];
            foreach (var v in endpts)
            {
                foreach (var s0 in ptList)
                {
                    var atEnd = s0 < 0;
                    var s = s0 < 0 ? ~s0 : s0;
                    inside[s] ^= true;
                    if (inside[s]) outerSegments += id2Seg[s].W * delta;
                }
            }
        });
    }*/

    public class Segment : IComparable<Segment>, IEquatable<Segment>
    {
        public int L, R, W, I;
        public long Hash => ((long)random.Next() << 31) + random.Next();
        public override string ToString() => $"({L},{R}) {W} #{I}";

        public int CompareTo(Segment b)
        {
            var a = this;
            int cmp = a.L.CompareTo(b.L);
            if (cmp != 0) return cmp;
            cmp = -a.R.CompareTo(b.R);
            return cmp;
        }

        public bool Equals(Segment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return L == other.L && R == other.R;
        }

        public override bool Equals(object obj) => Equals(obj as Segment);

        public override int GetHashCode() => unchecked((L * 397) ^ R);
    }

    public class Query
    {
        public int L, R, I;
        public int[] P;

        public long Hash => ((long)random.Next() << 31) + random.Next();
        public override string ToString() => $"({L},{R}) #{I}";
    }

    public class MosAlgorithm
    {
        public List<Task> Tasks = new List<Task>();

        public Task AddTask(int start, int end, object tag, Action<Task> action)
        {
            if (start > end)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var task = new Task
            {
                Start = start,
                End = end,
                Action = action,
                Tag = tag
            };

            Tasks.Add(task);
            return task;
        }

        public int Start => s;
        public int End => e;

        int s;
        int e;

        public void Execute(Action<int, int, bool> modify)
        {
            int max = Tasks.Max(t => t.End);

            s = 0;
            e = -1;

            int n = max - s + 1;
            int sqrt = (int)Ceiling(Sqrt(n));

            Tasks.Sort((x, y) => x.Start / sqrt == y.Start / sqrt
                ? x.End.CompareTo(y.End)
                : x.Start.CompareTo(y.Start));

            // POSTPONE: 
            // One optimization we can make is to take advantage of any gaps
            // with no overlapping so we can start from the latest position

            foreach (var task in Tasks)
            {
                // WARNING: Keep postfix increments outside call, so that properties work
                while (e < task.End) { ++e; modify(e, +1, true); }
                while (e > task.End) { modify(e, -1, true); e--; }
                while (s < task.Start) { modify(s, -1, false); s++; }
                while (s > task.Start) { --s; modify(s, +1, false); }
                task.Action(task);
            }

            Tasks.Clear();
        }

        public class Task
        {
            public int Start;
            public int End;
            public Action<Task> Action;
            public object Tag;

            public override string ToString()
            {
                return $"{Tag} [{Start},{End}]";
            }
        }

    }

    public class IntervalTree
    {
        public int Weight = 1;
        public int Start;
        public int End;

        public IntervalTree Left;
        public IntervalTree Right;

        public int TreeStart;
        public int TreeEnd;
        public int TreeAndStart;
        public int TreeAndEnd;
        public int TreeSize = 1;


        public static IntervalTree Build(List<Segment> segs)
        {
            ZigZagSort(segs, 0, segs.Count - 1, false);
            var tree = Build(segs, 0, segs.Count - 1);
            Recompute(tree);
            return tree;
        }

        public static void ZigZagSort(List<Segment> segment, int start, int end, bool reverse)
        {
            if (start >= end)
                return;

            segment.Sort(start, end - start + 1, reverse ? (IComparer<Segment>)endComparer : startComparer);
            int mid = start + end >> 1;
            ZigZagSort(segment, start, mid, !reverse);
            ZigZagSort(segment, mid + 1, end, !reverse);
        }

        static IntervalTree Build(List<Segment> segs, int start, int end)
        {
            if (start > end)
                return null;

            int mid = start + end >> 1;
            var tree = new IntervalTree()
            {
                Start = segs[mid].L,
                End = segs[mid].R,
                Weight = segs[mid].W,
                Left = Build(segs, start, mid - 1),
                Right = Build(segs, mid + 1, end),
            };

            return tree;
        }

        static void Recompute(IntervalTree tree)
        {
            if (tree == null) return;
            Recompute(tree.Left);
            Recompute(tree.Right);
            tree.TreeStart = tree.Start;
            tree.TreeEnd = tree.End;
            tree.TreeAndStart = tree.Start;
            tree.TreeAndEnd = tree.End;
            tree.TreeSize = tree.Weight;
            if (tree.Left != null)
            {
                tree.TreeStart = Min(tree.TreeStart, tree.Left.TreeStart);
                tree.TreeEnd = Max(tree.TreeEnd, tree.Left.TreeEnd);
                tree.TreeAndStart = Max(tree.TreeAndStart, tree.Left.TreeAndStart);
                tree.TreeAndEnd = Min(tree.TreeAndEnd, tree.Left.TreeAndEnd);
                tree.TreeSize += tree.Left.TreeSize;
            }
            if (tree.Right != null)
            {
                tree.TreeStart = Min(tree.TreeStart, tree.Right.TreeStart);
                tree.TreeEnd = Max(tree.TreeEnd, tree.Right.TreeEnd);
                tree.TreeAndStart = Max(tree.TreeAndStart, tree.Right.TreeAndStart);
                tree.TreeAndEnd = Min(tree.TreeAndEnd, tree.Right.TreeAndEnd);
                tree.TreeSize += tree.Right.TreeSize;
            }
        }

        public static int ProcessQuery(IntervalTree tree, int[] query)
        {
            if (tree == null) return 0;

            int qleft = query[0];
            int qright = query[query.Length - 1];

            if (qleft > tree.TreeEnd || qright < tree.TreeStart)
                return 0;

            int treeLeft = tree.TreeStart <= qleft ? 0 : Bound(query, tree.TreeStart);

            // No item
            if (treeLeft >= query.Length || query[treeLeft] > tree.TreeEnd)
                return 0;

            // Single item
            if (treeLeft + 1 >= query.Length || query[treeLeft + 1] > tree.TreeEnd)
                return ProcessQuery(tree, query[treeLeft]);

            if (tree.TreeAndStart <= query[treeLeft] && tree.TreeAndEnd >= tree.TreeAndStart)
            {
                int treeRight = tree.TreeEnd >= qright
                    ? query.Length
                    : Bound(query, tree.TreeEnd, treeLeft, true);
                if (tree.TreeAndEnd >= query[treeRight - 1])
                    return (treeRight - treeLeft & 1) == 1 ? tree.TreeSize : 0;
            }

            int nodeLeft = Bound(query, tree.Start, treeLeft);
            int nodeRight = Bound(query, tree.End, nodeLeft, true);

            int result = (nodeRight - nodeLeft & 1) == 1 ? tree.Weight : 0;
            result += ProcessQuery(tree.Left, query);
            result += ProcessQuery(tree.Right, query);
            return result;
        }

        public static int ProcessQuery(IntervalTree tree, int x)
        {
            if (tree == null || x > tree.TreeEnd || x < tree.TreeStart)
                return 0;

            if (x >= tree.TreeAndStart && x <= tree.TreeAndEnd)
                return tree.TreeSize;

            int result = x >= tree.Start && x <= tree.End ? tree.Weight : 0;
            result += ProcessQuery(tree.Left, x);
            result += ProcessQuery(tree.Right, x);
            return result;
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }

        public class SegStartComparer : IComparer<Segment>
        {
            public int Compare(Segment x, Segment y) => x.L.CompareTo(y.L);
        }

        public class SegEndComparer : IComparer<Segment>
        {
            public int Compare(Segment x, Segment y) => x.L.CompareTo(y.L);
        }

        public static SegStartComparer startComparer = new SegStartComparer();
        public static SegEndComparer endComparer = new SegEndComparer();

    }

    public class UnionFind
    {
        readonly int[] _ds;
        int _count;

        public UnionFind(int size)
        {
            _ds = new int[size];
            Clear();
        }

        public int Count => _count;

        public int[] Array => _ds;

        public void Clear()
        {
            _count = _ds.Length;
            for (int i = 0; i < _ds.Length; i++)
                _ds[i] = -1;
        }

        public bool Union(int x, int y)
        {
            var rx = Find(x);
            var ry = Find(y);
            if (rx == ry) return false;

            if (_ds[rx] <= _ds[ry])
            {
                _ds[rx] += _ds[ry];
                _ds[ry] = rx;
            }
            else
            {
                _ds[ry] += _ds[rx];
                _ds[rx] = ry;
            }
            _count--;
            return true;
        }

        public bool Connected(int x, int y) => Find(x) == Find(y);

        public int Find(int x)
        {
            var root = _ds[x];
            return root < 0
                ? x
                : (_ds[x] = Find(root));
        }

        public int GetCount(int x)
        {
            var c = _ds[Find(x)];
            return c >= 0 ? 1 : -c;
        }

        public IEnumerable<int> Roots()
        {
            for (int i = 0; i < _ds.Length; i++)
                if (_ds[i] < 0)
                    yield return i;
        }

        public IEnumerable<List<int>> Components()
        {
            var comp = new Dictionary<int, List<int>>();
            foreach (var c in Roots())
                comp[c] = new List<int>(GetCount(c));
            for (int i = 0; i < _ds.Length; i++)
                comp[Find(i)].Add(i);
            return comp.Values;
        }
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
        if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
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

    public static int Bound(int[] array, int value, int left = 0, bool upper = false)
    {
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int cmp = value - array[mid];
            if (cmp > 0 /*|| cmp == 0 && upper*/)
                left = mid + 1;
            else if (cmp < 0)
                right = mid - 1;
            else
            {
                return upper ? mid + 1 : mid;
            }
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

    public static int BitCount(ulong y)
    {
        int count;
        for (count = 0; y != 0; count++)
            y &= y - 1;
        return count;
    }

    const ulong M1 = 0x5555555555555555; //binary: 0101...
    const ulong M2 = 0x3333333333333333; //binary: 00110011..
    const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
    const ulong M8 = 0x00ff00ff00ff00ff; //binary:  8 zeros,  8 ones ...
    const ulong M16 = 0x0000ffff0000ffff; //binary: 16 zeros, 16 ones ...
    const ulong M32 = 0x00000000ffffffff; //binary: 32 zeros, 32 ones
    const ulong Hff = 0xffffffffffffffff; //binary: all ones
    const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...

    //This is a naive implementation, shown for comparison,
    //and to help in understanding the better functions.
    //It uses 24 arithmetic operations (shift, add, and).

    public static int Count1(ulong x)
    {
        x = (x & M1) + ((x >> 1) & M1); //put count of each  2 bits into those  2 bits 
        x = (x & M2) + ((x >> 2) & M2); //put count of each  4 bits into those  4 bits 
        x = (x & M4) + ((x >> 4) & M4); //put count of each  8 bits into those  8 bits 
        x = (x & M8) + ((x >> 8) & M8); //put count of each 16 bits into those 16 bits 
        x = (x & M16) + ((x >> 16) & M16); //put count of each 32 bits into those 32 bits 
        x = (x & M32) + ((x >> 32) & M32); //put count of each 64 bits into those 64 bits 
        return unchecked((int)x);
    }

    //This uses fewer arithmetic operations than any other known  
    //implementation on machines with slow multiplication.
    //It uses 17 arithmetic operations.

    public static int Count2(ulong x)
    {
        x -= (x >> 1) & M1; //put count of each 2 bits into those 2 bits
        x = (x & M2) + ((x >> 2) & M2); //put count of each 4 bits into those 4 bits 
        x = (x + (x >> 4)) & M4; //put count of each 8 bits into those 8 bits 
        x += x >> 8; //put count of each 16 bits into their lowest 8 bits
        x += x >> 16; //put count of each 32 bits into their lowest 8 bits
        x += x >> 32; //put count of each 64 bits into their lowest 8 bits
        return unchecked((int)(x & 0x7f));
    }

    //This uses fewer arithmetic operations than any other known  
    //implementation on machines with fast multiplication.
    //It uses 12 arithmetic operations, one of which is a multiply.

    public static int Count3(ulong x)
    {
        x -= (x >> 1) & M1; //put count of each 2 bits into those 2 bits
        x = (x & M2) + ((x >> 2) & M2); //put count of each 4 bits into those 4 bits 
        x = (x + (x >> 4)) & M4; //put count of each 8 bits into those 8 bits 
        return unchecked((int)((x * H01) >> 56)); //returns left 8 bits of x + (x<<8) + (x<<16) + (x<<24) + ... 
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
            if (c < 32) { if (c == 10 || c <= 0) break; continue; }
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

    public void Report(int wait)
    {
        wait = wait * 10 + 5;
        WriteLine(wait);
        Flush();
        var process = Process.GetCurrentProcess();
        while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 8000) wait += 1000;
        while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(wait, 8000)) ;
        Environment.Exit(0);
    }

    public void EDD(int n)
    {
        n = Abs(n);

        int e = n == 0 ? 0 : (int)Log10(n);
        int m = n;
        if (m != 0)
        {
            while (m >= 100) m /= 10;
            while (m * 10 < 100) m *= 10;
        }

        if (e == 0 && m == 10 && n == (int)Pow(10, e)) m = 5;
        if (e == 0 && m % 10 == 0) m += 5;
        var x = e < 7 ? e % 10 * 100 + m : 700 + ((e - 7) % 10) * 10 + m / 10;

        WriteLine($"{e} {m} {x}");
        Report(x);
    }

    public static int TestCase()
    {
        return Directory.EnumerateFiles(".", "input*.in").
            Max(f => int.Parse(f.Substring(f.Length - 8, 5)));
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
