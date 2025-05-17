namespace HackerRank.WeekOfCode32.GeometricTrick
{
	// https://www.hackerrank.com/contests/w32/challenges/geometric-trick

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();
		}

		int n;
		char[] s;
		int[] factors;

		public void solve()
		{
			n = Ni();
			s = Nc(n);

			factors = PrimeFactorsUpTo(n);

			long count = 0;
			int j1;
			for (int j = 0; j < n; j++)
			{
				if (s[j] != 'b') continue;
				j1 = j + 1;
				EnumerateFactorsX2(factors, j1, j1, f =>
				{
					if (s[f - 1] == 'b') return;
					var f2 = 1L * j1 * j1 / f;
					Console.Error.WriteLine($"{j1},{f},{f2}");
					if (f2 > n || s[f - 1] != (s[f2 - 1] ^ 2)) return;
					count++;
				});
			}

			WriteLine(count);
		}

		public static int[] PrimeFactorsUpTo(int n)
		{
			var factors = new int[n + 1];

			for (int i = 2; i <= n; i += 2)
				factors[i] = 2;

			var sqrt = (int)Math.Sqrt(n);
			for (int i = 3; i <= sqrt; i += 2)
			{
				if (factors[i] != 0) continue;
				for (int j = i * i; j <= n; j += i + i)
				{
					if (factors[j] == 0)
						factors[j] = i;
				}
			}

			for (int i = 3; i <= n; i += 2)
			{
				if (factors[i] == 0)
					factors[i] = i;
			}

			return factors;
		}

		public static int EnumerateFactorsX2(int[] factors, long n, int max, Action<long> action = null, long f = 1)
		{
			if (f > max)
				return 0;

			if (n == 1)
			{
				action?.Invoke(f);
				return 1;
			}

			long p = factors[n];
			long c = 1;
			long next = n / p;
			while (next > 1 && factors[next] == p)
			{
				c++;
				next = next / p;
			}

			c *= 2;
			int result = EnumerateFactorsX2(factors, next, max, action, f);
			while (c-- > 0)
			{
				f *= p;
				result += EnumerateFactorsX2(factors, next, max, action, f);
			}

			return result;
		}

	}


}