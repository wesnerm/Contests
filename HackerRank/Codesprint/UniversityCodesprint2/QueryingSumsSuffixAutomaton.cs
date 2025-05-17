namespace HackerRank.UniversityCodesprint2.QueryingSumsSuffixAutomaton
{
    using System;
    using System.Collections.Generic;
	using System.Diagnostics;
	using static System.Math;
    using static System.Console;

    public class Solution
    {
        #region Variables

        static SuffixAutomaton sa;
        static string s;
        static int m;
        static int q;
        static int[][] pairs = new int[m][];
        #endregion

        public static void Main()
        {
            var input = Array.ConvertAll(ReadLine().Split(), int.Parse);
            m = input[1];
            q = input[2];
            s = ReadLine();
            sa = new SuffixAutomaton(s);

            pairs = new int[m][];
            for (int i = 0; i < m; i++)
                pairs[i] = Array.ConvertAll(ReadLine().Split(), int.Parse);

            for (int a0 = 0; a0 < q; a0++)
            {
                var arr = ReadLine().Split();
                string w = arr[0];
                int a = int.Parse(arr[1]);
                int b = int.Parse(arr[2]);

                long answer = 0;
                for (int i = a; i <= b; i++)
                {
                    int left = pairs[i][0];
                    int right = pairs[i][1];
                    answer += sa.Occurrences(w, left, right - left + 1);
                }
                WriteLine(answer);
            }
        }


#if DEBUG
        public static bool Verbose = true;
#else
        public static bool Verbose = false;
#endif
    }

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
            ListMap<int, int> Next;
            public int Fpos;
            public int Count;
            public int Count2 = -1;
            public bool IsTerminal;
            public State(SuffixAutomaton automaton)
            {
                Automaton = automaton;
            }

            public ListMap<int, int> Dictionary
            {
                get { return Next; }
                set { Next = value; }
            }

            public int this[int ch]
            {
                get
                {
                    return Next[ch];
                }
                set
                {
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

            FixTerminals();
            BuildCounts(0);
            Environment.Exit(0);
        }

        ListMap<int, int> NewDictionary()
        {
            return new ListMap<int, int>();
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
                        Dictionary = _states[q].Dictionary.Clone(),
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


        public int FirstPosition(string pattern)
        {
            int state = 0;
            foreach (var ch in pattern)
                state = _states[state][ch];
            return _states[state].Fpos - pattern.Length + 1;
        }

        public List<int> FixTerminals()
        {
            List<int> terminals = new List<int>();
            int p = _last;
            while (p > 0)
            {
                terminals.Add(p);
                _states[p].IsTerminal = true;
                p = _states[p].Link;
            }
            return terminals;
        }

        int depth;
        public int BuildCounts(int state)
        {
            if (depth > 1000) Debugger.Break();
            depth++;
            try
            {
                var s = _states[state];
                if (s.Count2 != -1)
                    return s.Count2;

                int count = 0;
                int hits = 0;
                foreach (var s2 in s.Dictionary.Values)
                {
                    if (s2 == 0) continue;
                    count += BuildCounts(s2);
                    hits++;
                }

                if (s.IsTerminal)
                    count++;
                s.Count2 = count;
                return count;

            }
            finally
            {
                depth--;
            }

        }

        public int Occurrences(string pattern, int index, int count)
        {
            int state = 0;
            for (int i = 0; i < count; i++)
            {
                state = _states[state][pattern[index + i]];
                if (state <= 0) return 0;
            }
            return _states[state].Count2;
        }

        public int Occurrences2(string pattern, int index, int count)
        {
            int state = 0;
            for (int i = 0; i < count; i++)
            {
                state = _states[state][pattern[index + i]];
                if (state <= 0) return 0;
            }
            return _states[state].Count;
        }

        public int Occurrences(string pattern)
        {
            return Occurrences(pattern, 0, pattern.Length);
        }


        public static void Main2()
        {
            var sa = new SuffixAutomaton("How much wood would a woodchuck chuck if a woodchuck could chuck wood");

            var w = "woodchuck";
            var hashSet = new HashSet<string>();
            for (int i = 0; i < w.Length; i++)
                for (int j = i; j < w.Length; j++)
                {
                    var ww = w.Substring(i, j - i + 1);
                    var count1 = sa.Occurrences(w, i, j - i + 1);
                    var count2 = sa.Occurrences2(w, i, j - i + 1);
                    Console.WriteLine($"{ww}) {count1} {count2}");
                }
        }
    }

    public class ListMap<K, V> where K : IComparable<K>
    {
        Node head;

        public class Node
        {
            public Node Next;
            public K Key;
            public V Value;
        }

        /*public ListMap<K, V> Clone()
        {
            Node list = null;

            for (Node cur = head; cur != null; cur = cur.Next)
            {
                var node = new Node { Next = list, Key = cur.Key, Value = cur.Value };
                list = node;
            }

            for (Node cur = list; cur != null;)
            {
                var node = cur;
                cur = cur.Next;
                node.Next = head;
                head = node;
            }

            return new ListMap<K, V> { head = list };
        }*/

        public ListMap<K,V> Clone()
        {
            return new ListMap<K, V> {head = Clone(head)};
        }

        static Node Clone(Node n)
        {
            if (n == null) return null;
            return new Node {Next = Clone(n.Next), Key = n.Key, Value = n.Value};
        }

        public V this[K key]
        {
            get
            {
                for (Node cur = head; cur != null; cur = cur.Next)
                {
                    int cmp = key.CompareTo(cur.Key);
                    if (cmp > 0) break;
                    if (cmp == 0) return cur.Value;
                }
                return default(V);
            }

            set
            {
                Node prev = null;
                Node cur;
                for (cur = head; cur != null; prev = cur, cur = cur.Next)
                {
                    int cmp = key.CompareTo(cur.Key);
                    if (cmp > 0) break;
                    if (cmp == 0)
                    {
                        cur.Value = value;
                        return;
                    }
                }

                var node = new Node { Key = key, Value = value, Next = cur };
                if (prev == null)
                    head = node;
                else
                    prev.Next = node;
            }
        }

        public bool Remove(K key)
        {
            Node prev = null;
            Node cur;
            for (cur = head; cur != null; prev = cur, cur = cur.Next)
            {
                int cmp = key.CompareTo(cur.Key);
                if (cmp > 0) break;
                if (cmp == 0)
                {
                    if (prev == null) head = cur.Next;
                    else prev.Next = cur.Next;
                    return true;
                }
            }
            return false;
        }
        
        public IEnumerable<K> Keys
        {
            get
            {
                for (Node cur = head; cur != null; cur = cur.Next)
                    yield return cur.Key;
            }
        }
        
        public IEnumerable<V> Values
        {
            get
            {
                for (Node cur = head; cur != null; cur = cur.Next)
                    yield return cur.Value;
            }
        }

        public IEnumerable<KeyValuePair<K, V>> GetEnumerator()
        {
            for (Node cur = head; cur != null; cur = cur.Next)
                yield return new KeyValuePair<K, V>(cur.Key, cur.Value);
        }

    }
}