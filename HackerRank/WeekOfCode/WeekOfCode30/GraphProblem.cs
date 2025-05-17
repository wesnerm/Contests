namespace HackerRank.WeekOfCode30.GraphProblem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using static System.Console;

    public class Solution
    {
        static int[][] g;
        static int n;
        static long best = 0;
        static double bestScore = -1;
        static HashSet<long> visited = new HashSet<long>();
        static Dictionary<long, double> scores = new Dictionary<long, double>();
        static bool[,] isSide;
        static HashSet<long> triangleSet;
        static int[] triangles;

        public static void Main()
        {
            LaunchTimer(Report, 2000);
            n = Convert.ToInt32(Console.ReadLine());
            g = new int[n][];
            for (int g_i = 0; g_i < n; g_i++)
                g[g_i] = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);

            isSide = new bool[n + 1, n + 1];
            triangleSet = new HashSet<long>();
            triangles = new int[n];

            var ds = new DisjointSet(n);
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (g[j][i] == 0) continue;
                    for (int k = j + 1; k < n; k++)
                    {
                        if (g[k][j] == 0 || g[k][i] == 0) continue;
                        ds.Union(i, j);
                        ds.Union(j, k);
                        isSide[i, j] = true;
                        triangleSet.Add(1L << i | 1L << j | 1L << k);
                        triangles[i]++;
                        triangles[j]++;
                        triangles[k]++;
                    }
                }
            }

            SetBest((1L << n) - 1);
            foreach (var current in ds.GetComponents())
            {
                Solve(current);
            }

            Report();
        }

        static bool Report()
        {
            Console.WriteLine(BitCount(best));
            Console.WriteLine(string.Join(" ", Decode(best).Select(x => x + 1)));
            return true;
        }

        static double SetBest(long code)
        {
            var score = Score(code);
            if (score > bestScore)
            {
                best = code;
                bestScore = score;
                if (verbose) Console.WriteLine($"{indent}best: {bestScore} {Text(best)}");
            }
            return score;
        }

        static string Text(long code)
        {
            return "(" + string.Join(",", Decode(code).Select(x => x + 1)) + ")";
        }

        static string indent = "";

        static void Solve(List<int> current)
        {
            var code = Code(current);

            var newCode = Dfs(code);
            var newlist = DecodeList(newCode);
            var newMask = newCode;

            int count = newlist.Count;
            if (count > 3)
            {
                newlist.Sort((a, b) => triangles[a].CompareTo(triangles[b]));

                while (count-->19)
                    newMask &= ~(1L << newlist[count]);

                BruteForce(newCode, newMask);
            }
        }

        static long BruteForce(long code, long bits)
        {
            if (bits == 0)
            {
                SetBest(code);
                return code;
            }

            long bit = bits & -bits;
            bits &= ~bit;

            var code1 = BruteForce(code, bits);
            var code2 = BruteForce(code & ~bit, bits);
            return (Score(code1) > Score(code2)) ? code1 : code2;
        }

        static long Dfs(long code, int depth = 0)
        {
            int count = BitCount(code);
            if (count < 3)
                return code;

            indent = verbose ? new string(' ', depth) : "";
            if (verbose) WriteLine($"{indent}Dfs({Text(code)}) {Score(code)}");

            double bestScore = SetBest(code);
            long bestChild = code;

            if (count > 3)
                foreach (var v in Decode(code))
                {
                    var b = 1L << v;
                    long subg = code & ~b;

                    var subscore = Score(subg);
                    if (subscore >= bestScore)
                    {
                        bestChild = subg;
                        bestScore = subscore;
                    }
                }

            if (bestChild != code)
                return Dfs(bestChild, depth + 1);

            return code;
        }

        static int Triangles(long code)
        {
            int triangles = 0;

            int count2 = n * (n - 1) * (n - 2);
            //int count2 = n * (n - 1) * (n - 2) / 6;

            if (triangleSet.Count < count2)
            {
                foreach (var t in triangleSet)
                {
                    if ((t & code) == t)
                        triangles++;
                }
                return triangles;
            }

            var nodes = DecodeList(code);
            for (int i = 0; i < nodes.Count; i++)
            {
                var v1 = nodes[i];
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var v2 = nodes[j];
                    if (!isSide[v1, v2]) continue;
                    for (int k = j + 1; k < nodes.Count; k++)
                    {
                        var v3 = nodes[k];
                        if (g[v2][v3] == 0 || g[v1][v3] == 0) continue;
                        triangles++;
                    }
                }
            }

            if (verbose) WriteLine($"{Text(code)} has {triangles} triangles");
            return triangles;
        }

        static double Score(long code)
        {
            double score = 0;
            if (scores.TryGetValue(code, out score))
                return score;

            double t = Triangles(code);
            score = n == 0 ? 0 : t / BitCount(code);
            scores[code] = score;
            return score;
        }

        public static int BitCount(long x)
        {
            int count;
            var y = unchecked((ulong)x);
            for (count = 0; y != 0; count++)
                y &= y - 1;
            return count;
        }

        private const ulong M1 = 0x5555555555555555; //binary: 0101...
        private const ulong M2 = 0x3333333333333333; //binary: 00110011..
        private const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
        private const ulong M8 = 0x00ff00ff00ff00ff; //binary:  8 zeros,  8 ones ...
        private const ulong M16 = 0x0000ffff0000ffff; //binary: 16 zeros, 16 ones ...
        private const ulong M32 = 0x00000000ffffffff; //binary: 32 zeros, 32 ones
        private const ulong Hff = 0xffffffffffffffff; //binary: all ones
        private const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...

        public static int BitCount2(long x2)
        {
            var x = (ulong)x2;
            x -= (x >> 1) & M1; //put count of each 2 bits into those 2 bits
            x = (x & M2) + ((x >> 2) & M2); //put count of each 4 bits into those 4 bits 
            x = (x + (x >> 4)) & M4; //put count of each 8 bits into those 8 bits 
            x += x >> 8; //put count of each 16 bits into their lowest 8 bits
            x += x >> 16; //put count of each 32 bits into their lowest 8 bits
            x += x >> 32; //put count of each 64 bits into their lowest 8 bits
            return unchecked((int)(x & 0x7f));
        }


        static long Code(List<int> current)
        {
            long code = 0;
            foreach (int i in current) code |= 1L << i;
            return code;
        }

        static List<int> DecodeList(long code)
        {
            var list = new List<int>(BitCount(code));
            for (int index = 0; code != 0; index++)
            {
                if ((code & 1) == 1) list.Add(index);
                code >>= 1;
            }
            return list;
        }

        static IEnumerable<int> Decode(long code)
        {
            for (int index = 0; code != 0; index++)
            {
                if ((code & 1) == 1) yield return index;
                code >>= 1;
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

            public int[] Array => _ds;

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

        static System.Threading.Timer _timer;
        static Func<bool> _timerAction;
        public static System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
        public static double Elapsed => process.TotalProcessorTime.TotalMilliseconds;

        public static void LaunchTimer(Func<bool> action, long ms = 2900)
        {
            _timerAction = action;
            ms -= (long)Elapsed + 1;

            _timer = new System.Threading.Timer(
                delegate
                {
#if !DEBUG
                    if (_timerAction())
                        Environment.Exit(0);
#endif
                }, null, ms, 0);
        }

        static bool verbose = false;


    }
}
