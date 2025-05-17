namespace HackerRank.WeekOfCode30.Poles
{
	using System;
	using System.Collections.Generic;
	using static System.Math;
	using static FastIO;

	public static class Solution
	{
		#region Variables
		static int[] x;
		static int[] w;
		static int[] wsum;
		#endregion

		public static void Main()
		{
			int n = Ni();
			int k = Ni();
			x = new int[n + 1];
			w = new int[n + 1];
			wsum = new int[n + 1];

			int sum = 0;
			for (int i = 0; i < n; i++)
			{
				x[i] = Ni();
				w[i] = Ni();
				wsum[i + 1] = sum = sum + w[i];
			}

			long cost = Dp(k, n);
			WriteLine(cost);
		}

		public static long Dp(int k, int n)
		{
			const long MaxCost = long.MaxValue >> 15;
			var cache = new long[n + 1];
			var cache2 = new long[n + 1];
			var buffer = new long[n + 1];

			for (int nn = 1; nn <= n; nn++)
				cache[nn] = MaxCost;

			for (int kk = 1; kk <= k; kk++)
			{
				cache2[kk] = 0;

				long min = int.MaxValue;
				int limit = n - (k - kk);

				for (int nn = kk + 1; nn <= limit; nn++)
				{
					long rollCost = 0;
					long rollsCost = 0;
					long minCost = MaxCost;
					long xprev = x[nn - 1];
					for (int i = nn - 1; i >= kk - 1; i--)
					{
						rollsCost += rollCost * (xprev - x[i]);
						long cost = cache[i] + rollsCost;
						if (cost < minCost) minCost = cost;
						rollCost += w[i];
						if (rollsCost >= minCost) break;
						xprev = x[i];
					}
					cache2[nn] = minCost;
				}

				var tmp = cache;
				cache = cache2;
				cache2 = tmp;
			}

			return cache[n];
		}
	}


}