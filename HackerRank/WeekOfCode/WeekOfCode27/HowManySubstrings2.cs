
namespace HackerRank.CodeSprint.WeekOfCode26.HowManySubstrings2
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Linq;
	using static FastIO;
	using static BitTools;
	using static System.Math;

	public class Solution
	{
		public static void Main()
		{
			InitIO();
			int n = Ni(), Q = Ni();
			char[] s = Nc(n);
			var qs = new Query[Q];
			for (int i = 0; i < Q; i++)
			{
				qs[i] = new Query { Left = Ni(), Right = Ni(), Index = i };
			}
			Array.Sort(qs, (a,b)=> a.Right - b.Right);

			var sa = new SuffixAutomaton(s);
			var nodes = sa.GetNodes();
			int[] from = new int[nodes.Length - 1];
			int[] to = new int[nodes.Length - 1];
			int p = 0;
			foreach (var node in nodes)
			{
				if (node.Index >= 1)
				{
					from[p] = node.Link.Index;
					to[p] = node.Index;
					p++;
				}
			}
			Debug.Assert(p == nodes.Length - 1);
			int[][] g = PackU(nodes.Length, from, to);
			int[][] pars = Parents3(g, 0);
			int[] par = pars[0], ord = pars[1], dep = pars[2];
			var hld = new HeavyLightDecomposition(g, par, ord, dep);
			int m = hld.Cluspath.Length;
			var sts = new SegmentTreeOverwrite[m];
			for (int i = 0; i < m; i++)
				sts[i] = new SegmentTreeOverwrite(hld.Cluspath[i].Length);

			int[] bs = new int[n];
			int qp = 0;
			int np = 0;
			var ft0 = new long[n + 3];
			var ft1 = new long[n + 3];
			var ans = new long[Q];
			for (int i = 0; i < n; i++)
			{
				while (!(nodes[np].Len == i + 1 && nodes[np].Original == null))
					np++;
				bs[i] = np;
				//			tr("base", base[i]);

				// 5 3 1 0
				// 5 3 1 0
				// 8 6 3 1 0 ?
				// aaba

				// delete
				int cur = 0;
				int ppos = 0;
				while (true)
				{
					int last = sts[hld.Clus[cur]].Get(hld.Clusiind[cur]);
					if (last == -1)
						break;
					int lca = hld.Lca(bs[last], bs[i]);
					// delete from lca to cur
					//			nodes[cur].len, nodes[lca].len;
					int inf = last - nodes[lca].Len + 1;
					int sup = last - ppos + 1;
					AddFenwick(ft0, 0, -(sup - inf));
					AddFenwick(ft0, sup + 1, +(sup - inf));
					AddFenwick(ft0, inf + 1, -(inf + 1));
					AddFenwick(ft0, sup + 1, +inf + 1);
					AddFenwick(ft1, inf + 1, 1);
					AddFenwick(ft1, sup + 1, -1);
					ppos = nodes[lca].Len;
					Debug.Assert(dep[bs[i]] - dep[lca] - 1 >= 0);
					cur = hld.Ancestor(bs[i], dep[bs[i]] - dep[lca] - 1);
				}

				// paint
				int cx = hld.Clus[bs[i]]; // cluster
				int ind = hld.Clusiind[bs[i]]; // pos in cluster
				while (true)
				{
					sts[cx].Update(0, ind + 1, i);
					int con = par[hld.Cluspath[cx][0]];
					if (con == -1)
						break;
					ind = hld.Clusiind[con];
					cx = hld.Clus[con];
				}

				AddFenwick(ft0, 0, i + 1 + 1);
				AddFenwick(ft0, i + 1 + 1, -(i + 1 + 1));
				AddFenwick(ft1, 0, -1);
				AddFenwick(ft1, i + 1 + 1, 1);

				while (qp < Q && qs[qp].Right <= i)
				{
					ans[qs[qp].Index] = SumRangeFenwick(ft0, ft1, qs[qp].Left);
					qp++;
				}
			}

			foreach (long an in ans)
				Console.WriteLine(an);
		}


		class Query
		{
			public int Index;
			public int Left;
			public int Right;
		}

		static long SumFenwick(long[] ft, int i)
		{
			long sum = 0;
			for (i++; i > 0; i -= i & -i)
			{
				sum += ft[i];
			}
			return sum;
		}

		static void AddFenwick(long[] ft, int i, long v)
		{
			if (v == 0)
			{
				return;
			}
			int n = ft.Length;
			for (i++; i < n; i += i & -i)
			{
				ft[i] += v;
			}
		}

		static int FirstGeFenwick(long[] ft, long v)
		{
			int i = 0, n = ft.Length;
			for (int b = HighestOneBit(n); b != 0; b >>= 1)
			{
				if ((i | b) < n && ft[i | b] < v)
				{
					i |= b;
					v -= ft[i];
				}
			}
			return i;
		}

		public static long[] RestoreFenwick(long[] ft)
		{
			int n = ft.Length - 1;
			long[] ret = new long[n];
			for (int i = 0; i < n; i++)
				ret[i] = SumFenwick(ft, i);

			for (int i = n - 1; i >= 1; i--)
				ret[i] -= ret[i - 1];

			return ret;
		}

		public static int FindGFenwick(long[] ft, long v)
		{
			int i = 0;
			int n = ft.Length;
			for (int b = HighestOneBit(n); b != 0 && i < n; b >>= 1)
			{
				if (i + b < n)
				{
					int t = i + b;
					if (v >= ft[t])
					{
						i = t;
						v -= ft[t];
					}
				}
			}
			return v != 0 ? -(i + 1) : i - 1;
		}

		public static long[] BuildFenwick(long[] a)
		{
			int n = a.Length;
			long[] ft = new long[n + 1];
			Array.Copy(a, 0, ft, 1, n);
			for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
			{
				for (int i = k; i <= n; i += k)
				{
					ft[i] += ft[i - h];
				}
			}
			return ft;
		}

		public static void AddRangeFenwick(long[] ft0, long[] ft1, int i, long v)
		{
			AddFenwick(ft1, i + 1, -v);
			AddFenwick(ft1, 0, v);
			AddFenwick(ft0, i + 1, v * (i + 1));
		}

		public static void AddRangeFenwick(long[] ft0, long[] ft1, int a, int b, long v)
		{
			if (a <= b)
			{
				AddFenwick(ft1, b + 1, -v);
				AddFenwick(ft0, b + 1, v * (b + 1));
				AddFenwick(ft1, a, v);
				AddFenwick(ft0, a, -v * a);
			}
		}

		public static long SumRangeFenwick(long[] ft0, long[] ft1, int i)
		{
			return SumFenwick(ft1, i) * (i + 1) + SumFenwick(ft0, i);
		}

		public class SegmentTreeOverwrite
		{
			public int M, H, N;
			public int[] Cover;
			public int I = int.MaxValue;

			public SegmentTreeOverwrite(int len)
			{
				N = len;
				M = HighestOneBit(Math.Max(N - 1, 1)) << 2;
				H = (int) ((uint) M >> 1);
				Cover = new int[M];
				for (int i = 0; i < Cover.Length; i++)
					Cover[i] = I;

				for (int i = 0; i < N; i++)
				{
					Cover[H + i] = -1;
				}
				for (int i = H - 1; i >= 1; i--)
				{
					Propagate(i);
				}
			}

			internal void Propagate(int i)
			{
			}

			public void Update(int l, int r, int v)
			{
				Update(l, r, v, 0, H, 1);
			}

			internal void Update(int l, int r, int v, int cl, int cr, int cur)
			{
				if (l <= cl && cr <= r)
				{
					Cover[cur] = v;
					Propagate(cur);
				}
				else
				{
					int mid = (int) ((uint) cl + cr >> 1);
					if (Cover[cur] != I)
					{
						// back-propagate
						Cover[2 * cur] = Cover[2 * cur + 1] = Cover[cur];
						Cover[cur] = I;
						Propagate(2 * cur);
						Propagate(2 * cur + 1);
					}
					if (cl < r && l < mid)
					{
						Update(l, r, v, cl, mid, 2 * cur);
					}
					if (mid < r && l < cr)
					{
						Update(l, r, v, mid, cr, 2 * cur + 1);
					}
					Propagate(cur);
				}
			}

			public int Get(int x)
			{
				int val = I;
				for (int i = H + x; i >= 1; i = (int) ((uint) i >> 1))
				{
					if (Cover[i] != I)
					{
						val = Cover[i];
					}
				}
				return val;
			}
		}


		public class HeavyLightDecomposition
		{
			public int[] Clus;
			public int[][] Cluspath;
			public int[] Clusiind;
			public int[] Par, Dep;

			public HeavyLightDecomposition(int[][] g, int[] par, int[] ord, int[] dep)
			{
				Init(g, par, ord, dep);
			}

			public void Init(int[][] g, int[] par, int[] ord, int[] dep)
			{
				Clus = DecomposeToHeavyLight(g, par, ord);
				Cluspath = ClusPaths(Clus, ord);
				Clusiind = ClusIInd(Cluspath, g.Length);
				Par = par;
				Dep = dep;
			}

			public static int[] DecomposeToHeavyLight(int[][] g, int[] par, int[] ord)
			{
				int n = g.Length;
				int[] size = new int[n];
				for (int i = 0; i < size.Length; i++)
					size[i] = 1;

				for (int i = n - 1; i > 0; i--)
				{
					size[par[ord[i]]] += size[ord[i]];
				}

				int[] clus = new int[n];
				for (int i = 0; i < clus.Length; i++)
					clus[i] = -1;


				int p = 0;
				for (int i = 0; i < n; i++)
				{
					int u = ord[i];
					if (clus[u] == -1)
					{
						clus[u] = p++;
					}
					// centroid path (not heavy path)
					int argmax = -1;
					foreach (int v in g[u])
					{
						if (par[u] != v && (argmax == -1 || size[v] > size[argmax]))
							argmax = v;
					}
					if (argmax != -1)
					{
						clus[argmax] = clus[u];
					}
				}
				return clus;
			}

			public static int[][] ClusPaths(int[] clus, int[] ord)
			{
				int n = clus.Length;
				int[] rp = new int[n];
				int sup = 0;
				for (int i = 0; i < n; i++)
				{
					rp[clus[i]]++;
					sup = Math.Max(sup, clus[i]);
				}
				sup++;

				int[][] row = new int[sup][];
				for (int i = 0; i < sup; i++)
				{
					row[i] = new int[rp[i]];
				}

				for (int i = n - 1; i >= 0; i--)
				{
					row[clus[ord[i]]][--rp[clus[ord[i]]]] = ord[i];
				}
				return row;
			}

			public static int[] ClusIInd(int[][] clusPath, int n)
			{
				int[] iind = new int[n];
				foreach (int[] path in clusPath)
				{
					for (int i = 0; i < path.Length; i++)
					{
						iind[path[i]] = i;
					}
				}
				return iind;
			}

			public int Lca(int x, int y)
			{
				int rx = Cluspath[Clus[x]][0];
				int ry = Cluspath[Clus[y]][0];
				while (Clus[x] != Clus[y])
				{
					if (Dep[rx] > Dep[ry])
					{
						x = Par[rx];
						rx = Cluspath[Clus[x]][0];
					}
					else
					{
						y = Par[ry];
						ry = Cluspath[Clus[y]][0];
					}
				}
				return Clusiind[x] > Clusiind[y] ? y : x;
			}

			public int Ancestor(int x, int v)
			{
				while (x != -1)
				{
					if (v <= Clusiind[x])
					{
						return Cluspath[Clus[x]][Clusiind[x] - v];
					}
					v -= Clusiind[x] + 1;
					x = Par[Cluspath[Clus[x]][0]];
				}
				return x;
			}
		}


		public static int Lca2(int a, int b, int[][] spar, int[] depth)
		{
			if (depth[a] < depth[b])
			{
				b = Ancestor(b, depth[b] - depth[a], spar);
			}
			else if (depth[a] > depth[b])
			{
				a = Ancestor(a, depth[a] - depth[b], spar);
			}

			if (a == b)
			{
				return a;
			}
			int sa = a, sb = b;
			for (int low = 0, high = depth[a], t = HighestOneBit(high), k = NumberOfTrailingZeros(t);
				t > 0;
				t = (int) ((uint) t >> 1), k--)
			{
				if ((low ^ high) >= t)
				{
					if (spar[k][sa] != spar[k][sb])
					{
						low |= t;
						sa = spar[k][sa];
						sb = spar[k][sb];
					}
					else
					{
						high = low | t - 1;
					}
				}
			}
			return spar[0][sa];
		}

		public static int Ancestor(int a, int m, int[][] spar)
		{
			for (int i = 0; m > 0 && a != -1; m = (int) ((uint) m >> 1), i++)
			{
				if ((m & 1) == 1)
				{
					a = spar[i][a];
				}
			}
			return a;
		}

		public static int[,] LogstepParents(int[] par)
		{
			int n = par.Length;
			int m = BitTools.NumberOfTrailingZeros(BitTools.HighestOneBit(n - 1)) + 1;
			int[,] pars = new int[m, n];

			for (int i = 0; i < par.Length; i++)
				pars[0, i] = par[i];

			for (int j = 1; j < m; j++)
				for (int i = 0; i < n; i++)
					pars[j,i] = pars[j - 1,i] == -1 ? -1 : pars[j - 1,pars[j - 1,i]];

			return pars;
		}


		public static int[][] Parents3(int[][] g, int root)
		{
			int n = g.Length;
			int[] par = new int[n];
			for (int i = 0; i < par.Length; i++)
				par[ i] = -1;

			int[] depth = new int[n];
			depth[0] = 0;

			int[] q = new int[n];
			q[0] = root;
			for (int p = 0, r = 1; p < r; p++)
			{
				int cur = q[p];
				foreach (int nex in g[cur])
				{
					if (par[cur] != nex)
					{
						q[r++] = nex;
						par[nex] = cur;
						depth[nex] = depth[cur] + 1;
					}
				}
			}
			return new[] {par, q, depth};
		}


		internal static int[][] PackU(int n, int[] from, int[] to)
		{
			int[][] g = new int[n][];
			int[] p = new int[n];
			foreach (int f in from)
			{
				p[f]++;
			}
			foreach (int t in to)
			{
				p[t]++;
			}
			for (int i = 0; i < n; i++)
			{
				g[i] = new int[p[i]];
			}
			for (int i = 0; i < from.Length; i++)
			{
				g[from[i]][--p[from[i]]] = to[i];
				g[to[i]][--p[to[i]]] = from[i];
			}
			return g;
		}
	}

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

		/// <summary>
		/// More carefully sorted list of nodes that orders suffix links properly
		/// </summary>
		/// <returns></returns>
		public Node[] SortTopologically()
		{
			int[] indeg = new int[NodeCount];
			var nodes = GetNodes();
			for (int i = 0; i < NodeCount; i++)
			{
				Node cur = nodes[i];
				for (int j = 0; j < cur.NextCount; j++)
					indeg[cur.Next[j].Index]++;
			}

			var sorted = new Node[NodeCount];
			sorted[0] = Start;
			int p = 1;
			for (int i = 0; i < NodeCount; i++)
			{
				Node cur = sorted[i];
				for (int j = 0; j < cur.NextCount; j++)
				{
					if (--indeg[cur.Next[j].Index] == 0)
						sorted[p++] = cur.Next[j];
				}
			}

			_nodes = sorted;
			UpdateNodeIndices();
			return sorted;
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
			public int Len;
			public int Index;
			int KeyMask;
			public Node Link;
			public Node Original;
			public Node[] Next;

			public Node()
			{
				Next = Array.Empty<Node>();
			}

			public int FirstOccurrence => Original != null ? Original.Len : this.Len;

			public Node this[char ch]
			{
				get
				{
					if ((KeyMask << ~ch) < 0)
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
						Array.Resize(ref Next, Max(2, NextCount * 2));
					if (NextCount > left)
						Array.Copy(Next, left, Next, left + 1, NextCount - left);
					NextCount++;
					Next[left] = value;
					KeyMask |= 1 << ch;
				}
			}

			public bool IsCloned => Original != null;

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

			/// <summary>
			/// Clones an node
			/// </summary>
			/// <returns></returns>
			public Node Clone()
			{
				var node = (Node)MemberwiseClone();
				node.Original = Original ?? this;
				node.Next = (Node[])node.Next.Clone();
				return node;
			}

		}
	}

	public static class FastIO
	{
		static Stream stream;
		static int idx, bytesRead;
		static byte[] buffer;
		const int MonoBufferSize = 4096;


		public static void InitIO(
			Stream input = null)
		{
			stream = input ?? Console.OpenStandardInput();
			idx = bytesRead = 0;
			buffer = new byte[MonoBufferSize];
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

		public static char[] Nc(int n)
		{
			var list = new char[n];
			for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
			return list;
		}

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint)c - 33 >= (127 - 33));
			return c;
		}
	}




	public static class BitTools
	{
		public static int HighestOneBit(int n)
		{
			return n!=0 ? 1 << Log2(n) : 0;
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
