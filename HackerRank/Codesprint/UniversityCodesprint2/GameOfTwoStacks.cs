
namespace HackerRank.UniversityCodesprint2.GameOfTwoStacks
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;
	using static FastIO;

	class Solution
	{

		static long[] a;
		static long[] b;

		static void Main(String[] args)
		{

			int g = Ni();

			for (int a0 = 0; a0 < g; a0++)
			{
				var na = Ni();
				var nb = Ni();
				var x = Ni();

				a = new long[na];
				long sum = 0;
				for (int i = 0; i < na; i++) a[i] = sum = sum + Ni();

				b = new long[nb];
				sum = 0;
				for (int i = 0; i < nb; i++) b[i] = sum = sum + Ni();

				long max = 0;
				for (int i = -1; i < a.Length; i++)
				{
					var asum = i < 0 ? 0 : a[i];
					if (asum > x) break;

					var t0 = Try(x - asum);
					var t = i + 1 + t0;
					if (t > max)
						max = t;
				}

				Console.WriteLine(max);
			}
		}

		static int Try(long v)
		{
			int i = Array.BinarySearch(b, 0, b.Length, v + 1);
			if (i < 0) i = ~i;
			while (i < b.Length && i > 0 && b[i] == b[i - 1]) i--;
			return i;
		}
	}

}