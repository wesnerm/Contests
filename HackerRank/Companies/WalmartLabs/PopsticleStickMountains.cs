namespace HackerRank.Walmart
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	class PopsicleStickMountains
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			// Catalan number

			var catalan = new int[4001];
			var presum = new int[4001];

			for (int i = 2; i <= 4000; i += 2)
				catalan[i] = Catalan(i / 2);

			for (int i = 1; i <= 4000; i++)
				presum[i] = (presum[i - 1] + catalan[i]) % MOD;

			int n = int.Parse(Console.ReadLine());
			for (int i = 0; i < n; i++)
			{
				int c = int.Parse(Console.ReadLine());
				Console.WriteLine(presum[c]);
			}
		}

		public static int Catalan(int n)
		{
			return Div(Comb(2 * n, n), 1 + n);
		}

		public static int Comb(int n, int k)
		{
			int result = 1;
			int den = 1;
			for (int i = 1; i <= k; i++)
			{
				result = Mult(result, n - i + 1);
				den = Mult(den, i);
			}

			result = Div(result, den);
			return result;
		}


		public const int MOD = 1000 * 1000 * 1000 + 7;

		public static int Inverse(long n)
		{
			return ModPow(n, MOD - 2);
		}

		public static int Mult(long left, long right)
		{
			return (int)((left * right) % MOD);
		}

		public static int Div(long left, long divisor)
		{
			return Mult(left, Inverse(divisor));
		}

		public static int ModPow(long n, long p)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result = (result * b) % MOD;
				p >>= 1;
				b = (b * b) % MOD;
			}
			return (int)result;
		}

	}
}