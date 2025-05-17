//#define SLOWSIZE
//#define SLOWTOP
//#define TEST
#define VERSION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;

class Solution
{
	int[] compressedColors;
	int maxColor;

	int n;
	List<Edge>[] g;
	List<int>[] g2;
	Edge[] edges;
	Query[] queries;
	FenwickTree bit;

	public void ReadData()
	{
		n = Ni();
		g = new List<Edge>[n];

		for (int i = 0; i < n; i++)
			g[i] = new List<Edge>();
		edges = new Edge[n - 1];

		for (int i = 0; i < n - 1; i++)
		{
			int u = Ni() - 1;
			int v = Ni() - 1;
			int c = Ni();
			if (u > v) Swap(ref u, ref v);
			var e = edges[i] = new Edge { parent = u, child = v, color = c, i = i };
			g[u].Add(e);
			g[v].Add(e);
		}

		queries = new Query[Ni()];
		for (int i = 0; i < queries.Length; i++)
		{
			var op = Ni();
			var q = queries[i] = new Query { op = op, item1 = Ni(), item2 = op != 3 ? Ni() : 0 };
			if (q.op == 1 || q.op == 3) q.item1--;
		}

	}


	public void solve()
	{
		ReadData();

		CompressColors();

		tree = new TreeGraph(g);

		SetupHld();

		InitialCalculation();

		ProcessQueries();
	}

	public void ProcessQueries()
	{
		foreach (var q in queries)
		{
			switch (q.op)
			{
				case 1:
					ChangeColor(q.item1, q.item2);
					break;

				case 2:
					// Compute Sum(l,r, T(Sc))
					var ans = ComputeSum(q.item1, q.item2);
					WriteLine(ans);
					break;

				case 3:
					// Compute P(Xi)
					ans = ComputeP(q.item1);
					WriteLine(ans);
					break;
			}

		}
	}

	public long ComputeSum(int left, int right)
	{
		// Non-existent range
		if (left > right) return 0;

		return bit.SumInclusive(left, right);
	}

	long ComputeP(int ei)
	{
		var r = FindTopFromEdge(ei);
		var e2p = edges[vertex2Edge[r]];
		long c = ComputeSizeFromColor2(e2p.parent, e2p.color);
		return Prod(c);
	}

	long Prod(long x)
	{
		long result = x * (x + 1) >> 1;
		return result;
	}

    public void ChangeColor(int ei, int color)
	{
		var e = edges[ei];
		var prevColor = e.color;
		if (prevColor == color) return;

		var vertex = e.child;
		var p = tree.Parents[vertex];
		int oldSize = GetSize(vertex);

		int pcolor = VertexColor(p);
		//int oldRoot = pcolor == prevColor  ? FindRoot(p) : vertex;
		//var newRoot = pcolor == color  ? FindRoot(p) : vertex;

		int oldRoot = pcolor == prevColor && p != 0 ? FindTop(p) : vertex;
		var newRoot = pcolor == color && p != 0 ? FindTop(p) : vertex;

		var oldRootPreviousSize = ComputeSizeFromColor(oldRoot, prevColor);
		var newRootPreviousSize = ComputeSizeFromColor(newRoot, color);

		// Adjust parents
		UpdateSizes(vertex, oldRoot, -oldSize);
        FixColor(tree.Parents[oldRoot], prevColor, -(int)oldSize);

		// Remove children values
		var childrenColor = ComputeSizeFromColor2(vertex, color);
		bit.Add(color, -Prod(childrenColor));

		e.color = color;

		var childrenPrevColor = ComputeSizeFromColor2(vertex, prevColor);
		bit.Add(prevColor, Prod(childrenPrevColor));

		long newSize = e.weight + childrenColor;

		// FIX THIS BOTTLENECK
		if (childrenPrevColor != 0 || childrenColor != 0)
		{
			foreach (var e2 in g[vertex])
			{
				var c = e2.child;
				if (e2.parent != vertex) continue;
				var childCol = VertexColor(c);
#if !SLOWTOP
				if (childCol == color || childCol == prevColor)
					SetTop(c, childCol == color ? 0 : 1);
#endif
			}
		}

#if !SLOWTOP
		SetTop(vertex, newRoot == vertex ? 1 : 0);
#endif

		UpdateSizes(vertex, newRoot, (int)newSize);
        FixColor(tree.Parents[newRoot], color, (int)newSize);

		var oldRootNewSize = oldRootPreviousSize - oldSize; //ComputeSizeFromColor(oldRoot, prevColor);
		var newRootNewSize = newRootPreviousSize + newSize; //ComputeSizeFromColor(newRoot, color);

		bit.Add(prevColor, Prod(oldRootNewSize) - Prod(oldRootPreviousSize));
		bit.Add(color, Prod(newRootNewSize) - Prod(newRootPreviousSize));
	}

	void UpdateSizes(int v, int anc, int size)
	{
#if SLOWSIZE
		var cur = v;
		for (; cur != anc && cur != 0; cur = tree.Parents[cur])
			vertexSizes[cur] += size;
		vertexSizes[cur] += size;
#else
		AddSizeBit(v, anc, size);
#endif
	}


	int[] vertex2Edge;
	private int[] vertexSizes;
	void InitialCalculation()
	{
		vertexSizes = new int[n];

		int colorSize = 0;
		var colormap = new int[n + 1];
		var vertexMap = new int[n];
		var nodeHit = new int[n];
		var edgeMap = new int[n - 1];
		vertex2Edge = new int[n];

		for (int i = 0; i < vertexMap.Length; i++)
		{
			vertexMap[i] = i;
			nodeHit[i] = 1;
		}

		for (int i = 0; i < n - 1; i++)
		{
			var e = edges[i];
			if (tree.Depths[e.child] < tree.Depths[e.parent])
				Swap(ref e.child, ref e.parent);
			vertex2Edge[e.child] = i;
			colormap[i] = e.color;
			edgeMap[i] = i;
		}

		/*
		foreach (var q in queries)
		{
			if (q.op == 1 && colormap[q.item1] != q.item2)
				colormap[q.item1] = -1;
		}

		for (int u = 1; u < size; u++)
		{
			var iEdge = vertex2Edge[u];
			if (colormap[iEdge] < 0) continue;
			var map = vertexMap[u];
			foreach (var childEdge in g[u])
			{
				var child = childEdge.child;
				if (child == u) continue;
				if (colormap[childEdge.i] == colormap[iEdge])
				{
					edgeMap[childEdge.i] = edgeMap[iEdge];
					vertexMap[child] = map;
					nodeHit[map]++;
					nodeHit[child] = 0;
				}
			}
		} */

		// Set descendant sizes
		for (int ii = tree.TreeSize - 1; ii >= 0; ii--)
		{
			var p = tree.Queue[ii];
			var e = edges[vertex2Edge[p]];
			int c = e.color;
			int childSizes = e.weight;
			foreach (var childEdge in g[p])
			{
				var child = childEdge.child;
				if (child == p) continue;
				if (VertexColor(child) == c)
					childSizes += vertexSizes[child];
			}
			vertexSizes[p] = childSizes;
		}

#if !SLOWSIZE
		for (int i = 0; i < n; i++)
			AddSizeBit(i, vertexSizes[i]);
#endif

		// Set up initial statistics
		var visited = new HashSet<long>();
		for (int i = 1; i < n; i++)
		{
			var p = tree.Parents[i];
			var c = VertexColor(i);
			var code = ((long) c << 24) + p;

			if (!IsTopFromVertex(i)) continue;

			if (visited.Contains(code)) continue;
			visited.Add(code);

			long count = ComputeSizeFromColor2(p, c);
			if (count != 0)
				bit.Add(c, Prod(count));
		}

#if !SLOWTOP
		for (int i = 1; i < n; i++)
		{
			if (IsTopFromVertex(i))
				SetTop(i, 1);
		}
#endif
	}

	public long ComputeSizeFromColor(int v, int col)
	{
		return ComputeSizeFromColor2(tree.Parents[v], col);
	}

	private int edgeColorTime;
	private int[] edgeColorTimestamp;
	private int[] edgeColorArray;

	private Dictionary<int, int>[] colorMaps;
	private int[] lastUpdateTimes;
    const int CutOff = 20;
    
	public long ComputeSizeFromColor2(int p, int col)
	{
		int count = 0;

#if VERSION
		if (g[p].Count < CutOff)
		{
#endif		
			foreach (var e in g[p])
			{
				if (e.color != col || e.child == p) continue;
				count += GetSize(e.child);
			}
#if VERSION
		}
		else
		{
			int lastUpdateTime = -1;
			if (lastUpdateTimes != null)
			{
				lastUpdateTime = lastUpdateTimes[p];
			}
			else
			{
				lastUpdateTimes = new int[n];
				colorMaps = new Dictionary<int, int>[n];
			}

			var colorMap = colorMaps[p];
			if (colorMap == null)
				colorMaps[p] = colorMap = new Dictionary<int, int>();
			
			long version = GetVersion(p);
			if (lastUpdateTime < version)
			{
				lastUpdateTimes[p] = (int)version;
				colorMap.Clear();

				foreach (var e in g[p])
				{
					if (e.child == p) continue;
					var childCol = VertexColor(e.child);
					int prevCount;
					colorMap.TryGetValue(childCol, out prevCount);
					colorMap[childCol] = prevCount + GetSize(e.child);
				}
			}
			colorMap.TryGetValue(col, out count);
		}
#endif
		return count;
	}
    
    void FixColor(int p, int col, int size)
    {
        
#if VERSION
		if (g[p].Count >= CutOff && lastUpdateTimes != null)
		{
			int lastUpdateTime = lastUpdateTimes[p];
			var colorMap = colorMaps[p];
			
			long version = GetVersion(p);
			if (lastUpdateTime >= version)
			{
                int prevCount;
                colorMap.TryGetValue(col, out prevCount);
                colorMap[col] = prevCount + size;
			}
		}
#endif            
    }

	class ColorCache
	{
		private Dictionary<int, int> HashCache;
		private int[] ArrayCache;

	}


	bool IsTopFromVertex(int v)
	{
		if (v == 0) return true;
		return IsTopFromEdge(vertex2Edge[v]);
	}

	bool IsTopFromEdge(int ie)
	{
		var e = edges[ie];
		return (e.parent == 0 || edges[vertex2Edge[e.parent]].color != e.color);
	}

	int FindTopFromEdge(int edge)
	{
#if SLOWTOP
		return FindTopSlow(edge);
#else
		var e = edges[edge];
		var result = FindTop(e.child);
		return result;
#endif
	}

	int FindTopSlow(int edge)
	{
		var e = edges[edge];

		while (true)
		{
			if (e.parent == 0) return e.child;
			var ie2 = vertex2Edge[e.parent];
			var e2 = edges[ie2];
			if (e2.color != e.color)
				return e.child;

			e = e2;
		}
	}


	int FindTopFromVertex(int v)
	{
		if (v == 0) return 0;
		return FindTopFromEdge(vertex2Edge[v]);
	}

	public int VertexColor(int v)
	{
		return edges[vertex2Edge[v]].color;
	}

	public void CompressColors()
	{
		var hashSet = new Dictionary<int, int>();
		foreach (var e in edges)
			hashSet[e.color] = 0;
		foreach (var q in queries)
			if (q.op == 1)
				hashSet[q.item2] = 0;

		var colors = compressedColors = hashSet.Keys.ToArray();
		Sort(colors);

		maxColor = colors[colors.Length - 1];

#if true
		// Change colors

		for (var ind = 0; ind < colors.Length; ind++)
			hashSet[colors[ind]] = ind;

		foreach (var e in edges)
			e.color = hashSet[e.color];

		foreach (var q in queries)
		{
			if (q.op == 1)
				q.item2 = hashSet[q.item2];
			else if (q.op == 2)
			{
				q.item1 = BinarySearch(colors, q.item1, 0, colors.Length - 1, false);
				q.item2 = BinarySearch(colors, q.item2, 0, colors.Length - 1, true) - 1;
			}
		}

		maxColor = colors.Length - 1;
#endif

		bit = new FenwickTree(maxColor + 2);
	}


	#region HLD

	List<Path> path = new List<Path>(100);
	public class ChainData
	{
		public FenwickTree Sizes;
#if VERSION		
		public FenwickTree Version;
#endif
		public FenwickTree TopTrees;
	}

	public struct Path
	{
		public int Top;
		public int Node;
		public bool Up;
		public override string ToString()
		{
			var up = Up ? "->" : "<-";
			return $"{Node + 1} {up} {Top + 1}  ({Node} {up} {Top})";
		}
	}

	private ChainData[] chains;
	private HeavyLightDecomposition2 hld;
	private TreeGraph tree;

	public void SetupHld()
	{
		hld = new HeavyLightDecomposition2(tree);

		chains = new ChainData[hld.Paths.Length];
		for (int i = 0; i < chains.Length; i++)
		{
			var len = hld.Paths[i].Length;
			chains[i] = new ChainData
			{
				Sizes = new FenwickTree(len),
				TopTrees = new FenwickTree(len),
#if VERSION
				Version = new FenwickTree(len),
#endif
			};

			chains[i].Sizes.AltRangeUpdatePointQueryMode = true;
#if VERSION
			chains[i].Version.AltRangeUpdatePointQueryMode = true;
#endif
		}
	}

	public int FindTop(int v)
	{
		BuildPath(path, v, 0, 0);

		foreach (var step in path)
		{
			int top = step.Top;
			int topIndex = hld.Positions[top];
			int bottom = step.Node;
			int bottomIndex = hld.Positions[bottom];
			int chain = hld.Chains[top];
			var data = chains[chain];
			var chainpath = hld.Paths[chain];

			var sum2 = data.TopTrees.SumInclusive(0, bottomIndex);
			if (sum2 != 0)
			{
				var index = data.TopTrees.GetIndexGreater(sum2 - 1);
				return chainpath[index];
			}
		}

		return 0;
	}

	public void SetTop(int v, int value)
	{
		var pos = hld.Positions[v];
		var topTree = chains[hld.Chains[v]].TopTrees;
		topTree.Add(pos, value - topTree.SumInclusive(pos, pos));
	}

	public int GetSize(int v)
	{
#if SLOWSIZE
		return vertexSizes[v];
#else
		return (int)chains[hld.Chains[v]].Sizes[hld.Positions[v]];
#endif
	}

#if VERSION
	public int GetVersion(int v)
	{
#if SLOWSIZE
		return vertexSizes[v];
#else
		return (int)chains[hld.Chains[v]].Version[hld.Positions[v]];
#endif
	}
#endif	

	public void AddSizeBit(int v, int size)
	{
		var pos = hld.Positions[v];
		var chain = chains[hld.Chains[v]];
		chain.Sizes.AltAddInclusive(pos, pos, size);
#if VERSION		
		chain.Version.AltAddInclusive(pos, pos, 1);
#endif
	}

	public void AddSizeBit(int v, int u, long add)
	{
		BuildPath(path, u, v, hld.Lca(u, v));

		foreach (var step in path)
		{
			int top = step.Top;
			int topIndex = hld.Positions[top];
			int bottom = step.Node;
			int bottomIndex = hld.Positions[bottom];
			int chain = hld.Chains[top];
			var data = chains[chain];
			data.Sizes.AltAddInclusive(topIndex, bottomIndex, add);
#if VERSION			
			data.Version.AltAddInclusive(topIndex, bottomIndex, 1);
#endif
		}
	}

	public void BuildPath(List<Path> path, int u, int v, int lca)
	{
		path.Clear();

		var lcaChain = hld.Chains[lca];
		int head;
		int lcaCount = 0;

		// u to lca
		for (var node = u; node != lca; node = tree.Parents[head])
		{
			var chain = hld.Chains[node];

			if (chain == lcaChain)
			{
				path.Add(new Path { Top = lca, Node = node, Up = true });
				lcaCount++;
				break;
			}

			head = hld.Paths[chain][0];
			path.Add(new Path { Top = head, Node = node, Up = true });
		}

		int sav = path.Count;

		// lca to v
		for (var node = v; node != lca; node = tree.Parents[head])
		{
			var chain = hld.Chains[node];
			if (chain == lcaChain)
			{
				path.Add(new Path { Top = lca, Node = node, Up = false });
				lcaCount++;
				break;
			}

			head = hld.Paths[chain][0];
			path.Add(new Path { Top = head, Node = node, Up = false });
		}

		Debug.Assert(lcaCount < 2);
		if (lcaCount == 0)
			path.Add(new Path { Top = lca, Node = lca, Up = false });

		path.Reverse(sav, path.Count - sav);
	}


	#endregion


	public class Node
	{
		public Node Parent;
		public int Count = 1;
		public int Weight = 1;
		public int Id;
		public int VertexCount => Weight + 1;

		public override string ToString()
		{
			return $"Id={Id} Count={Count} ";
		}
	}

}

public class FenwickTree
{
	#region Variables
	public readonly long[] A;
	#endregion

	#region Constructor


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
		//	Add(i, a[i]);
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

	public void Clear()
	{
		Array.Clear(A, 0, A.Length);
	}

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
		for (int bit = BitTools.HighestOneBit(n); bit != 0; bit >>= 1)
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

	public bool AltRangeUpdatePointQueryMode;

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

public static class BitTools
{
	public static int HighestOneBit(int n)
	{
		return n != 0 ? 1 << Log2(n) : 0;
	}

	public static int Log2(int value)
	{
		// TESTED
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


public class Query
{
	public int op;
	public int item1;
	public int item2;
	public int item1Orig;
	public int item2Orig;
	public int i;

	public override string ToString()
	{
		return $" {op} {item1}/{item1Orig} {item2}/{item2Orig}";
	}
}

public class Edge
{
	public int i;
	public int parent;
	public int child;
	public int color;
	public int weight = 1;
	public bool delete;

	public int Other(int x)
	{
		return x == parent ? child : parent;
	}
	public override string ToString()
	{
		return $" #{i} {parent}->{child} C={color}";
	}
}


public class TreeGraph
{
	#region Variables
	public int[] Parents;
	public int[] Queue;
	public int[] Depths;
	public int[] Sizes;
	public List<Edge>[] Graph;
	public int Root;
	public int TreeSize;
	public int Separator;

	bool sizesInited;
	#endregion

	#region Constructor
	public TreeGraph(List<Edge>[] g, int root = 0, int avoid = -1)
	{
		Graph = g;
		if (root >= 0)
			Init(root, avoid);
	}
	#endregion

	#region Methods
	public void Init(int root, int avoid = -1)
	{
		var g = Graph;
		int n = g.Length;
		Root = root;
		Separator = avoid;

		Queue = new int[n];
		Parents = new int[n];
		Depths = new int[n];

		for (int i = 0; i < Parents.Length; i++)
			Parents[i] = -1;

		Queue[0] = root;

		int treeSize = 1;
		for (int p = 0; p < treeSize; p++)
		{
			int cur = Queue[p];
			var par = Parents[cur];
			foreach (var childEdge in g[cur])
			{
				int child = childEdge.Other(cur);
				if (child != par && child != avoid)
				{
					Queue[treeSize++] = child;
					Parents[child] = cur;
					Depths[child] = Depths[cur] + 1;
				}
			}
		}

		TreeSize = treeSize;
	}

	public void InitSizes()
	{
		if (sizesInited)
			return;

		if (Sizes == null)
			Sizes = new int[Graph.Length];
		sizesInited = true;

		Sizes[Separator] = 0;
		for (int i = TreeSize - 1; i >= 0; i--)
		{
			int current = Queue[i];
			Sizes[current] = 1;
			foreach (var edge in Graph[current])
			{
				var e = edge.Other(current);
				if (Parents[current] != e)
					Sizes[current] += Sizes[e];
			}
		}
	}
	#endregion


}

public class UnionFind
{
	readonly int[] _ds;
	int _count;

	public UnionFind(int size)
	{
		_ds = new int[size];
		Clear();
	}

	public int Count => _count;

	public int[] Array => _ds;

	public void Clear()
	{
		_count = _ds.Length;
		for (int i = 0; i < _ds.Length; i++)
			_ds[i] = -1;
	}

	public bool Union(int x, int y)
	{
		var rx = Find(x);
		var ry = Find(y);
		if (rx == ry) return false;

		if (_ds[rx] <= _ds[ry])
		{
			_ds[rx] += _ds[ry];
			_ds[ry] = rx;
		}
		else
		{
			_ds[ry] += _ds[rx];
			_ds[rx] = ry;
		}
		_count--;
		return true;
	}

	public int Find(int x)
	{
		var root = _ds[x];
		return root < 0
			? x
			: (_ds[x] = Find(root));
	}

	public int GetCount(int x)
	{
		var c = _ds[Find(x)];
		return c >= 0 ? 1 : -c;
	}

	public IEnumerable<int> Roots()
	{
		for (int i = 0; i < _ds.Length; i++)
			if (_ds[i] < 0)
				yield return i;
	}

	public IEnumerable<List<int>> Components()
	{
		var comp = new Dictionary<int, List<int>>();
		foreach (var c in Roots())
			comp[c] = new List<int>(GetCount(c));
		for (int i = 0; i < _ds.Length; i++)
			comp[Find(i)].Add(i);
		return comp.Values;
	}
}


#region Heavy Light
public class HeavyLightDecomposition2
{
	public int[] Chains;
	public int[] Positions;
	public int[][] Paths;

	public TreeGraph Tree;
	public List<Edge>[] Graph;

	public HeavyLightDecomposition2(TreeGraph tree)
	{
		this.Graph = tree.Graph;
		Tree = tree;
		Chains = DecomposeToHeavyLight();
		Paths = BuildPaths();
		Positions = BuildPositions();
	}

	int[] DecomposeToHeavyLight()
	{
		int n = Graph.Length;
		var queue = Tree.Queue;
		var parents = Tree.Parents;

		int[] size = new int[n];
		for (int i = 0; i < n; i++)
			size[i] = 1;
		for (int i = n - 1; i > 0; i--) size[parents[queue[i]]] += size[queue[i]];

		int[] chain = new int[n];
		for (int i = 0; i < n; i++)
			chain[i] = -1;

		int p = 0;
		foreach (int u in queue)
		{
			if (chain[u] == -1) chain[u] = p++;
			int argmax = -1;
			foreach (var e in Graph[u])
			{
				var v = e.Other(u);
				if (parents[u] != v && (argmax == -1 || size[v] > size[argmax]))
					argmax = v;
			}
			if (argmax != -1) chain[argmax] = chain[u];
		}
		return chain;
	}

	int[][] BuildPaths()
	{
		int n = Chains.Length;
		int[] rp = new int[n];
		int sup = 0;
		for (int i = 0; i < n; i++)
		{
			rp[Chains[i]]++;
			sup = Max(sup, Chains[i]);
		}
		sup++;

		int[][] row = new int[sup][];
		for (int i = 0; i < sup; i++) row[i] = new int[rp[i]];

		var queue = Tree.Queue;
		for (int i = n - 1; i >= 0; i--)
		{
			var v = queue[i];
			row[Chains[v]][--rp[Chains[v]]] = v;
		}
		return row;
	}

	int[] BuildPositions()
	{
		int n = Graph.Length;
		int[] iind = new int[n];
		foreach (int[] path in Paths)
		{
			for (int i = 0; i < path.Length; i++)
				iind[path[i]] = i;
		}
		return iind;
	}

	public int Lca(int x, int y)
	{
		int rx = Paths[Chains[x]][0];
		int ry = Paths[Chains[y]][0];

		var depth = Tree.Depths;
		var parent = Tree.Parents;
		while (Chains[x] != Chains[y])
		{
			if (depth[rx] > depth[ry])
			{
				x = parent[rx];
				rx = Paths[Chains[x]][0];
			}
			else
			{
				y = parent[ry];
				ry = Paths[Chains[y]][0];
			}
		}
		return Positions[x] > Positions[y] ? y : x;
	}

	public int Ancestor(int x, int v)
	{
		var parent = Tree.Parents;
		while (x != -1)
		{
			if (v <= Positions[x]) return Paths[Chains[x]][Positions[x] - v];
			v -= Positions[x] + 1;
			x = parent[Paths[Chains[x]][0]];
		}
		return x;
	}
}


#endregion
class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}

static partial class Library
{

	#region Common

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

	public static int BinarySearch<T>(T[] array, T value, int left, int right, bool upper = false)
		where T : IComparable<T>
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

	#endregion

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
		inputIndex = 0;
		bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
		if (bytesRead <= 0) inputBuffer[0] = 32;
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

	public static int[] Ni(int n)
	{
		var list = new int[n];
		for (int i = 0; i < n; i++) list[i] = Ni();
		return list;
	}

	public static long[] Nl(int n)
	{
		var list = new long[n];
		for (int i = 0; i < n; i++) list[i] = Nl();
		return list;
	}

	public static string[] Ns(int n)
	{
		var list = new string[n];
		for (int i = 0; i < n; i++) list[i] = Ns();
		return list;
	}

    public static int Ni()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg) { c = Read(); }

        int number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if ((uint)d > 9) break;
            number = number * 10 + d;
	        if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c=='-';
        if (neg) { c = Read(); }

        long number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if ((uint)d > 9) break;
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

	public static byte[] Nb(int n)
	{
		var list = new byte[n];
		for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte)c;
		return list;
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
		AppDomain.CurrentDomain.ProcessExit += delegate { Flush(); };
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
		ulong number = (ulong)signedNumber;
		if (signedNumber < 0)
		{
			Write('-');
			number = (ulong)(-signedNumber);
		}

		Reserve(20 + 1); // 20 digits + 1 extra
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

}


public class Program
{
	public static void Main(string[] args)
	{
		InitInput(Console.OpenStandardInput());
		InitOutput(Console.OpenStandardOutput());
		Solution solution = new Solution();
		solution.solve();
		Flush();
#if DEBUG
		Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
	}
}
