
namespace HackerRank.WeekOfCode29.MegaprimeNumbers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Diagnostics;
	class Solution
	{

		static long first;
		static long last;

		static void Main(String[] args)
		{
			var sw = new Stopwatch();
			sw.Start();
			var tokens_first = Console.ReadLine().Split();
			first = Convert.ToInt64(tokens_first[0]);
			last = Convert.ToInt64(tokens_first[1]);

			long index = 1;
			while (index < last)
				index *= 10;

			long count = Dfs(0, index);
			Console.WriteLine(count);
			//Console.WriteLine(sw.Elapsed);
		}

		static long[] digits = { 2, 3, 5, 7 };
		static long Dfs(long number, long index)
		{
			if (index == 0)
			{
				if (number >= first && number <= last && MillerRabin.IsPrime(number))
					return 1;
				return 0;
			}

			long count = 0;
			long newIndex = index / 10;
			if (number == 0 && first < index)
				count += Dfs(number, newIndex);

			foreach (var d in digits)
			{
				long n = number + d * index;
				if (n > last) break;
				if (n + index <= first) continue;
				count += Dfs(n, newIndex);
			}
			return count;
		}

		public static class MillerRabin
		{

			private const long PrimesUnder64 = 0
				| 1L << 02 | 1L << 03 | 1L << 05 | 1L << 07
				| 1L << 11 | 1L << 13 | 1L << 17 | 1L << 19
				| 1L << 23 | 1L << 29
				| 1L << 31 | 1L << 37
				| 1L << 41 | 1L << 43 | 1L << 47
				| 1L << 53 | 1L << 59
				| 1L << 61;

			private const int PrimeFilter235 = 0
				| 1 << 1 | 1 << 7
				| 1 << 11 | 1 << 13 | 1 << 17 | 1 << 19
				| 1 << 23 | 1 << 29;

			// Witnesses must all be less that 64-2=62
			// We filter out numbers below 64
			// https://miller-rabin.appspot.com
			static readonly int[] Witness32 = { 2, 7, 61 }; //  4759123141
			static readonly long[] Witness40 = { 2, 13, 23, 1662803 }; //  1122004669633
			static readonly long[] Witness41 = { 2, 3, 5, 7, 11, 13 }; // 3,474,749,660,383
			static readonly long[] Witness51 = { 2, 75088, 642735, 203659041, 3613982119 }; // 3071837692357849
			static readonly long[] Witness64 = { 2, 325, 9375, 28178, 450775, 9780504, 1795265022 }; // Can't be witness if w | n

			// TEST CASE: 46817 is a prime

			// Sieve is 10X faster for checking multiple primes.

			public static bool IsPrime(uint n)
			{
				// 2 is the first prime
				if (n < 2) return false;

				// Return primes under 64 in constant time
				// Important Step! witnesses < 64 <= n
				if (n < 64) return (PrimesUnder64 & 1L << (int)n) != 0;

				// Filter out easy composites (3/4 of positive integers)
				if ((PrimeFilter235 & 1 << (int)(n % 30)) == 0)
					return false;

				// Hard test
				uint s = n - 1;
				while ((s & 1) == 0) { s >>= 1; }

				foreach (var w in Witness32)
				{
					// NOTE: V needs to be long because we are squaring
					long v = Pow(w, s, n);

					if (v != 1)
					{
						for (var t = s; v != n - 1; t <<= 1)
						{
							if (t >= n - 1)
								return false;
							v = v * v % n;
						}
					}
				}

				return true;
			}

			public static bool IsPrime(long n)
			{
				if (n < 2) return false;
				if (n <= uint.MaxValue) return IsPrime((uint)n);
				if ((PrimeFilter235 & 1 << (int)(n % 30)) == 0)
					return false;

				var witnesses = n < 1122004669633
					? Witness40
					: n < 3071837692357849
					? Witness51
					: Witness64;

				var s = n - 1;
				while ((s & 1) == 0) { s >>= 1; }

				foreach (var w in witnesses)
				{
					// Witnesses can't be a multiple of n
					// The inequality w < 2^31 < n is guaranteed by the 32-bit int rerouting
					// if (w % n == 0) continue;

					long v = Pow(w, s, n);
					if (v != 1)
					{
						for (var t = s; v != n - 1; t <<= 1)
						{
							if (t >= n - 1)
								return false;
							v = Mult(v, v, n);
						}
					}
				}
				return true;
			}


			public static long Mult(long a, long b, long mod)
			{
				// If both integers are positive 32-bit integers, use shortcut
				if ((ulong)(a | b) <= (1UL << 31))
					return a * b % mod;

				// If mod is positive 32-bit integer, use shortcut
				a %= mod;
				b %= mod;
				if ((ulong)mod <= (1UL << 31))
					return a * b % mod;

				if (a < b)
				{
					long tmp = a;
					a = b;
					b = tmp;
				}

				long result = 0;
				long y = a;
				while (b > 0)
				{
					if (b % 2 == 1)
						result = (result + y) % mod;
					y = (y * 2) % mod;
					b /= 2;
				}
				return result % mod;
			}

			public static long Pow(long n, long p, long mod)
			{
				long result = 1;
				long b = n;
				while (p > 0)
				{
					if ((p & 1) == 1) result = Mult(result, b, mod);
					p >>= 1;
					b = Mult(b, b, mod);
				}
				return result;
			}
		}
	}

}