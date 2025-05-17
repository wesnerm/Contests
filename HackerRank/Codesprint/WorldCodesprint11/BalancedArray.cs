namespace HackerRank.WorldCodeSprint11.BalancedArray
{
	// https://www.hackerrank.com/contests/world-codesprint-11/challenges

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static long solve(int[] a)
		{
			long sum = a.Sum();

			int n = a.Length / 2;
			long half = 0;
			for (int i = 0; i < n; i++)
				half += a[i];

			var righthalf = sum - half;
			var diff = Math.Abs(righthalf - half);
			return (diff);
		}

		static void Main(String[] args)
		{
			int n = Convert.ToInt32(Console.ReadLine());
			string[] a_temp = Console.ReadLine().Split(' ');
			int[] a = Array.ConvertAll(a_temp, Int32.Parse);
			long result = solve(a);
			Console.WriteLine(result);
		}
	}

}