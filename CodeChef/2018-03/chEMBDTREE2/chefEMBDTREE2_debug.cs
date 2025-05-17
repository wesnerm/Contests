using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using Console = System.Console;

partial class Solution
{

    static Random rgen = new Random(1);


    static int rand(int left, int right)
    {
        return rgen.Next(left, right + 1);
    }

    int[] Fixup(int[] onebased)
    {
        int[] zerobased = new int[onebased.Length - 1];

        for (int i = 0; i < zerobased.Length; i++)
            zerobased[i] = onebased[i + 1] - 1;
        return zerobased;
    }

    public int[] Type1()
    {
        int n = 1000;
        int[] parent = new int[n + 1];
        int k = 10;
        for (int i = 2; i <= n; i++)
        {
            if (i <= k)
                parent[i] = 1;
            else
                parent[i] = rand(i - k, i-1);
        }

        return Fixup(parent);
    }



    public int[] Type2()
    {
        int n = 1000;
        int[] parent = new int[n + 1];
        int k = 10;
        for (int i = 2; i <= n; i++)
        {
            parent[i] = rand(1, Min(i - 1, k));
        }

        return Fixup(parent);
    }

    public int[] Type3()
    {
        int n = 1000;
        int[] parent = new int[n + 1];
        int k = n / 3;
        for (int i = 2; i <= n; i++)
        {
            if (i >= k && i <= n - k)
                parent[i] = i - 1;
            else if (i < k)
                parent[i] = rand(1, i-1);
            else
                parent[i] = rand(n - k, i - 1);
        }

        return Fixup(parent);
    }


    public static void Error(object message = null)
    {
        System.Console.Error.WriteLine((message??"").ToString());
    }

    public void WriteOut()
    {
        var array = Type1();
        Error("======");
        Error(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            var len = rand(1, 1);
            if (array[i] >= 0)
                Error($"{i + 1} {array[i] + 1} {len}");
        }
        Error("======");
    }

    bool inited = false;



    public static bool Check(int[][] points, int[][] edges)
    {
        var n = points.Length;
        var good = true;
        int intersections = 0;
        for (int i = 0; i < n - 1; i++)
        {
            var u = edges[i][0];
            var v = edges[i][1];
            var len = edges[i][2];

            var p1 = new Point2D(points[u][0], points[u][1]);
            var p2 = new Point2D(points[v][0], points[v][1]);
            var d = Point2D.Dist2(p1, p2);
            if (d < len)
            {
                System.Console.Error.WriteLine($"Error: For ({i + 1},{v + 1}) distance {d} < {len}");
                good = false;
            }

            for (int j = 0; j < i; j++)
            {
                var s = edges[j][0];
                var t = edges[j][1];

                if (t == u || t == v || s == u || s == v) continue;
                var p3 = new Point2D(points[s][0], points[s][1]);
                var p4 = new Point2D(points[t][0], points[t][1]);

                if (Point2D.SegmentsIntersect(p1, p2, p3, p4))
                {
                    intersections++;
                    good = false;
                }
            }
        }

        if (intersections > 0 )
            System.Console.Error.WriteLine($"{intersections} intersections.");
        return good;
    }
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
