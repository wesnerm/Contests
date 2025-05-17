//#undef DEBUG
#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    const bool RegularGradient = false;
    const int StartDiv = 1; // 8/5->98055
    const int EndDiv = 1;
    const int Approx = 0;
    const int GradientThreshold = 1;

    int n, m;
    int[][] bounds;
    long[][] a;
    int t;
    TestCase testCase;
    Process process = System.Diagnostics.Process.GetCurrentProcess();
    static Random random = new Random(1000);
    List<TestCase> testCases = new List<TestCase>();
    FenwickTree ft = new FenwickTree(1000);
    #endregion

    public void Solve()
    {
        ft = new FenwickTree(1000);

#if DEBUG
        var totalInversions = 0L;
        var totalRealInversions = 0L;
        var totalScore = 0L;
        TestData();
#else
        ReadData();
#endif

        InitTestCases(limit: true);

        for (t = 0; t < testCases.Count; t++)
        {
            LoadTestCase(testCases[t]);
#if DEBUG
            var startTime = process.TotalProcessorTime.TotalMilliseconds;
            Console.Error.WriteLine($"\nN = {n} M={m}");
#endif

            var result1 = RankFromInversions();
            //var result1 = GradientDescent(RegularGradient);
            //var result1 = GeneticAlgo();
#if DEBUG
            var bucket = new Bucket(this, result1);
            var inversions = bucket.InversionCount;
            var realInversions = bucket.RealInversions;
            var score = Max(0, n * 1L * (n - 1) / 4 - realInversions);
            var duration = process.TotalProcessorTime.TotalMilliseconds - startTime;
            totalInversions += inversions;
            totalRealInversions += realInversions;
            totalScore += score;

            Console.Error.WriteLine($"Score: {score}    Inversions: Appr-{inversions}  Real-{realInversions} Duration: {duration}  Validated: {Validate(result1)}");

#endif

            testCase.Answer = result1;
        }

        foreach (var v in testCases.OrderBy(x => x.I))
            WriteLine(string.Join(" ", v.Answer));

        Flush();

#if DEBUG
        Console.Error.WriteLine($"\nTotal Inversions: Test-{totalInversions} Real-{totalRealInversions} Score: {totalScore}");
#endif
    }

    public bool Validate(int[] assignments)
    {
        for (int i = 0; i < assignments.Length; i++)
        {
            var v = assignments[i];
            if (v > bounds[i][1] || v < bounds[i][0])
                return false;
        }

        return true;
    }

    #region Gradient Descent
    int[] GradientDescent(bool regular = true)
    {
        //var bucket = new Bucket(this, bounds.Select(x => x[0] + (int)((x[1]+0L-x[0]) *3L/4L) ).ToArray());
        //var bucket = new Bucket(this, bounds.Select(x => random.Next(x[0], x[1])).ToArray());

        //var bucket = new Bucket(this, RankFromInversions());
        var bucket = new Bucket(this, bounds.Select(x => x[0]).ToArray());
        var start = process.TotalProcessorTime.TotalMilliseconds;
        var limit = testCase.TimeLimit;

        // Emergency scoring
        if (start > limit) return RankFromWeights();

        for (int i = 0; i < 16; i++)
        {
            var prevInversions = bucket.InversionCount;

            int dir = Math.Sign(EndDiv - StartDiv);
            var div = StartDiv + dir * i;
            div = Min(div, Max(StartDiv, EndDiv));
            div = Max(div, Min(StartDiv, EndDiv));
            bucket = GradientPass(bucket, true, div, limit);

#if DEBUG
            var time = process.TotalProcessorTime.TotalMilliseconds;
            Console.Error.WriteLine($"{prevInversions} -> {bucket.InversionCount} = {bucket.InversionCount - prevInversions}  Divs {div} {time - start}/{time}ms");
#endif

            if (Abs(prevInversions - bucket.InversionCount) <= GradientThreshold
                || process.TotalProcessorTime.TotalMilliseconds > limit) break;
        }

        bucket = GradientPass(bucket, false, 20);

        return bucket.Assignment;
    }


    public static void Shuffle<T>(IList<T> list, int start, int count)
    {
        var n = start + count;
        var random = new Random();
        for (var i = start; i + 1 < n; i++)
        {
            var j = random.Next(i, n); // random # from [i,n)
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static void Shuffle<T>(IList<T> list)
    {
        Shuffle(list, 0, list.Count);
    }

    Bucket GradientPass(Bucket bucket,
        bool regular = true, int divisions = 2, int limit = int.MaxValue)
    {
        var newAssts = regular ? (int[])bucket.Assignment.Clone() : null;
        int inv = bucket.InversionCount;
        bool changed = false;

        var ms = Enumerable.Range(0, m).ToList();
        Shuffle(ms);

        foreach(var j in ms)
        {
            if (process.TotalProcessorTime.TotalMilliseconds > limit) break;

            long lo = bounds[j][0];
            long diff = bounds[j][1] - lo;
            long chop = Min(divisions, diff);
            for (int k = 0; k <= chop; k++)
            {
                int newv = (int)(lo + diff * k / chop);
                //int newv = random.Next((int) lo, (int) (lo + diff + 1));
                bucket.Update(j, newv, false);
            }

            if (newAssts != null)
            {
                if (bucket.InversionCount < inv)
                {
                    newAssts[j] = bucket.Assignment[j];
                    changed = true;
                }
                bucket.Update(j, newAssts[j], false, inv);
            }
        }

        bucket.PrevInversions = inv;
        if (changed)
        {
            bucket = new Bucket(this, newAssts);
            bucket.PrevInversions = inv;
        }

        return bucket;
    }
    #endregion

    #region Statistics

    public static double Average(params double[] numbers)
    {
        return Sum(numbers) / numbers.Length;
    }

    public static double Sum(params double[] numbers)
    {
        double sum = 0;
        foreach (var t in numbers)
            sum += t;
        return sum;
    }

    public static double SumSquared(params double[] numbers)
    {
        double sum = 0;
        foreach (var n in numbers)
            sum += n * n;
        return sum;
    }

    public static int Sum(params int[] numbers)
    {
        var sum = 0;
        foreach (var t in numbers)
            sum += t;
        return sum;
    }

    [DebuggerStepThrough]
    public static double Variance(double[] x)
    {
        return Covariance(x, x);
    }

    [DebuggerStepThrough]
    public static double Stdev(double[] x)
    {
        return Math.Sqrt(Variance(x));
    }

    [DebuggerStepThrough]
    public static double RSquared(double[] x, double[] y)
    {
        var cov = Covariance(x, y);
        return cov * cov / (Variance(x) * Variance(y));
    }

    [DebuggerStepThrough]
    public static double Correlation(double[] x, double[] y)
    {
        return Math.Sqrt(RSquared(x, y));
    }

    public static double Covariance(double[] x, double[] y)
    {
        var n = x.Length;
        if (n != y.Length)
            throw new ArgumentException();

        double sumxy = 0;
        var sumx = Sum(x);
        var sumy = x == y ? sumx : Sum(y);
        var averagex = sumx / n;
        var averagey = sumy / n;

        for (var i = 0; i < n; i++)
            sumxy += (x[i] - averagex) * (y[i] - averagey);

        return sumxy / n;
    }


    public static void FindLine(double[] x, double[] y, out double slope, out double bias)
    {
        var n = x.Length;
        if (n != y.Length)
            throw new ArgumentException();

        var sumx = Sum(x);
        var sumy = Sum(y);
        var averagex = sumx / n;
        var averagey = sumy / n;
        double wnum = 0;
        double den = 0;
        double sumx2 = 0;
        double sumxy = 0;

        for (var i = 0; i < n; i++)
        {
            var diffx = x[i] - averagex;
            var diffy = y[i] - averagey;

            den += diffx * diffx;
            wnum += diffx * diffy;
            sumx2 += x[i] * x[i];
            sumxy += x[i] * y[i];
        }

        bias = (sumx2 * sumy - sumx * sumxy) / den;
        slope = wnum / den;
    }

    int[] RankFromInversions()
    {
        long[] scores = new long[m];
        int[] rank = new int[n];
        double[] w = new double[n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                rank[j] = j;
                w[j] = a[j][i];
            }

            Sort(w, rank);
            Reverse(rank);
            scores[i] = ComputeInversions(rank);
        }

        return ScoresToResult(scores);
    }

    #endregion


    #region Genetic Algo
    int[] GeneticAlgo()
    {
        var start = process.TotalProcessorTime.TotalMilliseconds;
        var limit = testCase.TimeLimit;

        var sortedSet = new List<Bucket>();
        var list = new List<Bucket>();

        var gp = new List<Bucket>();
        foreach (var e in Seed())
        {
            sortedSet.Add(e);
            //gp.Add(GradientPass(e, true, 2, limit));
        }

        SortBuckets(sortedSet);
        for (int i = 0; i < 16; i++)
        {
            if (process.TotalProcessorTime.TotalMilliseconds > limit) break;

            var min = sortedSet[0];
            var prevInversions = min.InversionCount;

            list.Clear();
            list.AddRange(sortedSet);
            sortedSet.Clear();

            foreach (var e in list)
            {
                if (e.PrevInversions == null || e.InversionCount - e.PrevInversions > 100)
                   gp.Add(GradientPass(e, true, 2, limit));
            }

            for (int j = 0; j < list.Count; j++)
            {
                if (process.TotalProcessorTime.TotalMilliseconds > limit) break;

                var aj = list[j].Assignment;
                sortedSet.Add(new Bucket(this, Perturb(aj)));
                for (int k = j + 1; k < list.Count; k++)
                {
                    var ak = list[k].Assignment;
                    sortedSet.Add(new Bucket(this, Blend(aj, ak)));
                    sortedSet.Add(new Bucket(this, RandomSelection(aj, ak)));
                    sortedSet.Add(new Bucket(this, RandomSelection(aj, ak)));
                }
            }

            // Produce a composite child
            var combined = new int[m];
            for (int j = 0; j < list.Count; j++)
            {
                var aj = list[j].Assignment;
                for (int k = 0; k < m; k++)
                    combined[k] += aj[k];
            }
            for (int k = 0; k < m; k++)
                combined[k] /= list.Count;
            sortedSet.Add(new Bucket(this, combined));

            SortBuckets(sortedSet);

            var bucket = sortedSet[0];
#if DEBUG
            var time = process.TotalProcessorTime.TotalMilliseconds;
            Console.Error.WriteLine($"{prevInversions} -> {bucket.InversionCount} = {bucket.InversionCount - prevInversions}   {time - start}/{time}ms");
#endif
            if (bucket.InversionCount - prevInversions < 100) break;
        }

        sortedSet.AddRange(gp);
        sortedSet.Sort();
        return sortedSet[0].Assignment;
    }

    void SortBuckets(List<Bucket> buckets)
    {
        buckets.Sort();
        Bucket prev = null;
        int count = 0;
        buckets.RemoveAll(x =>
        {
            if (x == prev || count == 6) return true;
            prev = x;
            count++;
            return false;
        });
    }

    #region Operators
    int[] Blend(int[] x, int[] y)
    {
        var result = new int[m];
        for (int i = 0; i < m; i++)
            result[i] = x[i] + y[i] >> 1;
        return result;
    }

    int[] RandomSelection(int[] x, int[] y)
    {
        var result = new int[m];
        var r = 0;
        for (int i = 0; i < m; i++)
        {
            while (r < 2) r = random.Next();
            result[i] = (r & 1) == 0 ? x[i] : y[i];
            r >>= 1;
        }
        return result;
    }

    int[] RandomMap()
    {
        var result = new int[m];
        for (int i = 0; i < m; i++)
            result[i] = random.Next(bounds[i][0], bounds[i][1] + 1);
        return result;
    }

    int[] Perturb(int[] x)
    {
        var result = new int[m];
        for (int i = 0; i < m; i++)
            result[i] = Max(Min(x[0] + random.Next(-3, 4), bounds[i][1]), bounds[i][0]);
        return result;
    }
    #endregion
    #endregion

 

    #region Seed
    SortedSet<Bucket> Seed(int random = 50)
    {
        var sortedSet = new SortedSet<Bucket>
        {
            new Bucket(this, RankFromWeights()),
            new Bucket(this, RankFromInversions()),
        };

        sortedSet.UnionWith(RankFromConstraints());

        sortedSet.UnionWith(new[]
        {
            new Bucket(this, bounds.Select(x => x[0] + x[1] >> 1).ToArray()),
            new Bucket(this, bounds.Select(x => x[0]).ToArray()),
            new Bucket(this, bounds.Select(x => x[1]).ToArray()),
        });

        for (int i = 0; i < random; i++)
            sortedSet.Add(new Bucket(this, RandomMap()));

        return sortedSet;
    }

    public Bucket[] RankFromConstraints()
    {
        var total = new double[m];
        var localInv = new int[m];
        var result = new int[m];
        var result2 = new int[m];

        for (int j = 0; j < m; j++)
        {
            for (int i = 1; i < n; i++)
            {
                var diff = a[i][j] - a[i - 1][j];
                total[j] += diff;
                if (diff > 0) localInv[j]++;
            }

            result[j] = total[j] < 0 ? bounds[j][1] : bounds[j][0];
            result2[j] = localInv[j] * 2 < n ? bounds[j][1] : bounds[j][0];
        }

        var results = new[]
        {
            new Bucket(this, result),
            new Bucket(this, result2)
        };

        return results;
    }


    #endregion


    #region Buckets

    public class Bucket : IComparable<Bucket>
    {
        public static int _counter;
        public Solution sol;
        public int InversionCount;
        public int[] Assignment;
        public long[] Scores;
        public long[] Swap;
        public int Id;
        public int? PrevInversions;

        public Bucket(Solution sol, int[] assignment)
        {
            this.sol = sol;
            this.Assignment = assignment;
            this.Id = _counter++;
            FullUpdate();
        }

        public Bucket Clone()
        {
            var clone = (Bucket)MemberwiseClone();
            clone.Id = _counter++;
            clone.Assignment = (int[])Assignment.Clone();
            clone.Scores = (long[])Scores.Clone();
            clone.Swap = (long[])Swap.Clone();
            return clone;
        }

        public bool Update(int index, int value, bool force = true, int inv = -1)
        {
            int prev = Assignment[index];
            Assignment[index] = value;

            var diff = value - prev;
            if (diff == 0)
                return false;

            Swap(ref Scores, ref Swap);

            for (int i = sol.n - 1; i >= 0; i--)
                Scores[i] = Swap[i] + diff * sol.a[i][index];

            if (inv != -1)
            {
                InversionCount = inv;
                return true;
            }

            //var inversions = SortAndCountInversions();

            var inversions = ComputeInversions();
            //Debug.Assert(inversions == inversions2);

            if (force || inversions <= InversionCount)
            {
                InversionCount = inversions;
            }
            else
            {
                // -1 for undo
                Assignment[index] = prev;
                Swap(ref Scores, ref Swap);
                return false;
            }

            return true;
        }

        void FullUpdate()
        {
            var a = sol.a;
            var n = sol.n;
            var m = sol.m;
            var scores = Scores = new long[n];
            Swap = new long[n];

            for (int i = 0; i < n; i++)
            {
                var row = a[i];
                var score = 0L;
                for (int j = 0; j < m; j++) score += row[j] * Assignment[j];
                scores[i] = score;
            }

            InversionCount = ComputeInversions();
        }

        public static int[] ordering;
        public static int[] sortBuffer;
        public long SortAndCountInversions()
        {
            for (int i = 0; i < ordering.Length; i++) ordering[i] = i;
            return SaciMerge(ordering, 0, ordering.Length - 1);
        }

        long SaciMerge(int[] arr, int left, int right)
        {
            //if (right <= left) return 0;

            if (right - left <= 1)
            {
                if (left >= right || CompareScore(arr[left], arr[right]) <= 0) return 0;
                Swap(ref arr[left], ref arr[right]);
                return 1;
            }


            int mid = right + left >> 1;
            var invCount = SaciMerge(arr, left, mid) + SaciMerge(arr, mid + 1, right);

            int i = left, j = mid + 1, k = left;
            var temp = sortBuffer;
            while (i <= mid && j <= right)
            {
                if (CompareScore(arr[i], arr[j]) <= 0)
                    temp[k++] = arr[i++];
                else
                {
                    temp[k++] = arr[j++];
                    invCount += mid - i + 1;
                }
            }

            while (i <= mid) temp[k++] = arr[i++];
            while (j <= right) temp[k++] = arr[j++];
            Copy(temp, left, arr, left, right - left + 1);
            return invCount;
        }

        int ComputeInversions()
        {

            return sol.ComputeInversions(Ordering());
        }

        public int RealInversions => sol.ComputeInversions(Ordering());

        int[] Ordering()
        {
            if (ordering == null || ordering.Length != sol.n)
            {
                ordering = new int[sol.n];
                sortBuffer = new int[sol.n];
                for (int i = 0; i < sol.n; i++)
                    ordering[i] = i;
            }
            Sort(ordering, CompareScore);
            return ordering;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CompareScore(int x, int y)
        {
            int cmp = -Scores[x].CompareTo(Scores[y]);
            if (cmp != 0) return cmp;
            return -x.CompareTo(y);
        }

        #region Helpers

        public int CompareTo(Bucket other)
        {
            int cmp = InversionCount.CompareTo(other.InversionCount);
            return cmp != 0 ? cmp : Id.CompareTo(other.Id);
        }

        public override string ToString()
        {
            return $"#{Id} - {InversionCount}";
        }

        #endregion
    }

    #region Scoring
    int ComputeInversions(int[] arr)
    {
        ft.Clear();

        int count = 0;
        for (int i = arr.Length - 1; i >= 0; i--)
        {
            var v = arr[i];
            count += ft.SumInclusive(v - 1);
            ft.Add(v, 1);
        }
        return count;
    }

    FenwickTree ft2 = new FenwickTree(256);

    long[] buffer1 = new long[1000];
    long[] buffer2 = new long[1000];
    int[] offsets = new int[256+1];
  

    int[] RankFromWeights()
    {
        long[] scores = new long[m];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                scores[i] += (long)(j * a[j][i] * 1000000);
        }

        return ScoresToResult(scores);
    }

    void Start()
    {
        double[][] vectors = new double[m][];
        for (int i = 0; i < m; i++)
        {
            vectors[i] = new double[n];
            for (int j = 0; j < n; j++)
                vectors[i][j] = n;
        }


    }
    int[] ScoresToResult2(long[] scores)
    {
        double min = scores.Min();
        double max = scores.Max();

        var inv = n * (n - 1) / 4;
        int[] result = new int[m];
        for (int i = 0; i < m; i++)
        {
            if (scores[i] > inv)
                result[i] = bounds[i][0];
            else
                result[i] = bounds[i][1];
        }

        return result;
    }


    int[] ScoresToResult(long[] scores)
    {
        double min = scores.Min();
        double max = scores.Max();

        int[] result = new int[m];
        for (int i = 0; i < m; i++)
        {
            double alpha = (scores[i] - min) / (max - min);
            result[i] = bounds[i][alpha>.5?0:1];
        }

        return result;
    }
    #endregion

    #endregion

    #region IO
    public class TestCase
    {
        public int N, M, I;
        public long[][] A;
        public int[][] Bounds;

        public int TimeLimit;
        public int[] Answer;

        public override string ToString() => $"{I} N={N} M={M}";
    }

    public void ReadData()
    {
        int testCount = Ni();

        for (t = 0; t < testCount; t++)
        {
            n = Ni();
            m = Ni();
            bounds = new int[m][];

            for (int i = 0; i < m; i++)
                bounds[i] = new[] { Ni(), Ni() };

            a = new long[n][];
            for (int i = 0; i < n; i++)
                a[i] = Nl(m);

            FillTestCase();
        }
    }

    void TestData()
    {
        for (t = 0; t < 5; t++)
        {
            n = 1000;

            m = new int[] { 10, 20, 50, 200, 1000 }[4 - t];

            bounds = new int[m][];
            for (int i = 0; i < m; i++)
            {
                int lo = random.Next(1, 1000000);
                int hi = random.Next(1, 1000000);
                if (hi < lo) Swap(ref lo, ref hi);
                bounds[i] = new[] { lo, hi };
            }

            a = new long[n][];
            for (int i = 0; i < n; i++)
                a[i] = N(m, () => 1L * random.Next(1, 1000000));

            FillTestCase();
        }
    }

    void FillTestCase()
    {
        testCases.Add(new TestCase
        {
            I = t,
            N = n,
            M = m,
            A = a,
            Bounds = bounds,
            TimeLimit = int.MaxValue>>2,
        });
    }

    void InitTestCases(bool limit = true)
    {
        testCases.Sort((a, b) => a.M.CompareTo(b.M));

        if (limit && testCases.Count == 5)
        {
            testCases[4].TimeLimit = 5900; // 1000
            testCases[3].TimeLimit = testCases[4].TimeLimit - 3000; // 200
            testCases[2].TimeLimit = testCases[3].TimeLimit - 1500; // 50
            testCases[1].TimeLimit = testCases[2].TimeLimit - 500; // 20
            testCases[0].TimeLimit = testCases[1].TimeLimit - 200; // 10
        }

#if DEBUG
        foreach (var testCase in testCases)
            testCase.TimeLimit *= 2;
#endif
    }

    void LoadTestCase(TestCase t)
    {
        testCase = t;
        n = testCase.N;
        m = testCase.M;
        a = testCase.A;
        bounds = testCase.Bounds;
    }

    #endregion

    #region Misc
    #region Helpers
    public class FenwickTree
    {
        #region Variables
        public readonly int[] A;
        #endregion

        #region Constructor
        public FenwickTree(int[] a) : this(a.Length)
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

        public FenwickTree(int size)
        {
            A = new int[size + 1];
        }
        #endregion

        #region Properties
        public int this[int index] => AltRangeUpdatePointQueryMode ? SumInclusive(index) : SumInclusive(index, index);

        public int Length => A.Length - 1;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public int[] Table
        {
            get
            {
                int n = A.Length - 1;
                int[] ret = new int[n];
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

        public void Add(int i, int val)
        {
            if (val == 0) return;
            for (i++; i < A.Length; i += (i & -i))
                A[i] += val;
        }

        // Sum from [0 ... i]
        public int SumInclusive(int i)
        {
            int sum = 0;
            for (i++; i > 0; i -= (i & -i))
                sum += A[i];
            return sum;
        }

        public int SumInclusive(int i, int j)
        {
            return SumInclusive(j) - SumInclusive(i - 1);
        }

        // get largest value with cumulative sum less than x;
        // for smallest, pass x-1 and add 1 to result
        public int GetIndexGreater(int x)
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
        public void AltAddInclusive(int left, int right, int delta)
        {
            Add(left, delta);
            Add(right + 1, -delta);
        }

        public int AltQuery(int index)
        {
            return SumInclusive(index);
        }


        #endregion
    }


    #endregion

    #region Mod Math
    public const int MOD = 1000000007;
    const int FactCache = 1000;

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

    public static long Mult(long left, long right) =>
        (left * right) % MOD;

    public static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    public static long Add(long x, long y) =>
        (x += y) >= MOD ? x - MOD : x;

    public static long Subtract(long x, long y) => (x -= y) < 0 ? x + MOD : x;

    public static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

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

    static List<long> _fact, _ifact;

    public static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(FactCache) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    public static long InverseFact(int n)
    {
        if (_ifact == null) _ifact = new List<long>(FactCache) { 1 };
        for (int i = _ifact.Count; i <= n; i++)
            _ifact.Add(Div(_ifact[i - 1], i));
        return _ifact[n];
    }

    public static long Fact(int n, int m)
    {
        var fact = Fact(n);
        if (m < n) fact = fact * InverseFact(n - m) % MOD;
        return fact;
    }

    public static long Comb(int n, int k)
    {
        if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
        return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
    }
    #endregion

    #region Common
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref T a, ref T b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    public static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    public static V Get<K, V>(Dictionary<K, V> dict, K key) where V : new()
    {
        V result;
        if (dict.TryGetValue(key, out result) == false)
            result = dict[key] = new V();
        return result;
    }

    public static int Bound<T>(T[] array, T value, bool upper = false)
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

    public static long IntPow(long n, long p)
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
    public static int Log2(long value)
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
    public static int BitCount(long x)
    {
        int count;
        var y = unchecked((ulong)x);
        for (count = 0; y != 0; count++)
            y &= y - 1;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HighestOneBit(int n) => n != 0 ? 1 << Log2(n) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long HighestOneBit(long n) => n != 0 ? 1L << Log2(n) : 0;
    #endregion

    #region Fast IO
    #region  Input
    static System.IO.Stream inputStream;
    static int inputIndex, bytesRead;
    static byte[] inputBuffer;
    static System.Text.StringBuilder builder;
    const int MonoBufferSize = 4096;

    public static void InitInput(System.IO.Stream input = null, int stringCapacity = 16)
    {
        builder = new System.Text.StringBuilder(stringCapacity);
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
        inputBuffer[0] = (byte)'\n';
    }

    public static int Read()
    {
        if (inputIndex >= bytesRead) ReadMore();
        return inputBuffer[inputIndex++];
    }

    public static T[] N<T>(int n, Func<T> func)
    {
        var list = new T[n];
        for (int i = 0; i < n; i++) list[i] = func();
        return list;
    }

    public static int[] Ni(int n) => N(n, Ni);

    public static long[] Nl(int n) => N(n, Nl);

    public static string[] Ns(int n) => N(n, Ns);

    public static int Ni() => checked((int)Nl());

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg) { c = Read(); }

        long number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
            if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static char[] Nc(int n)
    {
        var list = new char[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
        return list;
    }

    public static string Ns()
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

    public static int SkipSpaces()
    {
        int c;
        do c = Read(); while (unchecked((uint)c - 33 >= (127 - 33)));
        return c;
    }

    public static string ReadLine()
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
    static System.IO.Stream outputStream;
    static byte[] outputBuffer;
    static int outputIndex;

    public static void InitOutput(System.IO.Stream output = null)
    {
        outputStream = output ?? Console.OpenStandardOutput();
        outputIndex = 0;
        outputBuffer = new byte[65535];
    }

    public static void WriteLine(object obj = null)
    {
        Write(obj);
        Write('\n');
    }

    public static void WriteLine(long number)
    {
        Write(number);
        Write('\n');
    }

    public static void Write(long signedNumber)
    {
        ulong number = unchecked((ulong)signedNumber);
        if (signedNumber < 0)
        {
            Write('-');
            number = unchecked((ulong)(-signedNumber));
        }

        Reserve(20 + 1); // 20 digits + 1 extra for sign
        int left = outputIndex;
        do
        {
            outputBuffer[outputIndex++] = (byte)('0' + number % 10);
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

    public static void Write(object obj)
    {
        if (obj == null) return;

        var s = obj.ToString();
        Reserve(s.Length);
        for (int i = 0; i < s.Length; i++)
            outputBuffer[outputIndex++] = (byte)s[i];
    }

    public static void Write(char c)
    {
        Reserve(1);
        outputBuffer[outputIndex++] = (byte)c;
    }

    public static void Write(byte[] array, int count)
    {
        Reserve(count);
        Array.Copy(array, 0, outputBuffer, outputIndex, count);
        outputIndex += count;
    }

    static void Reserve(int n)
    {
        if (outputIndex + n <= outputBuffer.Length)
            return;

        Dump();
        if (n > outputBuffer.Length)
            Array.Resize(ref outputBuffer, Math.Max(outputBuffer.Length * 2, n));
    }

    static void Dump()
    {
        outputStream.Write(outputBuffer, 0, outputIndex);
        outputIndex = 0;
    }

    public static void Flush()
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
            while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(wait, 3000)) ;
            Environment.Exit(1);
        };

        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());
#if __MonoCS__
        var thread = new Thread(()=>new Solution().Solve());
        var f = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = typeof(Thread).GetField("internal_thread", f).GetValue(thread);
        t.GetType().GetField("stack_size", f).SetValue(t, 32 * 1024 * 1024);
        thread.Start();
        thread.Join();
#else
        new Solution().Solve();
#endif
        Flush();
        Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
    }
    #endregion
    #endregion
}

