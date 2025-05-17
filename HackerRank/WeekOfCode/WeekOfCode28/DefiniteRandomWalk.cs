
namespace HackerRank.WeekOfCode28.DefiniteRandomWalk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;

    public class Solution
    {
        public const int Mod = 998244353;

        public static void Main()
        {
            var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
            int n = arr[0]; // number of positions/fields
            int m = arr[1]; // die faces
            int k = arr[2]; // turns


            var jump = Array.ConvertAll(Console.ReadLine().Split(), x=>int.Parse(x)-1);
            var prob = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
            map = new int[n];

            Debug.Assert(n == jump.Length);
            Debug.Assert(m == prob.Length);


            var problem = new WalkProblem(k, jump, prob);
            var result = problem.Solve();

            var equiprob = (int)Inverse(n);
            foreach (var r in result)
                Console.WriteLine(Mult(r, equiprob));
        }

        private static int[] map;

        public class WalkProblem
        {
            public int N;
            public int M;
            public int[] Prob;
            public int[] Jump;
            public int Turns;

            public WalkProblem(int k, int[] jump, int[] prob)
            {
                Prob = prob;
                Jump = jump;
                Turns = k;
                int n = jump.Length;
                int m = prob.Length;
                N = n;
                M = m;
            }

            public int[] Solve()
            {
                int[] result = new int[N];
                var components = GetComponents();
                foreach (var list in components)
                {
                    var jump = new int[list.Count];

                    for (int i = 0; i < list.Count; i++)
                        map[list[i]] = i;


                    for (int i = 0; i < list.Count; i++)
                    {
                        int j = list[i];
                        jump[i] = map[Jump[j]];
                    }

                    var problem = new WalkProblem(Turns, jump, Prob);
                    var compresult = problem.SolveComponent();
                    for (int i = 0; i < list.Count; i++)
                        result[list[i]] = compresult[i];
                }

                return result;
            }

            IEnumerable<List<int>> GetComponents()
            {
                var ds = new DisjointSet(N);
                for (var i = 0; i < Jump.Length; i++)
                    ds.Union(i, Jump[i]);

                var comp = new Dictionary<int, List<int>>();
                foreach (var c in ds.Components())
                    comp[c] = new List<int>(ds.GetCount(c));

                for (int i = 0; i < N; i++)
                    comp[ds.Find(i)].Add(i);
                return comp.Values;
            }

            public int[] SolveComponent()
            {
                // Find SCC
                int[] num = new int[N];
                int[] low = new int[N];
                int[] depth = new int[N];

                for (int i = 0; i < N; i++)
                    Dfs(num, low, depth, i);

                int max = 0;
                int argmax = 0;
                for (int i = 0; i < N; i++)
                {
                    if (depth[i] > max)
                    {
                        max = depth[i];
                        argmax = i;
                    }
                }
                
                int[] result = new int[N];
                for (int i = 0; i < N; i++)
                    result[i] = 1;

                if (max == 0)
                    return result;

                var table = new int[N];
                int cur = argmax;
                for (int i = 0; i < N; i++)
                {
                    table[i] = cur;
                    cur = Jump[cur];
                }

                // Inside a cycle the probability is even

                for (int i = 0; i < N; i++)
                {
                    if (i < max)
                    {
                        // Not in a cycle
                        // We need the probability that we land on this spot if j^k

                        
                        
                    }
                }


                return result;
            }

            void Dfs(int[] num, int[] low, int[] depth, int i)
            {
                if (num[i] != 0)
                    return;

                num[i] = i+1;
                var j = Jump[i];
                Dfs(num, low, depth, j);
                depth[i] = low[j] != num[j] ? 0 : depth[j] + 1;
                low[i] = Math.Min(num[i], low[j]);
            }

        }

        public class DisjointSet
        {
            private readonly int[] _ds;
            private readonly int[] _counts;
            private int _components;

            public int Count => _components;

            public DisjointSet(int size, bool onesBased = false)
            {
                _ds = new int[size + 1];
                _counts = new int[size + 1];
                _components = size;

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

        }


        static readonly int[] _inverse = new int[3000];
        public static long Inverse(long n)
        {
            long result;
            if (n < _inverse.Length && (result = _inverse[n]) != 0)
                return result - 1;

            result = ModPow(n, Mod - 2);
            if (n < _inverse.Length)
                _inverse[n] = (int)(result + 1);
            return result;
        }

        public static long Mult(long left, long right)
        {
            return (left * right) % Mod;
        }

        public static long Div(long left, long divisor)
        {
            if (left % divisor == 0)
                return left / divisor;

            return Mult(left, Inverse(divisor));
        }

        public static long Subtract(long left, long right)
        {
            return (left + (Mod - right)) % Mod;
        }

        public static long ModPow(long n, long p)
        {
            long b = n;
            long result = 1;
            while (p != 0)
            {
                if ((p & 1) != 0)
                    result = (result * b) % Mod;
                p >>= 1;
                b = (b * b) % Mod;
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


    }
}
