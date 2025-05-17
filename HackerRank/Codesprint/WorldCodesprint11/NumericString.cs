namespace HackerRank.WorldCodeSprint11.NumericString
{
	// https://www.hackerrank.com/contests/world-codesprint-11/challenges/numeric-string

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static long getMagicNumber(string s, long k, long b, long m)
		{
			long b_k = 1;
			for (int i = 1; i <= k; i++)
				b_k = b_k * b % m;

			long sum1 = 0;
			long result = 0;
			for (int i = 0; i < s.Length; i++)
			{
				int d = s[i] - '0';

				sum1 = (b * sum1 + d) % m;

				var x = i - k;
				if (x >= 0)
				{
					int d2 = s[(int)x] - '0';
					sum1 = ((sum1 - d2 * b_k) % m + m) % m;
				}


				if (x >= -1)
					result += sum1;
			}
			return result;
		}

		static void Main(String[] args)
		{
			string s = Console.ReadLine();
			string[] tokens_k = Console.ReadLine().Split(' ');
			int k = Convert.ToInt32(tokens_k[0]);
			int b = Convert.ToInt32(tokens_k[1]);
			int m = Convert.ToInt32(tokens_k[2]);
			var result = getMagicNumber(s, k, b, m);
			Console.WriteLine(result);
		}
	}

}