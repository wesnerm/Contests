    namespace HackerRank.WorldCodeSprint8.KingdomDivision
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
            static void Driver(String[] args)
            {
                int n = Convert.ToInt32(Console.ReadLine());
                edges = new List<int>[n + 1];

                for (int i = 0; i < edges.Length; i++)
                    edges[i] = new List<int>();

                for (int a0 = 0; a0 < n - 1; a0++)
                {
                    var arr = Console.ReadLine().Split();
                    int u = int.Parse(arr[0]);
                    int v = int.Parse(arr[1]);
                    edges[u].Add(v);
                    edges[v].Add(u);
                }
                
                //int root = FindBalancedRoot(edges, n);
                long answer = CountKingdoms(1);
                Console.WriteLine(answer);
            }


            public static int FindBalancedRoot(List<int>[] graph, int n)
            {
                // Find Balanced Root O(V) -- it gives us good distance metrics
                // -- start from leaves
                // -- last queued item is the root
                var queue = new Queue<int>();
                var degree = new int[n + 1];

                for (var index = 1; index < graph.Length; index++)
                {
                    var v = 1;
                    var count = degree[v] = graph[index].Count;
                    if (count != 1) continue;
                    degree[v] = 0;
                    queue.Enqueue(v);
                }

                int root = -1;
                while (queue.Count > 0)
                {
                    root = queue.Dequeue();
                    foreach (var v2 in graph[root])
                        if (--degree[v2] == 1) // Might still be buggy
                        {
                            degree[v2] = 0;
                            queue.Enqueue(v2);
                        }
                }

                return root;
            }

            private static List<int>[] edges;

            static long CountKingdoms(int node)
            {
                var result = CountCore(node, -1);
                return Mult(2, result.Count);
            }

            static State CountCore(int node, int parent)
            {
                var stack = new Stack<State>(100000);
                stack.Push(new State { Node = node, Parent = parent });

                State state = null;
                while (stack.Count > 0)
                {
                    state = stack.Pop();
                    node = state.Node;
                    if (state.Index >= edges[state.Node].Count)
                    {
                        state.Count = Subtract(state.Count, state.SingleCount);
                        var ps = state.ParentState;
                        if (ps != null)
                        {
                            ps.SingleCount = Mult(ps.SingleCount, state.Count);
                            ps.Count = Mult(ps.Count, 2 * state.Count + state.SingleCount);
                        }
                        continue;
                    }

                    stack.Push(state);
                    var child = edges[node][state.Index++];
                    if (child == parent) continue;
                    stack.Push(new State
                    {
                        Node = child,
                        Parent = node,
                        ParentState = state,
                    });
                }

                return state;
            }

            public class State
            {
                public int Node;
                public int Parent;
                public State ParentState;
                public int Index;
                public long SingleCount = 1;
                public long Count = 1;
            }

        }

        public struct Result
        {
            public long Count;
            public long SingleCount;
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

