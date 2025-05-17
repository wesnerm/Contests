using System;

namespace HackerRank.WeekOfCode33.PathMatching
{
	// https://www.hackerrank.com/contests/w33/challenges/path-matching

	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;
	using static System.Array;
	using static System.Math;
	using static Library;

	class Solution
	{

		int n;
		int q;
		char[] s;
		string p;
		List<int>[] graph;
		long[] answers;
		Query[] queries;
		TreeGraph tree;
		HeavyLightDecomposition2 hld;
		Kmp kmp;
		ChainData[] chains;
		char[] buffer;

		public void solve()
		{
			ReadData();
			// TestData();

			//tree = new TreeGraph(graph);
			kmp = new Kmp(p);
			hld = new HeavyLightDecomposition2(graph, 0);
			tree = hld.Tree;
			buffer = new char[n];

			SetupHld();

			answers = new long[q];

			foreach (var qq in queries)
			{
				int u = qq.U;
				int v = qq.V;
				int lca = hld.Lca(u, v);

				int dist = tree.Depths[u] + tree.Depths[v] - 2 * tree.Depths[lca];

				long ans = dist >= 4000
					? SolveHld(u, v, lca)
					: BruteForce(u, v, lca);

				answers[qq.Index] = ans;
			}

			for (int qq = 0; qq < q; qq++)
				WriteLine(answers[qq]);

#if DEBUG
			Verify();
#endif
		}


		void Verify()
		{
			for (int u = 0; u < n; u++)
				for (int v = 0; v < n; v++)
				{
					int lca = hld.Lca(u, v);

					int dist = tree.Depths[u] + tree.Depths[v] - 2 * tree.Depths[lca];
					long ans1 = SolveHld(u, v, lca);
					long ans2 = BruteForce(u, v, lca);

					Debug.Assert(ans1 == ans2);
				}
		}


		List<Path> path = new List<Path>(100);


		public void SetupHld()
		{

			chains = new ChainData[hld.Paths.Length];
			for (int i = 0; i < chains.Length; i++)
			{
				var path = hld.Paths[i];

				for (int j = 0; j < path.Length; j++)
				{
					var u = path[j];
					buffer[j] = s[u];
				}

				// Handle reverse direction
				var reverseCounts = new int[path.Length];
				foreach (var offset in kmp.Instances(buffer, 0, path.Length))
					reverseCounts[offset]++;

				int sum = 0;
				for (int k = 0; k < path.Length; k++)
					reverseCounts[k] = sum = sum + reverseCounts[k];

				// Handle forward
				Reverse(buffer, 0, path.Length);
				var counts = new int[path.Length];
				foreach (var offset in kmp.Instances(buffer, 0, path.Length))
					counts[offset]++;

				sum = 0;
				for (int k = 0; k < path.Length; k++)
					counts[k] = sum = sum + counts[k];


				chains[i] = new ChainData
				{
					Counts = counts,
					ReverseCounts = reverseCounts,
				};
			}

		}

		public long SolveHld(int u, int v, int lca)
		{
			long ans = 0;

			BuildPath(path, u, v, lca);
			int bufferSize = 0;

			foreach (var step in path)
			{
				// Handle core
				int top = step.Top;
				int topIndex = hld.Positions[top];
				int bottom = step.Node;
				int bottomIndex = hld.Positions[bottom];
				bool up = step.Up;
				int chain = hld.Chains[top];
				var chainpath = hld.Paths[chain];
				var data = chains[chain];

				// Add the block count

				int len = bottomIndex - topIndex + 1;

				//if (len >= p.Length)
				//{
				int[] c = up ? data.Counts : data.ReverseCounts;
				int left = up ? chainpath.Length - 1 - bottomIndex : topIndex;
				int right = up ? chainpath.Length - 1 - topIndex : bottomIndex;
				int lo = left - 1;
				int hi = Max(lo, right - p.Length + 1);
				int clo = lo >= 0 ? c[lo] : 0;
				int chi = hi >= 0 ? c[hi] : 0;
				int matches = chi - clo;
				ans += matches;
				//}

				// Add the edge counts
				int matches2;
				int chked = Max(0, len - p.Length + 1);
				int nchk = len - chked;
				int examine = nchk + p.Length - 1;
				if (up)
				{
					int start = hld.Paths[chain][bottomIndex - chked];
					matches2 = BruteForce(start, v, lca, examine);
				}
				else
				{
					int start = hld.Paths[chain][topIndex + chked];
					matches2 = BruteForce(start, v, start, examine);
				}

				ans += matches2;
			}

			return ans;
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

		public int BruteForce(int u, int v, int lca, int max = int.MaxValue)
		{
			if (max < p.Length) return 0;

			int count = 0;
			while (u != lca && count < max)
			{
				buffer[count++] = s[u];
				u = tree.Parents[u];
			}

			if (count < max)
			{
				buffer[count++] = s[lca];

				if (max < int.MaxValue)
				{
					int dist = tree.Depths[v] - tree.Depths[lca];
					int adjust = dist - (max - count);
					if (adjust > 0)
						v = hld.Ancestor(v, adjust);
				}

				if (count < max)
				{
					int sav = count;
					while (v != lca && count < max)
					{
						buffer[count++] = s[v];
						v = tree.Parents[v];
					}
					Reverse(buffer, sav, count - sav);
				}
			}
			if (max < p.Length) return 0;
			return kmp.CountInstances(buffer, 0, count);
		}

		public class ChainData
		{
			public int[] ReverseCounts;
			public int[] Counts;
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


		public void TestData()
		{

		}

		public void ReadData()
		{
			n = Ni();
			q = Ni();
			s = Nc(n);
			p = Ns();

			graph = new List<int>[n];

			for (int i = 0; i < n; i++)
				graph[i] = new List<int>();


			for (int i = 0; i + 1 < n; i++)
			{
				var u = Ni() - 1;
				var v = Ni() - 1;
				graph[u].Add(v);
				graph[v].Add(u);
			}

			queries = new Query[q];
			for (int i = 0; i < q; i++)
				queries[i] = new Query { Index = i, U = Ni() - 1, V = Ni() - 1 };
		}


		public class Query
		{
			public int U;
			public int V;
			public int Index;
			public override string ToString()
			{
				return $"#{Index} ({U + 1},{V + 1}) U={U} V={V}";
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

	#region Heavy Light
	public class HeavyLightDecomposition2
	{
		public int[] Chains;
		public int[] Positions;
		public int[][] Paths;

		public TreeGraph Tree;
		public List<int>[] Graph;

		public HeavyLightDecomposition2(List<int>[] graph, int root)
		{
			this.Graph = graph;
			Tree = new TreeGraph(graph, root);
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
				foreach (int v in Graph[u])
				{
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

	public class HeavyLightDecomposition
	{
		public readonly List<Chain> Chains = new List<Chain>();
		public readonly Chain[] Chain;
		public readonly int[] ChainIndex;
		public readonly TreeGraph tg;
		public readonly IList<int>[] Graph;
		public int[] Paths;

		public HeavyLightDecomposition(TreeGraph tg)
		{
			this.tg = tg;
			tg.InitSizes();
			Graph = tg.Graph;
			int size = Graph.Length;
			Chain = new Chain[size + 1];
			ChainIndex = new int[size + 1];
			Dfs(tg.Root);

			int pos = 0;
			foreach (var chain in Chains)
			{
				chain.Index = pos;
				pos += chain.Size;
			}
		}

		void Dfs(int treeParam)
		{
			var chain = new Chain { Index = Chains.Count, Head = treeParam };
			var parent = tg.Parents[treeParam];
			if (parent >= 0) chain.Parent = Chain[parent];
			Chains.Add(chain);

			var sizes = tg.Sizes;
			for (int tree = treeParam, special = -1; tree >= 0; tree = special)
			{
				Chain[tree] = chain;
				ChainIndex[tree] = chain.Size++;

				foreach (var child in Graph[tree])
				{
					if (special < 0 || sizes[special] < sizes[child])
						special = child;
				}

				foreach (var child in Graph[tree])
				{
					if (child != special)
						Dfs(child);
				}
			}
		}

		public int GetPosition(int node) => Chain[node].Index + ChainIndex[node];

		public void QueryUp(int d, int p,
			Action<int, int> action)
		{
			var pChain = Chain[p];
			while (d != p)
			{
				var chain = Chain[d];
				if (chain == pChain)
				{
					// Both d and p are in the same chain, so we need to query from d to p
					action(p, d);
					break;
				}
				action(chain.Head, d);

				// Move to parent of the head of the chain 
				d = tg.Parents[chain.Head];
			}
		}


		public int Lca(int x, int y)
		{
			int rx = Chain[x].Head;
			int ry = Chain[y].Head;

			var depth = tg.Depths;
			var parent = tg.Parents;
			while (Chain[x] != Chain[y])
			{
				if (depth[rx] > depth[ry])
				{
					x = parent[rx];
					rx = Chain[x].Head;
				}
				else
				{
					y = parent[ry];
					ry = Chain[y].Head;
				}
			}
			return ChainIndex[x] > ChainIndex[y] ? y : x;
		}

		public void InitPaths()
		{
			if (Paths != null)
				return;

			int n = Graph.Length;
			Paths = new int[n];
			for (int i = 0; i < n; i++)
				Paths[ChainIndex[i] + Chain[i].Index] = i;
		}

		public int Ancestor(int x, int v)
		{
			var parent = tg.Parents;
			while (x != -1)
			{
				if (v <= ChainIndex[x])
					return Paths[GetPosition(x) - v];
				v -= ChainIndex[x] + 1;
				x = parent[Chain[x].Head];
			}
			return x;
		}

	}

	public class Chain
	{
		public Chain Parent;
		public int Head;
		public int Index;
		public int Size;
	}
	#endregion

	public class Kmp
	{
		public readonly int[] Lps;
		public readonly string Pattern;

		public Kmp(string pattern)
		{
			Pattern = pattern;
			var m = pattern.Length;
			Lps = new int[m + 1];

			Lps[0] = -1;
			for (int i = 1, j = -1; i <= pattern.Length; i++)
			{
				while (j >= 0 && pattern[i - 1] != pattern[j])
					j = Lps[j];
				Lps[i] = ++j;
			}
		}

		public IEnumerable<int> Instances(string text, int i = 0)
		{
			var m = Pattern.Length;
			int j = 0;
			while (i < text.Length)
			{
				while (j >= 0 && text[i] != Pattern[j]) j = Lps[j];
				i++;
				j++;
				if (j == m)
				{
					yield return i - j;
					j = Lps[j];
				}
			}
		}

		public IEnumerable<int> Instances(char[] text, int start, int limit)
		{
			var m = Pattern.Length;
			int j = 0;
			int i = start;
			while (i < limit)
			{
				while (j >= 0 && text[i] != Pattern[j]) j = Lps[j];
				i++;
				j++;
				if (j == m)
				{
					yield return i - j;
					j = Lps[j];
				}
			}
		}

		public int CountInstances(char[] text, int start, int limit)
		{
			var m = Pattern.Length;
			int j = 0;
			int i = start;
			int count = 0;
			while (i < limit)
			{
				while (j >= 0 && text[i] != Pattern[j]) j = Lps[j];
				i++;
				j++;
				if (j == m)
				{
					count++;
					j = Lps[j];
				}
			}
			return count;
		}

	}
	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}

	static partial class 
		Library
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



