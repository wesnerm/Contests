namespace HackerRank.AdInfinitum17
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class PrimitiveProblem
	{

		public static void Main(string[] args)
		{
			int n = int.Parse(Console.ReadLine());
			var s = TotientFunction(n);
			var primes = PrimeFactors(s).Keys.ToList();

			for (int a = 2; a < n; a++)
			{
				bool good = true;
				foreach (var prime in primes)
				{
					var pow = Pow(a, s / prime, n);
					if (pow == 1)
					{
						good = false;
						break;
					}
				}
				if (good == true)
				{
					Console.WriteLine(a + " " + TotientFunction(s));
					return;
				}
			}

			Console.WriteLine("0 0");
		}

		public static long Gcd(long n, long m)
		{
			unchecked
			{
				if (n < 0) n = -n;
				if (m < 0) m = -m;
				return (long) Gcd((ulong) n, (ulong) m);
			}
		}

		public static ulong Gcd(ulong n, ulong m)
		{
			if (m == n)
				return n;

			while (true)
			{
				if (m == 0) return n;
				n %= m;
				if (n == 0) return m;
				m %= n;
			}
		}

		public static Dictionary<int, int> PrimeFactors(int n)
		{
			var dict = new Dictionary<int, int>();
			if (IsPrimeMillerRabin(n))
				return dict;

			int cnt = 0;
			while (n % 2 == 0)
			{
				n /= 2;
				cnt++;
			}
			if (cnt > 0)
				dict[2] = cnt;

			for (int i = 3; i * i <= n; i += 2)
			{
				cnt = 0;
				while (n % i == 0)
				{
					n /= i;
					cnt++;
				}
				if (cnt > 0)
					dict[i] = cnt;
			}

			if (n != 1)
				dict[n] = 1;

			return dict;
		}

		public static int TotientFunction(int n)
		{
			if (IsPrimeMillerRabin(n))
				return n - 1;

			int result = n;

			if (n % 2 == 0)
			{
				while (n % 2 == 0) n /= 2;
				result /= 2;
			}

			for (int p = 3; p * p <= n; p += 2)
			{
				if (n % p == 0)
				{
					while (n % p == 0) n /= p;
					result -= result / p;
				}
			}

			if (n > 1)
				result -= result / n;
			return result;
		}

		public static int[] PrimeFactorsUpTo(int n)
		{
			var factors = new int[n + 1];

			for (int i = 2; i <= n; i += 2)
				factors[i] = 2;

			var sqrt = (int) Math.Sqrt(n);
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
		private static readonly int[] Witness32 = {2, 7, 61};
		private const long Range0 = 4759123141;

		// TEST CASE: 46817 is a prime

		// Sieve is 10X faster for checking multiple primes.

		public static bool IsPrimeMillerRabin(int n)
		{
			// 2 is the first prime
			if (n < 2) return false;

			// Return primes under 64 in constant time
			// Important Step! witnesses < 64 <= n
			if (n < 64) return (PrimesUnder64 & 1L << n) != 0;

			// Filter out easy composites (3/4 of positive integers)
			if ((PrimeFilter235 & 1 << (n % 30)) == 0)
				return false;

			// Hard test
			int d = n - 1;
			while ((d & 1) == 0)
			{
				d >>= 1;
			}

			foreach (var w in Witness32)
			{
				// NOTE: V needs to be long because we are squaring
				long v = Pow(w, d, n);
				if (v != 1)
				{
					for (; v != n - 1; d <<= 1)
					{
						if (d >= n - 1)
							return false;
						v = v * v % n;
					}
				}
			}

			return true;
		}

		public int Mult(int a, int b, int mod)
		{
			return (int) ((long) a * b % mod);
		}


		public static int Pow(int n, int p, int mod)
		{
			long result = 1;
			long b = n;
			while (p > 0)
			{
				if ((p & 1) == 1) result = result * b % mod;
				p >>= 1;
				b = b * b % mod;
			}
			return (int) result;
		}
	}
}
