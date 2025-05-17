using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HackerRank.WeekOfCode31.NominatingGroupLeaders
{
	using static Math;
	using static Console;
	using static FastIO;

	public class Solution
	{
		#region Variables
		int n;
		int[] v;
		int g;
		Group[] groups;
		int[][] inverted;
		int sqrt;
		int[] updateLog;
		#endregion

		public static void Main()
		{
			InitIO();
			InitOutput();

			int t = Ni();
			for (int a0 = 0; a0 < t; a0++)
			{
				var sol = new Solution();
				sol.Run();
			}

			Flush();
		}

		void Run()
		{
			n = Ni();
			v = Ni(n);
			g = Ni();

			sqrt = Math.Min(Parameters.MinSqrt, (int)(Sqrt(n) + 1));

			groups = new Group[g];
			for (int i = 0; i < g; i++)
			{
				groups[i] = new Group()
				{
					Index = i,
					L = Ni(),
					R = Ni(),
					X = Ni(),
				};
			}

			SolveLargeCases();

			Solve();

			foreach (var group in groups)
				FastWrite(group.Answer ?? -1);

		}

		void SolveLargeCases()
		{
			var counts = new int[n];
			var indices = new int[n];
			foreach (var e in v)
				counts[e]++;

			inverted = new int[n][];
			var largeElements = new List<int>(sqrt);
			for (int i = 0; i < inverted.Length; i++)
			{
				if (counts[i] >= sqrt)
				{
					inverted[i] = new int[counts[i]];
					largeElements.Add(i);
				}
			}

			for (var i = 0; i < v.Length; i++)
			{
				var e = v[i];
				var ie = inverted[e];
				if (ie != null)
					ie[indices[e]++] = i;
			}

			foreach (var group in groups)
			{
				if (group.X >= sqrt)
				{
					group.Answer = -1;
					foreach (var e in largeElements)
					{
						var array = inverted[e];
						if (array.Length < group.X) continue;
						int lower = BinarySearch(array, group.L, 0, array.Length - 1);
						int upper = BinarySearch(array, group.R, 0, array.Length - 1, true);
						if (upper - lower == group.X)
						{
							group.Answer = e;
							break;
						}
					}
				}
			}
		}

		int[] counts;
		MinBag[] bags;

		void Solve()
		{
			counts = new int[n];
			updateLog = new int[n];

			var sqrtN = (int)Sqrt(n);
			var queriesSorted = groups.ToList();
			queriesSorted.RemoveAll(q => q.Answer != null);
			queriesSorted.Sort((x, y) =>
			{
				int cmp = (x.L / sqrtN).CompareTo(y.L / sqrtN);
				if (cmp != 0) return cmp;
				cmp = x.R.CompareTo(y.R);
				if (cmp != 0) return cmp;
				return x.L.CompareTo(y.L);
			});

			bags = new MinBag[sqrt];
			foreach (var q in queriesSorted)
			{
				var x = q.X;
				if (bags[x] == null)
					bags[x] = new MinBag(updateLog);
			}

			int mosLeft = 0;
			int mosRight = -1;

			foreach (var q in queriesSorted)
			{
				while (mosRight < q.R) AddDelta(v[++mosRight], +1);
				while (mosRight > q.R) AddDelta(v[mosRight--], -1);
				while (mosLeft > q.L) AddDelta(v[--mosLeft], +1);
				while (mosLeft < q.L) AddDelta(v[mosLeft++], -1);
				var x = q.X;
				q.Answer = bags[x].Count != 0 ? bags[x].Min() : -1;
			}
		}

		void AddDelta(int e, int delta)
		{
			var eCount = counts[e];

			if (eCount < bags.Length)
				bags[eCount]?.Remove(e);

			eCount += delta;

			if (eCount < bags.Length)
				bags[eCount]?.Insert(e);

			counts[e] = eCount;
		}


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

		public static int BinarySearch(int[] array, int value, int left, int right, bool upper = false)
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

		public class Group
		{
			public int Index;
			public int L;
			public int R;
			public int X;
			public int? Answer;
			public override string ToString()
			{
				return $"{Index} {L} {R} {X}";
			}
		}


		static Stream outputStream = Console.OpenStandardOutput();
		static byte[] scratch = new byte[256];
		static byte[] buffer = new byte[300000];
		static int index;

		public static void InitOutput()
		{

		}

		public void FastWrite(int number)
		{
			int count = 0;

			if (number < 0)
			{
				scratch[count++] = (byte)'-';
				number = -number;
			}

			int start = count;
			while (number > 0 || count == start)
			{
				scratch[count++] = (byte)('0' + number % 10);
				number /= 10;
			}

			int left = start;
			int right = count - 1;
			while (left < right)
			{
				byte tmp = scratch[left];
				scratch[left++] = scratch[right];
				scratch[right--] = tmp;
			}

			scratch[count++] = (byte)'\n';

			if (index + count + 1 > buffer.Length)
				Dump();

			for (int i = 0; i < count; i++)
				buffer[index++] = scratch[i];
		}


		static void Dump()
		{
			outputStream.Write(buffer, 0, index);
			index = 0;
		}

		static void Flush()
		{
			Dump();
			outputStream.Flush();
		}


	}


	public class MinBag
	{
		SegmentTree tree;

		public int Count;
		int[] updateLog;

		public MinBag(int[] updateLog)
		{
			tree = new SegmentTree(0);
			this.updateLog = updateLog;
		}

		public void Insert(int v)
		{
			int check = updateLog[v];
			if (check >= 0 && check < Count && tree[check] == v)
				return;

			tree.Ensure(v);
			check = Count++;
			tree[check] = v;
			updateLog[v] = check;
		}

		public void Remove(int v)
		{
			int check = updateLog[v];
			if (check >= 0 && check < Count && tree[check] == v)
			{
				updateLog[v] = -1;

				Count--;
				if (check < Count)
				{
					var rep = tree[Count];
					tree[check] = rep;
					updateLog[rep] = check;
				}
			}
		}

		public int Min()
		{
			return tree.QueryInclusive(0, Count - 1);
		}

	}

	public class SegmentTree
	{
		private int[] _tree;
		private static int[] empty = new int[0];

		public SegmentTree(int size)
		{
			_tree = CreateTree(size);
		}

		public int Length => _tree.Length >> 1;

		public void Ensure(int index)
		{
			if (index << 1 < _tree.Length) return;
			int newSize = Max(Parameters.InitialBagSize, Max(index + 1, _tree.Length));
			var oldTree = _tree;
			var oldTreeSize = oldTree.Length >> 1;
			_tree = CreateTree(newSize);
			Transfer(oldTree, oldTreeSize, oldTreeSize);
		}

		int[] CreateTree(int size)
		{
			if (size == 0) return empty;
			var tree = new int[size * 2];
			for (int i = 0; i < tree.Length; i++)
				tree[i] = int.MaxValue;
			return tree;
		}

		void Transfer(int[] array, int start, int count)
		{
			int size = _tree.Length >> 1;
			Array.Copy(array, start, _tree, size, count);
			for (int i = size - 1; i > 0; i--)
				_tree[i] = Math.Min(_tree[i << 1], _tree[i << 1 | 1]);
		}

		public int this[int index]
		{
			get
			{
				return _tree[index + (_tree.Length >> 1)];
			}
			set
			{
				int i = index + (_tree.Length >> 1);
				_tree[i] = value;
				var min = value;
				for (; i > 1; i >>= 1)
				{
					var newMin = Math.Min(_tree[i], _tree[i ^ 1]);
					if (_tree[i >> 1] == newMin) break;
					_tree[i >> 1] = newMin;
				}
			}
		}

		public int QueryInclusive(int left, int right)
		{
			int size = _tree.Length / 2;
			left += size;
			right += size;

			int result = int.MaxValue;
			for (; left <= right; left >>= 1, right >>= 1)
			{
				if (left % 2 == 1) result = Math.Min(result, _tree[left++]); // if parent is the left child, then parents have the sum
				if (right % 2 == 0) result = Math.Min(result, _tree[right--]); // if parent is the right child, then parents have the sum
			}
			return result;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (int t in _tree)
			{
				sb.Append(t);
				sb.Append(',');
			}
			return sb.ToString();
		}

	}

	public static class FastIO
	{
		static Stream _stream;
		static int _idx, _bytesRead;
		static byte[] _buffer;

		public static void InitIO(
			int bufferSize = 1 << 20,
			Stream input = null)
		{
			_stream = input ?? OpenStandardInput();
			_idx = _bytesRead = 0;
			_buffer = new byte[bufferSize];
		}


		static void ReadMore()
		{
			_idx = 0;
			_bytesRead = _stream.Read(_buffer, 0, _buffer.Length);
			if (_bytesRead <= 0) _buffer[0] = 32;
		}

		public static int Read()
		{
			if (_idx >= _bytesRead) ReadMore();
			return _buffer[_idx++];
		}

		public static int[] Ni(int n)
		{
			var list = new int[n];
			for (int i = 0; i < n; i++) list[i] = Ni();
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

	public static class Parameters
	{

		#region  Parameters
		public const int InitialBagSize = 16;
		public const int MinSqrt = 100;


#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif

		#endregion
	}

}
