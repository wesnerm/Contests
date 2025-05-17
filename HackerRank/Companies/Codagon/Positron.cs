namespace HackerRank.Codagon.Positron
{
    using System;
    using System.Collections.Generic;
    using static System.Math;
    using static HackerRankUtils;

    public class Solution
    {

        public class Edge
        {
            public readonly int Cap;
            public readonly int From;
            public readonly int Index;
            public readonly int To;
            public int Flow;

            public Edge(int from, int to, int cap, int flow, int index)
            {
                From = @from;
                To = to;
                Cap = cap;
                Flow = flow;
                Index = index;
            }

            public int Rcap()
            {
                return Cap - Flow;
            }
        }

        public class Dinic
        {
            private readonly List<Edge>[] _g;
            private readonly int[] _layer;
            private readonly List<Edge>[] _lf;
            private readonly int _n;
            private readonly int[] _q;

            public Dinic(int n)
            {
                _g = new List<Edge>[n];
                _q = new int[n];
                _lf = new List<Edge>[n];
                _layer = new int[n];

                _n = n;

                for (int i = 0; i < n; i++)
                {
                    _g[i] = new List<Edge>();
                    _lf[i] = new List<Edge>();
                }

            }

            public void AddEdge(int from, int to, int cap)
            {
                if (from == to) return;
                _g[from].Add(new Edge(from, to, cap, 0, _g[to].Count));
                _g[to].Add(new Edge(to, from, 0, 0, _g[from].Count - 1));
            }

            private long BlockingFlow(int s, int t)
            {
                for (int i = 0; i < _n; i++)
                    _layer[i] = -1;
                for (int i = 0; i < _n; i++)
                    _lf[i].Clear();

                int head = 0, tail = 0;
                _q[tail++] = s;
                while (head < tail)
                {
                    var x = _q[head++];
                    for (var i = 0; i < _g[x].Count; i++)
                    {
                        var e = _g[x][i];
                        if (e.Rcap() <= 0) continue;
                        if (_layer[e.To] == -1)
                        {
                            _layer[e.To] = _layer[e.From] + 1;
                            _q[tail++] = e.To;
                        }
                        if (_layer[e.To] > _layer[e.From])
                        {
                            _lf[e.From].Add(e);
                        }
                    }
                }
                if (_layer[t] == -1) return 0;

                var totflow = 0;
                var p = new List<Edge>();
                while (_lf[s].Count > 0)
                {
                    var curr = p.Count == 0 ? s : p[p.Count - 1].To;
                    if (curr == t)
                    {
                        // Augment
                        var amt = p[0].Rcap();
                        foreach (var e1 in p)
                        {
                            amt = Math.Min(amt, e1.Rcap());
                        }
                        totflow += amt;
                        for (var i = p.Count - 1; i >= 0; --i)
                        {
                            p[i].Flow += amt;
                            _g[p[i].To][p[i].Index].Flow -= amt;
                            if (p[i].Rcap() <= 0)
                            {
                                var list = _lf[p[i].From];
                                if (list.Count > 0)
                                    list.RemoveAt(list.Count - 1);
                                p.RemoveRange(i, p.Count - i);
                            }
                        }
                    }
                    else if (_lf[curr].Count == 0)
                    {
                        // Retreat
                        p.RemoveAt(p.Count - 1);
                        for (var i = 0; i < _n; ++i)
                            for (var j = 0; j < _lf[i].Count; ++j)
                                if (_lf[i][j].To == curr)
                                    _lf[i].RemoveAt(j);
                    }
                    else
                    {
                        // Advance
                        p.Add(_lf[curr][_lf[curr].Count - 1]);
                    }
                }
                return totflow;
            }

            public long GetMaxFlow(int s, int t)
            {
                long totflow = 0;
                long flow;
                while ((flow = BlockingFlow(s, t)) != 0)
                    totflow += flow;
                return totflow;
            }
        }

        struct Point
        {
            public int x, y;
        };

        static bool collide(Point a, int da, Point b, int db)
        {
            if (a.x == b.x && a.y == b.y)
            {
                return true;
            }
            else if (a.x == b.x)
            {
                if (a.y > b.y)
                {
                    Swap(ref a, ref b);
                    Swap(ref da, ref db);
                }
                if (da == 2 && db == 4)
                {
                    return true;
                }
                return false;
            }
            else if (a.y == b.y)
            {
                if (a.x > b.x)
                {
                    Swap(ref a, ref b);
                    Swap(ref da, ref db);
                }
                if (da == 3 && db == 5)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (a.x > b.x)
                {
                    Swap(ref a, ref b);
                    Swap(ref da, ref db);
                }
                int DX = Abs(a.x - b.x);
                int DY = Abs(a.y - b.y);
                if (DX != DY)
                {
                    return false;
                }
                // case 1 : a is higher
                if (da == 3 && db == 2)
                {
                    return true;
                }
                if (da == 4 && db == 5)
                {
                    return true;
                }
                // case 2 : b is higher
                if (da == 2 && db == 5)
                {
                    return true;
                }
                if (da == 3 && db == 4)
                {
                    return true;
                }
                return false;
            }
        }

        static void Driver()
        {
            int t = int.Parse(Console.ReadLine());
            int[] arr;
            while (t-- > 0)
            {
                arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                int n = arr[0], m = arr[1];
                List<Point> a = new List<Point>(), b = new List<Point>();
                var mf = new Dinic(2 + 5 * n + 5 * m);
                int source = 0;
                int sink = 5 * (n + m) + 1;
                // naming convention :
                // 1 - [2, 3, 4, 5]
                // 6 - [7, 8, 9, 10]
                // 11 -
                for (int i = 0; i < n; ++i)
                {
                    arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                    int x = arr[0], y = arr[1];
                    a.Add(new Point { x = x, y = y });
                    mf.AddEdge(source, 5 * i + 1, 1);
                    for (int j = 2; j <= 5; ++j)
                    {
                        mf.AddEdge(5 * i + 1, 5 * i + j, 1);
                    }
                }
                int bas = 5 * n;
                for (int i = 0; i < m; ++i)
                {
                    arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                    int x = arr[0], y = arr[1];
                    b.Add(new Point { x = x, y = y });
                    mf.AddEdge(bas + 5 * i + 1, sink, 1);
                    for (int j = 2; j <= 5; ++j)
                    {
                        mf.AddEdge(bas + 5 * i + j, bas + 5 * i + 1, 1);
                    }
                }

                for (int i = 0; i < n; ++i)
                {
                    Point u = a[i];
                    for (int j = 0; j < m; ++j)
                    {
                        Point v = b[j];
                        for (int di = 2; di <= 5; ++di)
                        {
                            for (int dj = 2; dj <= 5; ++dj)
                            {
                                if (collide(u, di, v, dj))
                                {
                                    mf.AddEdge(5 * i + di, bas + 5 * j + dj, 1);
                                }
                            }
                        }
                    }
                }
                Console.WriteLine(mf.GetMaxFlow(source, sink));
            }
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