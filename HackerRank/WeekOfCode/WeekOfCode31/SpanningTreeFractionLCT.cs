using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Math;

namespace HackerRank.WeekOfCode31.SpanningTreeFractionLCT
{
	using static FastIO;
	using static BridgesAndCuts;

	public class Solution
	{
		#region Variables

		List<Edge> edges;
		int n, m;
		List<Edge>[] nodes;

		#endregion

		public static void Main()
		{
			InitIO();
			var sol = new Solution();
			sol.Run();
		}

		void Run()
		{
			n = Ni();
			m = Ni();

			nodes = new List<Edge>[n];
			for (int i = 0; i < nodes.Length; i++)
				nodes[i] = new List<Edge>();

			edges = new List<Edge>(m);
			edgeMap = new Dictionary<long, Edge>();

			for (int i = 0; i < m; i++)
			{
				var e = new Edge
				{
					I = i + 1,
					U = Ni(),
					V = Ni(),
					A = Ni(),
					B = Ni(),
				};

				if (e.U == e.V) continue;
				if (e.U > e.V)
				{
					var tmp = e.U;
					e.U = e.V;
					e.V = tmp;
				}

				edges.Add(e);
				nodes[e.U].Add(e);
				nodes[e.V].Add(e);
			}

			m = edges.Count;

			PruneGraph();

			List<Edge> closestEdges = new List<Edge>();
			foreach (var e in edges)
			{
				// Compute score for edge
				closestEdges.Clear();
				closestEdges.AddRange(nodes[e.U].Take(3));
				closestEdges.AddRange(nodes[e.V].Take(3));
				Edge bestEdge = null;
				int bestn = 0;
				int bestd = 0;
				foreach (var ce in closestEdges)
				{
					if (ce.U == e.U && ce.V == e.V) continue;
					var scoren = e.A + ce.A;
					var scored = e.B + ce.B;
					if (bestEdge == null
						|| scoren * bestd > scored * bestn)
					{
						bestEdge = ce;
						bestn = scoren;
						bestd = scored;
					}
				}

				e.AScore = bestEdge != null ? bestn : e.A;
				e.BScore = bestEdge != null ? bestd : e.B;
			}


			BuildSameEdges();
			edges.Sort((a, b) =>
			{
				int cmp = ((long)b.AScore * a.BScore).CompareTo((long)a.AScore * b.BScore);
				if (cmp != 0) return cmp;
				cmp = a.AScore.CompareTo(b.AScore);
				if (cmp != 0) return cmp;
				return a.I.CompareTo(b.I);
			});
			Kruskal();
		}

		long _initialA;
		long _initialB;
		LinkCutTree[] _links;

		void PruneGraph()
		{
			int[][] adj = new int[n][];
			for (int i = 0; i < n; i++)
			{
				var neighbors = nodes[i];
				var neighbors2 = adj[i] = new int[neighbors.Count];
				for (int j = 0; j < neighbors.Count; j++)
				{
					var e = neighbors[j];
					neighbors2[j] = e.U == i ? e.V : e.U;
				}
			}

			var cuts = new BridgesAndCuts(adj);
			foreach(var c in cuts.Bridges)
			{
				Edge e;
				var code = Combine(c.X, c.Y);
				if (edgeMap.TryGetValue(code, out e))
				{
					e.Include = true;
					_initialA += e.A;
					_initialB += e.B;
				}
			}
			
			edges.RemoveAll(e => e.Include);
			foreach (var list in nodes)
				list?.RemoveAll(e => e.Include);

			int[] remap = new int[n];
			int index = 0;
			for (int i = 0; i < n; i++)
			{
				if (nodes[i].Count != 0)
				{
					nodes[index] = nodes[i];
					remap[i] = index++;
				}
			}

			for (int i = index; i < n; i++)
				nodes[i] = new List<Edge>();

			foreach (var e in edges)
			{
				e.U = remap[e.U];
				e.V = remap[e.V];
			}

			n = index;
		}

		Dictionary<long, Edge> edgeMap;

		void BuildSameEdges()
		{
			edgeMap.Clear();

			foreach (var e in edges)
			{
				Edge e2;
				if (edgeMap.TryGetValue(e.Code, out e2))
				{
					e.Duplicate = true;
					e2.Duplicate = true;
				}
				else
				{
					edgeMap[e.Code] = e;
				}
			}

			edgeMap.Clear();
			foreach (var e in edges)
			{
				if (e.Include)
					edgeMap[e.Code] = e;
			}
		}

		void Kruskal()
		{

			_links = new LinkCutTree[n];
			;
			for (var i = 0; i < _links.Length; i++)
				_links[i] = new LinkCutTree(null);

			edges.Sort();

			long numerator = _initialA;
			long denominator = _initialB;

			Edge duplicate;
			bool changed = true;
			for (int i = 0; i < 1 && changed; i++)
			{
				changed = false;
				foreach (var e in edges)
				{
					if (e.Include) continue;

					if (_links[e.U].Link(_links[e.V]))
					{
						AddEdge(e, ref numerator, ref denominator);
						changed = true;
						continue;
					}

					duplicate = null;
					if (e.Duplicate 
						&& edgeMap.TryGetValue(e.Code, out duplicate))
					{
						Debug.Assert(duplicate.Include);
						if (duplicate.Include)
						{
							long n3 = numerator - duplicate.A + e.A;
							long d3 = denominator - duplicate.B + e.B;
							if (n3 * denominator > numerator * d3)
							{
								RemoveEdge(duplicate, ref numerator, ref denominator);
								AddEdge(e, ref numerator, ref denominator);
								changed = true;
							}
						}
						continue;
					}

					var minEdge = _links[e.U].Query(_links[e.V]);
					if (minEdge != null)
					{
						long n3 = numerator - minEdge.A + e.A;
						long d3 = denominator - minEdge.B + e.B;

						if (n3 * denominator > numerator * d3)
						{
							_links[minEdge.U].Cut(_links[minEdge.V]);
							RemoveEdge(minEdge, ref numerator, ref denominator);
							AddEdge(e, ref numerator, ref denominator);
							_links[e.U].Link(_links[e.V]);
							changed = true;
						}
					}
				}
			}

			Verify(numerator, denominator);
			Report(numerator, denominator);
		}

		public void AddEdge(Edge e, ref long n, ref long d)
		{
			e.Include = true;
			edgeMap[e.Code] = e;
			if (_links[e.U].Value == null)
				_links[e.U].Value = e;
			else if (_links[e.V].Value == null)
				_links[e.V].Value = e;
			else
				Debug.Fail("Both links have edges");
			n += e.A;
			d += e.B;

		}

		public void RemoveEdge(Edge e, ref long n, ref long d)
		{
			e.Include = false;
			Edge prev;
			if (edgeMap.TryGetValue(e.Code, out prev) && prev == e)
				edgeMap.Remove(e.Code);
			if (_links[e.U].Value == e)
				_links[e.U].Value = null;
			else if (_links[e.V].Value == e)
				_links[e.V].Value = null;
			else
				Debug.Fail("Edge not assigned");

			n -= e.A;
			d -= e.B;
		}

		void Report(long numerator, long denominator)
		{
			var gcd = Gcd(numerator, denominator);
			numerator /= gcd;
			denominator /= gcd;
			Console.WriteLine($"{numerator}/{denominator}");
		}

		[Conditional("DEBUG")]
		void Verify(long numerator, long denominator)
		{
			long numerator2 = _initialA;
			long denominator2 = _initialB;
			int count2 = 0;
			var ds = new DisjointSet(n);
			foreach (var e in edges)
			{
				if (!e.Include) continue;
				numerator2 += e.A;
				denominator2 += e.B;
				ds.Union(e.U, e.V);
				count2++;
			}

			/*
			if (numerator != numerator2) throw new Exception();
            if (denominator != denominator2) throw new Exception();
            */

			// if (count2 != n - 1) throw new Exception();
			if (ds.Count != 1) throw new Exception();
		}

		static long Gcd(long a, long b)
		{
			while (b != 0)
			{
				a %= b;
				var tmp = a;
				a = b;
				b = tmp;
			}
			return a;
		}


		public class Edge : IComparable<Edge>
		{
			public int I;
			public int U;
			public int V;
			public int A;
			public int B;
			public int AScore;
			public int BScore;
			public bool Include;
			public bool Duplicate;
			public long Code => Combine(U, V);

			public int CompareTo(Edge b)
			{
				var a = this;
				int cmp = ((long)b.A * a.B).CompareTo((long)a.A * b.B);
				if (cmp != 0) return cmp;
				cmp = a.A.CompareTo(b.A);
				if (cmp != 0) return cmp;
				return a.I.CompareTo(b.I);

				/*var a = this;
				int cmp = a.B.CompareTo(b.B);
				if (cmp != 0) return cmp;
				cmp = b.A.CompareTo(a.A);
				if (cmp != 0) return cmp;
				return a.I.CompareTo(b.I);*/
			}

			public override string ToString()
			{
				return $"{U} {V} {A} {B}";
			}
		}

		public class LinkCutTree
		{
			Edge _value;
			Edge _subTreeValue;
			int _size;
			bool _revert;
			LinkCutTree _left;
			LinkCutTree _right;
			LinkCutTree _parent;

			public Edge Value
			{
				get
				{
					return _value;
				}

				set
				{
					MakeRoot();
					Access();
					_value = value;
					Update();
				}
			}

			internal LinkCutTree(Edge value)
			{
				_value = value;
				_subTreeValue = value;
				_size = 1;
			}

			bool IsRoot => _parent == null || (_parent._left != this && _parent._right != this);

			void Push()
			{
				if (_revert)
				{
					_revert = false;
					LinkCutTree t = _left;
					_left = _right;
					_right = t;
					if (_left != null)
						_left._revert = !_left._revert;
					if (_right != null)
						_right._revert = !_right._revert;
				}
			}

			static Edge QueryOperation(Edge leftValue, Edge rightValue)
			{
				if (leftValue == null) return rightValue;
				if (rightValue == null) return leftValue;

				if (leftValue.A * rightValue.B <= rightValue.A * leftValue.B)
					return leftValue;
				return rightValue;
			}

			void Update()
			{
				_subTreeValue = QueryOperation(QueryOperation(_left?._subTreeValue, Value), _right?._subTreeValue);
				_size = 1 + (_left?._size ?? 0) + (_right?._size ?? 0);
			}

			public void MakeRoot()
			{
				Access();
				_revert = !_revert;
			}

			public bool Link(LinkCutTree y)
			{
				if (Connected(y))
					return false;
				MakeRoot();
				_parent = y;
				return true;
			}

			public bool Cut(LinkCutTree y)
			{
				MakeRoot();
				y.Access();
				if (y._right != this || _left != null)
					return false;
				y._right._parent = null;
				y._right = null;
				return true;
			}

			public bool Connected(LinkCutTree y)
			{
				if (this == y)
					return true;
				Access();
				y.Access();
				return _parent != null;
			}

			internal LinkCutTree Access()
			{
				LinkCutTree last = null;
				for (LinkCutTree y = this; y != null; y = y._parent)
				{
					y.Splay();
					y._left = last;
					last = y;
				}
				Splay();
				return last;
			}

			void Rotate()
			{
				LinkCutTree p = _parent;
				LinkCutTree g = p._parent;
				bool isRootP = p.IsRoot;
				bool leftChildX = (this == p._left);

				// create 3 edges: (x.r(l),p), (p,x), (x,g)
				Connect((leftChildX ? _right : _left), p, leftChildX);
				Connect(p, this, !leftChildX);
				Connect(this, g, isRootP ? (bool?)null : p == g._left);
				p.Update();
			}

			void Splay()
			{
				while (!IsRoot)
				{
					LinkCutTree p = _parent;
					LinkCutTree g = p._parent;
					if (!p.IsRoot)
						g.Push();
					p.Push();
					Push();
					if (!p.IsRoot)
						((this == p._left) == (p == g._left) ? p : this).Rotate();
					Rotate();
				}
				Push();
				Update();
			}

			static void Connect(LinkCutTree linkCutTree, LinkCutTree p, bool? isLeftChild)
			{
				if (linkCutTree != null)
					linkCutTree._parent = p;
				if (isLeftChild != null)
				{
					if (isLeftChild == true)
						p._left = linkCutTree;
					else
						p._right = linkCutTree;
				}
			}

			public Edge Query(LinkCutTree to)
			{
				MakeRoot();
				to.Access();
				return to._subTreeValue;
			}
		}

	}

	public class DisjointSet
	{
		private readonly int[] _ds;
		private readonly int[] _counts;
		private int _components;
		private bool _oneBased;

		public int Count => _components;

		public DisjointSet(int size, bool onesBased = false)
		{
			_ds = new int[size + 1];
			_counts = new int[size + 1];
			_components = size;
			_oneBased = onesBased;

			for (int i = 0; i <= size; i++)
			{
				_ds[i] = i;
				_counts[i] = 1;
			}

			if (onesBased)
				_ds[0] = size;
			else
				_ds[size] = 0;
		}

		public int[] Array => _ds;

		public bool Union(int x, int y)
		{
			var rx = Find(x);
			var ry = Find(y);
			if (rx == ry) return false;

			if (_counts[ry] > _counts[rx])
			{
				_ds[rx] = ry;
				_counts[ry] += _counts[rx];
			}
			else
			{
				_ds[ry] = rx;
				_counts[rx] += _counts[ry];
			}
			_components--;
			return true;
		}

		public int Find(int x)
		{
			var root = _ds[x];
			return root == x
				? x
				: (_ds[x] = Find(root));
		}


		public int GetCount(int x)
		{
			var root = Find(x);
			return _counts[root];
		}

		public IEnumerable<int> Roots()
		{
			int start = _oneBased ? 1 : 0;
			int limit = _oneBased ? _ds.Length : _ds.Length - 1;
			for (int i = start; i < limit; i++)
			{
				if (_ds[i] == i)
					yield return i;
			}
		}

		public IEnumerable<List<int>> Components()
		{
			var comp = new Dictionary<int, List<int>>();
			foreach (var c in Roots())
				comp[c] = new List<int>(GetCount(c));

			int start = _oneBased ? 1 : 0;
			int limit = _oneBased ? _ds.Length : _ds.Length - 1;

			for (int i = start; i < limit; i++)
				comp[Find(i)].Add(i);
			return comp.Values;
		}
	}

	public static class FastIO
	{
		static System.IO.Stream stream;
		static int idx, bytesRead;
		static byte[] buffer;


		public static void InitIO(
			int stringCapacity = 16,
			int bufferSize = 1 << 20,
			System.IO.Stream input = null)
		{
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

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint)c - 33 >= (127 - 33));
			return c;
		}

	}


	public class BridgesAndCuts
	{
		#region Using
		IList<int>[] _graph;
		public HashSet<int> CutPoints;
		public List<Bridge> Bridges;
		#endregion

		public BridgesAndCuts(IList<int>[] graph)
		{
			_graph = graph;
			var builder = new Builder(_graph);
			CutPoints = builder.CutPoints;
			Bridges = builder.Bridges;
		}
		public static long Combine(int x, int y)
		{
			if (x > y)
			{
				var tmp = x;
				x = y;
				y = tmp;
			}

			return ((long)x << 32) + y;
		}


		public struct Builder
		{
			int[] low;
			int[] num;
			IList<int>[] _graph;
			int curnum;

			public HashSet<int> CutPoints;
			public List<Bridge> Bridges;

			public Builder(IList<int>[] graph)
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

		
		public DisjointSet GetComponents(bool avoidBridges = true, bool avoidCuts = true)
		{
			int n = _graph.Length;
			var ds = new DisjointSet(n);
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
					if (e < i || avoidCuts && CutPoints.Contains(e) || avoidBridges && hs.Contains(Combine(i, e)))
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

}

