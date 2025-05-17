namespace HackerRank.WorldCodeSprint8.TollCostDigits
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using static System.Math;
    using static System.Console;
    using static HackerRankUtils;

    public class Solution
    {

        #region Variables
        private static Dictionary<int, Edge>[] edgelist;
        private static List<int>[] ugraph;
        private static int e;
        private static int n;
        private static int[] map;
        private static DisjointSet ds;

        private static List<int>[] components;

        #endregion

        public static void Main()
        {
            Init();

            var arr = Console.ReadLine().Split();
            n = int.Parse(arr[0]);
            e = int.Parse(arr[1]);

            edgelist = new Dictionary<int, Edge>[n + 1];
            ds = new DisjointSet(n, true);
            ugraph = new List<int>[n + 1];
            map = new int[n + 1];


            for (int i = 0; i < edgelist.Length; i++)
            {
                edgelist[i] = new Dictionary<int, Edge>();
                ugraph[i] = new List<int>();
            }

            for (int a0 = 0; a0 < e; a0++)
            {
                arr = Console.ReadLine().Split();
                int x = int.Parse(arr[0]);
                int y = int.Parse(arr[1]);
                int r = int.Parse(arr[2]);

                if (x > y)
                {
                    int tmp = x;
                    x = y;
                    y = tmp;
                    r = 1000 - r;
                }

                r = 1 << (r % 10);

                Edge edge;
                if (edgelist[x].ContainsKey(y))
                {
                    edge = edgelist[x][y];
                    edge.rates |= r;
                }
                else
                {
                    edge = new Edge { i = a0, u = x, v = y, rates = r };
                    edgelist[x][y] = edge;
                    edgelist[y][x] = edge;
                }
                ds.Union(x, y);
            }


            //
            var finalcounts = Run();
            //var finalcounts = DeleteAlgorithm();
            for (int i = 0; i < 10; i++)
                Console.WriteLine(finalcounts[i] + finalcounts[(10 - i) % 10]);
        }


        public static long[] Run()
        {
            components = ds.GetComponents().ToArray();
            //bridgeCuts = new BridgesAndCuts(n);
            //bridgeCuts.CutPointsAndBridges(ugraph, out cutpoints, out bridges);

            var finalcounts = new long[10];
            foreach (var comp in components)
            {
                if (comp.Count == 1)
                    continue;

                int[] counts;

                //if (IsTree(comp) || comp.Count>1000)
                counts = DeleteAlgorithm(comp);
                //else
                //{
                //    counts = FloydWarshall(comp);
                //}
                for (int i = 0; i < 10; i++)
                    finalcounts[i] += counts[i];
            }

            return finalcounts;
        }

        public static int FixedPoint(int cost)
        {
            return cost;
            /*
            if (cost <= 1) return cost;

            int prev;
            int next = cost;
            do
            {
                prev = next;
                next |= AddCost(next, next);
            } while (prev != next);
            return next;*/
        }


        public static int[] FloydWarshall(List<int> nodes)
        {
            int count = nodes.Count;
            var dist = new int[count, count];

            for (int i = 0; i < count; i++)
                map[nodes[i]] = i;

            foreach (var u in nodes)
            {
                foreach (var edge in edgelist[u].Values)
                {
                    edge.MakeMain(u);
                    dist[map[u], map[edge.v]] = FixedPoint(edge.rates);
                }
            }


            for (var k = 0; k < count; k++)
                for (var i = 0; i < count; i++)
                    for (var j = 0; j < count; j++)
                        dist[i, j] = FixedPoint(dist[i, j] | AddCost(dist[i, k], dist[k, j]));

            int[] dict = new int[1024];
            for (var i = 0; i < count; i++)
                for (var j = i + 1; j < count; j++)
                    dict[dist[i, j]]++;

            // Tabulate quickly
            int[] result = new int[10];
            for (int i = 0; i < 10; i++)
            {
                int b = 1 << i;
                for (int j = 0; j < dict.Length; j++)
                {
                    if ((j & b) != 0)
                        result[i] += dict[j];
                }
            }

            return result;
        }

        public static bool IsTree(List<int> nodes)
        {
            int count = 0;
            foreach (var i in nodes)
                count += edgelist[i].Count;
            return count == 2 * (nodes.Count - 1);
        }


        public static int[] DeleteAlgorithm(List<int> nodes)
        {
            int[] counts = new int[10];

            foreach (var u in nodes)
            {
                int selfcost = 1;
                Edge selfEdge;
                if (edgelist[u].TryGetValue(u, out selfEdge))
                {
                    selfcost = FixedPoint(selfcost | selfEdge.rates | selfEdge.reverseRates);
                    edgelist[u].Remove(u);
                }

                int[] repCounts = new int[10];
                int[] openCounts = new int[10];
                repCounts[0] = 1;

                while (edgelist[u].Count > 0)
                {
                    var edges = edgelist[u].Values.ToList();

                    foreach (var edge in edges)
                    {
                        edge.MakeMain(u);
                        var v = edge.v;
                        var r = edge.u == u ? edge.rates : edge.reverseRates;
                        if (v == u)
                            continue;
 
                        // Add to total counts
                        for (int c = 0; c < 10; c++)
                        {
                            int bit = 1 << c;
                            if ((r & bit) == 0) continue;
                            for (int j = 0; j < 10; j++)
                            {
                                counts[(j + c) % 10] += repCounts[j];
                            }
                        }

                        // Add to edge
                        for (int c = 0; c < 10; c++)
                        {
                            int bit = 1 << c;
                            if ((r & bit) == 0)
                                repCounts[(10 - c) % 10]++;
                        }

                        // Adjust open connections


                        var edges2 = edgelist[v];
                        if (edges2.Count == 0)
                            continue;

                        edgelist[v] = new Dictionary<int, Edge>();
                        edgelist[u].Remove(v);
                        foreach (var edge2 in edges2.Values)
                        {
                            if (edge2.u == u || edge2.v == u)
                                continue;

                            if (edge2.u == v)
                                edge2.u = u;
                            if (edge2.v == v)
                                edge2.v = u;

                            edge2.MakeMain(u);

                            edge2.rates = AddCost(r, edge2.rates);

                            var v2 = edge2.v;
                            CombineEdge(u, v2, edge2);
                            edgelist[v2].Remove(v);
                        }
                    }
                }
            }

            return counts;
        }


        static void CombineEdge(int u, int v, Edge edge)
        {
            edge.MakeMain(u);

            Edge edge0;
            if (edgelist[u].TryGetValue(v, out edge0))
            {
                edge0.MakeMain(u);
                edge0.rates |= edge.rates;
                return;
            }

            edgelist[u][v] = edge;
            edgelist[v][u] = edge;
        }



        #region Costs

        const int rmax = (1 << 10) - 1;

        public static int ReverseCost(int cost)
        {
            return reversedCosts[cost & (1 << 10) - 1];
        }

        public static int ReverseCost2(int cost)
        {
            int result = 0;
            for (int i = 0; i < 10; i++)
            {
                if ((cost & 1 << i) != 0)
                    result |= i == 0 ? 1 : 1 << (10 - i);
            }
            return result;
        }

        public static int[] reversedCosts = new int[1024];
        public static void Init()
        {
            for (int i = 0; i < reversedCosts.Length; i++)
                reversedCosts[i] = ReverseCost2(i);
        }

        public static int AddCost(int cost1, int cost2)
        {
            var mask = cost2;
            var result = 0;
            while (mask != 0)
            {
                var bit = mask & -mask;
                result |= cost1 * bit;
                mask &= ~bit;
            }
            return Reduce(result);
        }

        public static int Reduce(int cost)
        {
            uint c = (uint)cost;
            c = (c | c >> 10) & (1 << 10) - 1;
            return (int)c;
        }
        #endregion


        public class Edge
        {
            public int i;
            public int u;
            public int v;
            public int rates;
            public int reverseRates => ReverseCost(rates);

            public void MakeMain(int node)
            {
                if (u == node)
                    return;
                Swap(ref u, ref v);
                rates = ReverseCost(rates);
            }
        }

    }

    public class BridgesAndCuts
    {
        private int[] low;
        private int[] num;
        int curnum;

        public BridgesAndCuts(int n)
        {
            low = new int[4 * (n + 1)];
            num = new int[4 * (n + 1)];
        }

        void Dfs(List<int>[] adj,
                List<int> cp,
                List<Bridge> bridges, int u, int p)
        {
            low[u] = num[u] = curnum++;
            int cnt = 0; bool found = false;

            for (int i = 0; i < adj[u].Count; i++)
            {
                int v = adj[u][i];
                if (num[v] == -1)
                {
                    Dfs(adj, cp, bridges, v, u);
                    low[u] = Min(low[u], low[v]);
                    cnt++;
                    found = found || low[v] >= num[u];
                    if (low[v] > num[u]) bridges.Add(new Bridge(u, v));
                }
                else if (p != v) low[u] = Min(low[u], num[v]);
            }

            if (found && (p != -1 || cnt > 1))
                cp.Add(u);
        }

        public void CutPointsAndBridges(List<int>[] adj,
            out List<int> cutpoints,
            out List<Bridge> bridges)
        {
            int n = adj.Length;
            cutpoints = new List<int>();
            bridges = new List<Bridge>();

            for (int i = 0; i < 4 * n; i++)
                num[i] = -1;

            curnum = 0;
            for (int i = 0; i < n; i++)
                if (num[i] == -1)
                    Dfs(adj, cutpoints, bridges, i, -1);
        }
    }

    public class Bridge
    {
        public int X;
        public int Y;

        public Bridge(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


    public class DisjointSet
    {
        private readonly int[] _ds;
        private readonly int[] _counts;
        private int _components;
        private bool _oneBased;

        public int Count => _components;

        public DisjointSet(int size, bool onesBased = false)
        {
            _ds = new int[size + 1];
            _counts = new int[size + 1];
            _components = size;
            _oneBased = onesBased;

            for (int i = 0; i <= size; i++)
            {
                _ds[i] = i;
                _counts[i] = 1;
            }

            if (onesBased)
                _ds[0] = size;
            else
                _ds[size] = 0;
        }

        public bool Union(int x, int y)
        {
            var rx = Find(x);
            var ry = Find(y);
            if (rx == ry) return false;

            if (_counts[ry] > _counts[rx])
            {
                _ds[rx] = ry;
                _counts[ry] += _counts[rx];
            }
            else
            {
                _ds[ry] = rx;
                _counts[rx] += _counts[ry];
            }
            _components--;
            return true;
        }

        public int Find(int x)
        {
            var root = _ds[x];
            return root == x
                ? x
                : (_ds[x] = Find(root));
        }


        public int GetCount(int x)
        {
            var root = Find(x);
            return _counts[root];
        }

        public IEnumerable<int> Components()
        {
            for (int i = 0; i < _ds.Length; i++)
            {
                if (_ds[i] == i)
                    yield return i;
            }
        }

        public IEnumerable<List<int>> GetComponents()
        {
            var comp = new Dictionary<int, List<int>>();
            foreach (var c in Components())
                comp[c] = new List<int>(GetCount(c));

            int start = _oneBased ? 1 : 0;
            int limit = _oneBased ? _ds.Length : _ds.Length - 1;

            for (int i = start; i < limit; i++)
                comp[Find(i)].Add(i);
            return comp.Values;
        }
    }

    public static class HackerRankUtils
    {
        public const int MOD = 1000 * 1000 * 1000 + 7;

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

        public static long Subtract(long left, long right)
        {
            return (left + (MOD - right)) % MOD;
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

        public static long Pow(long n, long p)
        {
            long b = n;
            long result = 1;
            while (p != 0)
            {
                if ((p & 1) != 0)
                    result *= b;
                p >>= 1;
                b *= b;
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

        public static long Comb(int n, int k)
        {
            if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
            if (k + k > n) return Comb(n, n - k);
            return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
        }

        public static string[] ReadArray()
        {
            int n = int.Parse(Console.ReadLine());
            string[] array = new string[n];
            for (int i = 0; i < n; i++)
                array[i] = Console.ReadLine();
            return array;
        }

        public static int LowerBound<T>(T[] array, T value, int left, int right)
            where T : IComparable<T>
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (value.CompareTo(array[mid]) > 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            return left;
        }

        public static int UpperBound<T>(T[] array, T value, int left, int right)
            where T : IComparable<T>
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (value.CompareTo(array[mid]) >= 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            return left;
        }

        public static void For(int n, Action<int> action)
        {
            for (int i = 0; i < n; i++) action(i);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public static void MemSet<T>(IList<T> list, T value)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                list[i] = value;
        }

        public static void MemSet<T>(T[,] list, T value)
        {
            int count = list.GetLength(0);
            int count2 = list.GetLength(1);
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count2; j++)
                    list[i, j] = value;
        }

        public static void MemSet<T>(IEnumerable<IList<T>> list, T value)
        {
            foreach (var sublist in list)
                MemSet(sublist, value);
        }

        public static void Iota(IList<int> list, int seed)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                list[i] = seed++;
        }

        public class Comparer<T> : IComparer<T>
        {
            readonly Comparison<T> _comparison;

            public Comparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T a, T b)
            {
                return _comparison(a, b);
            }
        }
    }


}
