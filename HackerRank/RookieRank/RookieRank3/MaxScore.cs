namespace CompetitiveProgramming.HackerRank.RookieRank3.MaxScore
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static long[] sums;
		static long[] scores;

		static long getMaxScore(long[] a)
		{
			var n = a.Length;
			var n2 = 1 << a.Length;
			sums = new long[n2];
			scores = new long[n2];

			for (int i = 0; i < n2; i++)
			{
				for (int j = 0; j < n; j++)
				{
					var b = 1 << j;
					if ((i & b) != 0) continue;

					var i2 = i | b;
					var v = a[j];
					sums[i2] = sums[i] + v;
					var myscore = scores[i] + (sums[i] % v);
					scores[i2] = Math.Max(scores[i2], myscore);
				}
			}

			return scores[n2 - 1];
		}

		static void Main(String[] args)
		{
			int n = Convert.ToInt32(Console.ReadLine());
			string[] a_temp = Console.ReadLine().Split(' ');
			long[] a = Array.ConvertAll(a_temp, Int64.Parse);
			long maxScore = getMaxScore(a);
			Console.WriteLine(maxScore);
		}
	}

}
