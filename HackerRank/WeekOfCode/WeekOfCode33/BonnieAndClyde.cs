namespace HackerRank.WeekOfCode33.BonnieAndClyde
{
	// https://www.hackerrank.com/contests/w33/challenges/bonnie-and-clyde

	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using static System.Array;
	using static System.Math;
	using static Library;

	class Solution
	{
		const int AdjacencyThreshold = 10;

		Query[] answers;
		List<Query> queries;

		List<int>[] graph;
		List<int>[] graph3;

		BridgesAndCuts bc;
		UnionFind connected;
		UnionFind nodeConnected;
		UnionFind edgeConnected;
		LowestCommonAncestor lca;

		LowestCommonAncestor lcaOld;
		private TreeGraph old;
		//TreeGraph tree;
		private HashSet<int>[] cutpointNeighbors;

		int n, m, q;

		public void TestData()
		{
			int limit = 10;
			n = limit;
			m = limit;
			q = limit;
			var r = new Random();

			graph = new List<int>[n];
			for (int i = 0; i < n; i++)
				graph[i] = new List<int>();

			for (int i = 0; i < m; i++)
			{
				var u = r.Next(limit);
				int v;
				do v = (u + r.Next(1, limit - 1)) % limit;
				while (u == v || graph[u].Contains(v));

				graph[u].Add(v);
				graph[v].Add(u);
			}

			PruneRedundantEdges(graph);

			queries = new List<Query>(q);
			for (int i = 0; i < q; i++)
				queries.Add(new Query { Index = i, U = r.Next(n), V = r.Next(n), W = r.Next(n) });
		}


		public void solve()
		{

#if DEBUG
			ReadData();
			//TestData();
#else
		ReadData();
#endif

			connected = new UnionFind(n + 1);
			for (int u = 0; u < n; u++)
				foreach (var v in graph[u])
					connected.Union(u, v);

			cutpointNeighbors = new HashSet<int>[n];
			visited = new long[n];
			parent = new int[n];
			banned = new int[n];
			bannedts = new long[n];

			bc = new BridgesAndCuts(graph);
			edgeConnected = bc.GetComponents(avoidBridges: true, avoidCuts: false);
			nodeConnected = bc.GetComponents(avoidBridges: true, avoidCuts: true);
			graph3 = BuildGraphFromComponents(nodeConnected, false);

			lca = BuildLca();

			InitializeQueries();
			ProcessQueries();

			for (int i = 0; i < q; i++)
				WriteLine(answers[i].Answer == true ? "YES" : "NO");
		}


		void InitializeQueries()
		{
			answers = new Query[q];
			for (int i = 0; i < q; i++)
				answers[i] = queries[i];

			int[] compareIndices = BuildCompareTable();

			foreach (var qu in queries)
			{
				if (compareIndices[qu.U] > compareIndices[qu.V])
					Swap(ref qu.U, ref qu.V);

				qu.Ua = nodeConnected.Find(qu.U);
				qu.Va = nodeConnected.Find(qu.V);
				qu.Wa = nodeConnected.Find(qu.W);

				qu.Ue = edgeConnected.Find(qu.U);
				qu.Ve = edgeConnected.Find(qu.V);
				qu.We = edgeConnected.Find(qu.W);

				// If w is in the same region as another vertex, make sure ue == ve
				if (qu.We == qu.Ve)
				{
					if (qu.We != qu.Ue || qu.Wa == qu.Va && qu.Wa != qu.Ua)
					{
						Swap(ref qu.U, ref qu.V);
						Swap(ref qu.Ue, ref qu.Ve);
						Swap(ref qu.Ua, ref qu.Va);
					}
				}
			}

			queries.Sort((a, b) =>
			{
				int cmp = compareIndices[a.W].CompareTo(compareIndices[b.W]);
				if (cmp != 0) return cmp;
				cmp = compareIndices[a.U].CompareTo(compareIndices[b.U]);
				if (cmp != 0) return cmp;
				cmp = compareIndices[a.V].CompareTo(compareIndices[b.V]);
				return cmp;
			});
		}

		void ProcessQueries()
		{
			Query prev = null;
			foreach (var qu in queries)
			{
				if (prev != null && qu.Va == prev.Va && qu.Ua == prev.Ua && qu.Wa == prev.Wa)
				{
					answers[qu.Index] = prev;
				}
				else
				{
					qu.Answer = HasDisjointPaths(qu);
#if DEBUG
					var bruteforce = SearchDisjointPaths(qu);
					Debug.Assert(bruteforce == qu.Answer);
#endif
					prev = qu;
				}
			}
		}

		bool HasDisjointPaths(Query qu)
		{

			// Make sure each one is in the same connected subgraph
			if (connected.Find(qu.U) != connected.Find(qu.V)
				|| connected.Find(qu.U) != connected.Find(qu.W))
				return false;

			// We found a disjoint path -- saves a lot of work
			int duv = lca.Distance(qu.Ua, qu.Va);
			int duw = lca.Distance(qu.Ua, qu.Wa);
			int dvw = lca.Distance(qu.Va, qu.Wa);
			if (duv == dvw + duw)
				return true;

			if (qu.Ua == qu.Wa || qu.Va == qu.Wa)
				return true;

			// We could have done this earlier and removed redundant code
			/*int rep = GetRep(qu.Ua, qu.Va, qu.Wa, x => Adjacent(nodeConnected.Find(x), qu.Wa));
			if (rep != qu.Wa)
			{
				if (rep == qu.Ua && bc.CutPoints.Contains(qu.Ua)
					|| rep == qu.Va && bc.CutPoints.Contains(qu.Va))
					return false;

				if (rep >= 0)
					return true;
			}*/


			// Areas are separated by bridges
			// We might be able to optimize this but too scary
			int duve = lca.Distance(qu.Ue, qu.Ve);
			int duwe = lca.Distance(qu.Ue, qu.We);
			int dvwe = lca.Distance(qu.Ve, qu.We);
			if (duve != dvwe + duwe)
				return false;

			// Two vertices must be concident or we fail
			Debug.Assert(qu.Ve != qu.We || qu.Ue == qu.We);
			if (qu.Ue != qu.We)
				return false;

			// If we get here, then we are assured that the
			// U and W are in the same e-region, V may not be
			// If V is not use V's rep in the e-region for searching

			// TODO: We need to validate this -- 
			if (qu.Ua == qu.Wa || !bc.CutPoints.Contains(qu.Ua) && Adjacent(qu.Ua, qu.Wa))
				return true;

			if (Adjacent(qu.Ua, qu.Va) && Adjacent(qu.Ua, qu.Wa) && Adjacent(qu.Va, qu.Wa))
				return true;

			return false;
		}

		int GetRep(int u, int v, int w, Func<int, bool> adjacent)
		{
			var lo = lca.Depth[u] < lca.Depth[v] ? v : u;
			var rep = lca.Lca(lo, w);
			//int uvLca = lca.Lca(u, v);

			//if (lca.Depth[rep] < lca.Depth[uvLca])
			//	rep = uvLca;

			if (adjacent(rep))
			{
				int duv = lca.Distance(u, v);
				int dur = lca.Distance(u, rep);
				int dvr = lca.Distance(v, rep);
				if (duv == dur + dvr)
					return rep;
			}

			return -1;
		}

		bool Adjacent(int u, int v)
		{
			int ua = nodeConnected.Find(u);
			int va = nodeConnected.Find(v);

			if (ua == va)
				return true;

			int ue = edgeConnected.Find(u);
			int ve = edgeConnected.Find(v);
			if (ue != ve)
				return false;

			if (!bc.CutPoints.Contains(va) && !bc.CutPoints.Contains(ua))
				return false;

			if (graph3[ua].Count > graph3[va].Count)
				Swap(ref ua, ref va);

			if (graph3[ua].Count < AdjacencyThreshold)
			{
				foreach (var x in graph3[ua])
					if (x == va)
						return true;
				return false;
			}
			else
			{
				var neighbors = cutpointNeighbors[va];
				if (neighbors == null)
					neighbors = cutpointNeighbors[va] = new HashSet<int>(graph3[va]);
				return neighbors.Contains(ua);
			}

		}

		void GetPath(List<int> list, int u, int v)
		{
			int anc = lca.Lca(u, v);

			while (u != anc)
			{
				var p = lca.GetParent(u);
				list.Add(p); ;
				u = p;
			}

			list.Add(u);
			int sav = 0;

			while (v != anc)
			{
				var p = lca.GetParent(v);
				list.Add(p); ;
				v = p;
			}

			list.Reverse(sav, list.Count - sav);
		}


		long time = 1;
		long[] visited;
		int[] parent;
		//int[] depth;
		Queue<int> queue = new Queue<int>(100001);

		int[] banned;
		long[] bannedts;
		HashSet<int> banset = new HashSet<int>();

		public bool SearchDisjointPaths(Query qu)
		{
			return SearchDisjointPaths(qu.U, qu.V, qu.W);
		}

		public bool SearchDisjointPaths(int u, int v, int w)
		{
			if (connected.Find(u) != connected.Find(v)
				|| connected.Find(u) != connected.Find(w))
				return false;

			if (u == w || v == w)
				return true;

			time++;
			int found = BfsDisjointPaths(w, u, v);
			if (found < 0) return false;

			time++;
			for (var current = found; parent[current] >= 0; current = parent[current])
			{
				var p = parent[current];
				if (bannedts[p] == time) break; // Avoid cycles
				bannedts[p] = time;
				banned[p] = current;
			}

			var other = found == u ? v : u;
			visited[found] = time;

			int found2 = BfsDisjointPaths(w, other, other, found);
			return found2 >= 0;
		}


		int BfsDisjointPaths(int wTarget, int uTarget, int vTarget, int banVertex = -1)
		{
			// var group = edgeConnected.Find(wTarget);
			queue.Clear();
			queue.Enqueue(wTarget);
			parent[wTarget] = -1;
			visited[wTarget] = time;

			while (queue.Count > 0)
			{
				var u = queue.Dequeue();
				int ban = banVertex != -1 && bannedts[u] >= time ? banned[u] : -1;

				// foreach (var node in graph3[u])
				foreach (var node in graph[u])
				{
					if (node == vTarget || node == uTarget)
					{
						parent[node] = u;
						return node;
					}

					if (banVertex == node)
						continue;

					// if (visited[node] < time && edgeConnected.Find(node) == group && node != ban)
					if (visited[node] < time && node != ban)
					{
						visited[node] = time;
						parent[node] = u;
						queue.Enqueue(node);
					}
				}
			}

			return -1;
		}

		LowestCommonAncestor BuildLca()
		{
			time++;
			queue.Clear();

			var depth = new int[n];
			var seen = new BitArray(n);
			var seenE = new BitArray(n);

			for (int iroot = 0; iroot < n; iroot++)
			{
				int root = nodeConnected.Find(iroot);
				if (visited[root] != time)
				{
					parent[root] = -1;
					depth[root] = 0;
					visited[root] = time;
					queue.Enqueue(root);
					while (queue.Count > 0)
					{
						var u = queue.Dequeue();
						var ua = nodeConnected.Find(u);
						var ue = edgeConnected.Find(u);

						foreach (var v in graph3[u])
						{
							if (visited[v] >= time) continue;

							// Group edges areas together
							var ve = edgeConnected.Find(v);
							if (ue != ve)
							{
								if (seenE[ve]) continue;
								seenE[ve] = true;
							}

							// Group articulation areas together
							var va = nodeConnected.Find(v);
							if (ua != va)
							{
								if (seen[va]) continue;
								seen[va] = true;
							}

							visited[v] = time;
							parent[v] = u;
							depth[v] = depth[u] + 1;
							queue.Enqueue(v);
						}
					}
				}
			}

			return new LowestCommonAncestor(parent, depth);
		}


		int[] BuildCompareTable()
		{
			int[] compareTable = new int[n];
			int[] compareIndices = new int[n];
			for (int i = 0; i < n; i++)
				compareTable[i] = i;

			Sort(compareTable, (a, b) =>
			{
				int cmp;
				cmp = edgeConnected.Find(a).CompareTo(edgeConnected.Find(b));
				if (cmp != 0) return cmp;
				cmp = nodeConnected.Find(a).CompareTo(nodeConnected.Find(b));
				if (cmp != 0) return cmp;
				return a.CompareTo(b);
			});

			for (int i = 0; i < n; i++)
				compareIndices[compareTable[i]] = i;
			return compareIndices;
		}

		List<int>[] BuildGraphFromComponents(UnionFind components, bool rooted = false)
		{
			var g = new List<int>[n + 1];
			for (int i = 0; i <= n; i++)
				g[i] = new List<int>(i < n ? graph[i].Count : 0);

			for (int i = 0; i < n; i++)
			{
				int ii = components.Find(i);
				foreach (var j in graph[i])
				{
					int jj = components.Find(j);
					if (ii == jj) continue;
					g[ii].Add(jj);
					g[jj].Add(ii);
				}
			}

			if (rooted)
				for (int i = 0; i < n; i++)
				{
					if (i == connected.Find(i))
					{
						int ii = components.Find(i);
						g[n].Add(ii);
						g[ii].Add(n);
					}
				}

			PruneRedundantEdges(g);
			return g;
		}

		void PruneRedundantEdges(List<int>[] g)
		{
			for (int i = 0; i < g.Length; i++)
			{
				var neighbors = g[i];
				if (neighbors.Count < 2) continue;

#if DEBUG
				g[i] = new HashSet<int>(neighbors).ToList();
#else
			time++;
			int write = 0;
			for (int read = 0; read < neighbors.Count; read++)
			{
				var x = neighbors[read];
				if (visited[x] == time)
					continue;
				visited[x] = time;
				neighbors[write++] = x;
			}
			neighbors.RemoveRange(write, neighbors.Count - write);
#endif
			}
		}

		public class Query
		{
			public int Index;
			public int U;
			public int V;
			public int W;
			public int Ue;
			public int Ve;
			public int We;
			public int Ua;
			public int Va;
			public int Wa;
			public bool? Answer;
			public override string ToString()
			{
				return $"#{Index} - {U} {V} {W}";
			}
		}

		public void ReadData()
		{
			n = Ni();
			m = Ni();
			q = Ni();

			graph = new List<int>[n + 1];
			for (int i = 0; i <= n; i++)
				graph[i] = new List<int>();

			for (int i = 0; i < m; i++)
			{
				int u = Ni() - 1;
				int v = Ni() - 1;
				graph[u].Add(v);
				graph[v].Add(u);
			}

			queries = new List<Query>(q);
			for (int i = 0; i < q; i++)
				queries.Add(new Query { Index = i, U = Ni() - 1, V = Ni() - 1, W = Ni() - 1 });

		}

		public string DrawParents()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < graph.Length; i++)
			{
				int v = lca.GetParent(i);
				if (v >= 0)
					sb.AppendLine($"{v} {i}");
			}

			return sb.ToString();
		}

		public string DrawGraph()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < graph.Length; i++)
			{
				foreach (var v in graph[i])
				{
					if (v < i) continue;
					sb.AppendLine($"{i} {v}");
				}
			}

			return sb.ToString();
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


			public UnionFind(UnionFind f)
			{
				_count = f._count;
				_ds = (int[])f._ds.Clone();
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

				if (rx <= ry)
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

		public class BridgesAndCuts
		{
			#region Using

			List<int>[] _graph;
			public HashSet<int> CutPoints;
			public List<Bridge> Bridges;

			#endregion

			public BridgesAndCuts(List<int>[] graph)
			{
				_graph = graph;
				var builder = new Builder(_graph);
				CutPoints = builder.CutPoints;
				Bridges = builder.Bridges;
			}

			public struct Builder
			{
				int[] low;
				int[] num;
				List<int>[] _graph;
				int curnum;

				public HashSet<int> CutPoints;
				public List<Bridge> Bridges;

				public Builder(List<int>[] graph)
				{
					int n = graph.Length;
					_graph = graph;
					low = new int[n + 1];
					num = new int[4 * n + 1];

					CutPoints = new HashSet<int>();
					Bridges = new List<Bridge>();

					for (int i = 0; i < 4 * n; i++)
						num[i] = -1;

					curnum = 0;
					for (int i = 0; i < n; i++)
						if (num[i] == -1)
							Dfs(i);
				}

				void Dfs(int u, int p = -1)
				{
					low[u] = num[u] = curnum++;
					int cnt = 0;
					bool found = false;

					for (int i = 0; i < _graph[u].Count; i++)
					{
						int v = _graph[u][i];
						if (num[v] == -1)
						{
							Dfs(v, u);
							low[u] = Min(low[u], low[v]);
							cnt++;
							found = found || low[v] >= num[u];
							if (low[v] > num[u]) Bridges.Add(new Bridge(u, v));
						}
						else if (p != v) low[u] = Min(low[u], num[v]);
					}

					if (found && (p != -1 || cnt > 1))
						CutPoints.Add(u);
				}
			}

			long Combine(int x, int y)
			{
				if (x > y)
				{
					var tmp = x;
					x = y;
					y = tmp;
				}

				return ((long)x << 24) + y;
			}

			public UnionFind GetComponents(bool avoidBridges = true, bool avoidCuts = true)
			{
				int n = _graph.Length;
				var ds = new UnionFind(n);
				var hs = new HashSet<long>();

				if (avoidBridges)
				{
					foreach (var bridge in Bridges)
						hs.Add(Combine(bridge.X, bridge.Y));
				}

				for (int i = 0; i < n; i++)
				{
					if (avoidCuts && CutPoints.Contains(i)) continue;
					foreach (var e in _graph[i])
					{
						if (e < i
							|| avoidCuts && CutPoints.Contains(e)
							|| avoidBridges && hs.Contains(Combine(i, e))
							|| _graph[e].Count < 2)
							continue;
						ds.Union(i, e);
					}
				}

				return ds;
			}

			public class Bridge
			{
				public int X;
				public int Y;

				public Bridge(int x, int y)
				{
					X = x;
					Y = y;
				}
			}
		}

		public class TreeGraph
		{
			#region Variables

			public int[] Parents;
			public int[] Queue;
			public int[] Depths;
			public int[] Sizes;
			public IList<int>[] Graph;
			public int Root;
			public int TreeSize;
			public int Separator;

			bool sizesInited;

			#endregion

			#region Constructor

			public TreeGraph(IList<int>[] g, int root = -1, int avoid = -1)
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
					foreach (var child in g[cur])
					{
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
					foreach (int e in Graph[current])
						if (Parents[current] != e)
							Sizes[current] += Sizes[e];
				}
			}

			#endregion
		}

		public class LowestCommonAncestor
		{
			public int[] Depth;
			public int[,] Ancestors;

			public LowestCommonAncestor(TreeGraph builder) : this(builder.Parents, builder.Depths)
			{

			}

			public LowestCommonAncestor(int[] parents, int[] depth)
			{
				this.Depth = depth;
				int n = parents.Length;
				int m = NumberOfTrailingZeros(HighestOneBit(n - 1)) + 1;
				var ancestors = new int[n, m];

				for (int i = 0; i < n; i++)
					ancestors[i, 0] = parents[i];

				for (int j = 1; j < m; j++)
				{
					for (int i = 0; i < n; i++)
						ancestors[i, j] = ancestors[i, j - 1] == -1
							? -1
							: ancestors[ancestors[i, j - 1], j - 1];
				}

				this.Ancestors = ancestors;
			}

			public int Lca(int a, int b)
			{
				if (Depth[a] < Depth[b])
					b = Ancestor(b, Depth[b] - Depth[a]);
				else if (Depth[a] > Depth[b])
					a = Ancestor(a, Depth[a] - Depth[b]);

				if (a == b)
					return a;
				int sa = a, sb = b;
				for (int low = 0,
						high = Depth[a],
						t = HighestOneBit(high),
						k = NumberOfTrailingZeros(t);
					t > 0;
					t >>= 1, k--)
				{
					if ((low ^ high) >= t)
					{
						if (Ancestors[sa, k] != Ancestors[sb, k])
						{
							low |= t;
							sa = Ancestors[sa, k];
							sb = Ancestors[sb, k];
						}
						else
						{
							high = low | t - 1;
						}
					}
				}
				return Ancestors[sa, 0];
			}

			public int GetParent(int node)
			{
				return Ancestors[node, 0];
			}

			public int Ancestor(int a, int m)
			{
				for (int i = 0; m > 0 && a != -1; m >>= 1, i++)
				{
					if ((m & 1) == 1)
					{
						a = Ancestors[a, i];
					}
				}
				return a;
			}

			public int Distance(int a, int b)
			{
				int lca = Lca(a, b);
				if (lca < 0) return lca;
				return Depth[a] + Depth[b] - 2 * Depth[lca];
			}

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

			public static int NumberOfTrailingZeros(int v)
			{
				int lastBit = v & -v;
				return lastBit != 0 ? Log2(lastBit) : 32;
			}

		}

	}

	class CaideConstants
	{
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
			}
			return neg ? -number : number;
		}

		public static long Nl()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg) { c = Read(); }

			long number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint)d > 9) break;
				number = number * 10 + d;
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
}