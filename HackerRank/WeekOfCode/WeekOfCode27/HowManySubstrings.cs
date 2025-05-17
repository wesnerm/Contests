using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Net.NetworkInformation;
using Softperson.Algorithms;
using Softperson.Collections;

namespace HackerRank.CodeSprint
{
    public unsafe class HowManySubstrings
    {

        public static void Main()
        {
            Console.SetIn(File.OpenText(@"d:\test\hr\howmanysubstrings.txt"));

            var tokens_n = Console.ReadLine().Split();
            int n = int.Parse(tokens_n[0]);
            int q = int.Parse(tokens_n[1]);
            string s = Console.ReadLine();

            var suffixArray = new SuffixArray(s);

            // 30% of test cases, n,q <= 100
            // 50% of test cases, n,q <= 3000
            // 100% of test cases, n,q <= 100,000

            var queries = new List<Query>();
            int i = 0;
            while (q-- > 0)
            {
                var tokens = Console.ReadLine().Split();
                int left = int.Parse(tokens[0]);
                int right = int.Parse(tokens[1]);
                var query = new Query { Index = i++, Left = left, Right = right };
                queries.Add(query);
            }

            // Sort the queries for convenience
            var queries2 = new List<Query>(queries);
            queries2.Sort((a, b) =>
            {
                int cmp = a.Left.CompareTo(b.Left);
                if (cmp == 0)
                    cmp = a.Right.CompareTo(b.Right);
                return cmp;
            });

            Process(s, queries2, suffixArray);

            foreach (var query in queries)
                Console.WriteLine(query.Answer);
        }

        public class Query
        {
            public int Index;
            public int Left;
            public int Right;
            public long Answer;
        }

        static void Process(string s, List<Query> queries, SuffixArray table)
        {
            foreach (var q in queries)
                Process(q, table);
        }

        static HashSet<string> set = new HashSet<string>();

        public static long BruteForce(string s, Query q)
        {
            set.Clear();
            for (int i = q.Left; i <= q.Right; i++)
                for (int j = i; j <= q.Right; j++)
                    set.Add(s.Substring(i, j - i + 1));
            q.Answer = set.Count;
            return q.Answer;
        }

        private static int[] postsum;
        private static int[] presum;
        public static void Prepwork(SuffixArray table)
        {
            var indices = table.Indices;
            int size = indices.Length;
            presum = new int[size];
            postsum = new int[size];

            var reversed = indices.Reverse().ToArray();
            presum = CreateSums(indices, table);
            postsum = CreateSums(reversed, table);
            Array.Reverse(postsum);
        }


        public static int[] CreateSums(int[] indices, SuffixArray table)
        {
            var suffixes = table.Suffixes;
            var sums  = new int[indices.Length];

            BT root = null;

            int cpsum = 0;

            foreach(var e in indices)
            {
                int i = suffixes[e];
                var node = BT.Insert(ref root, e);
                var prev = BT.Previous(node);
                var next = BT.Next(node);

                int cpOld = prev != null && next != null
                    ? table.LongestCommonPrefix(suffixes[prev.Value], suffixes[next.Value])
                    : 0;

                int cpPrev = prev != null
                    ? table.LongestCommonPrefix(suffixes[prev.Value], i)
                    : 0;

                int cpNext = next != null
                    ? table.LongestCommonPrefix(suffixes[next.Value], i)
                    : 0;

                cpsum += cpPrev + cpNext - cpOld;
                sums[i] = cpsum;
            }

            return sums;
        }


        public static long Process(Query q, SuffixArray table)
        {
            int left = q.Left;
            int right = q.Right;
            int limit = right + 1;

            var indices = table.Indices;
            var suffixes = table.Suffixes;

            int size = right - left + 1;

            var work = new int[size];
            for (int i = 0; i < work.Length; i++)
                work[i] = indices[left + i];

            RadixSort(work);

            for (int i = 0; i < work.Length; i++)
                work[i] = suffixes[work[i]];

            var lcps = stackalloc int[size];
            for (int i = 1; i < size; i++)
                lcps[i] = table.LongestCommonPrefix(work[i - 1], work[i]);

            // http://cs.stackexchange.com/questions/13140/number-of-distinct-substrings-in-a-string

            long count = 0;
            for (int i = 0; i < work.Length; i++)
            {
                var suf = work[i];
                var upperLcp = limit - suf;
                var lowerLcp = 0;
                for (int j = i; j > 0; j--)
                {
                    upperLcp = Math.Min(upperLcp, lcps[j]);
                    if (upperLcp <= lowerLcp) break;
                    lowerLcp = Math.Max(lowerLcp, Math.Min(upperLcp, limit - work[j - 1]));
                }
                count += limit - suf - lowerLcp;
            }

            q.Answer = count;
            return count;
        }

 

        public static void Assert(bool condition)
        {
            if (condition == false)
            throw new Exception();
    }
    
    private static int[] buffer = new int[200000];

    public static void RadixSort(IList<int> list)
    {
        RadixSort(list, 0, 6);
        RadixSort(list, 6, 6);
        RadixSort(list, 12, 6);
    }

    public static unsafe void RadixSort(IList<int> list, int shift, int bits)
    {
        int buckets = 1 << bits;
        int mask = buckets - 1;
        var offsets = stackalloc int[buckets + 1];

        for (int i = 0; i < buckets; i++)
            offsets[i] = 0;

        for (int i = 0; i < list.Count; i++)
        {
            var v = list[i];
            var index = (v >> shift) & mask;
            offsets[index]++;
        }

        int sum = 0;
        for (int i = 0; i < buckets; i++)
        {
            var newSum = sum + offsets[i];
            offsets[i] = sum;
            sum = newSum;
        }
            
        for (int i = 0; i < list.Count; i++)
        {
            var v = list[i];
            var index = (v >> shift) & mask;
            buffer[offsets[index]++] = v;
        }

        for (int i = 0; i < list.Count; i++)
            list[i] = buffer[i];

    }
    
    public class SuffixArray
    {
            private readonly int length;
            public readonly string String;
            private readonly List<int[]> _commonPrefix = new List<int[]>();
            private readonly List<Bucket> _matrix = new List<Bucket>();
            private int[] _suffixArray;
            private int[] _common;

            public SuffixArray(string s)
            {
                String = s;
                length = s.Length;
                Fill(_matrix, length);

                _commonPrefix.Add(new int[length]);

                for (int i = 0; i < length; i++)
                    _commonPrefix[0][i] = s[i];

                for (int skip = 1, level = 1; skip < length; skip *= 2, level++)
                {
                    _commonPrefix.Add(new int[length]);

                    for (int i = 0; i < length; i++)
                    {
                        int top = _commonPrefix[level - 1][i];
                        int bottom = i + skip < length ? _commonPrefix[level - 1][i + skip] : -1000;
                        _matrix[i] = new Bucket(top, bottom, i);
                    }

                    _matrix.Sort((a, b) => a.CompareTo(b));

                    for (int i = 0; i < length; i++)
                        _commonPrefix[level][_matrix[i].Index] =
                            i > 0
                            && _matrix[i].Item1 == _matrix[i - 1].Item1
                            && _matrix[i].Item2 == _matrix[i - 1].Item2
                                ? _commonPrefix[level][_matrix[i - 1].Index]
                                : i;
                }
            }


            public int[] Indices
            {
                get
                {
                    return _commonPrefix[_commonPrefix.Count - 1];
                }
            }


            public int[] Suffixes
            {
                get
                {
                    if (_suffixArray == null)
                    {
                        var array = new int[length];
                        for (int i = 0; i < length; i++)
                            array[i] = _matrix[i].Index;
                        _suffixArray = array;
                    }
                    return _suffixArray;
                }
            }

            public int[] CommonCounts
            {
                get
                {
                    if (_common == null)
                    {

                        var indices = Suffixes;
                        var common = new int[length];
                        for (int i = 1; i < length; i++)
                            common[i] = LongestCommonPrefix(indices[i], indices[i - 1]);
                        _common = common;
                    }
                    return _common;
                }
            }


            public struct Bucket : IComparable<Bucket>
            {
                public int Item1;
                public int Item2;
                public int Index;

                public Bucket(int item1, int item2, int index)
                {
                    Item1 = item1;
                    Item2 = item2;
                    Index = index;
                }

                public int CompareTo(Bucket b)
                {
                    int cmp = Item1.CompareTo(b.Item1);
                    if (cmp != 0) return cmp;

                    cmp = Item2.CompareTo(b.Item2);
                    if (cmp != 0) return cmp;

                    cmp = Index.CompareTo(b.Index);
                    return cmp;
                }

            }


            public int LongestCommonPrefix(int i, int j)
            {
                int len = 0;
                if (i == j)
                    return length - i;

                for (int k = _commonPrefix.Count - 1; k >= 0 && i < length && j < length; k--)
                {
                    if (_commonPrefix[k][i] == _commonPrefix[k][j])
                    {
                        i += 1 << k;
                        j += 1 << k;
                        len += 1 << k;
                    }
                }
                return len;
            }


            public void Fill<T>(List<T> list, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    list.Add(default(T));
                }
            }


            public void Fill<T>(List<T> list, int count, T value)
            {
                for (int i = 0; i < count; i++)
                {
                    list.Add(value);
                }
            }

            public void Fill<T>(List<T> list, int count, Func<T> func)
            {
                for (int i = 0; i < count; i++)
                {
                    list.Add(func());
                }
            }


            public List<T> Repeat<T>(int n, T value)
            {
                var list = new List<T>();
                Fill(list, n, value);
                return list;
            }

        }

    }
}
