using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Math;

namespace HackerRank.WeekOfCode31.SpanningTreeFraction
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

		List<Edge>[] edgesByB;

		int minb = int.MaxValue;
		int maxb = int.MinValue;

		void Run()
		{
			n = Ni();
			m = Ni();

			nodes = new List<Edge>[n];
			for (int i = 0; i < nodes.Length; i++)
				nodes[i] = new List<Edge>();

			edges = new List<Edge>(m);
			edgeMap = new Dictionary<long, Edge>();
			int[] bcount = new int[101];

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

			for ( int i=0; i<n; i++ )
				nodes[i].Sort();

			List<Edge> closestEdges = new List<Edge>();
			foreach (var e in edges)
			{
				bcount[e.B]++;
				minb = Min(minb, e.B);
				maxb = Max(maxb, e.B);

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

			edgesByB = new List<Edge>[101];
			for (int i = 0; i <= 100; i++)
				edgesByB[i] = new List<Edge>(bcount[i]);
			foreach (var e in edges)
				edgesByB[e.B].Add(e);
			for (int i = 0; i <= 100; i++)
				edgesByB[i].Sort();

			BuildSameEdges();
			ds = new DisjointSet(n);
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

		long initialA;
		long initialB;

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
			foreach (var c in cuts.Bridges)
			{
				Edge e;
				var code = Combine(c.X, c.Y);
				if (edgeMap.TryGetValue(code, out e))
				{
					e.Include = true;
					initialA += e.A;
					initialB += e.B;
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
		DisjointSet ds;

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
		}

		void Kruskal()
		{
			ds.Clear();

			long numerator = initialA;
			long denominator = initialB;

			int[] indexes = new int[101];
			for (int iter = 1; iter < n; iter++) // n-1 iterations
			{
				Edge best = null;
				for (int i = minb; i <= maxb; i++)
				{
					int j = indexes[i];
					if (j >= edgesByB[i].Count) continue;
					var e = edgesByB[i][j];
					if (ds.Find(e.U) == ds.Find(e.V))
					{
						indexes[i]++;
						i--;
						continue;
					}

					if (best == null)
					{
						best = e;
						continue;
					}

					var bestN = numerator + best.A;
					var bestD = denominator + best.B;
					var proposedN = numerator + e.A;
					var proposedD = denominator + e.B;
					long cmp = (bestN * proposedD) - (proposedN * bestD);
					if (cmp > 0 || cmp == 0 && iter != 1) continue;
					best = e;
				}
				if (best == null) break;

				ds.Union(best.U, best.V);
				indexes[best.B]++;
				AddEdge(best, ref numerator, ref denominator);
			}

			/*
			int maxStart = 0;
			while (maxStart + 1 < edges.Count &&
					(edges[maxStart].A * edges[maxStart + 1].B == edges[maxStart].B * edges[maxStart + 1].A))
				maxStart++;

			var edge = edges[maxStart];
			ds.Union(edge.U, edge.V);
			edge.Include = true;

			foreach (var e in edges)
			{
				if (ds.Find(e.U) == ds.Find(e.V)) continue;
				ds.Union(e.U, e.V);
				e.Include = true; ;
			}*/

			Report();
		}

		void FixUpDupes(ref long numerator, ref long denominator)
		{
			var dict = edgeMap;
			edgeMap.Clear();

			foreach (var e in edges)
			{
				if (!e.Include) continue;
				dict[e.Code] = e;
			}

			foreach (var e in edges)
			{
				Edge e2;
				if (e.Include || !dict.TryGetValue(e.Code, out e2)) continue;

				var n2 = numerator - e2.A + e.A;
				var d2 = denominator - e2.B + e.B;
				if (n2 * denominator > numerator * d2)
				{
					RemoveEdge(e2, ref numerator, ref denominator);
					AddEdge(e, ref numerator, ref denominator);
				}
			}
		}

		void RemoveAndRepeat(ref long numerator, ref long denominator)
		{
			for (int iter = 0; iter < 5; iter++)
			{
				long oldn = numerator;
				long oldd = denominator;

				ds.Clear();
				foreach (var e in edgeMap.Values.ToList())
				{
					var n2 = numerator - e.A;
					var d2 = denominator - e.B;
					if (numerator * d2 < n2 * denominator)
						RemoveEdge(e, ref numerator, ref denominator);
					else
						ds.Union(e.U, e.V);
				}

				foreach(var e in edges)
				{
					if (e.Include || ds.Find(e.U)==ds.Find(e.V)) continue;
					if (e.A * denominator > numerator * e.B)
						AddEdge(e, ref numerator, ref denominator);
				}

				if (oldn == numerator && oldd == denominator) break;
			}

			if (ds.Count != 1)
				// Pick improvement
				foreach (var e in edges)
				{
					if (e.Include || ds.Find(e.U) == ds.Find(e.V)) continue;
					if (e.A * denominator > numerator * e.B)
						AddEdge(e, ref numerator, ref denominator);
				}

			if (ds.Count!=1)
				// Pick any to connect
				foreach (var e in edges)
				{
					if (e.Include || ds.Find(e.U) == ds.Find(e.V)) continue;
					AddEdge(e, ref numerator, ref denominator);
				}
		}

		void FixUpProblems(ref long numerator, ref long denominator)
		{
			Edge best;
			long bestn;
			long bestd;
			long n = numerator;
			long d = denominator;

			if (n > 50000)
				edges.Sort((a, b) =>
				{
					int cmp = a.Include.CompareTo(b.Include);
					if (cmp != 0) return cmp;
					var an = n + a.A;
					var ad = d + a.B;
					var bn = n + b.A;
					var bd = d + b.B;
					cmp = -(an * bd).CompareTo(bn * ad);
					return cmp;
				});

			int iter = 0;
			long limit = 200000000 / n;
			Edge duplicate;
			foreach (var e in edges)
			{
				if (e.Include) continue;

				if (e.Duplicate
					&& edgeMap.TryGetValue(e.Code, out duplicate))
				{
					Debug.Assert(duplicate.Include);
					if (duplicate.Include)
					{
						long n3 = n - duplicate.A + e.A;
						long d3 = d - duplicate.B + e.B;
						if (n3 * d > n * d3)
						{
							RemoveEdge(duplicate, ref n, ref d);
							AddEdge(e, ref n, ref d);
						}
					}
					continue;
				}

				if (iter++ > limit) break;
				best = null;
				bestn = 0;
				bestd = 1;
				var pathFinder = new PathFinder(this, e.U, e.V, e2 =>
				{
					long n2 = n - e2.A;
					long d2 = d - e2.B;
					long cmp = 0;

					if (best == null
						|| (cmp = bestn * d2 - n2 * bestd) < 0)
					{
						best = e2;
						bestn = n2;
						bestd = d2;
					}
				});

				if (best != null)
				{
					long n3 = bestn + e.A;
					long d3 = bestd + e.B;
					if (n3 * d > n * d3)
					{
						RemoveEdge(best, ref n, ref d);
						AddEdge(e, ref n, ref d);
					}
				}
			}

			numerator = n;
			denominator = d;
		}

		public void AddEdge(Edge e, ref long n, ref long d)
		{
			e.Include = true;
			edgeMap[e.Code] = e;
			n += e.A;
			d += e.B;
		}

		public void RemoveEdge(Edge e, ref long n, ref long d)
		{
			e.Include = false;
			Edge prev;
			if (edgeMap.TryGetValue(e.Code, out prev) && prev == e)
				edgeMap.Remove(e.Code);
			n -= e.A;
			d -= e.B;
		}
		
		struct PathFinder
		{
			Solution sol;
			static BitArray visited;
			Action<Edge> action;

			public PathFinder(Solution sol,
				int from, int to,
				Action<Edge> action)
			{
				this.sol = sol;
				this.action = action;
				if (visited == null)
					visited = new BitArray(sol.n);
				else
					visited.SetAll(false);
				visited[to] = true;
				Dfs(from, -1);
			}

			bool Dfs(int u, int p, int maxDepth=5000)
			{
				if (visited[u])
					return true;
				visited[u] = true;
				if (maxDepth <= 0) return false;

				foreach (var e in sol.nodes[u])
				{
					if (!e.Include) continue;

					var c = e.U == u ? e.V : e.U;
					if (c == p) continue;

					if (Dfs(c, u, maxDepth-1))
					{
						action(e);
						return true;
					}
				}

				return false;
			}
		}

		[Conditional("DEBUG")]
		void Verify(long numerator, long denominator)
		{
			long numerator2 = initialA;
			long denominator2 = initialB;
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



		void Report()
		{
			long numerator = initialA;
			long denominator = initialB;
			int count = 0;
			foreach (var e in edges)
			{
				if (!e.Include) continue;
				numerator += e.A;
				denominator += e.B;
				count++;
				//Console.Error.WriteLine($"Added Edge {e.I}");
			}

			FixUpDupes(ref numerator, ref denominator);
			FixUpProblems(ref numerator, ref denominator);
			RemoveAndRepeat(ref numerator, ref denominator);

			var gcd = Gcd(numerator, denominator);
			numerator /= gcd;
			denominator /= gcd;

			Console.Error.WriteLine($"Length: {count}");
			Console.WriteLine($"{numerator}/{denominator}");
		}


		public static long Gcd(long a, long b)
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

			public override bool Equals(object obj)
			{
				return obj == this;
			}

			public override int GetHashCode()
			{
				return I;
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
			_oneBased = onesBased;
			Clear();
		}

		public void Clear()
		{
			int size = _ds.Length - 1;
			_components = size;

			for (int i = 0; i <= size; i++)
			{
				_ds[i] = i;
				_counts[i] = 1;
			}

			if (_oneBased)
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



