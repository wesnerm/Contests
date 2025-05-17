namespace HackerRank.WalmartLabs
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class DigitMinMax
	{
		static void Main(String[] args)
		{
			long a = long.Parse(Console.ReadLine());
			long b = long.Parse(Console.ReadLine());

			var sol = new DigitMinMax();
			long hi = sol.ComputeScore(b);
			long lo = sol.ComputeScore(a - 1);
			Console.WriteLine(hi - lo);
		}

		long ComputeScore(long n)
		{
			if (n <= 100)
				return 0;

			//Console.WriteLine("Computing " + n);

			long mmscore = 0;
			for (int i = 0; i <= 999; i++)
			{
				int d0 = i % 10;
				int d1 = i / 10 % 10;
				int d2 = i / 100 % 10;
				bool good = d1 < d0 && d1 < d2 || d1 > d0 && d1 > d2;
				if (!good) continue;

				for (long bs = 1; bs <= n; bs *= 10)
				{
					long n2 = n / bs;
					if (n2 < 100) break;

					long cnt = n2 / 1000;
					if (cnt == 0 && i < 100) break;

					long addend = cnt * bs;
					if (i < 100 && addend > 0) addend -= bs;
					mmscore += addend;

					cnt = n2 % 1000;
					if (cnt > i)
						mmscore += 1 * bs;
					else if (cnt == i)
						mmscore += (n % bs) + 1;
				}
			}

			return mmscore;
		}

	}
}