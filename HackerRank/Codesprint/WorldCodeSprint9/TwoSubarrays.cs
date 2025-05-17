using System.Collections.Generic;
using System.Diagnostics;
using Softperson;

namespace HackerRank.WorldCodeSprint8.TwoSubarrays
{
	using System;
	using System.Linq;
	using static System.Math;
	using static System.Console;

	public class Solution
	{
		static void Main()
		{
			n = int.Parse(ReadLine());
			var list = ReadLine().Split().Select(int.Parse).ToList();

			maxi = 2 * n + 5;
			bit = new int[maxi];
			a = new int[maxi];
			s = new int[maxi];

			for (int i = 1; i <= n; i++)
			{
				a[i] = list[i - 1];
				s[i] = s[i - 1] + a[i];
			}

			me = a.Max();
			ms = me * (me + 1) / 2 + 1;

			dp = new int[me + 1, ms];
			best = new int[me + 1, ms];

			int am;
			int ans = Solve100(out am);
			WriteLine($"{ans} {am}");
		}

		#region 20% Editorial

		static int n;
		static int maxi = (int)(3e5 + 5);
		static int me = 40;
		static int ms = 821;

		static int[] bit;
		static int[] a;
		static int[,] dp;
		static int[] s;
		static int[,] best;

		void Ocisti()
		{
			for (int i = 0; i <= maxi; i++)
				bit[i] = 0;
		}

		static int Get(int x)
		{
			int maxx = 0;
			for (int i = x; i > 0; i -= (i & (-i)))
				maxx = Max(maxx, bit[i]);
			return maxx;
		}

		static void Update(int x, int y)
		{
			for (int i = x; i < maxi; i += (i & (-i)))
				bit[i] = Max(bit[i], y);
		}

		static int Solve20(out int am)
		{
			int ans = 0;
			int len = 0;
			am = 0;

			for (int i = 1; i <= n; i++)
				if (a[i] > 0)
				{
					Array.Clear(bit, 0, bit.Length);
					int y = 0;
					for (int j = i; j <= n; j++)
						if (a[j] > 0)
						{
							int x = Get(a[j] - 1) + a[j];
							y = Max(x, y);
							Update(a[j], x);
							if (ans < s[j] - s[i - 1] - y)
							{
								ans = s[j] - s[i - 1] - y;
								len = j - i + 1;
								am = 1;
							}
							else if (ans == s[j] - s[i - 1] - y && len > j - i + 1)
							{
								len = j - i + 1;
								am = 1;
							}
							else if (ans == s[j] - s[i - 1] - y && len == j - i + 1)
								am++;

							ans = Max(ans, s[j] - s[i - 1] - y);
						}
				}

			if (ans == 0) am = n;
			return ans;
		}

		#endregion

		#region 100% Solution

		static int Solve100(out int am)
		{

			for (int i = 1; i <= n; i++)
				s[i] = s[i - 1] + a[i];

			//var rmq2 = new RangeMinimumQuery<int>(s);
			var rmq = new RangeMinimumQuery(s);
			//rmq2.GetMin(0, s.Length - 1);
			rmq.GetArgMin(0, s.Length - 1);

			int ans = 0;
			int len = 0;
			am = 0;

			for (int i = 1; i <= n; i++)
				if (a[i] > 0)
				{
					dp[a[i], a[i]] = i;
					for (int j = a[i] + 1; j <= (a[i] * (a[i] + 1)) / 2; j++)
						dp[a[i], j] = best[a[i] - 1, j - a[i]];

					for (int j = a[i]; j <= (a[i] * (a[i] + 1)) / 2; j++)
						for (int k = a[i]; k <= me; k++)
							best[k, j] = Max(best[k - 1, j], dp[k, j]);

					int lef = 0;
					for (int k = ms - 1; k >= 0; k--)
						if (best[me, k] > lef)
						{
							int idx = rmq.GetArgMin(lef, best[me, k] - 1);
							//int idx2 = rmq2.GetArgMin(lef, best[me, k] - 1);
							//Debug.Assert(idx == idx2);
							if (ans < s[i] - s[idx] - k)
							{
								ans = s[i] - s[idx] - k;
								am = 1;
								len = i - idx + 1;
							}
							else if (ans == s[i] - s[idx] - k && len > i - idx + 1)
							{
								len = i - idx + 1;
								am = 1;
							}
							else if (ans == s[i] - s[idx] - k && len == i - idx + 1) am++;

							lef = best[me, k];
						}
				}
			if (ans == 0) am = n;
			return ans;
		}


		#endregion

	}

	public class RangeMinimumQuery
	{
		readonly int[][] _table;
		readonly int[] _array;

		public RangeMinimumQuery(int[] array)
		{
			_array = array;

			int n = array.Length;
			int lgn = Log2(n);

			_table = new int[lgn+1][];

			if (lgn>0)
			{
				var table0 = _table[0] = new int[n - 1];
				for (int j = 0; j < table0.Length; j++)
					table0[j] = array[j] <= array[j + 1] ? j : j + 1;
			}

			for (int i = 1; i <= lgn; i++)
			{
				int[] prev = _table[i-1];
				int[] curr = _table[i] = new int[n - (1<<i)+1];
				int prevlen = 1 << (i-1);
				for (int j = 0; j < curr.Length; j++)
				{
					int pos1 = prev[j];
					int pos2 = prev[j + prevlen];
					_table[i][j] = array[pos1] <= array[pos2] ? pos1 : pos2;
				}
			}
		}

		public int GetArgMin(int left, int right)
		{
			if (left == right) return left;
			int curlog = Log2(right - left + 1);
			int pos1 = _table[curlog][left];
			int pos2 = _table[curlog][right - (1 << curlog) + 1];
			return _array[pos1] <= _array[pos2] ? pos1 : pos2;
		}

		public int GetMin(int left, int right)
		{
			return _array[GetArgMin(left, right)];
		}

		static int Log2(int value)
		{
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
	}

	public class RangeMinimumQuery<T>
	{
		readonly int[,] _table;
		readonly int _n;
		readonly IList<T> _array;
		readonly Comparison<T> _compare;

		public RangeMinimumQuery(IList<T> array, Comparison<T> compare = null)
		{
			compare = compare ?? Comparer<T>.Default.Compare;
			_array = array;
			_compare = compare;
			_n = array.Count;

			int n = array.Count;
			int lgn = Log2(n);
			_table = new int[lgn, n];

			_table[0, n - 1] = n - 1;
			for (int j = n - 2; j >= 0; j--)
				_table[0, j] = compare(array[j], array[j + 1]) <= 0 ? j : j + 1;

			for (int i = 1; i < lgn; i++)
			{
				int curlen = 1 << i;
				for (int j = 0; j < n; j++)
				{
					int right = j + curlen;
					var pos1 = _table[i - 1, j];
					int pos2;
					_table[i, j] =
						(right >= n || compare(array[pos1], array[pos2 = _table[i - 1, right]]) <= 0)
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
			return _compare(_array[pos1], _array[pos2]) <= 0 ? pos1 : pos2;
		}

		public T GetMin(int left, int right)
		{
			return _array[GetArgMin(left, right)];
		}

		static int Log2(int value)
		{
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
	}
}
