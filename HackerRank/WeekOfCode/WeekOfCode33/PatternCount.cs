namespace HackerRank.WeekOfCode33.PatternCount
{
	// https://www.hackerrank.com/contests/w33/challenges/pattern-count

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static int patternCount(char[] s)
		{

			for (int i = 1; i + 1 < s.Length; i++)
				if (s[i - 1] == '1' && s[i] == s[i + 1])
					s[i] = '1';

			int count = 0;
			for (int i = 1; i + 1 < s.Length; i++)
				if (s[i - 1] == '1' && s[i] == '0' && s[i + 1] == '1')
					count++;

			return count;
		}

		static void Main(String[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				var s = Console.ReadLine().ToCharArray();
				int result = patternCount(s);
				Console.WriteLine(result);
			}
		}
	}

}