using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HackerRank.WeekOfCode30.RangeModularQueries
{
    using static Solution.FastIO;
    using static Math;
    using static Console;

    public static class Solution
    {
        #region Variables
        static int n;
        static int queryCount;
        static int[] a;
        static Query[] queries;
        static int maxNumber;
        static Bucket[] buckets;
        static List<int> values;
        static List<int> mods;
        static int[][] modMap;


        static int sqrtN;
        static int window;
        static int scanWindow;
        static int bruteForceWindow;
        static int fewValuesWindow;

        #endregion


        public static void Solve()
        {
            CreateBuckets();

            // Quick process
            var scanBottom = maxNumber / scanWindow;
            foreach (var q in queries)
            {
                if (q.Answer != null) continue;

                var bf = UseBruteForce && q.Length <= bruteForceWindow ? q.Length : -1;
                var scan = UseScan && q.Mod >= scanBottom ? maxNumber / q.Mod : -1;

                // Choose the smallest window
                if (scan >= 0 && bf > (scan << ScanShift))
                    bf = -1;

                if (bf >= 0)
                    q.Answer = BruteForce(q.Left, q.Right, q.Mod, q.B);
                else if (scan >= 0)
                    q.Answer = Scan(q.Left, q.Right, q.Mod, q.B);
            }

            // Length is large (>window) and mod is small (<window)
            // We want to do as much preprocessing to reduce iteration overhead for the next 2

            if (UseFewValuesHeuristic && values != null)
                FewValuesHeuristic();
            else if (UseMOS)
                MosAlgorithmApproach();
            else if (UseSqrtDecomposition)
                SqrtDecompositionApproach();
            else
                BruteForceAll();
        }

        static void BruteForceAll()
        {
            foreach (var q in queries)
                if (q.Answer == null)
                    q.Answer = BruteForce(q.Left, q.Right, q.Mod, q.B);
        }

        static void SqrtDecompositionApproach()
        {
            // Collect unique mods
            int maxMod = 0;
            var modBuckets = new Dictionary<int, ModBucket>();
            foreach (var q in queries)
            {
                ModBucket bucket;
                if (!modBuckets.TryGetValue(q.Mod, out bucket))
                    modBuckets[q.Mod] = bucket = new ModBucket { Mod = q.Mod };
                bucket.Capacity++;
                maxMod = Max(maxMod, q.Mod);
            }

            if (modBuckets.Count == 0)
                return;

            foreach (var q in queries)
            {
                var bucket = modBuckets[q.Mod];
                if (bucket.List == null) bucket.List = new Query[bucket.Capacity];
                bucket.List[bucket.Count++] = q;
            }

            int[] remainders = new int[maxMod];

            foreach (var bucket in modBuckets.Values)
            {
                var mod = bucket.Mod;

                var leftList = bucket.List;
                var rightList = bucket.List.ToArray();

                Array.Sort(leftList, (x, y) => x.Left.CompareTo(y.Left));
                Array.Sort(rightList, (x, y) => x.Right.CompareTo(y.Right));

                int li = 0;
                int ri = 0;

                Array.Clear(remainders, 0, mod);

                for (int i=leftList[0].Left; ri < rightList.Length; i++)
                {
                    while (li < leftList.Length)
                    {
                        var q = leftList[li];
                        if (q.Left > i) break;
                        q.Answer = -remainders[q.B];
                        li++;
                    }

                    remainders[a[i] % mod]++;

                    while (ri < rightList.Length)
                    {
                        var q = rightList[ri];
                        if (q.Right > i) break;
                        q.Answer += remainders[q.B];
                        ri++;
                    }
                }
            }
        }

        static void MosAlgorithmApproach()
        {
            // Collect unique mods
            var set = new HashSet<int>();
            foreach (var q in queries)
                if (q.Answer == null)
                    set.Add(q.Mod);

            // Return if no more queries
            if (set.Count == 0)
                return;

            mods = set.ToList();
            mods.Sort();

            modMap = new int[mods[mods.Count - 1] + 1][];
            foreach (var m in mods)
                modMap[m] = new int[m];

            // Mohs
            var queriesSorted = queries.ToList();
            queriesSorted.RemoveAll(q => q.Answer != null);
            queriesSorted.Sort((x, y) =>
            {
                int cmp = (x.Left / sqrtN).CompareTo(y.Left / sqrtN);
                if (cmp != 0) return cmp;
                cmp = x.Right.CompareTo(y.Right);
                if (cmp != 0) return cmp;
                return x.Left.CompareTo(y.Left);
            });

            int mosLeft = 0;
            int mosRight = -1;

            foreach (var q in queriesSorted)
            {
                while (mosRight < q.Right) AddDelta(a[++mosRight], +1);
                while (mosRight > q.Right) AddDelta(a[mosRight--], -1);
                while (mosLeft > q.Left) AddDelta(a[--mosLeft], +1);
                while (mosLeft < q.Left) AddDelta(a[mosLeft++], -1);
                q.Answer = modMap[q.Mod][q.B];
            }
        }

        static void MosAlgorithmApproach2()
        {
            // Collect unique mods
            var set = new HashSet<int>();
            foreach (var q in queries)
                if (q.Answer == null)
                    set.Add(q.Mod);

            // Return if no more queries
            if (set.Count == 0)
                return;

            mods = set.ToList();
            mods.Sort();

            modMap = new int[mods[mods.Count - 1] + 1][];
            foreach (var m in mods)
                modMap[m] = new int[m];

            var mos = new MosAlgorithm();
            mos.Add += (p,se) => AddDelta(p, +1);
            mos.Remove += (p,se) => AddDelta(p, -1);

            foreach(var q in queries)
            {
                if (q.Answer == null) 
                    mos.AddTask(q.Left, q.Right, q, task => q.Answer = modMap[q.Mod][q.B]);
            }
        }

        static void AddDelta(int v, int delta)
        {
            foreach (var m in mods)
                modMap[m][v % m] += delta;
        }

        static void CreateBuckets()
        {
            buckets = new Bucket[maxNumber + 1];
            int valueCount = 0;
            for (int i = 0; i < a.Length; i++)
            {
                var v = a[i];
                if (buckets[v] == null)
                {
                    buckets[v] = new Bucket();
                    valueCount++;
                }
                buckets[v].List.Add(i);
            }

            if (UseFewValuesHeuristic && valueCount <= fewValuesWindow)
            {
                values = new List<int>(valueCount);
                for (int i = 0; i <= maxNumber; i++)
                {
                    var b = buckets[i];
                    if (b != null)
                        values.Add(i);
                }
            }
        }

        static void FewValuesHeuristic()
        {
            foreach (var q in queries)
            {
                if (q.Answer != null) continue;

                int count = 0;
                foreach (var v in values)
                    if (v % q.Mod == q.B)
                        count += Count(v, q.Left, q.Right);
                q.Answer = count;
            }
        }

        static int Scan(int left, int right, int mod, int c)
        {
            int count = 0;

            for (int i = c; i <= maxNumber; i += mod)
            {
                var b = buckets[i];
                if (b != null)
                    count += Count(b.List, left, right);
            }

            return count;
        }

        static int Count(int v, int left, int right)
        {
            var b = buckets[v];
            return (b == null) ? 0 : Count(b.List, left, right);
        }

        static int Count(List<int> list, int left, int right)
        {
            var lower = BinarySearch(list, left, 0, list.Count - 1);
            var upper = BinarySearch(list, right, lower, list.Count - 1, true);
            return upper - lower;
        }

        public static int BinarySearch(IList<int> array, int value,
                int left, int right, bool upper = false)
        {
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

        static int BruteForce(int left, int right, int mod, int b)
        {
            int count = 0;
            for (int i = left; i <= right; i++)
                if (a[i] % mod == b)
                    count++;
            return count;
        }

        static void ReadData()
        {
            var input = ReadLine().Split(' ');
            n = int.Parse(input[0]);
            queryCount = int.Parse(input[1]);
            a = Array.ConvertAll(ReadLine().Split(), int.Parse);

            for (int i = 0; i < a.Length; i++)
            {
                var v = a[i];
                maxNumber = Max(maxNumber, v);
            }

            queries = new Query[queryCount];
            for (int i = 0; i < queryCount; i++)
            {
                input = ReadLine().Split();
                queries[i] = new Query
                {
                    Index = i,
                    Left = int.Parse(input[0]),
                    Right = int.Parse(input[1]),
                    Mod = int.Parse(input[2]),
                    B = int.Parse(input[3])
                };
            }
        }

        static void ReadDataFast()
        {
            InitIO();
            n = Ni();
            queryCount = Ni();
            a = Ni(n);

            for (int i = 0; i < a.Length; i++)
            {
                var v = a[i];
                maxNumber = Max(maxNumber, v);
            }

            queries = new Query[queryCount];
            for (int i = 0; i < queryCount; i++)
            {
                queries[i] = new Query
                {
                    Index = i,
                    Left = Ni(),
                    Right = Ni(),
                    Mod = Ni(),
                    B = Ni()
                };
            }
        }

        static void GenerateData(int n0 = 40000, int q0 = 40000, int ai = 40000)
        {
            var random = new Random(1000);
            n = n0;
            queryCount = q0;

            a = new int[n];
            for (int i = 0; i < n; i++)
            {
                a[i] = random.Next(ai);
                maxNumber = Max(maxNumber, a[i]);
            }

            queries = new Query[queryCount];
            int mod;
            for (int i = 0; i < queryCount; i++)
            {
                //int len = random.Next(20, Math.Min(n,5000));
                //int len = random.Next(10000, n/2);
                //int len = random.Next(n/2, n);
                int len = random.Next(20, n);
                int start = random.Next(n - len);
                queries[i] = new Query
                {
                    Index = i,
                    Left = start,
                    Right = start + len,
                    Mod = mod = random.Next(2, n),
                    B = random.Next(mod)
                };
            }
        }

        static void WriteAnswers()
        {
            foreach (var q in queries)
            {
                if (q.Answer == null)
                    q.Answer = BruteForce(q.Left, q.Right, q.Mod, q.B);
                WriteLine(q.Answer);
            }
        }

        public class Query
        {
            public int Index;
            public int Left;
            public int Right;
            public int Length => Right - Left + 1;
            public int B;
            public int Mod;
            public int? Answer;
        }

        public class Bucket
        {
            public List<int> List = new List<int>();
        }

        public class ModBucket
        {
            public int Mod;
            public int Capacity;
            public int Count;
            public Query[] List;
        }

        #region Fast IO
        public static class FastIO
        {
            static Stream stream;
            static int idx, bytesRead;
            static byte[] buffer;

            public static void InitIO(
                int stringCapacity = 16,
                int bufferSize = 1 << 20,
                Stream input = null)
            {
                stream = input ?? OpenStandardInput();
                idx = bytesRead = 0;
                buffer = new byte[bufferSize];
            }

            static void ReadMore()
            {
                idx = 0;
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) buffer[0] = 32;
            }

            public static int Read()
            {
                if (idx >= bytesRead) ReadMore();
                return buffer[idx++];
            }


            public static T[] N<T>(int n, Func<T> func)
            {
                var list = new T[n];
                for (int i = 0; i < n; i++) list[i] = func();
                return list;
            }

            public static int[] Ni(int n)
            {
                var list = new int[n];
                for (int i = 0; i < n; i++) list[i] = Ni();
                return list;
            }

            public static int Ni()
            {
                var c = SkipSpaces();
                bool neg = c == '-';
                if (neg)
                {
                    c = Read();
                }

                int number = c - '0';
                while (true)
                {
                    var d = Read() - '0';
                    if ((uint)d > 9) break;
                    number = number * 10 + d;
                }
                return neg ? -number : number;
            }


            public static int SkipSpaces()
            {
                int c;
                do c = Read(); while ((uint)c - 33 >= (127 - 33));
                return c;
            }

        }

        #endregion


        public static void Main()
        {
            if (Production)
                ReadDataFast();
            GenerateData();

            sqrtN = (int)Sqrt(n);
            window = Max(MinWindow, (int)(sqrtN * WindowFactor));
            scanWindow = ScanWindow != 0 ? ScanWindow : window;
            bruteForceWindow = BruteForceWindow != 0 ? BruteForceWindow : window;
            fewValuesWindow = FewValuesWindow != 0 ? FewValuesWindow : window;

            Solve();

            if (Production)
                WriteAnswers();
            var elapsed = Process.GetCurrentProcess().TotalProcessorTime;
            WriteLine(elapsed);

            foreach (var q in queries)
            {
                var check = BruteForce(q.Left, q.Right, q.Mod, q.B);
                if (check != q.Answer)
                {
                    WriteLine($"Mismatch with {q} - {check}");
                    Debugger.Break();
                }
            }
        }

        public static bool Production = false;
		public static int MinWindow = 2;
		public static double WindowFactor = 2;

		public static bool UseMOS = true;
		public static bool UseSqrtDecomposition = false;

		public static bool UseBruteForce = false;
		public static int BruteForceWindow = 0;

		public static bool UseScan = true;
		public static int ScanWindow = 0;
		public static int ScanShift = 2;

		public static bool UseFewValuesHeuristic = true;
		public static int FewValuesWindow = 0;


    }

    public class MosAlgorithm
    {
        public List<Task> Tasks = new List<Task>();

        public Task AddTask(int start, int end, object tag, Action<Task> action)
        {
            if (start > end)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var task = new Task
            {
                Start = start,
                End = end,
                Action = action,
                Tag = tag
            };

            Tasks.Add(task);
            return task;
        }

        public Action<int, bool> Add;

        public Action<int, bool> Remove;

        public void Execute()
        {
            int max = Tasks.Max(t => t.End);
            int min = Tasks.Min(t => t.Start);

            int s = min;
            int e = s - 1;

            int n = max - s + 1;
            int sqrt = (int)Ceiling(Sqrt(n));

            Tasks.Sort((x, y) => x.Start / sqrt == y.Start / sqrt
                ? x.End.CompareTo(y.End)
                : x.Start.CompareTo(y.Start));

            // POSTPONE: 
            // One optimization we can make is to take advantage of any gaps
            // with no overlapping so we can start from the latest position

            foreach (var task in Tasks)
            {
                while (e < task.End) Add(++e, true);
                while (e > task.End) Remove(e--, true);
                while (s < task.Start) Remove(s++, false);
                while (s > task.Start) Add(--s, false);
                task.Action(task);
            }

            Tasks.Clear();
        }

        public void ExecuteCumulative()
        {
            int max = Tasks.Max(t => t.End);

            int s = 0;
            int e = s;

            int n = max - s + 1;
            int sqrt = (int)Ceiling(Sqrt(n));

            Tasks.Sort((x, y) => x.Start / sqrt == y.Start / sqrt
                ? x.End.CompareTo(y.End)
                : x.Start.CompareTo(y.Start));

            foreach (var task in Tasks)
            {
                while (e < task.End) Add(++e, true);
                while (e > task.End) Remove(e--, true);
                while (s < task.Start) Add(++s, false);
                while (s > task.Start) Remove(s--, false);
                task.Action(task);
            }

            Tasks.Clear();
        }

        public class Task
        {
            public int Start;
            public int End;
            public Action<Task> Action;
            public object Tag;

            public override string ToString()
            {
                return $"{Tag} [{Start},{End}]";
            }
        }

    }

}