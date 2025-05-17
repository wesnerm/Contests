using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Library;

namespace csharplib
{
    public class StringLibrary
    {
        #region String Library
        private int n;
        string s;
        private int queryCount;
        private Query[] queries;
        private long[] answers;

        public void Solve()
        {
            n = Ni();
            s = Ns();


            queryCount = Ni();

            queries = new Query[queryCount];
            answers = new long[queryCount];

            for (int q = 0; q < queryCount; q++)
            {
                var qu = queries[q] = new Query
                {
                    Index = q,
                    A = Ni(),
                    B = Ni(),
                };
            }

            foreach (var ans in answers)
                WriteLine(ans);
        }

        public class Query : IComparable<Query>
        {
            public int Index;
            public int A;
            public int B;

            public int CompareTo(Query other)
            {
                int cmp;
                cmp = A.CompareTo(other.A);
                if (cmp != 0) return cmp;
                cmp = B.CompareTo(other.B);
                return cmp;
            }

            public override string ToString()
            {
                return $"#{Index} ({A},{B})";
            }
        }

        #region Suffix Automaton
        public partial class SuffixAutomaton
        {
            public Node Start;
            public Node End;
            public int NodeCount;
            public IEnumerable<char> Text;

            Node[] _nodes;
            SummarizedState[] _summary;

            private SuffixAutomaton()
            {
                Start = new Node();
                End = Start;
                NodeCount = 1;
            }

            /// <summary>
            /// Constructs an automaton from the string
            /// </summary>
            /// <param name="s"></param>
            public SuffixAutomaton(IEnumerable<char> s) : this()
            {
                Text = s;

                foreach (var c in s)
                    Extend(c);

                for (var p = End; p != Start; p = p.Link)
                    p.IsTerminal = true;
            }

            /// <summary>
            /// Extends an automaton by one character
            /// </summary>
            /// <param name="c"></param>
            public void Extend(char c)
            {
                var node = new Node
                {
                    Key = c,
                    Len = End.Len + 1,
                    Link = Start,
                    Index = NodeCount,
                };
                NodeCount++;

                Node p;
                for (p = End; p != null && p[c] == null; p = p.Link)
                    p[c] = node;
                End = node;

                if (p == null) return;

                var q = p[c];
                if (p.Len + 1 == q.Len)
                    node.Link = q;
                else
                {
                    var clone = q.Clone();
                    clone.Len = p.Len + 1;
                    clone.Index = NodeCount;
                    NodeCount++;

                    for (; p != null && p[c] == q; p = p.Link)
                        p[c] = clone;

                    q.Link = node.Link = clone;
                }
            }

            /// <summary>
            /// Indicates whether the substring is contained with automaton
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public bool ContainsSubstring(string s)
            {
                return FindNode(s) != null;
            }

            /// <summary>
            /// Lazily constructs a list of nodes
            /// </summary>
            /// <returns></returns>
            public Node[] GetNodes()
            {
                if (_nodes != null && NodeCount == _nodes.Length)
                    return _nodes;

                var nodes = _nodes = new Node[NodeCount];
                int stack = 0;
                int idx = NodeCount;

                nodes[stack++] = Start;
                while (stack > 0)
                {
                    var current = nodes[--stack];

                    if (current.Index > 0)
                        current.Index = 0;

                    current.Index--;
                    var index = current.NextCount + current.Index;
                    if (index >= 0)
                    {
                        stack++;

                        var child = current.Next[index];
                        if (child.Index >= -child.NextCount)
                            nodes[stack++] = current.Next[index];
                    }
                    else if (index == -1)
                    {
                        nodes[--idx] = current;
                    }
                    Debug.Assert(idx >= stack);
                }

                if (idx != 0)
                {
                    Debug.Assert(idx == 0, "NodeCount smaller than number of nodes");
                    NodeCount -= idx;
                    _nodes = new Node[NodeCount];
                    Array.Copy(nodes, idx, _nodes, 0, NodeCount);
                }

                UpdateNodeIndices();
                return _nodes;
            }

            /// <summary>
            /// Iterates through nodes in bottom-up fashion
            /// </summary>
            public IEnumerable<Node> NodesBottomUp()
            {
                var nodes = GetNodes();
                for (int i = NodeCount - 1; i >= 0; i--)
                    yield return nodes[i];
            }

            void UpdateNodeIndices()
            {
                var nodes = _nodes;
                for (int i = 0; i < NodeCount; i++)
                    nodes[i].Index = i;
            }

            /// <summary>
            /// Goes through a node given a string
            /// </summary>
            /// <param name="pattern">string to search for</param>
            /// <param name="index">start of substring in pattern to search for</param>
            /// <param name="count">length of substring</param>
            /// <returns>returns node representing string or null if failed</returns>

            public Node FindNode(string pattern, int index, int count)
            {
                var node = Start;
                for (int i = 0; i < count; i++)
                {
                    node = node[pattern[index + i]];
                    if (node == null) return null;
                }
                return node;
            }

            public Node FindNode(string pattern)
            {
                return FindNode(pattern, 0, pattern.Length);
            }

            /// <summary>
            /// Provides a compressed view of the automaton, so that depth-first search of an automaton
            /// can be accomplished in O(n) instead of O(n^2) time.
            /// </summary>
            /// <returns></returns>
            public SummarizedState[] SummarizedAutomaton()
            {
                if (_summary != null)
                    return _summary;

                var summary = new SummarizedState[NodeCount];
                foreach (var n in NodesBottomUp())
                {

                    if (n.NextCount == 1 && !n.IsTerminal)
                    {
                        var c = summary[n.Next[0].Index];
                        summary[n.Index] = new SummarizedState { Node = c.Node, Length = c.Length + 1 };
                    }
                    else
                    {
                        summary[n.Index] = new SummarizedState { Node = n, Length = 1 };
                    }
                }

                _summary = summary;
                return summary;
            }

            /// <summary>
            /// A state in the compressed automaton
            /// </summary>
            public struct SummarizedState
            {
                /// <summary> the end node of a labeled multicharacter edge </summary>
                public Node Node;
                /// <summary> the number of characters to advance to reach the state </summary>
                public int Length;
                public override string ToString() => $"Node={Node?.Index} Length={Length}";
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var n in GetNodes())
                {
                    sb.Append($"{{id:{0}, len:{n.Len}, link:{n.Link?.Index ?? -1}, cloned:{n.IsCloned}, Next:{{");
                    sb.Append(string.Join(",", n.Children.Select(c => c.Key + ":" + c.Index)));
                    sb.AppendLine("}}");
                }
                return sb.ToString();
            }

            public class Node
            {
                public char Key;
                public bool IsTerminal;
                public byte NextCount;
                int _keyMask;
                public Node[] Next;

                public int Len;
                public int Index;
                public Node Link;
                public Node Original;
                public static readonly Node[] Empty = new Node[0];

                public Node()
                {
                    Next = Empty;
                }

                public int FirstOccurrence => Original?.Len ?? this.Len;

                public bool IsCloned => Original != null;

                public Node Clone()
                {
                    var node = (Node)base.MemberwiseClone();
                    node.Original = Original ?? this;
                    node.Next = (Node[])node.Next.Clone();
                    return node;
                }

                public Node this[char ch]
                {
                    get
                    {
                        if ((_keyMask << ~ch) < 0)
                        {
                            int left = 0;
                            int right = NextCount - 1;
                            while (left <= right)
                            {
                                int mid = (left + right) >> 1;
                                var val = Next[mid];
                                int cmp = val.Key - ch;
                                if (cmp < 0)
                                    left = mid + 1;
                                else if (cmp > 0)
                                    right = mid - 1;
                                else
                                    return val;
                            }
                        }
                        return null;
                    }
                    set
                    {
                        int left = 0;
                        int right = NextCount - 1;
                        while (left <= right)
                        {
                            int mid = (left + right) >> 1;
                            var val = Next[mid];
                            int cmp = val.Key - ch;
                            if (cmp < 0)
                                left = mid + 1;
                            else if (cmp > 0)
                                right = mid - 1;
                            else
                            {
                                Next[mid] = value;
                                return;
                            }
                        }

                        if (NextCount >= Next.Length)
                            Array.Resize(ref Next, Math.Max(2, NextCount * 2));
                        if (NextCount > left)
                            Array.Copy(Next, left, Next, left + 1, NextCount - left);
                        NextCount++;
                        Next[left] = value;
                        _keyMask |= 1 << ch;
                    }
                }

                /// <summary>
                /// Return child nodes
                /// </summary>
                public IEnumerable<Node> Children
                {
                    get
                    {
                        for (int i = 0; i < NextCount; i++)
                            yield return Next[i];
                    }
                }

            }
        }


        #endregion

        #region Suffix Array
        public class SuffixArray
        {
            #region Variables
            int length;
            public string String;
            int[] _suffixes;
            int[] _lcps;
            int[] _ranks;
            RangeMinimumQuery _rmq;

            #endregion

            #region Construction

            public SuffixArray(int[] suffixes, int[] ranks = null, Func<char, bool> isDelim = null)
            {
                Init(suffixes, ranks, isDelim);
            }

            void Init(int[] suffixes, int[] ranks, Func<char, bool> isDelim)
            {
                length = suffixes.Length;
                _suffixes = suffixes;
                _ranks = ranks ?? BuildRanks(suffixes);
                _lcps = BuildLongestCommonPrefixes(String, _suffixes, _ranks, isDelim);
            }

            #endregion

            #region Automaton Construction

            public SuffixArray(SuffixAutomaton sa, Func<char, bool> isDelim = null)
            {
                String = sa.Text as String;
                var ab = new AutomatonBuilder(sa);
                Init(ab.Suffixes, null, isDelim);
            }

            struct AutomatonBuilder
            {
                int idx;
                SuffixAutomaton sa;
                int n;
                SuffixAutomaton.SummarizedState[] summary;
                public int[] Suffixes;

                public AutomatonBuilder(SuffixAutomaton sa) : this()
                {
                    this.sa = sa;
                    summary = sa.SummarizedAutomaton();
                    n = sa.End.Len;
                    Suffixes = new int[n];
                    idx = 0;
                    DfsSuffixArray(sa.Start, 0);
                }

                void DfsSuffixArray(SuffixAutomaton.Node node, int length)
                {
                    SuffixAutomaton.SummarizedState c;

                    while (node.IsTerminal)
                    {
                        Suffixes[idx++] = n - length;
                        if (node.NextCount != 1) break;
                        c = summary[node.Next[0].Index];
                        node = c.Node;
                        length += c.Length;
                    }

                    for (var index = 0; index < node.NextCount; index++)
                    {
                        c = summary[node.Next[index].Index];
                        DfsSuffixArray(c.Node, length + c.Length);
                    }
                }
            }

            #endregion

            #region Classic Construction
            public SuffixArray(string s, Func<char, bool> isDelim = null)
            {
                String = s;
                var builder = new ClassicBuilder(s);
                Init(builder.Suffixes, builder.Indices, isDelim);
            }

            public struct ClassicBuilder
            {
                static int length;
                public int[] Suffixes;
                public int[] Indices;
                Bucket[] _matrix2;
                Bucket[] _matrix;

                public static bool UseQuickSort;
                public static bool LateQuit;

                public ClassicBuilder(string s) : this()
                {
                    length = s.Length;
                    var ranks = new int[length];
                    var ranksPrev = new int[length];
                    for (var i = 0; i < length; i++)
                        ranks[i] = s[i];

                    _matrix = new Bucket[length];
                    _matrix2 = new Bucket[length];

                    for (int skip = 1; skip < length; skip *= 2)
                    {
                        var tmp = ranks;
                        ranks = ranksPrev;
                        ranksPrev = tmp;

                        for (var i = 0; i < length; i++)
                        {
                            _matrix[i].Item1 = ranksPrev[i] + 1;
                            _matrix[i].Item2 = i + skip < length ? ranksPrev[i + skip] + 1 : 0;
                            _matrix[i].Index = i;
                        }

                        if (!UseQuickSort)
                            RadixSort();
                        else
                            Array.Sort(_matrix);

                        int rank = 0;

                        ranks[_matrix[0].Index] = 0;
                        for (var i = 1; i < length; i++)
                        {
                            if (_matrix[i].Item1 != _matrix[i - 1].Item1 || _matrix[i].Item2 != _matrix[i - 1].Item2)
                                rank++;
                            ranks[_matrix[i].Index] = rank;
                        }

                        if (rank >= length - 1 && !LateQuit)
                            break; // Important optimization
                    }

                    var array = new int[length];
                    for (var i = 0; i < length; i++)
                        array[i] = _matrix[i].Index;
                    Suffixes = array;
                    Indices = ranks;
                }

                const int shift = 8;
                const int buckets = 1 << shift;

                void RadixSort()
                {

                    RadixSort(b => b.Item2);
                    if (_matrix.Length >= 1 << shift)
                    {
                        RadixSort(b => (b.Item2 >> shift));
                        if (_matrix.Length >= 1 << shift * 2)
                        {
                            RadixSort(b => (b.Item2 >> shift * 2));
                            if (_matrix.Length >= 1 << shift * 3)
                                RadixSort(b => (b.Item2 >> shift * 3));
                        }
                    }
                    RadixSort(b => b.Item1);
                    if (_matrix.Length >= 1 << shift)
                    {
                        RadixSort(b => (b.Item1 >> shift));
                        if (_matrix.Length >= 1 << shift * 2)
                        {
                            RadixSort(b => (b.Item1 >> shift * 2));
                            if (_matrix.Length >= 1 << shift * 3)
                                RadixSort(b => (b.Item1 >> shift * 3));
                        }
                    }
                }

                unsafe void RadixSort(Func<Bucket, int> func)
                {
                    var offsets = stackalloc int[buckets + 1];

                    for (var i = 0; i < buckets; i++)
                        offsets[i] = 0;

                    foreach (Bucket b in _matrix)
                        offsets[func(b) & (buckets - 1)]++;

                    int sum = 0;
                    for (var i = 0; i < buckets; i++)
                    {
                        int newSum = sum + offsets[i];
                        offsets[i] = sum;
                        sum = newSum;
                    }

                    foreach (Bucket b in _matrix)
                        _matrix2[offsets[func(b) & (buckets - 1)]++] = b;

                    Swap(ref _matrix, ref _matrix2);
                }

                struct Bucket : IComparable<Bucket>
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
                        var cmp = Item1.CompareTo(b.Item1);
                        if (cmp != 0) return cmp;
                        return Item2.CompareTo(b.Item2);
                    }
                }

            }

            #endregion

            #region Properties

            public int[] Ranks => _ranks ?? (_ranks = BuildRanks(_suffixes));

            public int[] Suffixes => _suffixes;

            public int[] LcpArray => _lcps ?? (_lcps = BuildLongestCommonPrefixes(String, Suffixes, Ranks));

            #endregion

            #region Methods

            public static int[] BuildRanks(int[] suffixArray)
            {
                int[] ranks = new int[suffixArray.Length];
                for (int i = 0; i < suffixArray.Length; i++)
                    ranks[suffixArray[i]] = i;
                return ranks;
            }

            /// <summary>
            /// Builds the longest common prefixes using Kasai's algorithm.
            /// </summary>
            /// <param name="txt">The text.</param>
            /// <param name="suffixArray">The suffix array.</param>
            /// <param name="ranks">The indices.</param>
            /// <param name="isDelim"></param>
            /// <returns></returns>
            public static int[] BuildLongestCommonPrefixes(string txt,
                int[] suffixArray,
                int[] ranks,
                Func<char, bool> isDelim = null)
            {
                int n = suffixArray.Length;
                if (ranks == null)
                    ranks = BuildRanks(suffixArray);

                int[] lcp = new int[n];


                for (int i = 0, k = 0; i < n; i++)
                {
                    if (ranks[i] == n - 1)
                    {
                        k = 0;
                        continue;
                    }

                    int j = suffixArray[ranks[i] + 1];
                    while (i + k < n && j + k < n && txt[i + k] == txt[j + k] && (isDelim == null || !isDelim(txt[i + k])))
                        k++;

                    lcp[ranks[i] + 1] = k;
                    if (k > 0) k--;
                }

                return lcp;
            }

            public int LongestCommonPrefixOfString(int suffix1, int suffix2)
            {
                return LongestCommonPrefixOfLcpArray(_ranks[suffix1], _ranks[suffix2]);
            }

            public int LongestCommonPrefixOfLcpArray(int rank1, int rank2)
            {
                if (rank1 > rank2) Swap(ref rank1, ref rank2);
                if (rank1 == rank2) return length - _suffixes[rank1];
                return _rmq.GetMin(rank1 + 1, rank2);
            }

            public void InitializeRMQ()
            {
                if (_rmq != null) return;

                var lcps = LcpArray;
                _rmq = new RangeMinimumQuery(lcps);
            }

            public void FindExclusive(int pos, int len, out int left, out int right)
            {
                left = pos;
                right = pos + 1;
                FindExclusive(len, ref left, ref right);
            }

            public void FindExclusive(int len, ref int left, ref int right)
            {
                /*
                var leftSuffixOrig = leftSuffix;
                var rightSuffixOrig = rightSuffix;


                var leftSuffix2 = leftSuffix;
                var rightSuffix2 = rightSuffix;
                while (lcps[rightSuffix2] >= len)
                    rightSuffix2++;
                while (lcps[leftSuffix2] >= len && leftSuffix2 >= 0)
                    leftSuffix2--;
                    */

                int point = right;
                if (_lcps[right] >= len)
                {
                    int dist = 1;
                    while (right + dist < _lcps.Length && _rmq.GetMin(point, right + dist) >= len)
                    {
                        right += dist;
                        dist <<= 1;
                    }

                    for (; dist > 0; dist >>= 1)
                    {
                        while (right + dist < _lcps.Length && _rmq.GetMin(point, right + dist) >= len)
                            right += dist;
                    }

                    if (_lcps[right + 1] < len)
                        right++;
                }


                point = left;
                if (_lcps[left] >= len)
                {
                    int dist = 1;
                    while (left - dist >= 0 && _rmq.GetMin(left - dist + 1, point) >= len)
                    {
                        left -= dist;
                        dist <<= 1;
                    }

                    for (; dist > 0; dist >>= 1)
                    {
                        while (left - dist >= 0 && _rmq.GetMin(left - dist + 1, point) >= len)
                            left -= dist;
                    }
                }
            }


            public int[] ComputeOccurrencesPresums(int stringLength)
            {
                int sum = 0;
                int[] presum = new int[length];
                for (int i = 0; i < presum.Length; i++)
                {
                    presum[i] = sum;
                    if (_suffixes[i] < stringLength) sum++;
                }
                return presum;
            }

            public static int Occurrences(int[] presum, int left, int right)
            {
                return presum[right] - presum[left];
            }

            #endregion


            #region Helpers

            public static void Swap<T>(ref T a, ref T b)
            {
                T tmp = a;
                a = b;
                b = tmp;
            }

            #endregion
        }


        public class RangeMinimumQuery
        {
            readonly int[,] _table;
            readonly int _n;
            readonly int[] _array;

            public RangeMinimumQuery(int[] array)
            {
                _array = array;
                _n = array.Length;

                int n = array.Length;
                int lgn = Log2(n);
                _table = new int[lgn, n];

                _table[0, n - 1] = n - 1;
                for (int j = n - 2; j >= 0; j--)
                    _table[0, j] = array[j] <= array[j + 1] ? j : j + 1;

                for (int i = 1; i < lgn; i++)
                {
                    int curlen = 1 << i;
                    for (int j = 0; j < n; j++)
                    {
                        int right = j + curlen;
                        var pos1 = _table[i - 1, j];
                        int pos2;
                        _table[i, j] =
                            (right >= n || array[pos1] <= array[pos2 = _table[i - 1, right]])
                                ? pos1
                                : pos2;
                    }
                }
            }

            public int GetArgMin(int left, int right)
            {
                if (left == right) return left;
                int curlog = Log2(right - left + 1);
                int pos1 = _table[curlog - 1, left];
                int pos2 = _table[curlog - 1, right - (1 << curlog) + 1];
                return _array[pos1] <= _array[pos2] ? pos1 : pos2;
            }

            public int GetMin(int left, int right)
            {
                return _array[GetArgMin(left, right)];
            }


            static int Log2(int value)
            {
                var log = 0;
                if ((uint)value >= (1U << 12))
                {
                    log = 12;
                    value = (int)((uint)value >> 12);
                    if (value >= (1 << 12))
                    {
                        log += 12;
                        value >>= 12;
                    }
                }
                if (value >= (1 << 6))
                {
                    log += 6;
                    value >>= 6;
                }
                if (value >= (1 << 3))
                {
                    log += 3;
                    value >>= 3;
                }
                return log + (value >> 1 & ~value >> 2);
            }
        }

        #endregion
        #endregion
    }
}
