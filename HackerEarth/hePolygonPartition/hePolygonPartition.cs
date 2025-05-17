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
    int n;
    int vertices;
    const int Take = 3;
    const int Factor = 1;
    const int TimeLimit = 1500;
    const bool UseInitial = true;
    const int Shift = 13;

    static readonly Random random = new Random(1);

    static Point2D[] points;
    int xmax, ymax, xmin = int.MaxValue, ymin = int.MaxValue;
    List<Cluster> initialClusters, manhattanClusters, euclideanClusters;
    HashSet<int> initialPoints;

    #endregion

    public void Solve()
    {
        n = Ni();

        points = new Point2D[n + 1];
        for (int i = 1; i <= n; i++)
            points[i] = new Point2D { X = Ni(), Y = Ni() };

        if (UseInitial)
        {
            var manhattanTable = BuildTable(true);
            manhattanClusters = BuildInitialList(manhattanTable);
            var euclideanTable = BuildTable(false);
            euclideanClusters = BuildInitialList(euclideanTable);

            bool manhattan = Score(manhattanClusters) > Score(euclideanClusters);
            initialClusters = manhattan ? manhattanClusters : euclideanClusters;
        }
        else
        {
            initialClusters = new List<Cluster>();
        }

        List<Cluster> best = null;
        double bestScore = 0;
        int[] full = Enumerable.Range(1, n).ToArray();
        int[] ordering = Exclude(initialClusters.SelectMany(x => x.Points));

        var candidates = new List<List<Cluster>>();

        var triangles = FindAllTriangles(ordering);
        candidates.Add(PostFix2(FindMaximum(triangles)));
        candidates.Add(PostFix2(FindMaximum2(triangles)));

        var c1 = ProduceClusters(ordering, initialClusters);
        var c2 = ProduceClusters(full, null);
        var c3 = PostFix2(new List<Cluster>(c1));
        var c4 = PostFix2(new List<Cluster>(c2));
        var c5 = ProduceClusters(c3.SelectMany(x => x.Points).ToArray(), null);
        var c6 = PostFix2(new List<Cluster>(c5));

        candidates.Add(c1);
        candidates.Add(c2);
        candidates.Add(c3);
        candidates.Add(c4);
        candidates.Add(c5);
        candidates.Add(c6);

        var scores = candidates.ConvertAll(x => Score(x));

        int index = -1;
        var process = Process.GetCurrentProcess();
#if DEBUG
        var start = process.TotalProcessorTime.TotalMilliseconds;
#else
        var start = 0;
#endif

        while (process.TotalProcessorTime.TotalMilliseconds - start < TimeLimit)
        {
            index++;
            List<Cluster> trial;
            if (index < candidates.Count)
            {
                trial = candidates[index];
            }
            else
            {
                trial = PostFix2(Trial(ordering));
            }

            var score = Score(trial);
            if (score > bestScore)
            {
                bestScore = score;
                best = trial;
            }

            trial = PostFix2(ProduceClusters(trial.SelectMany(x => x.Points).ToArray(), null));

            score = Score(trial);
            if (score > bestScore)
            {
                bestScore = score;
                best = trial;
            }

            Shuffle(ordering, 0, ordering.Length);
        }

        PostFix(best);
        PostFix2(best);

        //var best2 = ProduceClusters(best.SelectMany(x=>x.Points).ToArray(), null);
        //var best3 = PostFix2(best2.ToList());
        //var score2 = Score(best2);
        //var score3 = Score(best3);
        // Add unused points
        var unusedPoints = Exclude(best.SelectMany(x => x.Points));
        if (unusedPoints.Length > 0)
            best.Add(ToClusterNoCV(unusedPoints));

        // Make sure result is valid
        var result = best.ConvertAll(x => x.Points);
        result.Sort((a, b) => a.Length.CompareTo(b.Length));
        var seen = new HashSet<int>();
        for (int i = 0; i < result.Count; i++)
        {
            result[i] = result[i].Except(seen).Distinct().ToArray();
            seen.UnionWith(result[i]);
        }
        result.RemoveAll(x => x.Length == 0);

        WriteLine(result.Count);
        foreach (var pts in result)
        {
            Write(pts.Length);
            Write(' ');
            Write(string.Join(" ", pts));
            Write('\n');
        }
    }

    #region BuildInitialList

    List<int>[] BuildTable(bool manhattan)
    {
        for (int i = 1; i <= n; i++)
        {
            var p = points[i];
            xmin = Min(xmin, (int)p.X);
            xmax = Max(xmax, (int)p.X);
            ymin = Min(ymin, (int)p.Y);
            ymax = Max(ymax, (int)p.Y);
        }

        var area = (xmax - xmin) * (ymax - ymin);
        var xmid = xmin + xmax >> 1;
        var ymid = ymin + ymax >> 1;

        var pts = new int[][]
        {
            new [] { xmin, ymin },
            new [] { xmax, ymin },
            new [] { xmin, ymax },
            new [] { xmax, ymax },
            new [] { xmin, ymid },
            new [] { xmid, ymin },
            new [] { xmid, ymax },
            new [] { xmax, ymid },
            new [] { xmid, ymid },
        };

        var list = new List<int>[pts.Length];
        for (int i = 0; i < pts.Length; i++)
            list[i] = SortByNearness(pts[i][0], pts[i][1], manhattan);
        return list;
    }

    List<Cluster> BuildInitialList(List<int>[] list)
    {
        var seen = new bool[n + 1];
        var initialSet = new List<Cluster>();

        var indices = new int[list.Length];
        while (true)
        {
            var cands = new int[4];
            var cands2 = new int[4];

            var pos1 = 0;
            var pos2 = 0;
            for (int i = 0; i < 8; i++)
            {
                while (indices[i] < n && seen[list[i][indices[i]]])
                    indices[i]++;

                var index = indices[i];
                if (index < n)
                {
                    if (i < 4)
                        cands[pos1++] = list[i][index];
                    else
                        cands2[pos2++] = list[i][index];
                }
            }

            var cluster1 = pos1 == 4 ? ToClusterCV(cands) : null;
            Cluster cluster2 = pos2 == 4 ? ToClusterCV(cands2) : null;

            if (cluster1 != null && cluster1.Points.Length < 4) cluster1 = null;
            if (cluster2 != null && cluster2.Points.Length < 4) cluster2 = null;
            if (cluster1 == null && cluster2 == null)
                break;

            Cluster cluster;
            if (cluster2 == null || cluster1 != null && cluster1.Score >= cluster2.Score)
            {
                cluster = cluster1;
                for (int i = 0; i < 4; i++)
                    indices[i]++;
            }
            else
            {
                cluster = cluster2;
                for (int i = 4; i < 8; i++)
                    indices[i]++;
            }

            foreach (var v in cluster.Points)
                seen[v] = true;

            if (initialSet.Count * Factor >= n) break;
            initialSet.Add(cluster);
        }

        return initialSet;
    }

    List<Cluster> ProduceClusters(int[] choices, IEnumerable<Cluster> initialClusters)
    {
        var queue = new Queue<int>();
        var backqueue = new Queue<int>();
        var throwAway = new Queue<int>();
        var list = new List<int>();
        int index = 0;

        List<Cluster> clusters = new List<Cluster>();
        if (initialClusters != null) clusters.AddRange(initialClusters);
        int[] seen = new int[101];

        while (true)
        {
            const int Chances = 3;

            list.Clear();

            while (list.Count < Shift)
            {
                int v;
                if (queue.Count > 0)
                    v = queue.Dequeue();
                else if (index < choices.Length)
                    v = choices[index++];
                else if (backqueue.Count > 0)
                    v = backqueue.Dequeue();
                else
                    break;

                seen[v]++;
                if (seen[v] < Chances)
                    list.Add(v);
                else if (seen[v] == Chances)
                    backqueue.Enqueue(v);
                else
                    throwAway.Enqueue(v);
            }

            if (list.Count == 0)
                break;

            Search(list, out var dp, out var backref);

            int head = (1 << list.Count) - 1;
            AddClusters(clusters, list, dp, backref, head, queue);
        }

        FlushQueue(throwAway, clusters, true);
        return clusters;
    }

    void AddClusters(List<Cluster> clusters,
        List<int> list,
        long[] dp,
        int[] backref,
        int head,
        Queue<int> queue)
    {
        if (head == 0)
            return;

        if (dp[head] == 0)
        {
            foreach (var v in FromBitsToArray(list, head))
                queue.Enqueue(v);
            return;
        }

        if (backref[head] == 0)
        {
            clusters.Add(FromBitsToCluster(list, head));
            return;
        }

        var head1 = backref[head];
        var head2 = head ^ head1;
        AddClusters(clusters, list, dp, backref, head1, queue);
        AddClusters(clusters, list, dp, backref, head2, queue);
    }


    int[] FromBitsToArray(IList<int> choices, int bits)
    {
        int bitCount = BitCount(bits);
        var array = new int[bitCount];
        int index = 0;
        for (int b = 0; 1 << b <= bits; b++)
        {
            if ((bits & 1 << b) != 0)
                array[index++] = choices[b];
        }
        return array;
    }

    List<int> sample = new List<int>();
    List<int> FromBitsToBuffer(IList<int> choices, int bits)
    {
        sample.Clear();
        int bitCount = BitCount(bits);
        int index = 0;
        for (int b = 0; 1 << b <= bits; b++)
        {
            if ((bits & 1 << b) != 0)
                sample.Add(choices[b]);
        }
        return sample;
    }


    Cluster FromBitsToCluster(IList<int> choices, int bits)
    {
        return ToClusterCV(FromBitsToArray(choices, bits));
    }


    int[] sizedSet;
    long[] dp = new long[1 << 16];
    int[] backref = new int[1 << 16];

    void Search(IList<int> choices, out long[] dp, out int[] backref)
    {
        int size = 1 << choices.Count;

        dp = this.dp;
        backref = this.backref;

        Array.Clear(dp, 0, size);
        Array.Clear(backref, 0, size);
        if (sizedSet == null)
        {
            sizedSet = new int[6000];
            for (int i = 1, pos = 0; pos < sizedSet.Length; i++)
            {
                var bc = BitCount(i);
                if (bc < 3 || bc > 6) continue;
                sizedSet[pos++] = i;
            }
        }

        for (int i = 0; i < size; i++)
        {
            if (BitCount(i) < 3) continue;

            var cv = FromBitsToBuffer(choices, i);
            dp[i] = Area(cv);

            for (int jj = 0; jj < sizedSet.Length; jj++)
            {
                var j = sizedSet[jj];
                if (j >= i) break;
                if ((j & i) != j) continue;
                var sum = dp[j] + dp[j ^ i];
                if (sum < dp[i]) continue;
                if (sum == dp[i] && BitCount(j) >= BitCount(backref[i]))
                    continue;

                dp[i] = sum;
                backref[i] = j;
            }
        }
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

    List<int> SortByNearness(int x, int y, bool manhattan)
    {
        var dist = new long[points.Length];
        for (int i = 0; i < dist.Length; i++)
        {
            long dx = points[i].X - x;
            long dy = points[i].Y - y;
            dist[i] = manhattan
                ? Abs(dx) + Abs(dy)
                : dx * dx + dy * dy;
        }

        var list = new List<int>(points.Length);
        for (int i = 1; i < points.Length; i++)
            list.Add(i);

        list.Sort((a, b) => dist[a].CompareTo(dist[b]));
        return list;
    }
    #endregion

    List<Cluster> Trial(int[] ordering)
    {
        var list = new List<Cluster>(initialClusters.Select(x => x.Clone()));
        var queue = new Queue<int>();
        foreach (var v in ordering) queue.Enqueue(v);
        FlushQueue(queue, list, true);
        return list;
    }



    void PostFix(List<Cluster> list)
    {
        var queue = new Queue<int>();

        list.RemoveAll(x =>
        {
            if (x.Points.Length >= 3) return false;
            foreach (var v in x.Points)
                queue.Enqueue(v);
            return true;
        });

        // Parade pts through each && pick best times
        int failures = 0;
        bool adjusted = true;
        while (queue.Count > 0)
        {
            var pop = queue.Dequeue();
            if (AddToBestCluster(queue, list, pop, adjusted))
            {
                failures = 0;
            }
            else
            {
                failures++;
                if (failures >= queue.Count)
                {
                    if (adjusted)
                    {
                        failures = 0;
                        adjusted = false;
                    }
                    else
                        break;
                }
            }
        }

        FlushQueue(queue, list);
    }


    List<int> bcTrial = new List<int>();

    bool AddToBestCluster(Queue<int> returnQueue, List<Cluster> cluster, int x, bool checkAdjusted)
    {
        Cluster best = null;
        int bestIndex = -1;
        long bestImprovement = 0;

        for (int i = 0; i < cluster.Count; i++)
        {
            var c = cluster[i];

            bcTrial.Clear();
            bcTrial.AddRange(c.Points);
            bcTrial.Add(x);

            var cv = ConvexHull(bcTrial);
            if (IndexOf(cv, x) < 0)
                continue;

            var c2 = ToClusterNoCV(cv);
            var impr = c2.Score - c.Score;
            if (impr <= bestImprovement) continue;

            if (checkAdjusted && c2.AdjustedScore < c.AdjustedScore) continue;

            bestIndex = i;
            best = c2;
            bestImprovement = impr;
        }

        if (best == null)
        {
            returnQueue.Enqueue(x);
            return false;
        }

        var old = cluster[bestIndex];
        cluster[bestIndex] = best;

        if (old.Points.Length + 1 != best.Points.Length)
        {
            foreach (var p in old.Points)
            {
                if (!best.Points.Contains(p))
                    returnQueue.Enqueue(p);
            }
        }

        return true;
    }

    List<Cluster> PostFix2(List<Cluster> clusters)
    {
        var queue = new Queue<int>();
        while (true)
        {
            var newClusters = new List<Cluster>();
            for (int i = 0; i < clusters.Count; i++)
            {
                var c1 = clusters[i];
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    var c2 = clusters[j];
                    var c3 = Join(c1, c2);
                    c3.Index1 = (byte)i;
                    c3.Index2 = (byte)j;
                    c3.Improvement = c3.Score - c1.Score - c2.Score;
                    if (c3.Improvement > 0)
                        newClusters.Add(c3);
                }
            }

            if (newClusters.Count == 0) break;

            var used = 0L;
            newClusters.Sort((a, b) => -a.Improvement.CompareTo(b.Improvement));
            for (int i = 0; i < newClusters.Count; i++)
            {
                var c = newClusters[i];
                if ((used & ((1L << c.Index1) | (1L << c.Index2))) != 0)
                    continue;

                var c1 = clusters[c.Index1];
                var c2 = clusters[c.Index2];
                if (c1.Points.Length + c2.Points.Length > c.Points.Length)
                {
                    foreach (var p in c1.Points)
                    {
                        if (!c.Points.Contains(p))
                            queue.Enqueue(p);
                    }
                    foreach (var p in c2.Points)
                    {
                        if (!c.Points.Contains(p))
                            queue.Enqueue(p);
                    }
                }

                clusters[c.Index1] = c;
                clusters[c.Index2] = null;
                used |= 1L << c.Index1 | 1L << c.Index2;
            }

            clusters.RemoveAll(x => x == null);
        }

        FlushQueue(queue, clusters);
        return clusters;
    }

    List<Cluster> FindAllTriangles(int[] choices)
    {
        var list = new List<Cluster>();
        int length = choices.Length;
        for (int i = 0; i < length - 2; i++)
            for (int j = i + 1; j < length - 1; j++)
                for (int k = j + 1; k < length; k++)
                {
                    var array = new[] { choices[i], choices[j], choices[k] };
                    var cluster = ToClusterNoCV(array);
                    list.Add(cluster);
                }
        list.Sort((a, b) => -a.Score.CompareTo(b.Score));
        return list;
    }

    List<Cluster> FindMaximum(List<Cluster> triangles)
    {
        var used = new bool[n + 1];
        int usedCount = 0;
        var result = new List<Cluster>(initialClusters);
        foreach (var triangle in triangles)
        {
            var good = true;
            foreach (var p in triangle.Points)
            {
                if (used[p])
                {
                    good = false;
                    break;
                }
            }

            if (!good) continue;

            foreach (var p in triangle.Points)
            {
                used[p] = true;
                usedCount++;
            }

            result.Add(triangle);
        }
        return result;
    }

    List<Cluster> FindMaximum2(List<Cluster> triangles)
    {
        var max = new long[n + 1];
        foreach (var t in triangles)
        {
            foreach (var p in t.Points)
                max[p] = Max(max[p], t.Score);
        }

        var points = new List<int>(n);
        for (int i = 1; i < max.Length; i++)
            if (max[i] != 0)
                points.Add(i);

        points.Sort((a, b) => max[a].CompareTo(max[b]));

        var used = new bool[n + 1];
        var result = new List<Cluster>(initialClusters);
        foreach (var p in points)
        {
            if (used[p]) continue;

            foreach (var t in triangles)
            {
                var good = true;
                var found = false;
                foreach (var p2 in t.Points)
                {
                    if (p2 == p) found = true;
                    else if (used[p2])
                    {
                        good = false;
                        break;
                    }
                }

                if (found && good)
                {
                    foreach (var p2 in t.Points)
                        used[p2] = true;
                    result.Add(t);
                    break;
                }
            }

        }
        return result;
    }

    //void CheckCluster(Cluster c)
    //{
    //    var newList = ConvertAll(c.Points, x => new CaideTester.Point2D {X = points[x].X, Y = points[x].Y});
    //    var cv = new CaideTester.ConvexHull(newList.ToList());
    //    var area = CaideTester.ConvexHull.Area(cv.Points);

    //    var cv2 = ConvexHull(c.Points);
    //    var area2 = Area(cv2.ToArray());

    //    Debug.Assert(c.Score == 2*area);
    //}


    #region Clusters

    Cluster ToClusterCV(int[] points)
    {
        var cv = ConvexHull(points);
        return ToClusterNoCV(cv.ToArray());
    }

    Cluster ToClusterNoCV(int[] points)
    {
        var score = Area(points);
        var c = new Cluster
        {
            Points = points,
            Score = score,
            AdjustedScore = score * 1.0 / points.Length
        };

        //CheckCluster(c);
        return c;
    }

    static void Shuffle<T>(T[] list, int start, int count)
    {
        var n = start + count;
        for (var i = start; i + 1 < n; i++)
        {
            var j = random.Next(i, n); // random # from [i,n)
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    long Score(List<Cluster> list)
    {
        var score = 0L;
        foreach (var v in list)
            score += v.Score;
        return score;
    }

    int[] Exclude(IEnumerable<int> set)
    {
        var used = new bool[n + 1];

        int count = n;
        foreach (var v in set)
        {
            if (used[v]) continue;
            used[v] = true;
            count--;
        }

        return ExcludeCore(used, count);
    }

    int[] ExcludeCore(bool[] used, int count)
    {
        var result = new int[count];
        int pos = 0;
        for (int v = 1; v <= n; v++)
        {
            if (used[v]) continue;
            result[pos++] = v;
        }

        Debug.Assert(pos == count);
        return result;
    }


    Cluster Join(Cluster cluster1, Cluster cluster2)
    {
        var join = Join(cluster1.Points, cluster2.Points);
        return ToClusterCV(join);
    }

    int[] Join(int[] array1, int[] array2)
    {
        var result = new int[array1.Length + array2.Length];
        int pos = 0;
        for (int i = 0; i < array1.Length; i++)
            result[pos++] = array1[i];
        for (int i = 0; i < array2.Length; i++)
            result[pos++] = array2[i];
        Debug.Assert(pos == result.Length);
        return result;
    }


    void FlushQueue(Queue<int> queue, List<Cluster> clusters, bool fix = false)
    {
        while (queue.Count >= 3)
        {
            //var take = Take;
            var take = queue.Count < 2 * Take ? queue.Count : Min(Take, queue.Count);
            int[] result = new int[take];
            for (int t = 1; t <= take; t++)
                result[t - 1] = queue.Dequeue();

            var cv = ConvexHull(result);
            if (cv.Length != result.Length)
            {
                foreach (var v in result)
                    if (IndexOf(cv, v) < 0)
                        queue.Enqueue(v);
            }

            clusters.Add(ToClusterNoCV(cv));
        }

        if (fix)
            PostFix(clusters);

        if (queue.Count > 0)
        {
            var result = new int[queue.Count];
            int pos = 0;
            while (queue.Count > 0)
                result[pos++] = queue.Dequeue();
            clusters.Add(ToClusterCV(result));
        }
    }

    #endregion

    #region Convex Hull
    readonly List<int> buffer = new List<int>();
    readonly List<int> up = new List<int>();
    readonly List<int> down = new List<int>();

    List<int> ConvexHull2(IEnumerable<int> origPoints)
    {
        buffer.Clear();
        buffer.AddRange(origPoints);
        buffer.Sort(PointCompare);

        var uniqueEnd = Unique(buffer);
        buffer.RemoveRange(uniqueEnd, buffer.Count - uniqueEnd);

        up.Clear();
        down.Clear();
        for (var i = 0; i < buffer.Count; i++)
        {
            while (up.Count > 1
                   && Area2(points[up[up.Count - 2]],
                            points[up[up.Count - 1]],
                            points[buffer[i]]) > 0) up.RemoveAt(up.Count - 1);
            while (down.Count > 1
                   && Area2(points[down[down.Count - 2]],
                       points[down[down.Count - 1]],
                       points[buffer[i]]) < 0)
                down.RemoveAt(down.Count - 1);
            up.Add(buffer[i]);
            down.Add(buffer[i]);
        }

        for (var i = up.Count - 2; i >= 1; i--)
            if (!down.Contains(up[i])) down.Add(up[i]);

        return down;
    }

    int[] ConvexHull(IEnumerable<int> origPoints)
    {
        return ConvexHull2(origPoints).ToArray();
    }

    public static Comparison<int> PointCompare = (a, b) =>
    {
        int cmp = points[a].X.CompareTo(points[b].X);
        return cmp != 0 ? cmp : points[a].Y.CompareTo(points[b].Y);
    };

    public static int Unique(List<int> list)
    {
        var write = Math.Min(list.Count, 1);
        for (int read = 1; read < list.Count; read++)
        {
            if (PointCompare(list[write - 1], list[read]) != 0)
                list[write++] = list[read];
        }

        list.RemoveRange(write, list.Count - write);
        return write;
    }

    static double Cross(Point2D p, Point2D q) => p.X * q.Y - p.Y * q.X;

    static double Area2(Point2D a, Point2D b, Point2D c)
        => Cross(a, b) + Cross(b, c) + Cross(c, a);
    //=> (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

    public static long Area(int[] p)
    {
        long area = 0;
        if (p.Length == 0) return 0;
        for (var i = 0; i < p.Length; i++)
        {
            var j = (i + 1) % p.Length;
            area += points[p[i]].X * points[p[j]].Y - points[p[j]].X * points[p[i]].Y;
        }
        return Abs(area);
    }

    public static long Area(List<int> p)
    {
        long area = 0;
        if (p.Count == 0) return 0;
        for (var i = 0; i < p.Count; i++)
        {
            var j = (i + 1) % p.Count;
            area += points[p[i]].X * points[p[j]].Y - points[p[j]].X * points[p[i]].Y;
        }
        return Abs(area);
    }

    #endregion

    #region Helpers
    public class Cluster : IComparable<Cluster>
    {
        public bool Shared;
        public byte Index1;
        public byte Index2;
        public long Score;
        public long Improvement;
        public double AdjustedScore;
        public int[] Points;

        public int CompareTo(Cluster other)
        {
            return -Score.CompareTo(other.Score);
        }

        public Cluster Clone(bool shared = true)
        {
            if (shared)
            {
                Shared = true;
                return this;
            }

            return new Cluster
            {
                Score = Score,
                AdjustedScore = AdjustedScore,
                Points = (int[])Points.Clone()
            };
        }

        public override string ToString() => $"Score={Score}    " + string.Join(" ", Points);
    }

    public struct Point2D : IEquatable<Point2D>
    {
        public long X, Y;

        public bool Equals(Point2D other) => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object obj)
            => obj is Point2D && Equals((Point2D)obj);

        public override int GetHashCode()
            => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
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

    static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    static int Bound<T>(T[] array, T value, bool upper = false)
        where T : IComparable<T>
    {
        int left = 0;
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int cmp = value.CompareTo(array[mid]);
            if (cmp > 0 || cmp == 0 && upper)
                left = mid + 1;
            else
                right = mid - 1;
        }
        return left;
    }

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
        outputBuffer = new byte[4096];
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
        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());

        new Solution().Solve();

        Flush();
    }
    #endregion
    #endregion
}
