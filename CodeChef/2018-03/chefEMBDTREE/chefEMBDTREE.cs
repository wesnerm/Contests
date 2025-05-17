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
    #region Variables
    const int MOD = 1000000007;
    const int FactCache = 1000;
    const long BIG = long.MaxValue << 15;
    const double Eps = 1e-6;
    const int Den500 = 41;
    const int Den1000 = 57;
    List<Rational> slopes;
    List<Edge>[] g;
    TreeGraph tree;
    int n;
    int[][] pts;
    #endregion

    #region Enums

    public enum Scorings
    {
        Brute = 0,
        Manhattan = 1, // Fastest and dominates grid in speed and performance
        Half = 2,
        Random = 3, // Outperforms half, but is slower -- performance matches up with brute
        Grid = 4,
    }
    #endregion

    const Scorings Scoring = Scorings.Random;
    const int MinAngles = 300;
    const int CandidatePool = 20;
    const int RootCount = 2;

    const double Delta = .0001;
    const double LineDelta = .0001;
    const bool FullPath = false; // Use min height or full path

    const bool UseRectangle = true;
    const bool ShrinkWidths = true;
    const bool AdvanceForward = true;

    static bool Inited;

    public void Solve()
    {
        //Prologue();
        //WriteOut();

        n = Ni();
        g = ReadGraph(n);

        var process = Process.GetCurrentProcess();
        var time = process.TotalProcessorTime;
        var newPts = NewAlgorithm(CandidatePool);


        int[][] oldPts;

        oldPts = OldAlgorithm();
        var newSc = ScoreSpecial(newPts, Scorings.Random);
        if (newSc > ScoreSpecial(oldPts, Scorings.Random))
            newPts = oldPts;

        //var scaledPts = new int[newPts.Length][];
        //for (int i = 0; i < newPts.Length; i++)
         //   scaledPts[i] = new int[] { ScaleDown(newPts[i][0]), ScaleDown(newPts[i][1]) };
 
       //if (CheckValid(scaledPts, edges)
       //     && newSc > ScoreSpecial(scaledPts, Scorings.Random))
       //     newPts = scaledPts;

        time = process.TotalProcessorTime - time;


#if DEBUG
        var newScore = Score(newPts);
        var oldScore = Score(oldPts);

        if (Inited == false)
        {
            Inited = true;
            Error("");
            Error($"Scoring = {Scoring}");
            Error($"MinAngles = {MinAngles}");
            Error($"CandidatePool = {CandidatePool}");
            Error($"RootCount = {RootCount}");
            Error($"Delta = {Delta}");
            Error($"LineDelta = {LineDelta}");

            Error($"UseRectangle = {UseRectangle}");
            Error($"ShrinkWidths = {ShrinkWidths}");
            Error($"AdvanceForward = {AdvanceForward}");
            Error($"FullPath = {FullPath}");
            Error("");
        }

        Error($"NewScore={newScore}  OldScore={oldScore}  Ratio={newScore / oldScore:P3}");
        Error($"Elapsed Time = {time}");
#endif

 

        // Write out the best candidates
        foreach (var p in newPts)
            WriteLine($"{p[0]} {p[1]}");
    }

    public static bool CheckValid(int[][] points, int[][] edges)
    {
        var n = points.Length;
        for (int i = 0; i < n-1; i++)
        {
            var u = edges[i][0];
            var v = edges[i][1];
            var len = edges[i][2];

            var p1 = new Point2D(points[u][0], points[u][1]);
            var p2 = new Point2D(points[v][0], points[v][1]);
            //var d = Point2D.Dist2(p1, p2);
            //if (d < len)
            //    return false;

            for (int j = 0; j < i; j++)
            {
                var s = edges[j][0];
                var t = edges[j][1];

                if (t == u || t == v || s == u || s == v) continue;
                var p3 = new Point2D(points[s][0], points[s][1]);
                var p4 = new Point2D(points[t][0], points[t][1]);

                if (Point2D.SegmentsIntersect(p1, p2, p3, p4))
                    return false;
            }
        }
        return true;
    }


    public int ScaleDown(int x)
    {
        var f = 9.0;
        if (x < 5000) f = 1.0;
        return (int)Round(x * f);
    }

    void BuildTreeAndFixParents(int root)
    {
        tree = new TreeGraph(g, root);

        for (int i = 0; i < n; i++)
        {
            foreach (var e in g[i])
                if (tree.Depths[e.U] > tree.Depths[e.V])
                    Swap(ref e.U, ref e.V);
        }
    }


    public int[][] NewAlgorithm(int poolSize)
    {
        var pq = new SimpleHeap<Candidate>((a, b) => -a.EstimatedScore.CompareTo(b.EstimatedScore));

        foreach (var root in FindMinPathLength().Take(RootCount))
        {
            BuildTreeAndFixParents(root);

            var region = BuildRegions(root);
            int pos = 0;
            DfsInit(region, ref pos);
            int count = region.Upper + 1;
            slopes = CreateFractions(Max(MinAngles, count), UseRectangle);

            int limit = Min(slopes.Count >> 1, Max(0,slopes.Count - count) + 1);
            for (int i = 0; i < limit; i++)
            {
                pts = new int[n][];
                Dfs(region, i);

                var cand = new Candidate
                {
                    Index = i,
                    Points = pts,
                    EstimatedScore = ScoreSpecial(pts, Scoring)
                };
                pq.Enqueue(cand);
                while (pq.Count > poolSize) pq.Dequeue(); // Remove worst performers
            }
        }

        // Select best among finalists
        var best = double.MaxValue;

        if (pq.Count == 1)
        {
            pts = pq.Dequeue().Points;
        }
        else
        {
            while (pq.Count > 0)
            {
                var pop = pq.Dequeue();
                pop.RealScore = Score(pop.Points);
                //Error($"Index {pop.Index}/ {slopes.Count}");
                if (pop.RealScore < best)
                {
                    best = pop.RealScore;
                    pts = pop.Points;
                }
            }
        }

        return pts;
    }


    public class Candidate
    {
        public int Index;
        public int[][] Points;
        public double EstimatedScore;
        public double RealScore;
    }

#region Scoring

    public static double Score(int[][] pts)
    {
        var score = 0.0;
        for (int i = 0; i < pts.Length; i++)
            for (int j = 0; j < i; j++)
            {
                var dx = pts[i][0] - pts[j][0];
                var dy = pts[i][1] - pts[j][1];
                score += Sqrt(1.0 * dx * dx + 1.0 * dy * dy);
            }
        return score;
    }

    public static double ScoreSpecial(int[][] pts, Scorings scoring)
    {
        switch (scoring)
        {
            case Scorings.Half: return Score2(pts);
            case Scorings.Random: return Score3(pts);
            case Scorings.Manhattan: return ScoreManhattan(pts);
            case Scorings.Grid: return ScoreGrid(pts);
        }
        return Score(pts);
    }

    public static double Score2(int[][] pts)
    {
        var score = 0.0;
        for (int i = 0; i < pts.Length >> 1; i++)
            for (int j = 0; j < i; j++)
            {
                var dx = pts[i][0] - pts[j][0];
                var dy = pts[i][1] - pts[j][1];
                score += Sqrt(1.0 * dx * dx + 1.0 * dy * dy);
            }
        return score;
    }

    public static double Score3(int[][] pts)
    {
        var score = 0.0;
        int inc = 3;
        for (int i = 0; i < pts.Length; i++)
        {
            for (int j = 0; j < i; j += inc, inc ^= 3 ^ 4)
            {
                var dx = pts[i][0] - pts[j][0];
                var dy = pts[i][1] - pts[j][1];
                score += Sqrt(1.0 * dx * dx + 1.0 * dy * dy);
            }

        }
        return score;
    }

    const int Divisions = 32;
    static int[,] grid = new int[Divisions, Divisions];
    public static unsafe double ScoreGrid(int[][] pts)
    {
        Array.Clear(grid, 0, pts.Length);
        int maxX = 0;
        int maxY = 0;

        foreach (var p in pts)
        {
            maxX = Max(p[0], maxX);
            maxY = Max(p[1], maxY);
        }

        var divX = Math.Max(1, (maxX + 1.0) / Divisions);
        var divY = Math.Max(1, (maxY + 1.0) / Divisions);
        var cellDist_2 = Sqrt(.5 * .5 + .5 * .5) / 2;//Sqrt(divX * divX + divY * divY) / 2;

        foreach (var p in pts)
            grid[(int)(p[0] / divX), (int)(p[1] / divY)]++;


        int pos = 0;
        var xs = stackalloc int[1001];
        var ys = stackalloc int[1001];

        double result = 0;
        for (int i = 0; i < Divisions; i++)
            for (int j = 0; j < Divisions; j++)
            {
                if (grid[i, j] != 0)
                {
                    for (int k = 0; k < pos; k++)
                    {
                        var ii = xs[k];
                        var jj = ys[k];
                        var dx = i - ii;
                        var dy = j - jj;
                        result += Sqrt(dx * dx + dy * dy) * grid[i, j] * grid[ii, jj];
                    }

                    xs[pos] = i;
                    ys[pos] = j;
                    pos++;

                    result += cellDist_2 * grid[i, j] * (grid[i, j] - 1);
                }
            }

        return result;
    }


    static int[] scratch;
    public static double ScoreManhattan(int[][] pts)
    {
        if (scratch == null || scratch.Length != pts.Length)
            scratch = new int[pts.Length];

        for (int i = 0; i < pts.Length; i++)
            scratch[i] = pts[i][0];

        double result = PairwiseManhattanSumX(scratch);

        for (int i = 0; i < pts.Length; i++)
            scratch[i] = pts[i][1];

        result += PairwiseManhattanSumX(scratch);
        return result;
    }

    public static long PairwiseManhattanSumX(int[] array)
    {
        Sort(array);
        long result = 0, sum = 0;
        int n = array.Length;
        for (long i = 0; i < n; i++)
        {
            result += i * array[i] - sum;
            sum += array[i];
        }
        return result;
    }

#endregion

#region MinPathLength
    public HashSet<int> FindMinPathLength()
    {
        var best = new HashSet<int>();
        var bestLength = long.MaxValue;
        for (int i = 0; i < g.Length; i++)
        {
            var t = FindPathLength(i);
            if (t < bestLength)
            {
                best.Clear();
                bestLength = t;
                best.Add(i);
            }
            else if (t == bestLength)
            {
                best.Add(i);
            }
        }

        return best;
    }

    public long FindPathLength(int u, int p = -1)
    {
        var path = 0L;
        foreach (var e in g[u])
        {
            var v = e.Other(u);
            if (v == p) continue;
            var childlen = FindPathLength(v, u);
            path = Max(path, (FullPath ? e.W : 1) + childlen);
        }
        return path;
    }
#endregion

    void Dfs(Region region, int offset)
    {
        pts[region.U] = new[] { region.X, region.Y };

        var parent = region.Parent;
        var list = region.Children;
        int count = list.Count;

        for (int i = 0; i < count; i++)
        {
            var r = list[i];
            if (!r.ParentFoldBack) continue;

            Debug.Assert(parent != null);
            var minlen = r.Edge.W + LineDelta;
            var rat = slopes[r.Lower + offset];

            var parent2 = parent;
            if (region.SkipParent && parent2.Parent != null)
                parent2 = parent2.Parent;

            r.X = parent2.X + rat.D;
            r.Y = parent2.Y + rat.N;
            var rat2 = slopes[region.Lower + offset];
            while (Distance(r.X - region.X, r.Y - region.Y) < minlen)
            {
                region.X += rat2.D;
                region.Y += rat2.N;
            }
            pts[region.U][0] = region.X;
            pts[region.U][1] = region.Y;
        }

        for (int i = 0; i < count; i++)
        {
            var r = list[i];
            if (!r.ParentFoldBack)
            {
                var minlen = r.Edge.W + Delta;
                var rat = slopes[r.Lower + offset];
                var ratlen = Distance(rat.N, rat.D);
                var multiplier = (int)Max(1, Ceiling(minlen / ratlen));
                r.X = region.X + multiplier * rat.D;
                r.Y = region.Y + multiplier * rat.N;
            }

            Dfs(r, offset);
        }
    }

    int CompareSizes(Region a, Region b)
    {
        int cmp = (a.Weight*a.Size).CompareTo(b.Weight*b.Size);
        if (cmp != 0) return cmp;
        return a.Weight.CompareTo(b.Weight);
        //return (a.TotalWeight - a.Weight).CompareTo(b.TotalWeight - b.Weight);
    }

    void DfsInit(Region region, ref int pos)
    {
        var parent = region.Parent;
        var list = region.Children;
        int count = list.Count;
        bool canFold = parent != null && !region.ParentFoldBack;

        region.Lower = region.Upper = pos;

        list.Sort(CompareSizes);

        var leaves = new List<Region>();
        var normal = new List<Region>();
        var ends = new List<Region>();
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var v = list[i];

            if (canFold)
            {
                if (v.Size == 1)
                {
                    leaves.Add(v);
                    continue;
                }

                if (ends.Count < 2)
                {
                    ends.Add(v);
                    continue;
                }
            }

            normal.Add(v);
        }

        if (normal.Count > 0
            && canFold
            && CheckTriple(normal[0])
            && leaves.Count + ends.Count == 0)
        {
            normal[0].SkipParent = true;
        }

        leaves.Reverse();
        normal.Reverse();

        foreach (var v in leaves)
            v.ParentFoldBack = true;

        foreach (var v in ends)
        {
            if (CheckTriple(v))
                v.SkipParent = true;
            v.ParentFoldBack = true;
        }

        list.Clear();
        if (ends.Count == 2)
        {
            ends[1].Flip = true;
            list.Add(ends[1]);
            ends.RemoveAt(1);
        }
        list.AddRange(normal);
        list.AddRange(leaves);
        list.AddRange(ends);

        // Optimizations:
        // Determine best node to fold back
        // Cost of increase in parent height * #(normal nodes) vs #(size of endnode) * diff in height
        // Fold back both ends
        // Fold back leaves
        // O(n lg n) intersection tests

        for (int i = 0; i < count; i++)
        {
            var r = list[i];
            if (!r.Flip && (AdvanceForward || r.ParentFoldBack && i == 0)) pos++;

            var save = pos;
            DfsInit(r, ref pos);
            var upper = Max(region.Upper, r.Upper);

            if (r.Flip)
            {
                Debug.Assert(pos==upper);
                Flip(r, save, pos);
                region.Upper = pos+1;
                region.Lower = pos+1;
                if (AdvanceForward) pos++;
            }
            else
            {
                region.Upper = upper;
            }

            if (!r.Flip && !AdvanceForward) pos++;
        }
    }


    public void Flip(Region region, int left, int right)
    {
        region.Upper = Reflect(region.Upper, left, right);
        region.Lower = Reflect(region.Lower, left, right);
        foreach (var v in region.Children)
            Flip(v, left, right);
    }

    public int Reflect(int v, int left, int right)
    {
        return left + right - v;
    }

    public bool CheckTriple(Region child)
    {
        if (child.Parent.SkipParent) return false;
        if (child.Children.Count != 1) return false;
        var grandchild = child.Children[0];
        return grandchild.Children.Count > 1;
    }

    Region BuildRegions(int u)
    {
        var region = BuildRegions(u, null, null);
        Recalc(region);
        Debug.Assert(region.Size == n);
        return region;
    }

    Region BuildRegions(int u, Region parent, Edge edge)
    {
        var p = parent?.U ?? -1;
        var depth = parent?.Depth + 1 ?? 0;
        var region = new Region { U = u, Parent = parent, Depth = depth + 1, Edge = edge };
        region.Children.Capacity = g[u].Count;

        foreach (var e in g[u])
        {
            var v = e.Other(u);
            if (v == p) continue;
            var r = BuildRegions(e.V, region, e);
            region.Children.Add(r);
        }
        return region;
    }

    void Recalc(Region region)
    {
        int pathLength = 0;
        int widths = 0;
        int size = 1;
        long dists = 0;
        int size2 = 0;
        int height = 0;

        foreach (var r in region.Children)
        {
            Recalc(r);
            widths += r.Width;
            pathLength = Max(pathLength, r.PathLength);
            size += r.Size;
            dists += r.Dists;
            size2 += r.TotalWeight;
            height = Max(height, r.Height);
        }

        int weight = region.Weight;
        region.Size = size;
        region.TotalWeight = weight + size2;
        region.PathLength = weight + pathLength;
        region.Width = Max(1, widths);
        region.Height = height + 1;
        region.Dists = dists + size;
    }

    static double Distance(double x, double y)
    {
        return Sqrt(x * x + y * y);
    }

    public class Region
    {
        public int U;
        public int X, Y;
        public int M = 1, B;
        public int Lower;
        public int Upper;
        public int Depth;
        public Region Parent;
        public List<Region> Children = new List<Region>();
        public Edge Edge;
        public int Width;
        public int PathLength;
        public int Size;
        public long Dists;
        public int TotalWeight;
        public int Height;
        public int Weight => Edge?.W ?? 0;
        public bool SkipParent;
        public bool Flip;
        public bool AllLeaves => Width == 1 && Height == 2;

        public bool ParentFoldBack;
        public bool DontFoldFlag;
    }


    public static List<Rational> CreateFractions(int width, bool rectangle)
    {
        var farey = new List<Rational>(1100);

        int j = 1;
        while (true)
        {
            int listSize = farey.Count + 2;
            if (rectangle)
                listSize += farey.Count + 1;
            if (listSize >= width) break;

            j++;
            farey.Clear();
            farey.AddRange(FareySequence(j));
        }

        var list = new List<Rational>(2065);
        list.Add(new Rational { N = 0, D = 1 });
        list.AddRange(farey);
        list.Add(new Rational { N = 1, D = 1 });

        //if (rectangle)
        {
            farey.Reverse();
            list.AddRange(farey.Select(f => f.Reciprocal));
            list.Add(new Rational { N = 1, D = 0 });
        }

#if DEBUG
        for (int i = 1; i < list.Count; i++)
            Debug.Assert(list[i - 1].CompareTo(list[i]) < 0);
#endif

        return list;
    }


    #region X
    public int[][] OldAlgorithm()
    {
        var best = double.MaxValue;
        int[][] bestPoints = null;

        foreach (var root in FindMinPathLength().Take(1))
        {
            BuildTreeAndFixParents(root);

            var region = BuildRegions(root);
            int pos = 0;
            DfsInitOld(region, ref pos);
            int count = region.Upper + 1;
            slopes = CreateFractions(count, UseRectangle);

            int limit = Min(slopes.Count >> 1,
                Max(0, slopes.Count - count) + 1);
            for (int i = 0; i < limit; i++)
            {
                pts = new int[n][];
                DfsOld(region, i);
                var score = ScoreSpecial(pts, Scorings.Half);
                if (score < best)
                {
                    best = score;
                    bestPoints = pts;
                }
            }
        }

        return bestPoints;
    }

    void DfsOld(Region region, int offset)
    {
        pts[region.U] = new[] { region.X, region.Y };

        var parent = region.Parent;
        var list = region.Children;
        int count = list.Count;

        bool handleLines = parent != null
                           && ShouldFold(region)
                           && count > 0
                           && !CheckTriple(list[count - 1]);
        if (handleLines)
        {
            var r = list[count - 1];
            var minlen = r.Edge.W + LineDelta;
            var rat = slopes[r.Lower + offset];

            var parent2 = parent;
            if (region.SkipParent)
            {
                parent2 = parent2.Parent;
                //if (parent2 != null && parent2.Parent != null)
                //    parent = parent2.Parent;
            }

            r.X = parent2.X + rat.D;
            r.Y = parent2.Y + rat.N;
            var rat2 = slopes[region.Lower + offset];
            while (Distance(r.X - region.X, r.Y - region.Y) < minlen)
            {
                region.X += rat2.D;
                region.Y += rat2.N;
            }

            pts[region.U][0] = region.X;
            pts[region.U][1] = region.Y;
            r.DontFoldFlag = true;
            DfsOld(r, offset);
            count--;
        }

        for (int i = 0; i < count; i++)
        {
            var r = list[i];
            var minlen = r.Edge.W + Delta;
            var rat = slopes[r.Lower + offset];
            var ratlen = Distance(rat.N, rat.D);
            var multiplier = (int)Max(1, Ceiling(minlen / ratlen));
            r.X = region.X + multiplier * rat.D;
            r.Y = region.Y + multiplier * rat.N;
            DfsOld(r, offset);
        }
    }

    void DfsInitOld(Region region, ref int pos)
    {
        region.Lower = region.Upper = pos;

        var parent = region.Parent;
        var list = region.Children;
        int count = list.Count;
        //children.Sort((a, b) => a.Edge.W.CompareTo(b.Edge.W));
        list.Sort((a, b) =>
        {
            int cmp = a.Size.CompareTo(b.Size);
            if (cmp != 0)
                return cmp;
            return a.TotalWeight.CompareTo(b.TotalWeight);
        });
        //list.Sort((a, b) => a.PathLength.CompareTo(b.PathLength));
        bool handleLines = parent != null && count > 0 && ShouldFold(region);

        for (int i = 0; i < count; i++)
        {
            var r = list[i];

            if (i == count - 1 && handleLines)
            {
                if (CheckTriple(r))
                {
                    r.SkipParent = true;
                }
                else
                {
                    if (!AdvanceForward && i == 0) pos++;
                    r.DontFoldFlag = true;
                }
            }

            if (AdvanceForward) pos++;
            DfsInitOld(r, ref pos);
            region.Upper = Max(region.Upper, r.Upper);
            if (!AdvanceForward) pos++;
        }
    }

    public bool ShouldFold(Region region)
    {
        if (region.DontFoldFlag)
            return false;

        //var children = region.Children;
        //if (children.Count == 1 && children[0].Children.Count > 1) return false;

        return true;
    }
    #endregion

    #region Helpers
    public static IEnumerable<Rational> FareySequence(int n)
    {
        int a = 0, b = 1, c = 1, d = n;
        while (d > 1)
        {
            int k = (n + b) / d;
            int aa = a, bb = b;
            a = c;
            b = d;
            c = k * c - aa;
            d = k * d - bb;
            yield return new Rational { N = a, D = b };
        }
    }


    public static int Gcd(int n, int m)
    {
        while (true)
        {
            if (m == 0) return n >= 0 ? n : -n;
            n %= m;
            if (n == 0) return m >= 0 ? m : -m;
            m %= n;
        }
    }

    public class Rational : IComparable<Rational>, IEquatable<Rational>
    {
        public int N;
        public int D;

        public Rational()
        {

        }

        public Rational(int n, int d)
        {
            N = n;
            D = d;
            var g = Gcd(n, d);
            if (g < 0) g = -g;
            if (g > 1)
            {
                N /= g;
                D /= g;
            }
        }

        public Rational Reciprocal => new Rational { D = N, N = D };

        public int CompareTo(Rational other)
        {
            return (N * other.D).CompareTo(D * other.N);
        }

        public bool Equals(Rational other)
        {
            return other != null && N == other.N && D == other.D;
        }

        public override string ToString() => $"{N} / {D}";
    }

#endregion

#region Graph Library
#region Helpers

    public class Query : IComparable<Query>
    {
        public int I;
        public int U;
        public int V;

        public override string ToString()
        {
            return $"#{I} ({U + 1},{V + 1})";
        }

        public int CompareTo(Query other)
        {
            var cmp = U.CompareTo(other.U);
            if (cmp != 0) return cmp;
            cmp = V.CompareTo(other.V);
            if (cmp != 0) return cmp;
            return I.CompareTo(other.I);
        }
    }

    public class Edge
    {
        public int I;
        public int U;
        public int V;
        public int W;

        public int Other(int u) => U == u ? V : U;

        public override string ToString()
        {
            return $"#{I} ({U + 1},{V + 1})  {W}";
        }
    }


    int[][] edges;
    public List<Edge>[] ReadGraph(int n)
    {
        var g = NewGraph(n);
        edges = new int[n - 1][];

        for (int i = 1; i < n; i++)
        {
            var u = Ni() - 1;
            var v = Ni() - 1;
            var w = Ni();
            var edge = new Edge { U = u, V = v, I = i, W = w };
            edges[i - 1] = new [] {u, v, w};
            g[u].Add(edge);
            g[v].Add(edge);
        }

        return g;
    }

    public static List<Edge>[] NewGraph(int n)
    {
        var g = new List<Edge>[n];
        for (int i = 0; i < n; i++)
            g[i] = new List<Edge>();
        return g;
    }

#endregion

#region TreeGraph
    public class TreeGraph
    {
#region Variables
        public int[] Parents;
        public int[] Queue;
        public int[] Depths;
        public int[] Sizes;
        public List<Edge>[] Graph;
        public int TreeSize;
        public int Separator;
        public int Root;

        bool sizesInited;
#endregion

#region Constructor
        public TreeGraph(List<Edge>[] g, int root = 0, int avoid = -1)
        {
            Graph = g;
            if (root >= 0)
                Init(root, avoid);
        }
#endregion

#region Methods
        public void Init(int root, int avoid = -1)
        {
            var g = Graph;
            int n = g.Length;
            Root = root;
            Separator = avoid;

            Queue = new int[n];
            Parents = new int[n];
            Depths = new int[n];

            for (int i = 0; i < Parents.Length; i++)
                Parents[i] = -1;

            Queue[0] = root;

            int treeSize = 1;
            for (int p = 0; p < treeSize; p++)
            {
                int cur = Queue[p];
                var par = Parents[cur];
                foreach (var e in g[cur])
                {
                    var child = e.Other(cur);
                    if (child != par && child != avoid)
                    {
                        Queue[treeSize++] = child;
                        Parents[child] = cur;
                        Depths[child] = Depths[cur] + 1;
                    }
                }
            }

            TreeSize = treeSize;
        }

        public void InitSizes()
        {
            if (sizesInited)
                return;

            if (Sizes == null)
                Sizes = new int[Graph.Length];
            sizesInited = true;

            if (Separator >= 0)
                Sizes[Separator] = 0;
            for (int i = TreeSize - 1; i >= 0; i--)
            {
                int current = Queue[i];
                Sizes[current] = 1;
                foreach (var edge in Graph[current])
                {
                    var e = edge.Other(current);
                    if (Parents[current] != e)
                        Sizes[current] += Sizes[e];
                }
            }
        }
#endregion
    }


#endregion

#endregion

#region Helpers
    [DebuggerStepThrough]
    public class SimpleHeap<T>
    {
        Comparison<T> comparison;
        List<T> list = new List<T>();

        public SimpleHeap(Comparison<T> comparison = null)
        {
            this.comparison = comparison ?? Comparer<T>.Default.Compare;
        }

        public int Count => list.Count;

        public T FindMin() => list[0];

        public T Dequeue()
        {
            var pop = list[0];
            var elem = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            if (list.Count > 0) ReplaceTop(elem);
            return pop;
        }

        public void ReplaceTop(T elem)
        {
            int i = 0;
            list[i] = elem;
            while (true)
            {
                int child = 2 * i + 1;
                if (child >= list.Count)
                    break;

                if (child + 1 < list.Count && comparison(list[child], list[child + 1]) > 0)
                    child++;

                if (comparison(list[child], elem) >= 0)
                    break;

                Swap(i, child);
                i = child;
            }
        }

        public void Enqueue(T push)
        {
            list.Add(push);
            int i = list.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (comparison(list[parent], push) <= 0)
                    break;
                Swap(i, parent);
                i = parent;
            }
        }

        void Swap(int t1, int t2)
        {
            T tmp = list[t1];
            list[t1] = list[t2];
            list[t2] = tmp;
        }


    }

#endregion

#region Library

#region Common
    partial void TestData();

    static void Swap<T>(ref T a, ref T b)
    {
        var tmp = a;
        a = b;
        b = tmp;
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
    }
#endregion
#endregion
}


#region Geometry

[DebuggerStepThrough]
public struct Point2D
{
    public double X;
    public double Y;

    public Point2D(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    public static Point2D operator +(Point2D p, Point2D p2)
    {
        return new Point2D(p.X + p2.X, p.Y + p2.Y);
    }

    public static Point2D operator -(Point2D p0, Point2D p)
    {
        return new Point2D(p0.X - p.X, p0.Y - p.Y);
    }

    public static Point2D operator *(Point2D p, double c)
    {
        return new Point2D(p.X * c, p.Y * c);
    }

    public static Point2D operator /(Point2D p, double c)
    {
        return new Point2D(p.X / c, p.Y / c);
    }

    public override string ToString()
    {
        return "(" + X + "," + Y + ")";
    }

    public double LengthSquared => X * X + Y * Y;


    public static bool operator <(Point2D lhs, Point2D rhs)
    {
        if (lhs.Y < rhs.Y)
            return true;

        return lhs.Y == rhs.Y && lhs.X < rhs.X;
    }

    public static bool operator >(Point2D lhs, Point2D rhs)
    {
        if (lhs.Y > rhs.Y)
            return true;

        return lhs.Y == rhs.Y && lhs.X > rhs.X;
    }

    public static bool operator ==(Point2D lhs, Point2D rhs)
    {
        return lhs.Y == rhs.Y && lhs.X == rhs.X;
    }

    public static bool operator !=(Point2D lhs, Point2D rhs)
    {
        return lhs.Y != rhs.Y || lhs.X != rhs.X;
    }

    public bool Equals(Point2D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Point2D && Equals((Point2D)obj);
    }

    public double Cross(Point2D vec2)
    {
        return X * vec2.Y - Y * vec2.X;
    }

    public double Dot(Point2D vector)
    {
        return X * vector.X + Y * vector.Y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }

    private const double Inf = double.PositiveInfinity;
    private const double Eps = 1e-12;

    public static bool LinesParallel(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        return Abs((b - a).Cross(c - d)) < Eps;
    }

    public static bool LinesCollinear(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        return LinesParallel(a, b, c, d)
               && Abs((a - b).Cross(a - c)) < Eps
               && Abs((c - d).Cross(c - a)) < Eps;
    }

    private static double Dot(Point2D p, Point2D q)
    {
        return p.X * q.X + p.Y * q.Y;
    }

    public static double Dist2(Point2D p, Point2D q)
    {
        return (p - q).LengthSquared;
    }


    public static bool SegmentsIntersect(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        if (LinesCollinear(a, b, c, d))
        {
            if (Dist2(a, c) < Eps || Dist2(a, d) < Eps ||
                Dist2(b, c) < Eps || Dist2(b, d) < Eps) return true;
            if ((c - a).Dot(c - b) > 0 && (d - a).Dot(d - b) > 0 && (c - b).Dot(d - b) > 0)
                return false;
            return true;
        }

        var ab = b - a;
        if ((d - a).Cross(ab) * (c - a).Cross(ab) > 0) return false;
        var cd = d - c;
        return (a - c).Cross(cd) * (b - c).Cross(cd) <= 0;
    }

}


#endregion


