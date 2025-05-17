namespace CompetitiveProgramming.HackerRank.RookieRank3.MaximalTourism
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Solution
	{

		static void Main(String[] args)
		{
			string[] tokens_n = Console.ReadLine().Split();
			int n = Convert.ToInt32(tokens_n[0]);
			int m = Convert.ToInt32(tokens_n[1]);
			int[][] route = new int[m][];
			var ds = new UnionFind(n);
			for (int route_i = 0; route_i < m; route_i++)
			{
				string[] route_temp = Console.ReadLine().Split();
				var arr = Array.ConvertAll(route_temp, Int32.Parse);
				ds.Union(arr[0] - 1, arr[1] - 1);
			}

			int max = 1;
			for (int i = 0; i < n; i++)
				max = Math.Max(max, ds.GetCount(i));
			Console.WriteLine(max);
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
	}

}
