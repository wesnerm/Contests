namespace HackerRank.Contests.AdInfinitum17
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using static System.Math;


    public class PaintingFigures
	{
        public static int SearchDepth = 15;
        public static bool UseWideLines = false;

        public static void Main()
        {
            int t = int.Parse(Console.ReadLine());
            var figures = new List<Figure>();
            while (t-- > 0)
            {
                var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                figures.Add(new Figure(arr[0], arr[1], arr[2], arr[3], arr[4]));
            }

            var rfigures = figures.ToList();
            figures.Sort((a, b) => a.XMin.CompareTo(b.XMin));
            rfigures.Sort((a, b) => a.XMax.CompareTo(b.XMax));
            var intersections = ComputeAllIntersections(figures);

            var xset = new HashSet<double>();
            foreach (var f in figures)
            {
                Console.WriteLine(ComputePolygonArea(f.FigurePoints()));
                xset.Add(f.XMin);
                xset.Add(f.XMax);
                xset.Add(f.X1);
                xset.Add(f.X2);
                if (UseWideLines)
                {
                    foreach (var p in f.WideLine())
                        xset.Add(p.X);
                }
            }

            xset.UnionWith(intersections.Select(x => x.X));
            var pts = xset.ToList();
            pts.Sort();

            int left = 0;
            int right = 0;
            double area = 0;
            var pq = new HashSet<Figure>();

            for (var i = 0; i < pts.Count; i++)
            {
                var x = pts[i];
                var xNext = i < pts.Count - 1 ? pts[i + 1] : x;

                // Add figures
                while (left < figures.Count && figures[left].XMin <= x)
                    pq.Add(figures[left++]);

                // Remove figures
                while (right < rfigures.Count && rfigures[right].XMax <= x)
                    pq.Remove(rfigures[right++]);

                if (pq.Count > 1)
                {
                    // Sweep y
                    area += SweepY(pq, x, xNext);
                }
                else if (pq.Count == 1)
                {
                    // Compute area of single figure between points
                    area += pq.First().EstimateArea(x, int.MinValue, xNext, int.MaxValue);
                }

            }

            Console.WriteLine(area);
        }

        public static IEnumerable<Arc> ComputeAllIntersections(List<Figure> figures)
        {
            for (int i = 0; i < figures.Count; i++)
            {
                var a = figures[i];
                var a1 = new Arc(a.X1, a.Y1);
                var a2 = new Arc(a.X2, a.Y2);
                for (int j = i + 1; j < figures.Count; j++)
                {
                    var b = figures[j];
                    var b1 = new Arc(b.X1, b.Y1);
                    var b2 = new Arc(b.X2, b.Y2);
                    if (SegmentsIntersect(a1, a2, b1, b2))
                        yield return ComputeLineIntersection(a1, a2, b1, b2);
                }
            }
        }

        public static double SweepY(HashSet<Figure> figureSet, double xLeft, double xRight)
        {
            var figures = figureSet.ToList();
            var yset = new HashSet<double>();
            foreach (var f in figures)
            {
                f.TempY1 = f.YAt(xLeft, false);
                f.TempY2 = f.YAt(xRight, true);
                if (f.TempY1 > f.TempY2)
                    Swap(ref f.TempY1, ref f.TempY2);
                f.TempYMax = f.TempY2 + f.R;
                f.TempYMin = f.TempY1 - f.R;

                yset.Add(f.TempYMin);
                yset.Add(f.TempYMax);
                yset.Add(f.TempY1);
                yset.Add(f.TempY2);

                if (UseWideLines)
                {
                    foreach (var p in f.WideLine())
                        yset.Add(p.Y);
                }
            }

            var pts = yset.ToList();
            pts.Sort();

            var rfigures = figures.ToList();
            figures.Sort((a, b) => a.TempYMin.CompareTo(b.TempYMin));
            rfigures.Sort((a, b) => a.TempYMax.CompareTo(b.TempYMax));

            int left = 0;
            int right = 0;
            double area = 0;
            var pq = new HashSet<Figure>();

            for (var i = 0; i < pts.Count; i++)
            {
                var y = pts[i];
                var yNext = i < pts.Count - 1 ? pts[i + 1] : y;

                // Add figures
                while (left < figures.Count && figures[left].TempYMin <= y)
                    pq.Add(figures[left++]);

                // Remove figures
                while (right < rfigures.Count && figures[right].TempYMax <= y)
                    pq.Remove(figures[right++]);

                if (pq.Count > 0)
                    area += ApproximateArea(pq, xLeft, y, xRight, yNext);
            }

            return area;
        }


        public static double ApproximateArea(HashSet<Figure> figures,
            double xMin, double yMin, double xMax, double yMax, int depth = 0)
        {

            double maxArea = 0;
            int count = 0;

            double fullArea = (xMax - xMin) * (yMax - yMin);

            foreach (var f in figures)
            {
                var v = f.EstimateArea(xMin, yMin, xMax, yMax);
                if (Abs(v) < Eps) continue;
                if (v > fullArea - Eps) return v;

                count++;
                if (v > maxArea) maxArea = v;
            }

            if (maxArea < 1e-9 || depth > SearchDepth)
                return maxArea;

            depth++;
            var xMid = (xMin + xMax) / 2;
            var yMid = (yMin + yMax) / 2;
            var area = 0.0;
            area += ApproximateArea(figures, xMin, yMin, xMid, yMid, depth);
            area += ApproximateArea(figures, xMid, yMin, xMax, yMid, depth);
            area += ApproximateArea(figures, xMin, yMid, xMid, yMax, depth);
            area += ApproximateArea(figures, xMid, yMid, xMax, yMax, depth);

            Console.WriteLine($"{xMin},{yMin},{xMax},{yMax} -> {area}");
            return area;
        }

        #region Old Code

        public static double AreaOfTriangle(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            var area = (x0 - x2) * (y1 - y0) - (x0 - x1) * (y2 - y0);
            return Abs(area) / 2;
        }

        public static double AreaOfTriangle(Arc a1, Arc a2, Arc a3)
        {
            double area = AreaOfTriangle(a1.X, a1.Y, a2.X, a2.Y, a3.X, a3.Y);

            double chord = 0;
            if (a1.IsArc)
            {
                if (a2.IsArc)
                    chord = AreaOfChord(a1, a2);
                else if (a3.IsArc)
                    chord = AreaOfChord(a1, a3);
            }
            else if (a2.IsArc && a3.IsArc)
                chord = AreaOfChord(a2, a3);

            return area + chord;
        }

        #endregion

        public static double AreaOfChord(Arc a1, Arc a2)
        {
            if (a1.CX != a2.CX || a1.CY != a2.CY || !a1.IsArc)
                return 0;

            double angle = AngleBetween(a1.Angle, a2.Angle);
            double area = a1.Radius * a2.Radius * (angle - Sin(angle)) / 2;
            return area;
        }

        public static double AngleBetween(double angle1, double angle2)
        {
            double angle = angle1 - angle2;
            if (angle < 0) angle = -angle;
            angle %= 2 * Math.PI;
            if (angle > Math.PI)
                angle = 2 * PI - angle;
            return angle;

        }

        static double ComputePolygonArea(IList<Arc> pts)
        {
            double area = 0;
            double chord = 0;

            for (int i = 0; i < pts.Count; i++)
            {
                int j = (i + 1) % pts.Count;
                area += pts[i].X * pts[j].Y - pts[j].X * pts[i].Y;

                if (pts[i].IsArc)
                    chord += AreaOfChord(pts[i], pts[j]);
            }
            return Abs(area) / 2 + chord;
        }


        public static double ComputeSignedArea(List<Arc> p)
        {
            double area = 0;
            for (var i = 0; i < p.Count; i++)
            {
                var j = (i + 1) % p.Count;
                area += p[i].X * p[j].Y - p[j].X * p[i].Y;
            }
            return area / 2.0;
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        static double Round(double d)
        {
            var round = Math.Round(d);
            if (Abs(round - d) < 1e-15)
                return round;
            return d;
        }

        private const double Inf = double.PositiveInfinity;
        private const double Eps = 1e-12;

        private static double Dot(Arc p, Arc q)
        {
            return p.X * q.X + p.Y * q.Y;
        }

        private static double Dist2(Arc p, Arc q)
        {
            return Dot(p - q, p - q);
        }

        private static double Cross(Arc p, Arc q)
        {
            return p.X * q.Y - p.Y * q.X;
        }

        public static bool LinesParallel(Arc a, Arc b, Arc c, Arc d)
        {
            return Abs(Cross(b - a, c - d)) < Eps;
        }

        public static bool LinesCollinear(Arc a, Arc b, Arc c, Arc d)
        {
            return LinesParallel(a, b, c, d)
                   && Abs(Cross(a - b, a - c)) < Eps
                   && Abs(Cross(c - d, c - a)) < Eps;
        }

        public static bool SegmentsIntersect(Arc a1, Arc a2, Arc b1, Arc b2)
        {
            if (LinesCollinear(a1, a2, b1, b2))
            {
                return Dist2(a1, b1) < Eps || Dist2(a1, b2) < Eps ||
                       Dist2(a2, b1) < Eps || Dist2(a2, b2) < Eps ||
                       Dot(b1 - a1, b1 - a2) <= 0 || Dot(b2 - a1, b2 - a2) <= 0 || Dot(b1 - a2, b2 - a2) <= 0;
            }
            return Cross(b2 - a1, a2 - a1) * Cross(b1 - a1, a2 - a1) <= 0
                   && Cross(a1 - b1, b2 - b1) * Cross(a2 - b1, b2 - b1) <= 0;
        }

        public static Arc ComputeLineIntersection(Arc a1, Arc a2, Arc b1, Arc b2)
        {
            a2 = a2 - a1;
            b2 = b1 - b2;
            b1 = b1 - a1;
            Debug.Assert(Dot(a2, a2) > Eps && Dot(b2, b2) > Eps);
            return a1 + Cross(b1, b2) / Cross(a2, b2) * a2;
        }


        public static bool AreClose(double d, double d2)
        {
            return Abs(d - d2) < Eps;
        }


        public static Arc RotateCcw90(Arc p)
        {
            return new Arc(-p.Y, p.X);
        }

        public static Arc RotateCw90(Arc p)
        {
            return new Arc(p.Y, -p.X);
        }

        public static Arc RotateCcw(Arc p, double t)
        {
            return new Arc(p.X * Cos(t) - p.Y * Sin(t),
                p.X * Sin(t) + p.Y * Cos(t));
        }


        public static Arc ComputeCircleCenter(Arc a, Arc b, Arc c)
        {
            b = (a + b) / 2;
            c = (a + c) / 2;
            return ComputeLineIntersection(b, b + RotateCw90(a - b), c, c + RotateCw90(a - c));
        }

        public static bool PointInPolygon(List<Arc> p, Arc q)
        {
            var c = false;
            for (var i = 0; i < p.Count; i++)
            {
                var j = (i + 1) % p.Count;
                if ((p[i].Y <= q.Y && q.Y < p[j].Y ||
                     p[j].Y <= q.Y && q.Y < p[i].Y) &&
                    q.X < p[i].X + (p[j].X - p[i].X) * (q.Y - p[i].Y) / (p[j].Y - p[i].Y))
                    c = !c;
            }
            return c;
        }

        public static bool PointOnPolygon(List<Arc> p, Arc q)
        {
            for (var i = 0; i < p.Count; i++)
                if (Dist2(ProjectPointSegment(p[i], p[(i + 1) % p.Count], q), q) < Eps)
                    return true;
            return false;
        }

        public static Arc ProjectPointSegment(Arc a, Arc b, Arc c)
        {
            var r = Dot(b - a, b - a);
            if (Abs(r) < Eps) return a;
            r = Dot(c - a, b - a) / r;
            if (r < 0) return a;
            if (r > 1) return b;
            return a + (b - a) * r;
        }

        public static List<Arc> CircleLineIntersection(Arc a, Arc b, Arc c, double r)
        {
            var ret = new List<Arc>();
            b = b - a;
            a = a - c;
            var A = Dot(b, b);
            var B = Dot(a, b);
            var C = Dot(a, a) - r * r;
            var d = B * B - A * C;
            if (d < -Eps) return ret;
            ret.Add(c + a + b * (-B + Sqrt(d + Eps)) / A);
            if (d > Eps)
                ret.Add(c + a + b * (-B - Sqrt(d)) / A);
            return ret;
        }

        public static List<Arc> CircleCircleIntersection(Arc a, Arc b, double r, double R)
        {
            var ret = new List<Arc>();
            var d = Sqrt(Dist2(a, b));
            if (d > r + R || d + Min(r, R) < Max(r, R)) return ret;
            var x = (d * d - R * R + r * r) / (2 * d);
            var y = Sqrt(r * r - x * x);
            var v = (b - a) / d;
            ret.Add(a + v * x + RotateCcw90(v) * y);
            if (y > 0)
            {
                ret.Add(a + v * x - RotateCcw90(v) * y);
            }
            return ret;
        }


        public class CompareFunction<T> : IComparer<T>
        {
            private Comparison<T> _comparer;

            public CompareFunction(Comparison<T> comparison)
            {
                _comparer = comparison;
            }

            public int Compare(T a, T b)
            {
                return _comparer(a, b);
            }
        }

        public struct Arc
        {

            public double X;
            public double Y;
            public double Angle;
            public double Radius;

            public bool IsArc;
            public double CX;
            public double CY;


            public void Update()
            {
                if (!IsArc) return;
                double dx = X - CX;
                double dy = Y - CY;
                Radius = Math.Sqrt(dx * dx + dy * dy);
                Angle = Atan2(dy, dx);
                if (Angle < 0) Angle += 2 * Math.PI;
            }

            public Arc(double x, double y) : this()
            {
                X = x;
                Y = y;
            }

            public Arc(double x, double y, double cx, double cy) : this()
            {
                X = x;
                Y = y;
                CX = cx;
                CY = cy;
                IsArc = true;
                Update();
            }

            public static Arc operator +(Arc p, Arc p2)
            {
                return new Arc(p.X + p2.X, p.Y + p2.Y);
            }

            public static Arc operator -(Arc p0, Arc p)
            {
                return new Arc(p0.X - p.X, p0.Y - p.Y);
            }

            public static Arc operator *(double c, Arc p)
            {
                return new Arc(p.X * c, p.Y * c);
            }

            public static Arc operator *(Arc p, double c)
            {
                return new Arc(p.X * c, p.Y * c);
            }

            public static Arc operator /(Arc p, double c)
            {
                return new Arc(p.X / c, p.Y / c);
            }

            public override string ToString()
            {
                return $"({X},{Y},{Radius},{Angle * 180 / Math.PI})";
            }

            public static bool operator <(Arc lhs, Arc rhs)
            {
                if (lhs.Y < rhs.Y)
                    return true;

                return lhs.Y == rhs.Y && lhs.X < rhs.X;
            }

            public static bool operator >(Arc lhs, Arc rhs)
            {
                if (lhs.Y > rhs.Y)
                    return true;

                return lhs.Y == rhs.Y && lhs.X > rhs.X;
            }

            public static bool operator ==(Arc lhs, Arc rhs)
            {
                return lhs.Y == rhs.Y && lhs.X == rhs.X;
            }

            public static bool operator !=(Arc lhs, Arc rhs)
            {
                return lhs.Y != rhs.Y || lhs.X != rhs.X;
            }

            public bool Equals(Arc other)
            {
                return X.Equals(other.X) && Y.Equals(other.Y);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Arc && Equals((Arc) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X.GetHashCode() * 397) ^ Y.GetHashCode();
                }
            }
        }

        public class Figure : IComparable<Figure>
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;
            public int R;
            public double Length;
            public int XMin;
            public int XMax;
            public int YMin;
            public int YMax;
            public readonly int Id = _counter++;
            public Arc[] _wideline;
            public Arc[] _figurePoints;
            static int _counter;

            // Equation for a line
            public double XF;
            public double YF;
            public double C;
            // Equation for normal
            public double XNF;
            public double YNF;
            public double CN;

            public double TempArea;
            public double TempYMin;
            public double TempYMax;
            public double TempY1;
            public double TempY2;

            public Figure(int x1, int y1, int x2, int y2, int r)
            {
                if (x1 > x2)
                {
                    Swap(ref x1, ref x2);
                    Swap(ref y1, ref y2);
                }

                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
                R = r;
                XMin = x1 - r;
                XMax = x2 + r;
                YMin = Min(y1, y2) - r;
                YMax = Max(y1, y2) + r;

                double dx = x2 - x1;
                double dy = y2 - y1;
                Length = Sqrt(dx * dx + dy * dy);

                XNF = (X2 - X1) / Length;
                YNF = (Y2 - Y1) / Length;

                XF = -YNF;
                YF = XNF;

                CN = -(X1 * XNF + Y1 * YNF);
                if (X2 * XNF + Y2 * YNF + CN < 0)
                {
                    CN = -CN;
                    XNF = -XNF;
                    YNF = -YNF;
                }
                Debug.Assert(X2 * XNF + Y2 * YNF + CN > 0);
                Debug.Assert(AreClose(0, Abs(X2 * XNF + Y2 * YNF + CN - Length)));

                C = -(X1 * XF + Y1 * YF);
            }

            public int CompareTo(Figure other)
            {
                int cmp = X1 - other.X1;
                if (cmp != 0) return cmp;
                cmp = X2 - other.X2;
                if (cmp != 0) return cmp;
                cmp = Y1 - other.Y1;
                if (cmp != 0) return cmp;
                cmp = Y2 - other.Y2;
                if (cmp != 0) return cmp;
                cmp = R - other.R;
                if (cmp != 0) return cmp;
                cmp = Id - other.Id;
                return cmp;
            }

            public override string ToString()
            {
                return $"({X1},{Y1},{X2},{Y2},{R})";
            }

            public double YAt(double x, bool max = true)
            {
                double dx = X2 - X1;
                if (Math.Abs(dx) < 1e-16)
                    return max ? YMax - R : YMin + R;

                double alpha = (x - X1) / dx;
                var result = Y1 + alpha * Y2;
                return result;
            }



            public double DistanceFromSegment(double x, double y, out int where)
            {
                var d = XNF * x + YNF * y + CN;
                var d2 = XF * x + YF * y + C;
                var sign = d2 >= 0 ? 1 : -1;

                // Behind X1 and Y1
                if (d < 0)
                {
                    @where = 1;
                    return Dist(x, y, X1, Y1) * sign;
                }

                // Behind X2 and Y2
                if (d > Length)
                {
                    @where = 2;
                    return Dist(x, y, X2, Y2) * sign;
                }

                @where = 0;
                return d2;
            }

            public static double Dist(double x1, double y1, double x2, double y2)
            {
                var dx = x2 - x1;
                var dy = y2 - y1;
                var d = dx * dx + dy * dy;
                return Sqrt(d);
            }

            public bool PointInside(double x, double y)
            {
                int w;
                return DistanceFromSegment(x, y, out w) <= R;
            }

            public static double Angle(double x0, double y0, double x1, double y1, double x2, double y2)
            {
                var vx1 = x1 - x0;
                var vx2 = x2 - x1;
                var vy1 = y1 - y0;
                var vy2 = y2 - y1;
                return Acos((vx1 * vx2 + vy1 * vy2) / Math.Sqrt((vx1 * vx1 + vy1 * vy1) * (vx2 * vx2 + vy2 * vy2)));

            }

            public unsafe double EstimateArea(double xMin, double yMin, double xMax, double yMax)
            {
                var pt = stackalloc Arc[4];
                var d = stackalloc double[4];
                var w = stackalloc int[4];

                pt[0] = new Arc(xMin, yMin);
                pt[1] = new Arc(xMin, yMax);
                pt[2] = new Arc(xMax, yMax);
                pt[3] = new Arc(xMax, yMin);

                for (int i = 0; i < 4; i++)
                {
                    d[i] = DistanceFromSegment(pt[i].X, pt[i].Y, out w[i]);
                }

                int cNz = 0;
                int neg = 0;
                int pos = 0;

                for (int i = 3; i >= 0; i--)
                {
                    if (d[i] < 0)
                    {
                        neg++;
                        d[i] = -d[i];
                    }
                    else if (d[i] > 0)
                        pos++;

                    if (d[i] <= R)
                        cNz++;
                }


                if (cNz == 0)
                {
                    // All outside
                    if (pos == 0 || neg == 0)
                        return 0;
                }

                if (cNz == 4)
                {
                    // All inside
                    return (xMax - xMin) * (yMax - yMin);
                }

                var clipper = new RectangleClipper(xMin, yMin, xMax, yMax);
                var points = FigurePoints();
                clipper.Clip(points);
                var area = ComputePolygonArea(clipper.Arcs);
                return area;
            }


            public bool Intersects(double xMin, double yMin, double xMax, double yMax)
            {
                return (xMin < XMax) && (yMin < YMax) && (xMax > XMin) && (yMax > YMin);
            }

            public bool CompletelyInside(double xMin, double yMin, double xMax, double yMax)
            {
                return xMin <= XMin + Eps && xMax >= XMax - Eps && yMin <= YMin + Eps && yMax >= YMax - Eps;
            }

            public double Area()
            {
                return R * (Length * 2 + PI * R);
            }

            public Arc[] WideLine()
            {
                if (_wideline != null)
                    return _wideline;

                double dx = X2 - X1;
                double dy = Y2 - Y1;
                // double c = dy * X1 + dx * Y1;
                double dist = Sqrt(dx * dx + dy * dy);
                double rx = dy / dist * R;
                double ry = dx / dist * R;
                return _wideline = new[]
                {
                    new Arc {X = X1 - rx, Y = Y1 - ry},
                    new Arc {X = X1 + rx, Y = Y1 + ry},
                    new Arc {X = X2 + rx, Y = Y2 + ry},
                    new Arc {X = X2 - rx, Y = Y2 - ry}
                };
            }

            public Arc[] FigurePoints()
            {
                if (_figurePoints != null)
                    return _figurePoints;

                var sign = ((X2 - X1) >= 0) == ((Y2 - Y1) >= 0) ? 1 : -1;
                int steps = 8;
                int half = steps / 2;
                var array = new Arc[steps + 2];
                double startAngle = Round(Atan2(X1 - X2, Y2 - Y1));
                double angle = startAngle;
                double inc = -2.0 * PI / steps;

                for (int i = 0; i <= half; i++, angle += inc)
                {
                    array[i] = new Arc(
                        Round(X1 + R * Cos(angle)),
                        Round(Y1 + R * Sin(angle)),
                        X1, Y1);
                }

                angle = startAngle + PI;
                for (int i = half; i <= steps; i++, angle += inc)
                {
                    array[i + 1] = new Arc(
                        Round(X2 + R * Cos(angle)),
                        Round(Y2 + R * Sin(angle)),
                        X2, Y2);
                }

                _figurePoints = array;
                return array;
            }

            public override int GetHashCode()
            {
                return Id;
            }
        }

        public struct RectangleClipper
        {
            public double xmin, xmax, ymin, ymax;
            public List<Arc> Arcs;

            public RectangleClipper(double xmin, double ymin, double xmax, double ymax)
            {
                this.xmin = xmin;
                this.ymin = ymin;
                this.xmax = xmax;
                this.ymax = ymax;
                Arcs = new List<Arc>(8);
            }

            public List<Arc> Clip(Arc[] p)
            {
                Arcs.Clear();
                int n = p.Length;
                int added = 0;
                for (int i = 0; i < n; i++)
                {
                    int j = i + 1;
                    if (j >= n) j -= n;

                    int prevCount = Arcs.Count;
                    CohenSutherlandLineClipAndDraw(p[i], p[j]);
                    added = Arcs.Count - prevCount;
                }

                if (added > 0)
                {
                    Arcs.Insert(0, Arcs[Arcs.Count - 1]);
                    Arcs.RemoveAt(Arcs.Count - 1);
                }

                int write = 0;

                Arc prev = new Arc(double.NaN, double.NaN);
                for (int read = 0; read < Arcs.Count; read++)
                {
                    var arc0 = Arcs[read];
                    if (arc0.X != prev.X || arc0.Y != prev.Y)
                        Arcs[write++] = arc0;
                    prev = arc0;
                }

                Arcs.RemoveRange(write, Arcs.Count - write);

                if (Arcs.Count > 1 && Arcs[0].IsArc && Arcs[Arcs.Count - 1].IsArc)
                {
                    if (Arcs[0] != p[0] || Arcs[Arcs.Count - 1] != p[p.Length - 1])
                    {
                        var arcx = (Arcs[0] + Arcs[Arcs.Count - 1]) / 2;
                        Arcs.Add(arcx);
                    }
                }

                return Arcs;
            }


            [Flags]
            public enum OutCode
            {
                INSIDE = 0, // 0000
                LEFT = 1, // 0001
                RIGHT = 2, // 0010
                BOTTOM = 4, // 0100
                TOP = 8, // 1000
            }


            OutCode ComputeOutCode(double x, double y)
            {
                OutCode code = OutCode.INSIDE; // initialised as being inside of [[clip window]]

                if (x < xmin) // to the left of clip window
                    code |= OutCode.LEFT;
                else if (x > xmax) // to the right of clip window
                    code |= OutCode.RIGHT;
                if (y < ymin) // below the clip window
                    code |= OutCode.BOTTOM;
                else if (y > ymax) // above the clip window
                    code |= OutCode.TOP;

                return code;
            }


            void CohenSutherlandLineClipAndDraw(Arc arc0, Arc arc1)
            {
                OutCode outcode0 = ComputeOutCode(arc0.X, arc0.Y);
                OutCode outcode1 = ComputeOutCode(arc1.X, arc1.Y);

                while (true)
                {
                    if ((outcode0 | outcode1) == 0)
                    {
                        arc0.Update();
                        arc1.Update();
                        Arcs.Add(arc0);
                        Arcs.Add(arc1);
                        return;
                    }

                    if ((outcode0 & outcode1) != 0)
                        return;

                    double x = 0, y = 0;

                    OutCode outcodeOut = outcode0 != 0 ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - arc0.X), x = arc0.X + (1 / slope) * (y - y0)
                    if ((outcodeOut & OutCode.TOP) != 0)
                    {
                        // point is above the clip rectangle
                        x = arc0.X + (arc1.X - arc0.X) * (ymax - arc0.Y) / (arc1.Y - arc0.Y);
                        y = ymax;
                    }
                    else if ((outcodeOut & OutCode.BOTTOM) != 0)
                    {
                        x = arc0.X + (arc1.X - arc0.X) * (ymin - arc0.Y) / (arc1.Y - arc0.Y);
                        y = ymin;
                    }
                    else if ((outcodeOut & OutCode.RIGHT) != 0)
                    {
                        y = arc0.Y + (arc1.Y - arc0.Y) * (xmax - arc0.X) / (arc1.X - arc0.X);
                        x = xmax;
                    }
                    else if ((outcodeOut & OutCode.LEFT) != 0)
                    {
                        y = arc0.Y + (arc1.Y - arc0.Y) * (xmin - arc0.X) / (arc1.X - arc0.X);
                        x = xmin;
                    }

                    if (outcodeOut == outcode0)
                    {
                        arc0.X = x;
                        arc0.Y = y;
                        outcode0 = ComputeOutCode(arc0.X, arc0.Y);
                    }
                    else
                    {
                        arc1.X = x;
                        arc1.Y = y;
                        outcode1 = ComputeOutCode(arc1.X, arc1.Y);
                    }
                }
            }
        }

    }
}