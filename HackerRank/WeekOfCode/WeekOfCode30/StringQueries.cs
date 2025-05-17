namespace HackerRank.WeekOfCode30.StringQueries
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using static System.Console;
    using static System.Math;
    using static Solution.FastIO;

    public class Solution
    {
        #region Variables

        static int _q;
        static int wordCount;
        static int slen;
        static Word[] _words;
        static List<Query> queries;
        static List<Query> finalQueries;
        static byte[] outbuffer = new byte[4096];
        static Stream stream;

        #endregion

        public static void Main()
        {
            InitIO();
            // CreateTests(2000,100000);
            // Console.SetOut(new StreamWriter(@"d:\test\hr\output.txt"));
            stream = Console.OpenStandardOutput();

            wordCount = Ni();
            _q = Ni();
            _words = new Word[wordCount];

            slen = 1;
            for (int i = 0; i < wordCount; i++)
            {
                var s = Ns();
                _words[i] = new Word
                {
                    Index = i,
                    String = s,
                    Left = slen,
                    Right = slen + s.Length - 1,
                    Length = s.Length,
                };
                slen += s.Length + 1;
            }

            queries = new List<Query>();
            for (int i = 0; i < _q; i++)
            {
                int a = Ni();
                int b = Ni();

                var query = new Query
                {
                    A = a,
                    B = b,
                    Index = i,
                };
                queries.Add(query);
            }

            PreprocessQueries();

            AutomataApproach();
            // BruteForce();

            foreach (var q in finalQueries)
            {
                if (Debugging)
                {
                    var bf = LongestCommonSubstringSlow(q.WordA, q.WordB);
                    if (q.Answer != bf.Length)
                        Write($"'{q.WordA}' & '{q.WordB}' have LCS of length {bf.Length} - '{bf}' -- not ");
                }
                WriteLine(q.Answer);
            }

            Dump();

#if DEBUG
            Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
#endif
        }

        static int index;
        static void Write(string s)
        {
            if (index + s.Length + 1 > outbuffer.Length)
                Dump();

            for (int i = 0; i <= s.Length; i++)
                outbuffer[index++] = (byte)s[i];
        }

        static void Dump()
        {
            stream.Write(outbuffer, 0, index);
            index = 0;
        }

        static Dictionary<long, Query> queryMap;

        public static void PreprocessQueries()
        {
            foreach (var w in _words)
            {
                long flags = 0;
                long rep = 0;
                foreach (var c in w.String)
                {
                    var bit = 1L << (c - 'a');
                    rep |= flags & bit;
                    flags |= bit;
                }
                w.CharacterFlags = flags;
                w.Repetitions = rep;
            }

            if (Deduplicate > 0)
            {
                var dict = new Dictionary<string, int>(wordCount);
                var remap = new int[wordCount];
                for (int i = 0; i < remap.Length; i++)
                    remap[i] = i;

                foreach (var w in _words)
                {
                    if (w.Length > Deduplicate) continue;
                    int wi;
                    if (dict.TryGetValue(w.String, out wi))
                        remap[w.Index] = wi;
                    else
                        dict[w.String] = w.Index;
                }

                foreach (var q in queries)
                {
                    q.A = remap[q.A];
                    q.B = remap[q.B];
                }
            }

            foreach (var q in queries)
            {
                if (q.Answer != null) continue;

                if (q.A == q.B)
                {
                    q.Answer = _words[q.A].Length;
                    continue;
                }

                var wa = _words[q.A];
                var wb = _words[q.B];

                if (Symmetry && wa.Length < wb.Length)
                {
                    var tmp = q.A;
                    q.A = q.B;
                    q.B = tmp;
                }

                var common = (wa.CharacterFlags & wb.CharacterFlags);
                if (common == 0)
                {
                    q.Answer = 0;
                    continue;
                }
                if (IsOneBit(common) && (common & wa.Repetitions & wb.Repetitions) == 0)
                {
                    q.Answer = 1;
                    continue;
                }

                int maxlen = Max(wa.Length, wb.Length);
                if (Deduplicate >= 2 && maxlen == 2)
                    q.Answer = 1;
            }

            queryMap = new Dictionary<long, Query>(queries.Count);
            for (int i = 0; i < queries.Count; i++)
            {
                var q = queries[i];
                if (q.Answer != null) continue;
                var code = Code(q.A, q.B);
                Query q2;
                if (queryMap.TryGetValue(code, out q2))
                    queries[i] = q2;
                else
                    queryMap[code] = q;
            }

            finalQueries = queries;
            queries = new List<Query>(queryMap.Values);

            foreach (var q in queries)
            {
                _words[q.A].Occurrences++;
                _words[q.B].Occurrences++;
            }
        }

        public static bool IsOneBit(long x)
        {
            return (x & x - 1) == 0;
        }

        public static void AutomataApproach()
        {
            var sortedQueries = queries.ToList();
            sortedQueries.Sort();

            SuffixAutomaton sa = null;
            Query prev = null;

            foreach (var query in sortedQueries)
            {
                if (query.Answer != null) continue;

                // New word
                if (prev == null || query.A != prev.A)
                {
                    sa = new SuffixAutomaton(query.WordA);
                }
                else if (query.CompareTo(prev) == 0)
                {
                    query.Answer = prev.Answer;
                    prev = query;
                    continue;
                }

                prev = query;

                query.Answer = sa.LCSLength(query.WordB);
                if (Debugging)
                {
                    var bf = LongestCommonSubstringSlow(query.WordA, query.WordB);
                    Debug.Assert(query.Answer == bf.Length);
                }

            }
        }


        public static string LongestCommonSubstringSlow(string s1, string s2)
        {
            // Brute force for testing purposes
            if (s1.Length > s2.Length)
                Swap(ref s1, ref s2);

            for (int len = s1.Length; len > 0; len--)
            {
                for (int j = s1.Length - len; j >= 0; j--)
                {
                    for (int k = s2.Length - len; k >= 0; k--)
                    {
                        if (string.CompareOrdinal(s1, j, s2, k, len) == 0)
                            return s1.Substring(j, len);
                    }
                }
            }

            return "";
        }



        #region Code

        const int CodeShift = 32;

        public static long Code(long w1, long w2)
        {
            if (Symmetry && w1 > w2)
                Swap(ref w1, ref w2);
            return w1 << CodeShift | w2;
        }

        public static int Uncode1(long code)
        {
            return (int)(code >> CodeShift);
        }

        public static int Uncode2(long code)
        {
            return (int)(code & (1L << CodeShift) - 1);
        }


        #endregion

        #region Helpers

        class Data
        {
            public int Index = -1;
            public int Clamp = int.MaxValue;
        }

        class Word
        {
            public int Index;
            public string String;
            public int Left;
            public int Right;
            public int Length;
            public int Occurrences;
            public long CharacterFlags;
            public long Repetitions;

            public override string ToString()
            {
                return $"#{Index} - {String} {Left}-{Right}";
            }
        }

        class Query : System.IComparable<Query>
        {
            public int Index;
            public int A;
            public int B;
            public string WordA => _words[A].String;
            public string WordB => _words[B].String;
            public int? Answer;

            public override string ToString()
            {
                return $"{Index} {_words[A].String} {_words[B].String}";
            }

            public int CompareTo(Query q)
            {
                int cmp;
                if (OrderByLength)
                {
                    cmp = -_words[A].Length.CompareTo(_words[q.A].Length);
                    if (cmp != 0)
                        return cmp;
                    cmp = -_words[A].Occurrences.CompareTo(_words[q.A].Occurrences);
                    if (cmp != 0)
                        return cmp;
                }
                else
                {
                    cmp = -_words[A].Occurrences.CompareTo(_words[q.A].Occurrences);
                    if (cmp != 0)
                        return cmp;
                    cmp = -_words[A].Length.CompareTo(_words[q.A].Length);
                    if (cmp != 0)
                        return cmp;
                }
                cmp = A.CompareTo(q.A);
                if (cmp != 0)
                    return cmp;
                cmp = B.CompareTo(q.B);
                return cmp;
            }
        }

        #endregion

        #region Helpers

        public static void Swap<T>(ref T item1, ref T item2)
        {
            var tmp = item1;
            item1 = item2;
            item2 = tmp;
        }

        public static class FastIO
        {
            static System.IO.Stream stream;
            static int idx, bytesRead;
            static byte[] buffer;
            static System.Text.StringBuilder builder;


            public static void InitIO(
                int stringCapacity = 16,
                int bufferSize = 1 << 20,
                System.IO.Stream input = null)
            {
                builder = new System.Text.StringBuilder(stringCapacity);
                stream = input ?? Console.OpenStandardInput();
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



            public static string Ns()
            {
                var c = SkipSpaces();
                builder.Clear();
                while (true)
                {
                    if ((uint)c - 33 >= (127 - 33)) break;
                    builder.Append((char)c);
                    c = Read();
                }
                return builder.ToString();
            }

            public static int SkipSpaces()
            {
                int c;
                do c = Read(); while ((uint)c - 33 >= (127 - 33));
                return c;
            }

        }

        #endregion

        #region Suffix Automaton

        public class SuffixAutomaton
        {
            string String;
            readonly List<Tuple<int, int>> WalkList = new List<Tuple<int, int>>();
            int chStart;
            int chLen;

            class State
            {
                public SuffixAutomaton Automaton;
                public int Len;
                public int Link;
                private int[] Next;
                public int Fpos;
                public int Count;
                public int Count2 = -1;

                public State(SuffixAutomaton automaton)
                {
                    Automaton = automaton;
                }

                public int[] Dictionary
                {
                    get { return Next; }
                    set { Next = value; }
                }

                public int this[int ch]
                {
                    get
                    {
                        ch -= Automaton.chStart;
                        return ch >= 0 && ch < Automaton.chLen ? Next[ch] : 0;
                    }
                    set
                    {
                        ch -= Automaton.chStart;
                        if (ch >= 0 && ch < Automaton.chLen)
                            Next[ch] = value;
                    }
                }

            };

            readonly List<State> _states = new List<State>();
            int _last;

            public SuffixAutomaton(string s)
            {
                String = s;

                chStart = int.MaxValue;
                int chEnd = 0;
                foreach (var c in s)
                {
                    chStart = Min(chStart, c);
                    chEnd = Max(chEnd, c);
                }

                if (chStart == int.MaxValue)
                {
                    chStart = 'a';
                    chEnd = 'z';
                }
                chLen = chEnd - chStart + 1;

                _last = 0;
                _states.Capacity = s.Length * 2;
                _states.Add(new State(this) { Len = 0, Link = -1, Dictionary = NewDictionary() });
                foreach (var c in s)
                    SaExtend(c);

                // In preparation for counting
                WalkList.Sort();
                WalkList.Reverse();
                foreach (var it in WalkList)
                    _states[_states[it.Item2].Link].Count += _states[it.Item2].Count;

            }

            int[] NewDictionary()
            {
                return new int[chLen];
            }


            void SaExtend(char c)
            {
                int cur = _states.Count;

                _states.Add(new State(this)
                {
                    Len = _states[_last].Len + 1,
                    Dictionary = NewDictionary(),
                    Count = 1,
                    Fpos = _states[_last].Len,
                });

                WalkList.Add(Tuple.Create(_states[cur].Len, cur));

                int p;
                for (p = _last; p != -1 && _states[p][c] == 0; p = _states[p].Link)
                    _states[p][c] = cur;
                if (p == -1)
                    _states[cur].Link = 0;
                else
                {
                    int q = _states[p][c];
                    if (_states[p].Len + 1 == _states[q].Len)
                        _states[cur].Link = q;
                    else
                    {
                        int clone = _states.Count;
                        _states.Add(new State(this)
                        {
                            Len = _states[p].Len + 1,
                            Dictionary = (int[])_states[q].Dictionary.Clone(),
                            Link = _states[q].Link,
                            Fpos = _states[q].Fpos,
                        });
                        WalkList.Add(Tuple.Create(_states[cur].Len, clone));
                        while (p != -1 && _states[p][c] == q)
                        {
                            _states[p][c] = clone;
                            p = _states[p].Link;
                        }
                        _states[q].Link = _states[cur].Link = clone;
                    }
                }
                _last = cur;
            }

            public int LCSLength(string t)
            {
                int length;
                int start;
                LongestCommonSubstring(t, out start, out length);
                return length;
            }

            public void LongestCommonSubstring(string t, out int start, out int length)
            {
                start = 0;
                length = 0;
                if (t.Length == 0) return;
                int v = 0;
                int len = 0;
                for (int i = 0; i < t.Length; i++)
                {
                    while (v != 0 && _states[v][t[i]] == 0)
                    {
                        v = _states[v].Link;
                        len = _states[v].Len;
                    }
                    if (_states[v][t[i]] > 0)
                    {
                        v = _states[v][t[i]];
                        len++;
                    }
                    if (len > length)
                    {
                        length = len;
                        start = i;
                    }
                }
            }

        }

		#endregion

#if DEBUG
		public static bool Verbose = false;
		public static bool Debugging = false;
#else
        public static  bool Verbose = false;
        public static  bool Debugging = false;
#endif
		public static bool Symmetry = true;
		public static int Deduplicate = int.MaxValue;
		public static bool OrderByLength = true;
    }
}
