namespace HackerRank.WeekOfCode33.TwinArrays
{
	// https://www.hackerrank.com/contests/w33/challenges/twin-arrays

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static long twinArrays(long[] ar1, long[] ar2)
		{
			var ind1 = new int[ar1.Length];
			var ind2 = new int[ar2.Length];

			for (int i = 0; i < ind1.Length; i++)
				ind1[i] = i;

			for (int i = 0; i < ind2.Length; i++)
				ind2[i] = i;

			Array.Sort(ar1, ind1);
			Array.Sort(ar2, ind2);

			long min1 = ar1[0] + (ind1[0] != ind2[0] ? ar2[0] : 1 < ar2.Length ? ar2[1] : int.MaxValue);
			long min2 = ar2[0] + (ind1[0] != ind2[0] ? ar1[0] : 1 < ar1.Length ? ar1[1] : int.MaxValue);
			return Math.Min(min1, min2);
		}

		static void Main(String[] args)
		{
			int n = Convert.ToInt32(Console.ReadLine());
			var ar1_temp = Console.ReadLine().Split();
			var ar1 = Array.ConvertAll(ar1_temp, long.Parse);
			var ar2_temp = Console.ReadLine().Split();
			var ar2 = Array.ConvertAll(ar2_temp, long.Parse);
			var result = twinArrays(ar1, ar2);
			Console.WriteLine(result);
		}
	}

}