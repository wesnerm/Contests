using System;
using System.Collections.Generic;
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
                parent[i] = rand(i - k, i - 1);
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
                parent[i] = rand(1, i - 1);
            else
                parent[i] = rand(n - k, i - 1);
        }

        return Fixup(parent);
    }


    public static void Error(object message = null)
    {
        System.Console.Error.WriteLine((message ?? "").ToString());
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

    void Prologue()
    {
        if (inited) return;
        inited = true;

        int i = 0;
        foreach (var v in FareySequence(8))
        {
            Error($"{++i}) {v}");
        }

        Error("\n\n");
        for (i = 1; i < 101; i++)
        {
            Error($"{i} {FareySequence(i).Count()}");
        }
    }



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

        if (intersections > 0)
            System.Console.Error.WriteLine($"{intersections} intersections.");
        return good;
    }

    
}
