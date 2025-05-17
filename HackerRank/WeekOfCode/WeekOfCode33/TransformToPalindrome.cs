namespace HackerRank.WeekOfCode33.TransformToPalindrome
{
	// https://www.hackerrank.com/contests/w33/challenges/transform-to-palindrome

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static void Main(String[] args)
		{

			var input = new StreamReader(Console.OpenStandardInput());
			string[] tokens_n = input.ReadLine().Split(' ');
			int n = Convert.ToInt32(tokens_n[0]);
			int k = Convert.ToInt32(tokens_n[1]);
			int m = Convert.ToInt32(tokens_n[2]);

			var dj = new DisjointSet(n + 1);

			for (int a0 = 0; a0 < k; a0++)
			{
				var tokens_x = input.ReadLine().Split();
				int x = Convert.ToInt32(tokens_x[0]);
				int y = Convert.ToInt32(tokens_x[1]);
				dj.Union(x, y);
			}

			var a_temp = input.ReadLine().Split();
			int[] a = Array.ConvertAll(a_temp, x => dj.Find(int.Parse(x)));

			int[,] dp = new int[m + 1, m + 1];

			int max = 1;
			for (int i = 0; i < m; i++)
				dp[i, i] = 1;

			for (int len = 2; len <= m; len++)
				for (int i = 0; i + len <= m; i++)
				{
					int j = i + len - 1;
					int tmp = Math.Max(dp[i, j - 1], dp[i + 1, j]);
					if (a[i] == a[j]) tmp = Math.Max(tmp, dp[i + 1, j - 1] + 2);
					dp[i, j] = tmp;
					if (tmp > max) max = tmp;
				}

			Console.WriteLine(max);
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

		public int Union(int x, int y)
		{
			var rx = Find(x);
			var ry = Find(y);
			if (rx == ry) return rx;

			_components--;
			if (_counts[ry] > _counts[rx])
			{
				_ds[rx] = ry;
				_counts[ry] += _counts[rx];
				return ry;
			}
			else
			{
				_ds[ry] = rx;
				_counts[rx] += _counts[ry];
				return rx;
			}
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


}