namespace HackerRank.WalmartLabs
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class CountYourProgressions
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */
			int n = int.Parse(Console.ReadLine());
			int[] array = Enumerable.Range(1, n).Select(x => int.Parse(Console.ReadLine())).ToArray();

			int count = n + 1;
			var prog = new int[101, 200];
			var dict = new int[101];

			foreach (var v in array)
			{
				count = Add(count, dict[v]);
				for (int v3 = 1; v3 <= 100; v3++)
				{
					int d = v3 - v + 100;
					dict[v3] = Add(dict[v3], prog[v, d] + 1);
					prog[v3, d] = Add(prog[v3, d], prog[v, d] + 1);
				}
			}

			Console.WriteLine(count);
		}

		public const int MOD = 1000 * 1000 * 1000 + 9;

		public static int Inverse(long n)
		{
			return ModPow(n, MOD - 2);
		}

		public static int Mult(long left, long right)
		{
			return (int) ((left * right) % MOD);
		}

		public static int Add(long left, long right)
		{
			return (int) ((left + right) % MOD);
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
			return (int) result;
		}

	}
}