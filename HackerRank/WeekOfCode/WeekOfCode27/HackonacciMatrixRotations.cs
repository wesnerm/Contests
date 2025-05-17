namespace HackerRank.WeekOfCode27
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class HackonacciMatrixRotations
	{
		const int mask = 1 << 0 | 1 << 1 | 1 << 3 | 1 << 6;

		static void Main(String[] args)
		{
			var tokens_n = Console.ReadLine().Split();
			int n = int.Parse(tokens_n[0]);
			int q = int.Parse(tokens_n[1]);

			// 1 <= n <= 2000
			// 1 <= n*n <= 4000 * 1000
			// 1 <= q <= 10000
			// 0 <= angle <= 100000

			/*
			for (int i=1; i<=n; i++)
			{
				var a = GetAnswers(i);
				Console.WriteLine($"{i}: {a[1]} {a[2]}");
			}
			*/

			// Variations
			var answers = GetAnswers(n);
			for (int a0 = 0; a0 < q; a0++)
			{
				int angle = int.Parse(Console.ReadLine());
				Console.WriteLine(answers[(angle / 90) % 4]);
			}
		}

		public static bool M(long r, long c)
		{
			var rc = r * c;
			return (mask & (1 << (int)(rc * rc % 7))) != 0;
		}

		public static long[] GetAnswers(int n)
		{
			long c1 = 0;
			long c2 = 0;

			for (int r = 1; r <= n; r++)
				for (int c = 1; c <= n; c++)
				{
					int mr = n - r + 1;
					int mc = n - c + 1;
					var v = M(r, c);
					if (v != M(c, mr)) c1++;
					if (v != M(mr, mc)) c2++;
				}

			return new long[] { 0, c1, c2, c1 };
		}

		public void Check()
		{
			int nn = 2000;
			var table = new int[nn * nn + 1];
			table[1] = 1;
			table[2] = 2;
			table[3] = 3;
			for (int i = 4; i < table.Length; i++)
				table[i] = (table[i - 1] + 2 * table[i - 2] + 3 * table[i - 3]) % 2;

			for (int i = 1; i < table.Length; i++)
			{
				var result1 = (mask & (1 << i % 7)) != 0;
				var result2 = table[i] % 2 != 0;
				if (result1 != result2)
				{
					Console.WriteLine($"{i} {result1} {result2}");
					throw new Exception();
				}
			}
		}

		// Test Variations
		// Inverting c3 and c1 in the answers array
		// Brute force versus modulo check versus log table
	}

}
