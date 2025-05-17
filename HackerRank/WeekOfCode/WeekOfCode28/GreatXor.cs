namespace HackerRank.WeekOfCode28
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class GreatXor
	{

		static void Main(String[] args)
		{
			int q = int.Parse(Console.ReadLine());


			for (int a0 = 0; a0 < 100000; a0++)
			{
				long x = long.Parse("1000000000");
				CountGreater(x);
			}

			for (int a0 = 0; a0 < q; a0++)
			{
				long x = long.Parse(Console.ReadLine());
				Console.WriteLine(CountGreater(x));
			}
		}

		static long CountGreater(long x)
		{
			if (x < 2) return 0;

			long hib = x;
			while ((hib & hib - 1) != 0)
				hib &= hib - 1;
			return x ^ hib ^ (hib - 1);
		}
	}
}

