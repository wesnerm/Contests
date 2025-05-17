namespace HackerRank.AdInfinitum18.PythagoreanTriple
{
	// https://www.hackerrank.com/contests/infinitum18/challenges/pythagorean-triple

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static long[] pythagoreanTriple(long s)
		{
			long f = 1;

			if (s % 4 == 0)
			{
				f = s / 4;
				return new[] { f * 3, s, f * 5 };
			}

			while (s > 0 && s % 2 == 0)
			{
				f *= 2;
				s /= 2;
			}

			long a = s;
			long k = (s - 1) / 2;
			long m = k + 1;
			long n = k;
			long b = 2 * m * n;
			long c = m * m + n * n;

			return new[] { f * a, f * b, f * c };
		}

		static void Main(String[] args)
		{
			int a = Convert.ToInt32(Console.ReadLine());
			long[] triple = pythagoreanTriple(a);
			Console.WriteLine(String.Join(" ", triple));
		}
	}

}