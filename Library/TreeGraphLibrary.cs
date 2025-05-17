using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;

public class TreeGraphLibrary
{
    #region Tree Graph Library

    List<int>[] g;
    TreeGraph tree;
    int n;

    public void solveGraph()
    {
        n = Ni();

        g = ReadGraph(n);
        tree = new TreeGraph(g, 0);

        int q = Ni();
        long[] answers = new long[q];
        var queries = new Query[q];

        for (int i = 0; i < q; i++)
        {
            var qq = queries[i] = new Query
            {
                I = i,
                X = Ni() - 1,
                Y = Ni() - 1,
                K = Ni()
            };

            if (qq.X > qq.Y)
                Swap(ref qq.X, ref qq.Y);
        }

        foreach(var ans in answers)
            WriteLine(ans);

    }

    #region Helpers

    public class Query
    {
        public int I;
        public int X;
        public int Y;
        public int K;

        public override string ToString()
        {
            return $"#{I} ({X + 1},{Y + 1}) U={X} V={Y}";
        }
    }



    #endregion


    #region Graph Construction

    public static List<int>[] ReadGraph(int n, int m = -1)
    {
        var g = NewGraph(n);
        if (m < 0)
            m = n - 1;
        for (int i = 0; i < m; i++)
        {
            var u = Ni();
            var v = Ni();
            g[u].Add(v);
            g[v].Add(u);
        }
        return g;
    }

    public static List<int>[] NewGraph(int n)
    {
        var g = new List<int>[n+1];
        for (int i = 0; i <= n; i++)
            g[i] = new List<int>();
        return g;
    }

    #endregion

    #region TreeGraph
    public partial class TreeGraph
    {
        #region Variables
        public List<int>[] Graph;
        public int[] Sizes;
        public int[] Begin;
        public int[] Parent;
        public int[] Head;
        public int[] Trace;
        public int TreeSize;
        public int Root => Trace[0];
        public int End(int v) => Begin[v] + Sizes[v] - 1;
        #endregion

        #region Construction
        public TreeGraph(List<int>[] graph, int root = 0, int avoid = -1)
        {
            Graph = graph;
            int n = graph.Length;
            Sizes = new int[n];
            Begin = new int[n];
            Head = new int[n];
            Trace = new int[n];
            Parent = new int[n];
            Build(root, avoid);
        }

        unsafe void Build(int r = 0, int avoid = -1)
        {
            var stack = stackalloc int[Graph.Length + 1];
            int stackSize = 0, treeSize = 0;
            stack[stackSize++] = r;
            Parent[r] = -1;
            while (stackSize > 0)
            {
                var u = stack[--stackSize];
                Trace[treeSize++] = u;
                Head[u] = u;
                Sizes[u] = 1;
                foreach (var v in Graph[u])
                {
                    if (Sizes[v] > 0 || v == avoid) continue;
                    Parent[v] = u;
                    stack[stackSize++] = v;
                }
            }

            for (int iu = treeSize - 1; iu >= 0; iu--)
            {
                var u = Trace[iu];
                var p = Parent[u];
                var neighbors = Graph[u];
                int maxSize = 0;
                for (var i = 0; i < neighbors.Count; i++)
                {
                    var v = neighbors[i];
                    if (v != p && v != avoid)
                    {
                        Sizes[u] += Sizes[v];
                        if (Sizes[v] > maxSize)
                        {
                            maxSize = Sizes[v];
                            neighbors[i] = neighbors[0];
                            neighbors[0] = v;
                        }
                    }
                }
            }

            stackSize = treeSize = 0;
            stack[stackSize++] = r;
            while (stackSize > 0)
            {
                var u = stack[--stackSize];
                Begin[u] = treeSize;
                Trace[treeSize++] = u;
                int heavy = -1;
                var children = Graph[u];
                for (int iv = children.Count - 1; iv >= 0; iv--)
                {
                    var v = children[iv];
                    if (v != Parent[u] && v != avoid)
                        stack[stackSize++] = heavy = v;
                }
                if (heavy >= 0) Head[heavy] = Head[u];
            }

            TreeSize = treeSize;
        }
        #endregion

        #region LCA
        public int Lca(int x, int y)
        {
            for (int rx = Head[x], ry = Head[y]; rx != ry;)
            {
                if (Begin[rx] > Begin[ry])
                {
                    x = Parent[rx];
                    rx = Head[x];
                }
                else
                {
                    y = Parent[ry];
                    ry = Head[y];
                }
            }
            return Begin[x] > Begin[y] ? y : x;
        }

        public int Distance(int x, int y)
        {
            var distance = 0;
            for (int rx = Head[x], ry = Head[y]; rx != ry;)
            {
                if (Begin[rx] > Begin[ry])
                {
                    distance += 1 + Begin[x] - Begin[rx];
                    x = Parent[rx];
                    rx = Head[x];
                }
                else
                {
                    distance += 1 + Begin[y] - Begin[ry];
                    y = Parent[ry];
                    ry = Head[y];
                }
            }
            return distance + Abs(Begin[x] - Begin[y]);
        }

        public int Ancestor(int x, int v)
        {
            while (x >= 0)
            {
                int position = Begin[x] - Begin[Head[x]];
                if (v <= position) return Trace[Begin[x] - v];
                v -= position + 1;
                x = Parent[Head[x]];
            }
            return x;
        }

        public int Intersect(int p1, int p2, int p3)
        {
            if (Begin[p1] > Begin[p2]) Swap(ref p1, ref p2);
            if (Begin[p2] > Begin[p3]) Swap(ref p2, ref p3);
            if (Begin[p1] > Begin[p2]) Swap(ref p1, ref p2);

            if (unchecked((uint)(Begin[p3] - Begin[p2]) < (uint)Sizes[p2]))
                return p2;

            var lca = Lca(p1, p2);
            return unchecked((uint)(Begin[p3] - Begin[lca]) >= (uint)Sizes[lca])
                ? lca : Lca(p2, p3);
        }

        #endregion

        #region HLD
        List<Segment> segs = new List<Segment>(32);
        public List<Segment> Query(int x, int y, bool edges = false)
        {
            // up segs in ascending order, down segs in descending order
            segs.Clear();

            for (int rx = Head[x], ry = Head[y]; rx != ry;)
            {
                if (Begin[rx] > Begin[ry])
                {
                    segs.Add(new Segment(Begin[rx], Begin[x], 1));
                    x = Parent[rx];
                    rx = Head[x];
                }
                else
                {
                    segs.Add(new Segment(Begin[ry], Begin[y], -1));
                    y = Parent[ry];
                    ry = Head[y];
                }
            }

            var lcaIndex = Min(Begin[x], Begin[y]);
            var nodeIndex = Max(Begin[x], Begin[y]);
            if (edges == false || lcaIndex < nodeIndex)
                segs.Add(new Segment(lcaIndex + (edges ? 1 : 0), nodeIndex,
                    nodeIndex == Begin[x] ? 1 : -1));

            return segs;
        }

        public struct Segment
        {
            public int HeadIndex, NodeIndex, Dir;
            public Segment(int headIndex, int nodeIndex, int dir)
            {
                HeadIndex = headIndex;
                NodeIndex = nodeIndex;
                Dir = dir;
            }
        }
        #endregion

        #region MOS
        public void AddTask(List<Task> tasks, int x, int y, Action action)
        {
            tasks.Add(new Task { Start = x, End = y, Action = action });
        }

        void SortTasks(List<Task> tasks)
        {
            int[] euler = new int[TreeSize];
            for (int i = 1; i < TreeSize; i++)
                euler[i] = euler[i - 1] + Distance(Trace[i - 1], Trace[i]);

            foreach (var t in tasks)
            {
                if (Begin[t.Start] > Begin[t.End]) Swap(ref t.Start, ref t.End);
                int s = t.Start, e = t.End;
                t.SI = euler[Begin[s]];
                t.EI = euler[Begin[e]];
                if (unchecked((uint)(Begin[e] - Begin[s]) >= (uint)Sizes[s]))
                    t.SI += Sizes[s] * 2 - 1; // -2 for nodal tours 
            }

            int r = (int)Ceiling(Sqrt(2 * TreeSize));
            tasks.Sort((x, y) => x.SI / r == y.SI / r ? x.EI - y.EI : x.SI - y.SI);
        }

        public void Execute(List<Task> tasks, Action<int> flip)
        {
            SortTasks(tasks);
            int s = (tasks.Count > 0 ? tasks[0].Start : Trace[0]), e = s;
            flip(s);

            foreach (var task in tasks)
            {
                s = Adjust(e, s, task.Start, flip);
                e = Adjust(s, e, task.End, flip);
                task.Action();
            }
        }

        int Adjust(int start, int oldEnd, int newEnd, Action<int> flip)
        {
            if (oldEnd == newEnd) return newEnd;
            foreach (var seg in Query(oldEnd, newEnd))
            {
                for (int i = seg.HeadIndex; i <= seg.NodeIndex; i++)
                    flip(Trace[i]);
            }
            flip(Intersect(start, oldEnd, newEnd));
            return newEnd;
        }

        public class Task
        {
            public int Start, End, SI, EI;
            public Action Action;
            public override string ToString() => $"[{Start},{End}]";
        }
        #endregion
    }

    #endregion

    #region Euler Tour

    public class EulerTour
    {
        public readonly int[] Trace;
        public readonly int[] Begin;
        public readonly int[] End;
        public readonly int[] Depth;

        public EulerTour(List<int>[] g, int root)
        {
            int n = g.Length;
            Trace = new int[2 * n];
            Begin = new int[n];
            End = new int[n];
            Depth = new int[n];
            int t = 0;

            for (int i = 0; i < n; i++)
                Begin[i] = -1;

            var stack = new int[n];
            var indices = new int[n];
            int sp = 0;
            stack[sp++] = root;

            while (sp > 0)
            {
                outer:
                int current = stack[sp - 1], index = indices[sp - 1];
                if (index == 0)
                {
                    Trace[t] = current;
                    Begin[current] = t;
                    Depth[current] = sp - 1;
                    t++;
                }

                var children = g[current];
                while (index < children.Count)
                {
                    int child = children[index++];
                    if (Begin[child] == -1)
                    {
                        indices[sp - 1] = index;
                        stack[sp] = child;
                        indices[sp] = 0;
                        sp++;
                        goto outer;
                    }
                }

                indices[sp - 1] = index;
                if (index == children.Count)
                {
                    sp--;
                    Trace[t] = current;
                    End[current] = t;
                    t++;
                }
            }
        }

        public bool IsBegin(int trace) => Begin[Trace[trace]] == trace;

        public bool IsEnd(int trace) => End[Trace[trace]] == trace;

        int ComputeTraceLength(IList<int>[] g, bool inbetween, bool collapseLeaves)
        {
            if (inbetween == false) return g.Length * 2;

            // TODO: May overshoot if g is bidirectional
            int count = 0;
            foreach (var list in g)
            {
                if (list == null) continue;
                count += 2 + Max(list.Count - 1, 0);
            }

            return count;
        }

        public int this[int index] => Trace[index];
    }

    public class EulerTourMos
    {
        public EulerTour Tour;
        public TreeGraph Tree;
        public List<Task> Tasks;
        private bool[] flipped;
        private Action<int, int> change;

        public EulerTourMos(List<int>[] graph, int root, int queryCount = 0)
        {
            Tour = new EulerTour(graph, root);
            Tree = new TreeGraph(graph, root);
            Tasks = new List<Task>(queryCount);
        }

        public void ClearQueries()
        {
            Tasks.Clear();
        }

        public void AddQuery(int x, int y, Action action)
        {
            var lca = Tree.Lca(x, y);
            if (lca == y) Swap(ref x, ref y);

            int start, end, lcaSpecial;
            if (lca == x)
            {
                start = Tour.Begin[x];
                end = Tour.Begin[y];
                lcaSpecial = -1;
            }
            else
            {
                if (Tour.Begin[x] > Tour.Begin[y]) Swap(ref x, ref y);
                start = Tour.End[x];
                end = Tour.Begin[y];
                lcaSpecial = lca;
            }

            if (start > end) Swap(ref start, ref end);

            var task = new Task
            {
                Start = start,
                End = end,
                Lca = lcaSpecial,
                Action = action,
            };

            Tasks.Add(task);
        }

        public void AddPairwiseQuery(int x, int y, Action action)
        {
            throw new NotImplementedException();
        }

        void Flip(int x)
        {
            var node = Tour.Trace[x];
            var add = flipped[node] ^= true;
            change(node, add ? 1 : -1);
        }

        public void Execute(Action<int, int> change)
        {
            this.change = change;
            flipped = new bool[Tree.TreeSize];

            int n = Tour.Trace.Length;
            int sqrt = (int)Ceiling(Sqrt(n));
            Tasks.Sort((x, y) => x.Start / sqrt == y.Start / sqrt
                ? x.End.CompareTo(y.End)
                : x.Start.CompareTo(y.Start));

            CoreOptimized();
        }

        void PerformAction(Task task)
        {
            var lca = task.Lca;
            if (lca != -1) Flip(Tour.Begin[lca]);
            task.Action();
            if (lca != -1) Flip(Tour.Begin[lca]);
        }

        void CoreSimple()
        {
            int s = 0;
            int e = s - 1;

            foreach (var task in Tasks)
            {
                while (e < task.End) Flip(++e);
                while (e > task.End) Flip(e--);
                while (s < task.Start) Flip(s++);
                while (s > task.Start) Flip(--s);
                PerformAction(task);
            }
        }

        void CoreOptimized()
        {
            int s = 0;
            int e = s - 1;
            var tour = Tour;

            foreach (var task in Tasks)
            {
                int start = task.Start;
                int end = task.End;

                int end2 = end << 1;
                do
                {
                    while (e < end)
                    {
                        ++e;
                        var node = tour.Trace[e];
                        var next = tour.End[node];
                        if (e == next || next + e >= end2)
                            // if (e == next || next > end && next - end >= end - e)
                            // if (e == next || next > end)
                            Flip(e);
                        else
                            e = next;
                    }
                    for (; e > end; e--)
                    {
                        var node = tour.Trace[e];
                        var prev = tour.Begin[node];
                        if (e == prev || end2 >= e + prev)
                            //if (e == prev || prev <= end && end - prev >= e - end)
                            //if (e == prev || prev <= end)
                            Flip(e);
                        else
                            e = prev;
                    }
                }
                while (e != end);

                int start2 = start << 1;
                do
                {
                    while (s > start)
                    {
                        --s;
                        var node = tour.Trace[s];
                        var prev = tour.Begin[node];
                        if (s == prev || start2 >= s + prev)
                            // if (s == prev || prev < start && start - prev >= s - start)
                            // if (s == prev || prev < start )
                            Flip(s);
                        else
                            s = prev;
                    }
                    for (; s < start; s++)
                    {
                        var node = tour.Trace[s];
                        var next = tour.End[node];
                        if (s == next || next + s >= start2)
                            //if (s == next || next >= start && next - start >= start - s)
                            //if (s == next || next >= start )
                            Flip(s);
                        else
                            s = next;
                    }
                }
                while (s != start);

                PerformAction(task);
            }
        }

        public class Task
        {
            public int Start;
            public int End;
            public int Lca = -1;
            public Action Action;
            public object Tag { get; set; }

            public override string ToString()
            {
                return $"{Tag} [{Start},{End}]";
            }
        }
    }

    #endregion

    #region Segment Trees

    public class FenwickTree
    {
        #region Variables
        public readonly long[] A;
        #endregion

        #region Constructor
        /*public Fenwick(int[] a) : this(a.Length)
        {
            for (int i = 0; i < a.Length; i++)
                Add(i, a[i]);
        }*/

        public FenwickTree(long[] a) : this(a.Length)
        {
            int n = a.Length;
            System.Array.Copy(a, 0, A, 1, n);
            for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
            {
                for (int i = k; i <= n; i += k)
                    A[i] += A[i - h];
            }

            //for (int i = 0; i < a.Length; i++)
            //    Add(i, a[i]);
        }

        public FenwickTree(long size)
        {
            A = new long[size + 1];
        }
        #endregion

        #region Properties
        public long this[int index] => AltRangeUpdatePointQueryMode ? SumInclusive(index) : SumInclusive(index, index);

        public int Length => A.Length - 1;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public long[] Table
        {
            get
            {
                int n = A.Length - 1;
                long[] ret = new long[n];
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
        // Increments value        
        /// <summary>
        /// Adds val to the value at i
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="val">The value.</param>
        public void Add(int i, long val)
        {
            if (val == 0) return;
            for (i++; i < A.Length; i += (i & -i))
                A[i] += val;
        }

        // Sum from [0 ... i]
        public long SumInclusive(int i)
        {
            long sum = 0;
            for (i++; i > 0; i -= (i & -i))
                sum += A[i];
            return sum;
        }

        public long SumInclusive(int i, int j)
        {
            return SumInclusive(j) - SumInclusive(i - 1);
        }

        // get largest value with cumulative sum less than x;
        // for smallest, pass x-1 and add 1 to result
        public int GetIndexGreater(long x)
        {
            int i = 0, n = A.Length - 1;
            for (int bit = (int)HighestOneBit(n); bit != 0; bit >>= 1)
            {
                int t = i | bit;

                // if (t <= n && Array[t] < x) for greater or equal 
                if (t <= n && A[t] <= x)
                {
                    i = t;
                    x -= A[t];
                }
            }
            return i;
        }

        #endregion

        #region Alternative Range Update Point Query Mode  ( cf Standard Point Update Range Query )

        public bool AltRangeUpdatePointQueryMode { get; set; }

        /// <summary>
        /// Inclusive update of the range [left, right] by value
        /// The default operation of the fenwick tree is point update - range query.
        /// We use this if we want alternative range update - point query.
        /// SumInclusive becomes te point query function.
        /// </summary>
        /// <returns></returns>
        public void AltAddInclusive(int left, int right, long delta)
        {
            Add(left, delta);
            Add(right + 1, -delta);
        }

        public long AltQuery(int index)
        {
            return SumInclusive(index);
        }


        #endregion
    }

    #endregion

    #endregion
}


