namespace HackerRank.AdInfinitum17
{
	using System;

	class VCuttingTool
	{

		static void Main(String[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				int n = Convert.ToInt32(Console.ReadLine());
				Console.WriteLine(Cut(n));
			}
		}


		static long Cut(long n)
		{
			if (n == 0) return 1;
			long x = 1 + 2 * (n - 1);
			return x * (x + 1) / 2 + 1;
		}
	}

}
