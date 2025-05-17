namespace HackerRank.WeekOfCode28.LuckyNumber8
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    class Solution
    {


		static void Main(string[] args)
		{
			int n = Convert.ToInt32(Console.ReadLine());
			string number = Console.ReadLine();
			var count = CountEights(number);
			Console.WriteLine(count);
		}

		private const long MOD = 1000L * 1000L * 1000L + 7;

		static long CountEights(string number)
		{
			long count = 0;
			var c2 = new long[2];
			var c4 = new long[4];
			c2[0] = 1;
			c4[0] = 1;

			for (int i = 0; i < number.Length; i++)
			{
				var ch = number[i] - '0';
				if ((ch & 1) == 0)
					count = (count + c4[ch * 3 >> 1 & 3]) % MOD;

				var ch4 = ch & 3;
				c4[ch4] += c2[0];
				c4[ch4 ^ 2] += c2[1];
				c2[ch & 1] = (c2[ch & 1] + c2[0] + c2[1]) % MOD;
			}

			return count % MOD;
		}
	}
}

