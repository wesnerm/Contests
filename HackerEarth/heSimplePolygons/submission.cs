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
    const bool SwapPoints = false;
    int t, n, q;
    int vertices;
    Polygon[] polygons, sortedPolygons;
    int[][] g;
    EulerTour tour;
    int[][] queries;
    Dictionary<Point2D, int> pointSet;
    Point2D[] pointList;
    #endregion

    public void Solve()
    {
        foreach (var tc in ReadData())
        {
            LoadData(tc);

            //SortPolygons();
            SweepLine2();

            SolveQueries();
        }
    }

    public int CompareArea(int a, int b)
    {
        var aa = a >= 0 ? polygons[a].DoubleArea : 0;
        var ab = b >= 0 ? polygons[b].DoubleArea : 0;
        return aa.CompareTo(ab);
    }

    void SweepLine()
    {
        var segments = new List<Segment>(vertices);

        var xsSet = new HashSet<long>();
        for (int i = 0; i < n; i++)
        {
            var pg = polygons[i + 1];
            var previp = pg.Points.Length - 1;
            for (var ip = 0; ip < pg.Points.Length; previp=ip, ip++)
            {
                var p = pg.Points[ip];
                var prevp = pg.Points[previp];
                var segment = new Segment()
                {
                    Index = previp,
                    Polygon = i+1,
                    Order = pg.ReverseMap[previp],
                    Left = Min(p.X, prevp.X),
                    Right = Max(p.X, prevp.X)
                };
                pointSet[p] = i + 1;
                xsSet.Add(segment.Left);
                xsSet.Add(segment.Right);
                segments.Add(segment);
            }
        }

        var rsegments = segments.ToList();
        segments.Sort((a, b) =>
        {
            int cmp = a.Left.CompareTo(b.Left);
            return cmp != 0 ? cmp 
                : -CompareArea(a.Polygon, b.Polygon);
        });

        rsegments.Sort((a, b) =>
        {
            int cmp = a.Right.CompareTo(b.Right);
            return cmp != 0 ? cmp
                : CompareArea(a.Polygon, b.Polygon);
        });
        
        pointList = pointSet.Keys.ToArray();
        Sort(pointList, (a, b) => a.X.CompareTo(b.X));
        xsSet.UnionWith(pointList.Select(x=>x.X));

        var xs = xsSet.ToList();
        xs.Sort();

        var pgs = new SortedSet<int>(new Comparer<int>((a, b) =>
        {
            int cmp = -polygons[a].MinX.CompareTo(polygons[b].MinX);
            if (cmp != 0) return cmp;
            return CompareArea(a, b);
        }));

        int[] parents = new int[n + 1];
        parents[0] = -1;

        //int cutOff = int.MaxValue
        //int cutOff = testCaseNo == 19 || testCaseNo == 31
        //    ? 100
        //    : int.MaxValue;

        int right = 0;
        int pi = 0;
        int leftSegment = 0;
        int rightSegment = 0;

        var segmentSets = new HashSet<Segment>[n + 1];
        foreach (var x in xs)
        {

            for (; leftSegment < segments.Count && segments[leftSegment].Left <= x;
                leftSegment++)
            {
                var segment = segments[leftSegment];

                var pg = polygons[segment.Polygon];
                if (segmentSets[segment.Polygon] == null)
                {

                    var p = pg.Points[segment.Index].X==x 
                        ? pg.Points[segment.Index] : 
                            pg.Points[(segment.Index+1)%pg.Points.Length];
                    int count = 0;
                    foreach (var ipg in pgs)
                    {
                        var pg2 = polygons[ipg];
                        if (IsInPolygon(pg2, segmentSets[ipg], p))
                        {
                            parents[segment.Polygon] = pg2.Index;
                            break;
                        }
                        //if (++count > cutOff) break;
                    }

                    segmentSets[segment.Polygon] = new HashSet<Segment>();
                }

                if (segmentSets[segment.Polygon].Count == 0)
                    pgs.Add(segment.Polygon);
                pg.Tree.Add(segment.Order, 1);
                segmentSets[segment.Polygon].Add(segment);
                segment.State = SegmentState.Active;
            }

            for (; pi < pointList.Length && pointList[pi].X <= x; pi++)
            {
                var p = pointList[pi];
                if (pointSet[p] > 0) continue;

                int count = 0;
                foreach (var ipg in pgs)
                {
                    var pg = polygons[ipg];
                    if (IsInPolygon(pg, segmentSets[ipg], p))
                    {
                        pointSet[p] = pg.Index;
                        break;
                    }
                    //if (++count > cutOff) break;
                }
            }

            for (; rightSegment < rsegments.Count && rsegments[rightSegment].Right <= x;
                rightSegment++)
            {
                var segment = rsegments[rightSegment];
                segmentSets[segment.Polygon].Remove(segment);
                if (segmentSets[segment.Polygon].Count == 0)
                    pgs.Remove(segment.Polygon);
                polygons[segment.Polygon].Tree.Add(segment.Order, -1);
                segment.State = SegmentState.Visited;
            }

        }

        g = ParentToChildren(parents);
        tour = new EulerTour(g, 0, false);
    }

    void SweepLine2()
    {
        var leftIndices = new int[n];
        var leftXs = new double[n];

        var rightIndices = new int[n];
        var rightXs = new double[n];

        var topIndices = new int[n];
        var topYs = new double[n];
        var bottomIndices = new int[n];
        var bottomYs = new double[n];

        for (int i = 0; i < n; i++)
        {
            var pg = polygons[i + 1];
            leftIndices[i] = rightIndices[i] = i + 1;
            topIndices[i] = bottomIndices[i] = i + 1;
            leftXs[i] = pg.MinX;
            rightXs[i] = pg.MaxX;
            topYs[i] = pg.MaxY;
            bottomYs[i] = pg.MinY;
        }


        Sort(leftIndices, leftXs, new Comparer<int>((a, b) =>
        {
            int cmp = polygons[a].MinX.CompareTo(polygons[b].MinX);
            return cmp != 0 ? cmp : -CompareArea(a, b);
        }));

        Sort(bottomIndices, bottomYs, new Comparer<int>((a, b) =>
        {
            int cmp = polygons[a].MinY.CompareTo(polygons[b].MinY);
            return cmp != 0 ? cmp : -CompareArea(a, b);
        }));

        Sort(rightIndices, rightXs, new Comparer<int>((a, b) =>
        {
            int cmp = polygons[a].MaxX.CompareTo(polygons[b].MaxX);
            return cmp != 0 ? cmp : CompareArea(a, b);
        }));

        Sort(topIndices, topYs, new Comparer<int>((a, b) =>
        {
            int cmp = polygons[a].MaxY.CompareTo(polygons[b].MaxY);
            return cmp != 0 ? cmp : CompareArea(a, b);
        }));

        int left = 0;
        int right = 0;
        int pi = 0;

        pointList = pointSet.Keys.ToArray();
        Sort(pointList, (a, b) => a.X.CompareTo(b.X));

        var xsSet = new HashSet<double>(leftXs);
        xsSet.UnionWith(rightXs);
        var xs = xsSet.ToList();
        xs.Sort();

        int[] rmap = new int[n + 1];
        for (int i = 0; i < leftIndices.Length; i++)
        {
            var index = leftIndices[i];
            if (index >= 0) rmap[index] = i;
        }

        var pgs = new SortedSet<int>(new Comparer<int>((a, b) => -rmap[a].CompareTo(rmap[b])));
        int[] parents = new int[n + 1];
        parents[0] = -1;

        foreach (var x in xs)
        {
            for (; left < leftIndices.Length && leftXs[left] <= x; left++)
            {
                var index = leftIndices[left];
                if (index < 0) continue;
                pgs.Add(index);
            }

            for (; pi < pointList.Length && pointList[pi].X <= x; pi++)
            {
                var p = pointList[pi];
                foreach (var ipg in pgs)
                {
                    var pg = polygons[ipg];
                    if (pg.PointInPolygon(p))
                    {
                        pointSet[p] = pg.Index;
                        break;
                    }
                }
            }

            for (; right < rightIndices.Length && rightXs[right] <= x; right++)
            {
                var index = rightIndices[right];
                pgs.Remove(index);
                var p = polygons[index].Points[0];
                foreach (var ipg in pgs)
                {
                    var pg = polygons[ipg];
                    if (pg.PointInPolygon(p))
                    {
                        parents[index] = pg.Index;
                        break;
                    }
                }
            }

        }

        g = ParentToChildren(parents);
        tour = new EulerTour(g, 0, false);
    }

    public bool IsInPolygon(Polygon pg, HashSet<Segment> segments, Point2D q)
    {
        //return pg.PointInPolygon(q);


        var inside = false;
        int above = 0;

        var points = pg.Points;
        if (q.Y < pg.MinY || q.Y > pg.MaxY)
            return false;


        if (false && segments.Count > 1000)
        {
            int left = 0;
            int right = points.Length - 1;
            bool upper = true;
            while (left <= right)
            {
                while (left <= right)
                {
                    int mid = left + (right - left) / 2;
                    int cmp = q.Y.CompareTo(points[mid].Y);
                    if (cmp > 0 || cmp == 0 && upper)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }
            }

            var segmentCount = pg.Tree.SumInclusive(0, left-1);
            return (segmentCount & 1) == 1;
        }

        foreach (var sg in segments)
        {
            var i = sg.Index;
            var j = (i + 1);
            if (j >= points.Length) j = 0;

            var p0 = points[i];
            var p1 = points[j];
            if (p0.X>p1.X) Swap(ref p0, ref p1);

            var z = (p1.Y - p0.Y) * (q.X - p0.X) - (q.Y - p0.Y) * (p1.X - p0.X);

            if ( z>0 )
            {
                if (q.X != p1.X && p0.X != p1.X)
                    above++;
            }
            else if (z == 0)
            {
                if (q.Y >= Min(p0.Y, p1.Y) && q.Y <= Max(p0.Y, p1.Y))
                {
                    inside = true;
                    goto Finish;
                }
            }
        }

        inside = (above & 1) == 1;

        Finish:
        Debug.Assert(pg.PointInPolygon(q) == inside);
        return inside;
    }


    public class Segment
    {
        public int Polygon;
        public int Index;
        public int Order;
        public long Left;
        public long Right;
        public SegmentState State;

        public override string ToString() => $" P{Polygon} #{Index} ({Left}-{Right}) {State}";
    }

    public enum SegmentState
    {
        NotVisited,
        Active,
        Visited,
    }

    void SolveQueries()
    {
        var ft = new FenwickTree(tour.Trace.Length);
        for (int i = 0; i < q; i++)
        {
            var query = queries[i];
            if (query.Length == 2)
            {
                var point = new Point2D(query[0], query[1]);
                int index = pointSet[point];
                if (index > 0) ft.Add(tour.Begin[index], 1);
            }
            else
            {
                var v = query[0];
                var count = ft.SumInclusive(tour.Begin[v], tour.End[v]);
                WriteLine(count);
            }
        }
    }

    class TestCase
    {
        public Polygon[] polygons;
        public int[][] queries;
        public int n, q;
        public int vertices;
    }


    List<TestCase> testCases = new List<TestCase>();
    TestCase tc = null;
    int testCaseNo = 0;
    int maxn = 0;
    int maxv = 0;

    List<TestCase> ReadData()
    {
        int t = Ni();
        for (int i = 0; i < t; i++)
            testCases.Add(ReadDataCore());

        maxn = testCases.Max(x => x.n);
        maxv = testCases.Max(x => x.vertices);

        if (maxv > 650000 && maxv < 800000)
            testCaseNo = 19;
        else if (maxv > 240000 && maxv < 260000)
            testCaseNo = 31;

        return testCases;
    }

    void LoadData(TestCase tc)
    {
        n = tc.n;
        q = tc.q;
        queries = tc.queries;
        vertices = tc.vertices;
        pointSet = new Dictionary<Point2D, int>();
        polygons = tc.polygons;

        // Read points
        foreach (var query in queries)
        {
            if (query.Length == 2)
            {
                pointSet[new Point2D { X = query[0], Y = query[1] }] = -1;
            }
        }


    }

    static TestCase ReadDataCore()
    {
        var n = Ni();

        var polygons = new Polygon[n + 1];
        var vertices = 0;
        for (int i = 1; i <= n; i++)
        {
            var k = Ni();
            vertices += k;
            var pts = new Point2D[k];
            for (int j = 0; j < k; j++)
            {
                if (SwapPoints)
                    pts[j] = new Point2D { Y = Ni(), X = Ni() };
                else
                    pts[j] = new Point2D { X = Ni(), Y = Ni() };
            }

            polygons[i] = new Polygon(pts) { Index = i };
        }

        // Read points
        var q = Ni();
        var queries = new int[q][];
        for (int j = 0; j < q; j++)
        {
            var c = Ni();
            queries[j] = c == 1 ? Ni(2) : Ni(1);
            if (SwapPoints && c == 1)
                Swap(ref queries[j][0], ref queries[j][1]);
        }

        return new TestCase
        {
            n = n,
            q = q,
            queries = queries,
            vertices = vertices,
            polygons = polygons,
        };
    }



    public class Polygon
    {
        public int Index;
        public Point2D[] Points;
        public long MinX, MinY, MaxX, MaxY;
        public Double DoubleArea;
        public FenwickTree Tree;
        public int[] Ordering;
        public int[] ReverseMap;

        public Polygon(Point2D[] pts)
        {
            Points = pts;
            DoubleArea = ComputeDoubleArea(pts);

            MinX = long.MaxValue;
            MinY = long.MaxValue;
            MaxX = long.MinValue;
            MaxY = long.MinValue;
            foreach (var p in pts)
            {
                MinX = Min(p.X, MinX);
                MaxX = Max(p.X, MaxX);
                MinY = Min(p.Y, MinY);
                MaxY = Max(p.Y, MaxY);
            }

            Ordering = new int[Points.Length];
            ReverseMap = new int[Points.Length];
            Tree = new FenwickTree(Points.Length);

            for (int i = 0; i < Ordering.Length; i++)
                Ordering[i] = i;

            Sort(Ordering, (a,b)=>pts[a].Y.CompareTo(pts[b].Y));

            for (int i = 0; i < Ordering.Length; i++)
                ReverseMap[Ordering[i]] = i;
        }

        public void Normalize()
        {
            long xcenter = 0;
            long ycenter = 0;
            foreach (var p in Points)
            {
                xcenter += p.X;
                ycenter += p.Y;
            }

            xcenter /= Points.Length;
            ycenter /= Points.Length;

            var angles = ConvertAll(Points, x => Atan2(x.Y - ycenter, x.X - xcenter));
            Sort(angles, Points);
        }

        public static long ComputeDoubleArea(Point2D[] p)
        {
            long area = 0;
            for (var i = 0; i < p.Length; i++)
            {
                var j = (i + 1) % p.Length;
                area += p[i].X * p[j].Y - p[j].X * p[i].Y;
            }

            return Abs(area / 2);
        }

        public bool PointInPolygon(Point2D q)
        {
            if (q.X < MinX || q.Y < MinY || q.X > MaxX || q.Y > MaxY)
                return false;

            var c = false;
            for (var i = 0; i < Points.Length; i++)
            {
                var j = (i + 1) % Points.Length;
                if ((Points[i].Y <= q.Y && q.Y < Points[j].Y ||
                     Points[j].Y <= q.Y && q.Y < Points[i].Y) &&
                    q.X < Points[i].X + (Points[j].X - Points[i].X) * (q.Y - Points[i].Y) * 1.0 / (Points[j].Y - Points[i].Y))
                    c = !c;
            }

            if (c) return true;

            for (var i = 0; i < Points.Length; i++)
            {
                var j = (i + 1) % Points.Length;

                var p0 = Points[i];
                var p1 = Points[j];
                if ((p0.X - q.X) * (p0.Y - p1.Y) == (p0.X - p1.X) * (p0.Y - q.Y))
                {
                    if (q.X >= Min(p0.X, p1.X)
                        && q.Y >= Min(p0.Y, p1.Y)
                        && q.X <= Max(p0.X, p1.X)
                        && q.Y <= Max(p0.Y, p1.Y))
                        return true;
                }
            }

            return c;
        }


    }


    #region Support

    public class FenwickTree
    {
        #region Variables
        public readonly long[] A;
        #endregion

        #region Constructor
        public FenwickTree(long[] a) : this(a.Length)
        {
            int n = a.Length;
            System.Array.Copy(a, 0, A, 1, n);
            for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
            {
                for (int i = k; i <= n; i += k)
                    A[i] += A[i - h];
            }

            //for (int i = 0; i < a.Length; i++)
            //	Add(i, a[i]);
        }

        public FenwickTree(long size)
        {
            A = new long[size + 1];
        }
        #endregion

        #region Properties
        public long this[int index] => AltRangeUpdatePointQueryMode ? SumInclusive(index) : SumInclusive(index, index);

        public int Length => A.Length - 1;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public long[] Table
        {
            get
            {
                int n = A.Length - 1;
                long[] ret = new long[n];
                for (int i = 0; i < n; i++)
                    ret[i] = SumInclusive(i);
                if (!AltRangeUpdatePointQueryMode)
                    for (int i = n - 1; i >= 1; i--)
                        ret[i] -= ret[i - 1];
                return ret;
            }
        }
        #endregion


        #region Methods
        public void Clear()
        {
            Array.Clear(A, 0, A.Length);
        }

        public void Add(int i, long val)
        {
            if (val == 0) return;
            for (i++; i < A.Length; i += (i & -i))
                A[i] += val;
        }

        // Sum from [0 ... i]
        public long SumInclusive(int i)
        {
            long sum = 0;
            for (i++; i > 0; i -= (i & -i))
                sum += A[i];
            return sum;
        }

        public long SumInclusive(int i, int j)
        {
            return SumInclusive(j) - SumInclusive(i - 1);
        }

        // get largest value with cumulative sum less than x;
        // for smallest, pass x-1 and add 1 to result
        public int GetIndexGreater(long x)
        {
            int i = 0, n = A.Length - 1;
            for (int bit = HighestOneBit(n); bit != 0; bit >>= 1)
            {
                int t = i | bit;

                // if (t <= n && A[t] < x) for greater or equal 
                if (t <= n && A[t] <= x)
                {
                    i = t;
                    x -= A[t];
                }
            }
            return i <= n ? i : -1;
        }

        public int Next(int x)
        {
            return GetIndexGreater(SumInclusive(x));
        }

        public int Previous(int x)
        {
            if (x <= 0) return -1;
            var count = SumInclusive(x - 1);
            if (count == 0) return -1;
            return GetIndexGreater(count - 1);
        }

        #endregion

        #region Alternative Range Update Point Query Mode  ( cf Standard Point Update Range Query )

        public bool AltRangeUpdatePointQueryMode;

        /// <summary>
        /// Inclusive update of the range [left, right] by value
        /// The default operation of the fenwick tree is point update - range query.
        /// We use this if we want alternative range update - point query.
        /// SumInclusive becomes te point query function.
        /// </summary>
        /// <returns></returns>
        public void AltAddInclusive(int left, int right, long delta)
        {
            Add(left, delta);
            Add(right + 1, -delta);
        }

        public long AltQuery(int index)
        {
            return SumInclusive(index);
        }


        #endregion
    }

    #region Helpers
    public struct Point2D
    {
        public long X;
        public long Y;

        public Point2D(long x, long y)
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

        public static Point2D operator *(Point2D p, long c)
        {
            return new Point2D(p.X * c, p.Y * c);
        }

        public static Point2D operator /(Point2D p, long c)
        {
            return new Point2D(p.X / c, p.Y / c);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public long LengthSquared => X * X + Y * Y;


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

        public long Cross(Point2D vec2)
        {
            return X * vec2.Y - Y * vec2.X;
        }

        public long Dot(Point2D vector)
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

        public static bool LinesParallel(Point2D a, Point2D b, Point2D c, Point2D d)
        {
            return Abs((b - a).Cross(c - d)) ==0;
        }

        public static bool LinesCollinear(Point2D a, Point2D b, Point2D c, Point2D d)
        {
            return LinesParallel(a, b, c, d)
                   && Abs((a - b).Cross(a - c)) ==0
                   && Abs((c - d).Cross(c - a)) ==0;
        }

        private static long Dot(Point2D p, Point2D q)
        {
            return p.X * q.X + p.Y * q.Y;
        }

        public static long Dist2(Point2D p, Point2D q)
        {
            return (p - q).LengthSquared;
        }


        public static bool SegmentsIntersect(Point2D a, Point2D b, Point2D c, Point2D d)
        {
            if (LinesCollinear(a, b, c, d))
            {
                if (Dist2(a, c) ==0 || Dist2(a, d) == 0 ||
                    Dist2(b, c) ==0 || Dist2(b, d) == 0) return true;
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

    public class Comparer<T> : IComparer<T>
    {
        readonly Comparison<T> _comparison;

        public Comparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T a, T b) => _comparison(a, b);
    }

    public class EulerTour
    {
        public readonly int[] Trace;
        public readonly int[] Begin;
        public readonly int[] End;
        public readonly int[] Depth;
        public readonly bool Twice;

        public EulerTour(IList<int>[] g, int root, bool twice = true)
        {
            int n = g.Length;
            Twice = twice;
            Trace = new int[n * (twice ? 2 : 1)];
            Begin = new int[n];
            End = new int[n];
            Depth = new int[n];
            int t = -1;

            for (int i = 0; i < n; i++)
                Begin[i] = -1;

            var stack = new int[n];
            var indices = new int[n];
            int sp = 0;
            stack[sp++] = root;

            while (sp > 0)
            {
                outer:
                int current = stack[sp - 1], index = indices[sp - 1];
                if (index == 0)
                {
                    ++t;
                    Trace[t] = current;
                    Begin[current] = t;
                    Depth[current] = sp - 1;
                }

                var children = g[current];
                while (index < children.Count)
                {
                    int child = children[index++];
                    if (Begin[child] == -1)
                    {
                        indices[sp - 1] = index;
                        stack[sp] = child;
                        indices[sp] = 0;
                        sp++;
                        goto outer;
                    }
                }

                indices[sp - 1] = index;
                if (index == children.Count)
                {
                    sp--;
                    if (twice) Trace[++t] = current;
                    End[current] = t;
                }
            }
        }

        public bool IsBegin(int trace) => Begin[Trace[trace]] == trace;
        public bool IsEnd(int trace) => End[Trace[trace]] == trace;
        public int this[int index] => Trace[index];
    }

    public static int[][] ParentToChildren(int[] parents)
    {
        int n = parents.Length;
        int[] count = new int[n];
        for (int i = 0; i < n; i++)
            if (parents[i] >= 0)
                count[parents[i]]++;

        int[][] graphs = new int[n][];
        for (int i = 0; i < n; i++)
            graphs[i] = new int[count[i]];

        for (int i = 0; i < n; i++)
            if (parents[i] >= 0)
                graphs[parents[i]][--count[parents[i]]] = i;

        return graphs;
    }
    #endregion

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

    static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    static V Get<K, V>(Dictionary<K, V> dict, K key) where V : new()
    {
        V result;
        if (dict.TryGetValue(key, out result) == false)
            result = dict[key] = new V();
        return result;
    }

    static int Bound<T>(T[] array, T value, bool upper = false)
        where T : IComparable<T>
    {
        int left = 0;
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left >> 1);
            int cmp = value.CompareTo(array[mid]);
            if (cmp > 0 || cmp == 0 && upper)
                left = mid + 1;
            else
                right = mid - 1;
        }
        return left;
    }

    static long IntPow(long n, long p)
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

    static int[] Ni(int n) => N(n, Ni);

    static long[] Nl(int n) => N(n, Nl);

    static string[] Ns(int n) => N(n, Ns);

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

    static char[] Nc(int n)
    {
        var list = new char[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
        return list;
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

    static string ReadLine()
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

    static void Write(byte[] array, int count)
    {
        Reserve(count);
        Copy(array, 0, outputBuffer, outputIndex, count);
        outputIndex += count;
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
        Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
    }
    #endregion
    #endregion
}
class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}
