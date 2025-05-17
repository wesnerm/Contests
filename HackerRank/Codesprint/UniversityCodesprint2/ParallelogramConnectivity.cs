

using System.Collections;

namespace HackerRank.UniversityCodesprint2.ParallelogramConnectivity
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using static System.Math;
	using static System.Console;
	using static HackerRankUtils;

	public class Solution
	{
		#region Variables

		static List<Query> queries;
		static MatrixSum matrixSum;
		static int[,] board;

		#endregion

		public static void Main()
		{
			var input = ReadLine().Split();

			n = int.Parse(input[0]);
			m = int.Parse(input[1]);
			board = new int[n, m];

			bool swap = false;
			if (m > n)
				Swap(ref m, ref n);

			for (var i = 0; i < n; i++)
			{
				var line = ReadLine();
				if (swap)
					for (var j = 0; j < line.Length; j++)
						board[j, i] = (line[j] == 'W') ? 1 : 0;
				else
					for (var j = 0; j < line.Length; j++)
						board[i, j] = (line[j] == 'W') ? 1 : 0;
			}

			Setup();

			var q = int.Parse(ReadLine());
			queries = new List<Query>(q);
			matrixSum = new MatrixSum(board);
			for (var i = 0; i < q; i++)
			{
				input = ReadLine().Split();
				var query = new Query
				{
					X1 = int.Parse(input[0]) - 1,
					Y1 = int.Parse(input[1]) - 1,
					X2 = int.Parse(input[2]) - 1,
					Y2 = int.Parse(input[3]) - 1
				};
				if (swap)
				{
					Swap(ref query.X1, ref query.Y1);
					Swap(ref query.X2, ref query.Y2);
				}
				queries.Add(query);
			}

			queries.Sort((a, b) => -a.X1.CompareTo(a.X2));

			foreach (var query in queries)
			{
				var x1 = query.X1;
				var x2 = query.X2;
				var y1 = query.Y1;
				var y2 = query.Y2;
				var dx = x2 - x1 + 1;
				var dy = y2 - y1 + 1;

				var sum = matrixSum.GetSumInclusive(x1, y1, x2, y2);
				if (sum == 0 || sum == dx * dy) query.Answer = 1;
				if (sum == 1 || sum == dx * dy - 1) query.Answer = 2;



				var components = Connect(query.X1, query.Y1, query.X2, query.Y2);
				query.Answer = components;
			}


			foreach (var query in queries)
				WriteLine(query.Answer);
		}


		static int m, n;

		static readonly int[,] dir2 =
		{
			{1, -1}, {1, 0}, {0, 1},
		};



		static QuadTree qt;
		static DisjointSet ds;
		static void Setup()
		{
			ds = new DisjointSet(n*m);
			qt = BuildTree(ds, 0, 0, n - 1, m - 1);
		}

		public class QuadTree
		{
			public int X1;
			public int X2;
			public int Y1;
			public int Y2;
			public int HalfComponents;
			public int Components;
			public QuadTree TL;
			public QuadTree TR;
			public QuadTree BL;
			public QuadTree BR;
		}

		static QuadTree BuildTree(DisjointSet set, int x1, int y1, int x2, int y2)
		{
			if (x1 == x2 && y1 == y2 || y1 > y2 || x1 > y2) return null;
			int midx = (x2 + y1) / 2;
			int midy = (y2 + y1) / 2;
			int dx = x2 - x1 + 1;
			int dy = y2 - x1 + 1;
			int elems = dx * dy;
			var qt = new QuadTree
			{
				X1 = x1,
				X2 = x2,
				Y1 = y1,
				Y2 = y2,
				TL = BuildTree(set, x1, y1, midx, midy),
				TR = BuildTree(set, x1 + 1, midy + 1, midx + 1, y2),
				BL = BuildTree(set, midx + 1, y1, x2, y2),
				BR = BuildTree(set, midx + 1, midy + 1, x2, y2),
			};


			int beforeCount = set.Count;
			ConnectDS(set, midx, y1, midx + 1, y2);
			int decrease = set.Count - beforeCount;
			qt.HalfComponents = elems - decrease;

			ConnectDS(set, x1, midy, x2, midy + 1);
			decrease = set.Count - beforeCount;
			qt.Components = elems - decrease;
			return qt;
		}

		/*
		public int CountComponents(QuadTree qt, ref DisjointSetPersistent set, int x1, int y1, int x2, int y2)
		{
			if (qt == null)
				return 0;

			if (x1 > qt.X2 || x2 < qt.X1 || y1 > qt.Y2 || y2 < qt.Y2) return 0;
			if (x1 <= qt.X1 && x2 >= qt.X2 && y1 <= qt.Y1 && y2 >= qt.Y2) return qt.Components;

			int comp = 0;
			comp += CountComponents(qt.TL, ref set, x1, y1, x2, y2);
			comp += CountComponents(qt.TR, ref set, x1, y1, x2, y2);
			comp += CountComponents(qt.BL, ref set, x1, y1, x2, y2);
			comp += CountComponents(qt.BR, ref set, x1, y1, x2, y2);
			return comp;
		}


		static DisjointSetPersistent ConnectDS(DisjointSetPersistent set, int x1, int y1, int x2, int y2)
		{
			if (y1 > y2)
				return set;

			for (var x = x1; x <= x2; x++)
				for (var y = y1; y <= y2; y++)
				{
					var color = Color(x, y);
					var crd = Coord(x, y);

					for (var i = 0; i < 3; i++)
					{
						var nx = x + dir2[i, 0];
						var ny = y + dir2[i, 1];
						if (nx < x || (uint)(nx - x1) >= m) continue;

						if (ny < y && nx <= x || (uint)(ny - y1) >= m) continue;

						var c = Color(nx, ny);
						if (c != color) continue;
						set = set.Union((nx - x1) * m + (ny - y1), (x - x1) * m + (y - y1));
					}
				}

			return set;
		}
		*/

		static void ConnectDS(DisjointSet set, int x1, int y1, int x2, int y2)
		{
			if (y1 > y2)
				return;

			for (var x = x1; x <= x2; x++)
				for (var y = y1; y <= y2; y++)
				{
					var color = Color(x, y);
					var crd = Coord(x, y);

					for (var i = 0; i < 3; i++)
					{
						var nx = x + dir2[i, 0];
						var ny = y + dir2[i, 1];
						if (nx < x || (uint)(nx - x1) >= m) continue;

						if (ny < y && nx <= x || (uint)(ny - y1) >= m) continue;

						var c = Color(nx, ny);
						if (c != color) continue;
						set.Union((nx - x1) * m + (ny - y1), (x - x1) * m + (y - y1));
					}
				}
		}

		static int Connect(int x1, int y1, int x2, int y2)
		{
			return ConnectDS(x1, y1, x2, y2).Count;
		}

		static DisjointSet ConnectDS(int x1, int y1, int x2, int y2)
		{
			var dx = x2 - x1 + 1;
			var dy = y2 - y1 + 1;
			var disjointSet = new DisjointSet(dx * dy);
			for (var x = x1; x <= x2; x++)
				for (var y = y1; y <= y2; y++)
				{
					var color = Color(x, y);
					var crd = Coord(x, y);

					for (var i = 0; i < 3; i++)
					{
						var nx = x + dir2[i, 0];
						var ny = y + dir2[i, 1];
						if (nx < x || (uint)(nx - x1) >= dx) continue;

						if (ny < y && nx <= x || (uint)(ny - y1) >= dy) continue;

						var c = Color(nx, ny);
						if (c != color) continue;
						disjointSet.Union((nx - x1) * dy + (ny - y1), (x - x1) * dy + (y - y1));
					}
				}

			return disjointSet;
		}



		public static int Coord(int x, int y)
		{
			if ((uint)x >= n || (uint)y >= m) return -1;
			return x * m + y;
		}

		public static int Color(int x, int y)
		{
			return (uint)x < n && (uint)y < m ? board[x, y] : -1;
		}


		public class Query
		{
			public int Answer;
			public int Index;
			public int X1;
			public int X2;
			public int Y1;
			public int Y2;

			public override string ToString()
			{
				return $"#{Index} ({X1},{Y1},{X2},{Y2}) {Answer}";
			}
		}


#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

	public class DisjointSet
	{
		readonly int[] _counts;
		readonly int[] _ds;
		readonly bool _oneBased;

		public DisjointSet(int size, bool onesBased = false)
		{
			_ds = new int[size + 1];
			_counts = new int[size + 1];
			Count = size;
			_oneBased = onesBased;

			for (var i = 0; i <= size; i++)
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

		public int Count { get; set; }

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
			Count--;
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

		public IEnumerable<int> Components()
		{
			for (var i = 0; i < _ds.Length; i++)
				if (_ds[i] == i)
					yield return i;
		}

		public IEnumerable<List<int>> GetComponents()
		{
			var comp = new Dictionary<int, List<int>>();
			foreach (var c in Components())
				comp[c] = new List<int>(GetCount(c));

			var start = _oneBased ? 1 : 0;
			var limit = _oneBased ? _ds.Length : _ds.Length - 1;

			for (var i = start; i < limit; i++)
				comp[Find(i)].Add(i);
			return comp.Values;
		}
	}


	public class MatrixSum
	{
		readonly int[,] _matrix;

		public MatrixSum(int[,] matrix, bool inplace = false)
		{
			var rows = matrix.GetLength(0);
			var cols = matrix.GetLength(1);

			if (!inplace)
				matrix = (int[,])matrix.Clone();

			_matrix = matrix;

			for (var i = 1; i < rows; i++)
				for (var j = 0; j < cols; j++)
					matrix[i, j] += matrix[i - 1, j];

			for (var i = 0; i < rows; i++)
				for (var j = 1; j < cols; j++)
					matrix[i, j] += matrix[i, j - 1];
		}

		public int GetSum(int x, int y, int dx, int dy)
		{
			return GetSumInclusive(x, y, x + dx - 1, y + dy - 1);
		}


		public int GetSumInclusive(int x1, int y1, int x2, int y2)
		{
			var result = _matrix[x2, y2];

			if (x1 > 0)
				result -= _matrix[x1 - 1, y2];

			if (y1 > 0)
				result -= _matrix[x2, y1 - 1];

			if (x1 > 0 && y1 > 0)
				result += _matrix[x1 - 1, y1 - 1];

			return result;
		}
	}


	public class PArray<T> : Persistent<T[]>
	where T : IEquatable<T>
	{
		#region Constructor

		public PArray()
			: this(Array.Empty<T>())
		{
		}

		public PArray(T[] array)
			: base(array)
		{
		}

		private PArray(Change change, PArray<T> array)
			: base(change, array)
		{
		}

		#endregion

		#region Properties

		public int Length => Data.Length;

		public T this[int index] => Data[index];

		public void Resize(int count)
		{
			T[] arr = Data;
			if (count >= arr.Length)
				Array.Resize(ref arr, count);
		}

		public void Ensure(int n)
		{
			int length = Length;
			if (n < length) return;
			int capacity = length * 2;
			if (capacity < n) capacity = n;
			Resize(capacity + 4);
		}

		public PArray<T> Set(int index, T element)
		{
			T[] arr = Data;
			T old = arr[index];
			if ((old != null) ? old.Equals(element) : (element == null))
				return this;

			return new PArray<T>(new Diff(index, element), this);
		}

		#endregion

		#region Helper Classes

		class Diff : Change
		{
			readonly int Index;
			T Value;

			public Diff(int index, T value)
			{
				Index = index;
				Value = value;
			}

			public override Change Apply(T[] a)
			{
				T vp = a[Index];
				a[Index] = Value;
				Value = vp;
				return this;
			}
		}

		#endregion
	}

	public class Persistent<T>
	{
		#region Variables

		protected Change _change;
		protected Persistent<T> _previous;

		#endregion

		#region Constructor

		public Persistent(T value)
			: this(new DataNode { Value = value }, null)
		{
		}

		protected Persistent(Change change, Persistent<T> previous)
		{
			Debug.Assert(previous != null || change is DataNode);
			_change = change;
			_previous = previous;
		}

		#endregion

		#region Properties

		public T Data
		{
			get
			{
				var n = _change as DataNode;
				return n != null ? n.Value : Sync();
			}
		}

		#endregion

		#region Methods

		public T Sync()
		{
			Persistent<T> current = this;
			var previous = (Persistent<T>)null;

			do
			{
				Persistent<T> tmp = current._previous;
				current._previous = previous;
				previous = current;
				current = tmp;
			} while (current != null);

			var core = (DataNode)previous._change;
			T value = core.Value;

			while (true)
			{
				current = previous;
				previous = current._previous;
				if (previous == null) break;
				current._change = previous._change.Apply(value);
			}

			_change = core;
			return value;
		}

		#endregion

		#region Helper Classes

		public abstract class Change
		{
			public abstract Change Apply(T unknown);
		}


		private class DataNode : Change
		{
			public T Value;

			public override Change Apply(T unknown)
			{
				return this;
			}
		}

		#endregion
	}

	public static class HackerRankUtils
	{
		#region Comparer

		public class Comparer<T> : IComparer<T>
		{
			readonly Comparison<T> _comparison;

			public Comparer(Comparison<T> comparison)
			{
				_comparison = comparison;
			}

			public int Compare(T a, T b)
			{
				return _comparison(a, b);
			}
		}

		#endregion

		#region Arrays

		public static int BinarySearch<T>(T[] array, T value, int left, int right, bool upper = false)
			where T : IComparable<T>
		{
			while (left <= right)
			{
				var mid = left + (right - left) / 2;
				var cmp = value.CompareTo(array[mid]);
				if (cmp > 0 || cmp == 0 && upper)
					left = mid + 1;
				else
					right = mid - 1;
			}
			return left;
		}

		public static void For(int n, Action<int> action)
		{
			for (var i = 0; i < n; i++) action(i);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		public static void MemSet<T>(IList<T> list, T value)
		{
			var count = list.Count;
			for (var i = 0; i < count; i++)
				list[i] = value;
		}

		public static void MemSet<T>(T[,] list, T value)
		{
			var count = list.GetLength(0);
			var count2 = list.GetLength(1);
			for (var i = 0; i < count; i++)
				for (var j = 0; j < count2; j++)
					list[i, j] = value;
		}

		public static void MemSet<T>(IEnumerable<IList<T>> list, T value)
		{
			foreach (var sublist in list)
				MemSet(sublist, value);
		}

		public static void Iota(IList<int> list, int seed)
		{
			var count = list.Count;
			for (var i = 0; i < count; i++)
				list[i] = seed++;
		}

		#endregion

		#region Reporting Answer

		static volatile bool _reported;
		static Timer _timer;
		static Func<bool> _timerAction;

		public static void LaunchTimer(Func<bool> action, long ms = 2800)
		{
			_timerAction = action;
			_timer = new Timer(
				delegate
				{
					Report();
#if !DEBUG
				if (_reported)
					Environment.Exit(0);
#endif
				}, null, ms, 0);
		}

		public static void Report()
		{
			if (_reported) return;
			_reported = true;
			_reported = _timerAction();
		}

#if DEBUG
		static readonly Stopwatch _stopWatch = new Stopwatch();
#endif

		public static void Run(string name, Action action)
		{
#if DEBUG
			Write(name + ": ");
			_stopWatch.Restart();
			action();
			_stopWatch.Stop();
			WriteLine($"Elapsed Time: {_stopWatch.Elapsed}\n");
#else
		action();
#endif
		}

		[Conditional("DEBUG")]
		public static void Run2(string name, Action action)
		{
			// Ignore
		}

		#endregion
	}
}