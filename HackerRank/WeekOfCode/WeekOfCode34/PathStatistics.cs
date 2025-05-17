namespace HackerRank.WeekOfCode34.PathStatistics
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using static System.Array;
	using static System.Math;
	using static Library;

	class Solution
	{
		const int Threshold = 10;

		private int n;
		private int q;
		private List<int>[] graph;
		private int[] w;
		private TreeGraph treeGraph;
		private long[] answers;
		private Query[] queries;
		private EulerTour tour;
		private LowestCommonAncestor Lca;

		private int offset = 1;

		public void ReadData()
		{
			n = Ni();
			q = Ni();

			w = new int[n + 1 - offset];
			for (int i = 1; i <= n; i++)
				w[i - offset] = Ni();

			graph = new List<int>[n];
			for (int i = 0; i < graph.Length; i++)
				graph[i] = new List<int>();


			for (int i = 1; i < n; i++)
			{
				int u = Ni() - offset;
				int v = Ni() - offset;
				graph[u].Add(v);
				graph[v].Add(u);
			}

			answers = new long[q];
			queries = new Query[q];

			for (int i = 0; i < q; i++)
			{
				var qq = queries[i] = new Query
				{
					I = i,
					X = Ni() - offset,
					Y = Ni() - offset,
					K = Ni()
				};

				if (qq.X > qq.Y)
					Swap(ref qq.X, ref qq.Y);
			}
		}


		public void solve()
		{
			ReadData();
			// TestData();

			tour = new EulerTour(graph, 1 - offset);
			treeGraph = new TreeGraph(graph, 1 - offset);
			Lca = new LowestCommonAncestor(treeGraph);
			buffer = new List<int>(n);

			// BruteForceApproach();
			MosAlgorithm();

			foreach (var ans in answers)
				WriteLine(ans);

		}

		#region Optimizations

		private Dictionary<int, int> coords;
		private int[] ids;
		private int[] maxFreqs;
		private int[] w2;
		static int MaxFreq;
		public static int MaxItems;

		void CompressCoordinates()
		{
			coords = new Dictionary<int, int>
			{
				[0] = 0
			};

			MaxFreq = 0;
			foreach (var c in w)
			{
				int f;
				coords.TryGetValue(c, out f);
				coords[c] = ++f;
				MaxFreq = Max(f, MaxFreq);
			}
			MaxFreq++;

			ids = coords.Keys.ToArray();
			Sort(ids);
			MaxItems = ids.Length;

			maxFreqs = new int[ids.Length];
			for (int i = 0; i < ids.Length; i++)
			{
				var id = ids[i];
				maxFreqs[i] = coords[id];
				coords[id] = i;
			}

			w2 = new int[w.Length];
			for (int i = 0; i < w.Length; i++)
				w2[i] = coords[w[i]];
		}


		private int[] itemFreq;
		private FenwickTree freqTable;
		private Collection[] freqCollection;

		#endregion


		private int[] tourFreq;

		void MosAlgorithm()
		{

			Tasks = new List<Task>(n);

			CompressCoordinates();

			itemFreq = new int[ids.Length];
			freqTable = new FenwickTree(MaxFreq);
			freqCollection = new Collection[MaxFreq];

			for (int i = 0; i < freqCollection.Length; i++)
				freqCollection[i] = new Collection();

			tourFreq = new int[tour.Trace.Length];
			pathValues.Clear();

			foreach (var query in queries)
			{
				var x = query.X;
				var y = query.Y;
				var k = query.K;

				var lca = Lca.Lca(x, y);

				if (lca == y)
					Swap(ref x, ref y);

				if (lca == x)
				{
					AddTask(tour.Begin[x], tour.Begin[y], task =>
					{
						//Nop(query, lca, x, y);
						answers[query.I] = Index(k);
					});
				}
				else
				{
					if (tour.Begin[x] > tour.Begin[y])
						Swap(ref x, ref y);

					AddTask(tour.End[x], tour.Begin[y], task =>
					{
						//Nop(query, lca, x, y);
						Change(tour.Begin[lca], 1);
						answers[query.I] = Index(k);
						Change(tour.Begin[lca], -1);
					});
				}
			}

			Execute();
		}

		public List<Task> Tasks;

		public Task AddTask(int start, int end, Action<Task> action)
		{
			if (start > end)
			{
				var tmp = start;
				start = end;
				end = tmp;
			}

			var task = new Task
			{
				Start = start,
				End = end,
				Action = action
			};

			Tasks.Add(task);
			return task;
		}


		public void Execute()
		{
			int max = Tasks.Max(t => t.End);
			int min = Tasks.Min(t => t.Start);

			int s = min;
			int e = s - 1;

			int n = max - s + 1;
			int sqrt = (int) Ceiling(Sqrt(n));

			Tasks.Sort((x, y) => x.Start / sqrt == y.Start / sqrt
				? x.End.CompareTo(y.End)
				: x.Start.CompareTo(y.Start));

			foreach (var task in Tasks)
			{
				while (e < task.End) Change(++e, 1);
				while (e > task.End) Change(e--, -1);
				while (s < task.Start) Change(s++, -1);
				while (s > task.Start) Change(--s, 1);
				task.Action(task);
			}

			Tasks.Clear();
		}

		public class Task
		{
			public int Start;
			public int End;
			public Action<Task> Action;
			public object Tag;

			public override string ToString()
			{
				return $"{Tag} [{Start},{End}]";
			}
		}


		public int Index(int k)
		{
			var oldk = k;
			// f is the frequency we should look at
			var total = freqTable.SumInclusive(MaxFreq - 1);
			k = (int) total - k;
			var f = freqTable.GetIndexGreater(k);
			var coll = freqCollection[f];
			int sum = (int) freqTable.SumInclusive(f - 1);
			int k2 = k - sum;
			int c = coll.Index(k2);
			return ids[c];
		}


		public void Change(int x, int d)
		{
			var node = tour.Trace[x];
			var c = w2[node];
			int df = 0;

			// Remove
			if (tourFreq[node] == 1)
				df = -1;

			tourFreq[node] += d;

			// Add
			if (tourFreq[node] == 1)
				df = 1;

			if (df != 0)
			{
				var f = itemFreq[c];
				var newF = f + df;

				if (f > 0)
				{
					freqTable.Add(f, -1);
					freqCollection[f].Remove(c);
				}

				itemFreq[c] = newF;

				if (newF > 0)
				{
					freqTable.Add(newF, 1);
					freqCollection[newF].Add(c);
				}
			}
		}

		#region Brute Force

		private List<int> buffer;
		private Dictionary<int, int> pathValues = new Dictionary<int, int>();

		void BruteForceApproach()
		{
			Query prev = null;
			for (var iq = 0; iq < queries.Length; iq++)
			{
				var qq = queries[iq];
				int u = qq.X;
				int v = qq.Y;
				int k = qq.K;

				long ans;

				if (prev != null && prev.X == u && prev.Y == v && k - 1 < buffer.Count)
				{
					answers[qq.I] = buffer[k - 1];
					prev = qq;
					continue;
				}
				prev = qq;

				ans = BruteForce(u, v, k);

				answers[qq.I] = ans;
			}

		}


		public void Change(int k)
		{
			if (pathValues.ContainsKey(k) == false) pathValues[k] = 1;
			else pathValues[k]++;
		}

		int ComparePathValues(int a, int b)
		{
			int cmp = -pathValues[a].CompareTo(pathValues[b]);
			if (cmp != 0) return cmp;
			return b - a;
		}

		public int BruteForce(int u, int v, int k, int max = int.MaxValue)
		{
			int lca = Lca.Lca(u, v);
			InitBuffer();
			FillPathValues(u, v, lca, max);
			FinishBuffer();
			return buffer[k - 1];
		}

		void InitBuffer()
		{
			pathValues.Clear();
			buffer.Clear();
		}

		void FinishBuffer()
		{
			buffer.AddRange(pathValues.Keys);
			buffer.Sort(ComparePathValues);
		}

		void FillPathValues(int u, int v, int lca, int max = int.MaxValue)
		{
			int count = 0;
			while (u != lca && count < max)
			{

				Change(w[u]);
				u = treeGraph.Parents[u];
			}

			if (count < max)
			{
				Change(w[lca]);

				if (max < int.MaxValue)
				{
					int dist = treeGraph.Depths[v] - treeGraph.Depths[lca];
					int adjust = dist - (max - count);
					if (adjust > 0)
						v = Lca.Ancestor(v, adjust);
				}

				if (count < max)
				{
					int sav = count;
					while (v != lca && count < max)
					{
						Change(w[v]);
						v = treeGraph.Parents[v];
					}
				}
			}
		}

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

		public class Collection
		{
			private HashSet<int> items;
			private FenwickTree bit;
			private int[] sortedArray;
			private int sortedCount;

			public Collection()
			{
				items = new HashSet<int>();
				sortedArray = new int[Threshold];
			}

			public void Add(int item)
			{
				if (bit != null)
					bit.Add(item, 1);
				else
				{
					items.Add(item);
					if (items.Count > Threshold)
					{
						bit = new FenwickTree(MaxItems);
						foreach (var v in items)
							bit.Add(v, 1);
					}
					sortedCount = 0;
				}
			}

			public void Remove(int item)
			{
				if (bit != null)
					bit.Add(item, -1);
				else
				{
					items.Remove(item);
					sortedCount = 0;
				}
			}

			public int Index(int k)
			{
				int c;
				if (bit != null)
				{
					c = bit.GetIndexGreater(k);
					return c;
				}

				if (sortedCount == 0)
				{
					items.CopyTo(sortedArray);
					sortedCount = items.Count;
					InsertionSort(sortedArray, sortedCount);
				}

				return sortedArray[k];
			}


			void InsertionSort(int[] arr, int n)
			{
				for (int i = 1; i < n; i++)
				{
					int j, key = arr[i];
					for (j = i - 1; j >= 0 && arr[j] > key; j--)
						arr[j + 1] = arr[j];
					arr[j + 1] = key;
				}
			}

			public override string ToString()
			{
				return string.Join(",", items);
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

			for (int i = 0; i < n; i++)
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

		#endregion


	}

	public class EulerTour
	{
		public readonly int[] Trace;
		public readonly int[] Begin;
		public readonly int[] End;
		public readonly int[] Depth;

		public EulerTour(IList<int>[] g, int root)
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

	public class LowestCommonAncestor
	{
		public int[] Depth;
		public int[,] Ancestors;

		public LowestCommonAncestor(TreeGraph builder) : this(builder.Parents, builder.Depths)
		{

		}

		public LowestCommonAncestor(int[] parents, int[] depth)
		{
			Depth = depth;
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

			Ancestors = ancestors;
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
			if ((uint) value >= (1U << 12))
			{
				log = 12;
				value = (int) ((uint) value >> 12);
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
			if ((uint) value >= (1U << 12))
			{
				log = 12;
				value = (int) ((uint) value >> 12);
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

	public class FenwickTree
	{
		public readonly long[] Array;

		/*public Fenwick(int[] a) : this(a.Length)
		{
			for (int i = 0; i < a.Length; i++)
				Add(i, a[i]);
		}*/

		public FenwickTree(long[] a) : this(a.Length)
		{
			int n = a.Length;
			Copy(a, 0, Array, 1, n);
			for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
			{
				for (int i = k; i <= n; i += k)
					Array[i] += Array[i - h];
			}
		}

		public FenwickTree(long size)
		{
			Array = new long[size + 1];
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
			for (i++; i < Array.Length; i += (i & -i))
				Array[i] += val;
		}

		// Sum from [0 ... i]
		public long SumInclusive(int i)
		{
			long sum = 0;
			for (i++; i > 0; i -= (i & -i))
				sum += Array[i];
			return sum;
		}

		public long SumInclusive(int i, int j)
		{
			return SumInclusive(j) - SumInclusive(i - 1);
		}


		public long this[int index]
		{
			get { return SumInclusive(index) - SumInclusive(index - 1); }
			set { Add(index, value - this[index]); }
		}

		// get largest value with cumulative sum less than or equal to x;
		// for smallest, pass x-1 and add 1 to result
		public int GetIndexGreaterEqual(long x)
		{
			int i = 0, n = Array.Length - 1;
			for (int bit = BitTools.HighestOneBit(n); bit != 0; bit >>= 1)
			{
				int t = i | bit;
				if (t <= n && Array[t] < x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return i;
		}

		// get largest value with cumulative sum less than or equal to x;
		// for smallest, pass x-1 and add 1 to result
		public int GetIndexGreater(long x)
		{
			int i = 0, n = Array.Length - 1;
			for (int bit = BitTools.HighestOneBit(n); bit != 0; bit >>= 1)
			{
				int t = i | bit;
				if (t <= n && Array[t] <= x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return i;
		}

		public int FindGFenwick(long x)
		{
			int i = 0;
			int n = Array.Length;
			for (int b = BitTools.HighestOneBit(n); b != 0 && i < n; b >>= 1)
			{
				int t = i + b;
				if (t < n && Array[t] <= x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return x != 0 ? -(i + 1) : i - 1;
		}

		public long[] GetArray()
		{
			int n = Array.Length - 1;
			long[] ret = new long[n];
			for (int i = 0; i < n; i++)
				ret[i] = SumInclusive(i);

			for (int i = n - 1; i >= 1; i--)
				ret[i] -= ret[i - 1];
			return ret;
		}

		public override string ToString()
		{
			return string.Join(",", GetArray());
		}

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
			if ((uint) value >= (1U << 12))
			{
				log = 12;
				value = (int) ((uint) value >> 12);
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
			if (neg)
			{
				c = Read();
			}

			int number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint) d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static long Nl()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg)
			{
				c = Read();
			}

			long number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint) d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static char[] Nc(int n)
		{
			var list = new char[n];
			for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char) c;
			return list;
		}

		public static byte[] Nb(int n)
		{
			var list = new byte[n];
			for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte) c;
			return list;
		}

		public static string Ns()
		{
			var c = SkipSpaces();
			builder.Clear();
			while (true)
			{
				if ((uint) c - 33 >= (127 - 33)) break;
				builder.Append((char) c);
				c = Read();
			}
			return builder.ToString();
		}

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint) c - 33 >= (127 - 33));
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
			ulong number = (ulong) signedNumber;
			if (signedNumber < 0)
			{
				Write('-');
				number = (ulong) (-signedNumber);
			}

			Reserve(20 + 1); // 20 digits + 1 extra
			int left = outputIndex;
			do
			{
				outputBuffer[outputIndex++] = (byte) ('0' + number % 10);
				number /= 10;
			} while (number > 0);

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
				outputBuffer[outputIndex++] = (byte) s[i];
		}

		public static void Write(char c)
		{
			Reserve(1);
			outputBuffer[outputIndex++] = (byte) c;
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