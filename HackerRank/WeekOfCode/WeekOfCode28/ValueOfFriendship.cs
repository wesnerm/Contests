namespace HackerRank.WeekOfCode28.ValueOfFriendship
{
    using System;
    using System.Collections.Generic;
    class Solution
    {

        static void Driver(String[] args)
        {
            int t = int.Parse(Console.ReadLine());

            while (t-- > 0)
            {
                var tokens_n = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                int n = (tokens_n[0]);
                int m = (tokens_n[1]);

                var edges = new int[m][];

                var ds = new DisjointSet(n, true);
                for (int a1 = 0; a1 < m; a1++)
                {
                    var tokens_x = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                    int x = tokens_x[0];
                    int y = tokens_x[1];
                    ds.Union(x, y);
                }

                var result = Compute(n, m, ds);
                Console.WriteLine(result);

            }
        }

        public static long Compute(int n, int m, DisjointSet ds)
        {
            var list = new List<long>();
            foreach (var c in ds.Components())
            {
                var cnt = ds.GetCount(c);
                if (ds.GetCount(c) < 2) continue;
                list.Add(cnt);
            }

            list.Sort((a,b) => -a.CompareTo(b));

            long result = 0;
            long otherFriends = 0;
            long usedEdges = 0;
            foreach (var c in list)
            {
                long matches = c - 1;
                result += matches * otherFriends + SumMatches(matches);
                otherFriends += c * (c - 1);
                usedEdges += matches;
            }

            // Add redundant edges
            result += otherFriends * (m - usedEdges);
            return result;
        }

        public static long SumMatches(long n)
        {
            long sum = n * (n + 1) * (n + 2) / 3;
            return sum;
        }
    }

    /// <summary>
    /// Works with one-base or zero-based.
    /// </summary>
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
}
