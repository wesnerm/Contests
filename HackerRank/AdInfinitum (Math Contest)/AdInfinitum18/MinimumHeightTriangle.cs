namespace HackerRank.AdInfinitum18.MinimumHeightTriangle
{
	// https://www.hackerrank.com/contests/infinitum18/challenges/lowest-triangle

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static int lowestTriangle(int b, int area)
		{
			return (int)(Math.Ceiling(area * 2.0 / b));
		}

		static void Main(String[] args)
		{
			string[] tokens_base = Console.ReadLine().Split(' ');
			int b = Convert.ToInt32(tokens_base[0]);
			int area = Convert.ToInt32(tokens_base[1]);
			int height = lowestTriangle(b, area);
			Console.WriteLine(height);
		}
	} 
}