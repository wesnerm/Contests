
namespace HackerRank.WorldCodeSprint8.OptimalPolygon
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using static System.Math;
    using static System.Console;
    using static HackerRankUtils;

    public class Solution
    {
        public static void Main()
        {
            LaunchTimer();

            var arr = ReadLine().Split();
            n = int.Parse(arr[0]);
            d = double.Parse(arr[1]);
            points = new Point[n];
            for (int i = 0; i < n; i++)
            {
                var a = Array.ConvertAll(ReadLine().Split(), double.Parse);
                points[i] = new Point(a[0], a[1]);
            }

            AttemptConvexHull();

            Run();
        }

        private static double d;
        private static Point[] points;
        private static int n;

        private static System.Threading.Timer timer;

        public static void LaunchTimer()
        {
            // Use a timer to quit the search
            timer = new System.Threading.Timer(
                delegate
                {
                    if (!movesReported && bestlist != null)
                    {
                        Report();
                        Environment.Exit(0);
                    }
                }, null, 2800, 0);
        }

        private static bool movesReported;
        public static void Report()
        {
            if (movesReported) return;
            movesReported = true;

            var ordering = bestlist.Select(x => x.Index + 1).ToArray();
            var asst = bestlist.Where(x => x.Moved).ToList();

            Console.WriteLine(asst.Count);
            for (var i = 0; i < asst.Count; i++)
            {
                var cand = asst[i];
                Console.Write($"{cand.Index + 1} {cand.Target.X:F8} {cand.Target.Y:F8}");
                //Console.WriteLine($" {Dist(cand.Target, cand.Original):F8}");
                Console.WriteLine();
            }

            Console.WriteLine(string.Join(" ", ordering));
        }


        public static void AttemptConvexHull()
        {

            var dict = new Dictionary<Point, int>();
            for (int i = 0; i < points.Length; i++)
                dict[points[i]] = i;


            var pts2 = points.ToList();
            ConvexHull(ref pts2, false);
            
            var c = new Point();

            foreach (var p in pts2)
                c += p;
            c /= pts2.Count;

            var r = 0.0;
            foreach (var p in pts2)
                r += Dist(p, c);
            r /= pts2.Count;


            var result = new List<Candidate>();
            for (var i = 0; i < points.Length; i++)
            {
                if (dict.ContainsKey(points[i]))
                {
                    result.Add(new Candidate
                    {
                        Index = i,
                        Original = points[i],
                        Target = points[i],
                    });
                    continue;
                }

                var sects = CircleCircleIntersection(c, points[i], r, d);
                if (sects.Count == 0)
                {
                    result.Add(new Candidate
                    {
                        Index = i,
                        Original = points[i],
                        Target = points[i],
                    });

                }
                else
                {
                    result.Add(new Candidate
                    {
                        Index = i,
                        Original = points[i],
                        Target = sects[0],
                    });
                }

            }

            foreach (var cand in result)
            {
                var p2 = cand.Target;
                var ang = Atan2(p2.Y - c.Y, p2.X - c.X);
                if (ang < 0) ang += Math.PI * 2;
                ang /= 2 * Math.PI;
                cand.Angle = ang * n;
            }

            UpdateScore(result);

        }

        public static void Run()
        {
            // Produce center points
            var c = new Point();

            foreach (var p in points)
                c += p;
            c /= n;

            var r = 0.0;
            foreach (var p in points)
                r += Dist(p, c);
            r /= n;

            var sects = new List<Point>[n];

            var candidates = new List<Candidate>();
            var dir = new int[] { 0, 1, 0, -1, 0 };
            var candCount = new int[n];
            for (var i = 0; i < points.Length; i++)
            {
                var p = points[i];
                sects[i] = CircleCircleIntersection(c, p, r, d);

                foreach (var p2 in sects[i])
                {
                    candidates.Add(new Candidate
                    {
                        Index = i,
                        Original = p,
                        Target = p2,
                    });
                    candCount[i]++;
                }

                if (sects[i].Count == 0)
                {
                    candidates.Add(new Candidate
                    {
                        Index = i,
                        Original = p,
                        Target = p,
                    });
                    candCount[i]++;
                }

                /*

                for (int j = 0; j < 4; j++)
                {
                    candidates.Add(new Candidate
                    {
                        Index = i,
                        Original = p,
                        Target = new Point(p.X+ d*dir[j], p.Y + d*dir[j+1])
                    });
                    candCount[i]++;
                }*/
            }

            foreach (var cand in candidates)
            {
                var p2 = cand.Target;
                var ang = Atan2(p2.Y - c.Y, p2.X - c.X);
                if (ang < 0) ang += Math.PI * 2;
                ang /= 2 * Math.PI;
                cand.Angle = ang * n;
            }

            candidates.Sort((a, b) => a.Angle.CompareTo(b.Angle));


            // Try bipartite matching
            var pie2pt = new HashSet<int>[n];
            var pt2pie = new HashSet<int>[n];
            for (int i = 0; i < pie2pt.Length; i++)
            {
                pie2pt[i] = new HashSet<int>();
                pt2pie[i] = new HashSet<int>();
            }

            foreach (var cand in candidates)
            {
                double k = cand.Angle;
                var prev = (int)Math.Floor(k + .75) % n;
                var next = (int)Math.Ceiling(k - .75) % n;
                pie2pt[prev].Add(cand.Index);
                pie2pt[next].Add(cand.Index);
                pt2pie[cand.Index].Add(prev);
                pt2pie[cand.Index].Add(next);
            }

            for (int i = 0; i < n; i++)
            {
                if (pie2pt[i].Count == 1)
                {
                    var target = pie2pt[i].First();
                    Assign(pt2pie, target, i, pie2pt);
                }

                if (pt2pie[i].Count == 1)
                {
                    var target = pt2pie[i].First();
                    Assign(pie2pt, target, i, pt2pie);
                }
            }

            var hs = new HashSet<int>[n + 1];
            for (int i=0; i<n; i++)
            {
                hs[i + 1] = new HashSet<int>(pie2pt[i].Select(x => x + 1));

            }

            var pt2cand = new List<Candidate>[n];
            for (int i = 0; i < pt2cand.Length; i++)
                pt2cand[i] = new List<Candidate>();

            foreach (var cand in candidates)
                pt2cand[cand.Index].Add(cand);


            var rand = new Random();
            for (int k = 0; k < 100; k++)
            {
                int[] mr;
                int[] mc;
                int match = BipartiteMatching(hs, n, out mr, out mc);

                List<Candidate> result = new List<Candidate>();
                var pre = new List<IList<Candidate>>();


                for (int i = 0; i < n; i++)
                {
                    var cand = pt2cand[i].First();
                    int a = mc[i + 1] - 1;
                    if (a >= 0)
                    {
                        pre.Add(pt2cand[i]);
                        cand = pt2cand[i].First();
                    }
                    else
                    {
                        pre.Add(new List<Candidate> { cand } );
                    }

                    result.Add(cand);
                }


                for (int t=1; t<5; t++)
                {
                    for (int j=0; j<result.Count; j++)
                    {
                        var pool = pre[j];
                        if (pool.Count <= 1) continue;

                        Func<Point, double> scoreIt = p => Score(new[] {
                            result[(j-1+result.Count)%result.Count].Target, p,
                            result[(j+1)%result.Count].Target, c });
                        var best = pool[0];
                        var bestScore = scoreIt(pool[0].Target);
                        for (int p = 0; p<pool.Count; p++)
                        {
                            var newP = pool[p].Target;
                            var sc = scoreIt(newP);
                            if (sc < bestScore)
                            {
                                best = pool[p];
                                bestScore = sc; 
                            }
                        }
                        result[j] = best;

                    }
                }




                UpdateScore(result);
            }

            var bestcand = new SearchBest(candidates, n, pt2pie, pie2pt, pt2cand);

            Report();
        }

        public static double Estimate(Point pt, Point pt2, Point pt3, Point center)
        {
            return Score(new[] {pt, pt2, pt3, center});
        }


        public static double UpdateScore(List<Candidate> cands)
        {
            cands.Sort((a, b) => a.Angle.CompareTo(b.Angle));
            var pts = cands.Select(x => x.Target).ToList();
            var score = Score(pts);
            if (bestlist == null || score < bestScore)
            {
                bestlist = cands.ToArray();
                bestScore = score;
            }
            return score;
        }

        public static double Score(IList<Point> pts)
        {
            var area = ComputeArea(pts);
            var perim = 0.0;
            for (int i = 0; i < pts.Count; i++)
                perim += Dist(pts[i], pts[(i + 1) % n]);

            var score = perim * perim / area;
            return score;
        }


        private const double Eps = 1e-7;

        private static double Cross(Point p, Point q)
        {
            return p.X * q.Y - p.Y * q.X;
        }

        private static double Area2(Point a, Point b, Point c)
        {
            return Cross(a, b) + Cross(b, c) + Cross(c, a);
        }

        private static bool Between(Point a, Point b, Point c)
        {
            return Math.Abs(Area2(a, b, c)) < Eps && (a.X - b.X) * (c.X - b.X) <= 0 && (a.Y - b.Y) * (c.Y - b.Y) <= 0;
        }

        public static int Unique<T>(IList<T> list, int start, int end)
        {
            var read = start + 1;

            var comparer = EqualityComparer<T>.Default;
            while (read < end && !comparer.Equals(list[read - 1], list[read]))
            {
                read++;
            }

            var write = read;
            while (read < end)
            {
                if (!comparer.Equals(list[write - 1], list[read]))
                    list[write++] = list[read];
                read++;
            }

            return write;
        }

        public static void ConvexHull(ref List<Point> pts, bool removeRedundant = true)
        {
            pts.Sort();
            var uniqueEnd = Unique(pts, 0, pts.Count);
            pts.RemoveRange(uniqueEnd, pts.Count - uniqueEnd);

            var up = new List<Point>();
            var dn = new List<Point>();
            for (var i = 0; i < pts.Count; i++)
            {
                while (up.Count > 1 && Area2(up[up.Count - 2], up[up.Count - 1], pts[i]) >= 0) up.RemoveAt(up.Count-1);
                while (dn.Count > 1 && Area2(dn[dn.Count - 2], dn[dn.Count - 1], pts[i]) <= 0) dn.RemoveAt(dn.Count-1);
                up.Add(pts[i]);
                dn.Add(pts[i]);
            }

            pts = dn;
            for (var i = up.Count - 2; i >= 1; i--)
            {
                pts.Add(up[i]);
            }

            if (removeRedundant)
            {
                if (pts.Count <= 2) return;
                dn.Clear();
                dn.Add(pts[0]);
                dn.Add(pts[1]);
                for (var i = 2; i < pts.Count; i++)
                {
                    if (Between(dn[dn.Count - 2], dn[dn.Count - 1], pts[i])) dn.RemoveAt(dn.Count-1);
                    dn.Add(pts[i]);
                }
                if (dn.Count >= 3 && Between(dn[dn.Count - 1], dn[0], dn[1]))
                {
                    dn[0] = dn[dn.Count - 1];
                    dn.RemoveAt(dn.Count-1);
                }
                pts = dn;
            }
        }



        public static bool FindMatch(int i, HashSet<int>[] w, int[] mr, int[] mc, BitArray seen)
        {
            foreach (var j in w[i])
            {
                if (!seen[j])
                {
                    seen[j] = true;
                    if (mc[j] < 0 || FindMatch(mc[j], w, mr, mc, seen))
                    {
                        mr[i] = j;
                        mc[j] = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public static int BipartiteMatching(HashSet<int>[] w, int m, out int[] mr, out int[] mc)
        {
            mr = new int[w.Length];
            mc = new int[m + 1];

            for (int i = 1; i < mr.Length; i++)
                mr[i] = -1;

            for (int i = 1; i < mc.Length; i++)
                mc[i] = -1;

            var ct = 0;
            var seen = new BitArray(m + 1);

            var list = new int[w.Length-1];
            for (int i = 0; i < list.Length; i++)
                list[i] = i+1;

            var r = new Random();
            for (int i = 0; i < list.Length; i++)
            {
                int k = r.Next(0, list.Length);
                Swap(ref list[i], ref list[k]);
            }


            foreach (var i in list)
            //for (var i = 1; i < w.Length; i++)
            {
                seen.SetAll(false);
                if (FindMatch(i, w, mr, mc, seen)) ct++;
            }
            return ct;
        }



        static void Assign(HashSet<int>[] src, int i, int v, HashSet<int>[] rev)
        {
            var hash = src[i];
            while (hash.Count > 1 && hash.Contains(v))
            {
                var findAlt = -1;
                foreach (var k in hash)
                    if (k != v)
                    {
                        findAlt = k;
                        break;
                    }

                if (findAlt == -1)
                    break;
                Remove(rev, findAlt, i, src);
            }
        }
        
        static void Remove(HashSet<int>[] src, int i, int v, HashSet<int>[] rev)
        {
            var hash = src[i];
            if (!hash.Contains(v))
                return;

            hash.Remove(v);
            Remove(rev, v, i, src);

            if (hash.Count == 1)
                Assign(rev, hash.First(), i, src);
        }



        public static Candidate[] bestlist;
        public static double bestScore = double.MaxValue;

        public class SearchBest
        {
            private List<Candidate> candidates;
            private bool[] seen;
            private int n;
            private double area;
            private double perim;
            private HashSet<int>[] pt2pie;
            private HashSet<int>[] pie2pt;
            private List<Candidate>[] pt2cand;

            private List<Candidate> buffer = new List<Candidate>();

            public SearchBest(List<Candidate> cand, int n, HashSet<int>[] pt2pie, HashSet<int>[] pie2pt,
                List<Candidate>[] pt2cand)
            {
                this.candidates = cand;
                this.pt2pie = pt2pie;
                this.pie2pt = pie2pt;
                this.n = n;
                this.seen = new bool[n + 1];

                this.pt2cand = pt2cand;
                Dfs(0);
            }

            public void Dfs(int index)
            {
                if (index == n)
                {
                    var pt = buffer[buffer.Count - 1].Target;
                    var pt2 = buffer[0].Target;
                    area += pt.X * pt2.Y - pt2.X * pt.Y;
                    perim += Dist(pt, pt2);
                    area = Abs(area / 2);
                    var score = perim * perim / area;
                    if (bestlist == null || score < bestScore)
                    {
                        bestScore = score;
                        bestlist = buffer.ToArray();
                    }
                    return;
                }

                var trylist = new List<Candidate>();
                foreach (var pt in pie2pt[index])
                {
                    if (seen[pt])
                        continue;

                    foreach (var cd in pt2cand[pt])
                    {
                        var dist = Abs(index - cd.Angle) % n;
                        dist = Min(dist, n - dist);
                        if (dist < .75)
                            trylist.Add(cd);
                    }
                }

                if (trylist.Count == 0)
                {
                    // if (bestlist == null) Dfs(index + 1);
                    return;
                }

                foreach (var c in trylist)
                {
                    seen[c.Index] = true;
                    var j = buffer.Count;
                    buffer.Add(c);
                    var oldArea = area;
                    var oldPerim = perim;

                    var pt = c.Target;
                    if (j > 0)
                    {
                        var pt0 = buffer[j - 1].Target;
                        area += pt0.X * pt.Y - pt.X * pt0.Y;
                        perim += Dist(pt, pt0);
                    }

                    Dfs(index + 1);
                    area = oldArea;
                    perim = oldPerim;
                    buffer.RemoveAt(j);
                    seen[c.Index] = false;
                }

            }
        }



        public class Candidate
        {
            public int Index;
            public Point Original;
            public Point Target;
            public double Angle;
            public bool Moved => Target.X != Original.X || Target.Y != Original.Y;
        }

        public static double ComputeArea(IList<Point> p)
        {
            double area = 0;
            for (var i = 0; i < p.Count; i++)
            {
                var j = (i + 1) % p.Count;
                area += p[i].X * p[j].Y - p[j].X * p[i].Y;
            }
            return Abs(area / 2.0);
        }


        public static List<Point> CircleCircleIntersection(Point a, Point b, double r, double R)
        {
            var ret = new List<Point>();
            var d = Dist(a, b);
            if (d > r + R || d + Min(r, R) < Max(r, R)) return ret;
            var x = (d * d - R * R + r * r) / (2 * d);
            var y = Sqrt(r * r - x * x);
            var v = (b - a) / d;
            ret.Add(a + v * x + RotateCcw90(v) * y);
            if (y > 0)
                ret.Add(a + v * x - RotateCcw90(v) * y);
            return ret;
        }

        private static double Dist(double x1, double y1, double x2, double y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            return Sqrt(dx * dx + dy * dy);
        }

        private static double Dist(Point a, Point b)
        {
            return Dist(a.X, a.Y, b.X, b.Y);
        }


        public static Point RotateCcw90(Point p)
        {
            return new Point(-p.Y, p.X);
        }


    }


    public struct Point : IComparable<Point>
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public int CompareTo(Point b)
        {
            int cmp = X.CompareTo(b.X);
            if (cmp != 0)
                return cmp;
            return Y.CompareTo(b.Y);
        }

        public static Point operator +(Point p, Point p2)
        {
            return new Point(p.X + p2.X, p.Y + p2.Y);
        }

        public static Point operator -(Point p0, Point p)
        {
            return new Point(p0.X - p.X, p0.Y - p.Y);
        }

        public static Point operator *(Point p, double c)
        {
            return new Point(p.X * c, p.Y * c);
        }

        public static Point operator /(Point p, double c)
        {
            return new Point(p.X / c, p.Y / c);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public static bool operator <(Point lhs, Point rhs)
        {
            if (lhs.Y < rhs.Y)
                return true;

            return lhs.Y == rhs.Y && lhs.X < rhs.X;
        }

        public static bool operator >(Point lhs, Point rhs)
        {
            if (lhs.Y > rhs.Y)
                return true;

            return lhs.Y == rhs.Y && lhs.X > rhs.X;
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            return lhs.Y == rhs.Y && lhs.X == rhs.X;
        }

        public static bool operator !=(Point lhs, Point rhs)
        {
            return lhs.Y != rhs.Y || lhs.X != rhs.X;
        }

        public bool Equals(Point other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point && Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
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
