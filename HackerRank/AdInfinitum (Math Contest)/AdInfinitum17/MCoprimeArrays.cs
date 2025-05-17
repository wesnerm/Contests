namespace HackerRank.AdInfinitum17
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;

	class MCoprimeArrays
	{

		static void Driver()
		{
			primes = PrimesListUpTo1e6();

			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				var array = Array.ConvertAll(Console.ReadLine().Split(), long.Parse);
				var n = array[0];
				var m = array[1];

				var multiplicities = CountDivisors(m);
				multiplicities.Sort();

				var answer = Counts(multiplicities, n);
				Console.WriteLine(answer);
			}
		}

		public static long Counts(List<int> multiplicities, long n)
		{
			long count = 1;
			long prevM = -1;
			long saved = 0;

			foreach (var m in multiplicities)
			{
				var arrangements = prevM == m ? saved : (saved = NonAdjacentArrangements(n, m));
				count = Mult(count, arrangements);
				prevM = m;
			}
			return count;
		}

		public static long NonAdjacentArrangementsSlow(long n, long multiplicity)
		{
			long nohead = 1;
			long head = multiplicity;

			for (int i = 2; i <= n; i++)
			{
				var newNohead = (head + nohead) % MOD;
				var newHead = Mult(multiplicity, nohead);
				head = newHead;
				nohead = newNohead;
			}

			return (head + nohead) % MOD;
		}

		public static long NonAdjacentArrangements(long n, long multiplicity)
		{
			var m = new Matrix(1, 1, (int) multiplicity, 0);
			long noHead = 1;
			long head = multiplicity;
			var pow = m.Pow(n);
			pow.Apply(ref noHead, ref head);
			return noHead;
		}

		/// <summary>
		/// Counts the number of divisors in O(n^(1/3)).
		/// </summary>
		/// <remarks>http://codeforces.com/blog/entry/22317</remarks>
		/// <returns></returns>
		public static List<int> CountDivisors(long N)
		{
			long n = N;
			List<int> multiplicities = new List<int>();

			if (IsPrimeMillerRabin(n))
			{
				multiplicities.Add(1);
				return multiplicities;
			}

			foreach (var p in primes)
			{
				if (p * p > n / p) break;
				if (n % p != 0) continue;

				int count = 0;
				while (n % p == 0)
				{
					n /= p;
					count++;
				}
				multiplicities.Add(count);
			}

			if (IsPrimeMillerRabin(n))
			{
				multiplicities.Add(1);
				return multiplicities;
			}

			//var sqrt = (long)Math.Ceiling(Math.Sqrt(n));
			var sqrt = floorSqrt(n);
			if (sqrt * sqrt == n && IsPrimeMillerRabin(sqrt))
			{
				multiplicities.Add(2);
			}

			else if (n != 1)
			{
				multiplicities.Add(1);
				multiplicities.Add(1);
			}

			return multiplicities;
		}

		static long floorSqrt(long x)
		{
			if (x == 0 || x == 1)
				return x;

			long start = 1, end = Math.Min(x, 3037000499L), ans = 1;
			while (start <= end)
			{
				long mid = start + (end - start) / 2;
				long sqr = mid * mid;
				if (sqr == x)
					return mid;

				if (sqr < x)
				{
					start = mid + 1;
					ans = mid;
				}
				else
					end = mid - 1;
			}
			return ans;
		}

		private static List<int> primes;

		// TODO: Make this more efficient
		public static List<int> PrimesListUpTo1e6()
		{
			List<int> primes = new List<int>();
			int max = 2000 * 1000 + 1;
			var factors = PrimeFactorsUpTo(max);

			for (int i = 2; i < max; i++)
			{
				if (factors[i] == i)
					primes.Add(i);
			}

			return primes;
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

		private static readonly int[] Witness41 = {2, 3, 5, 7, 11, 13};
		private static readonly int[] Witness64 = {2, 325, 9375, 28178, 450775, 9780504, 1795265022};


		public static bool IsPrimeMillerRabin(long n)
		{
			if (n < int.MaxValue)
				return IsPrimeMillerRabin((int) n);

			// Filter out multiples of 2, 3, and 5 (3/4 of positive integers)
			if ((PrimeFilter235 & 1 << (int) (n % 30)) == 0)
				return false;

			var witnesses = n < 3474749660383 // 41.6 bits 
				? Witness41
				: Witness64;

			var d = n - 1;
			while ((d & 1) == 0)
			{
				d >>= 1;
			}

			foreach (var w in witnesses)
			{
				long v = Pow(w, d, n);
				if (v != 1)
				{
					for (; v != n - 1; d <<= 1)
					{
						if (d >= n - 1)
							return false;
						v = Mult(v, v, n);
					}
				}
			}
			return true;
		}

		public const long MOD = 1000 * 1000 * 1000 + 7;

		public static long Mult(long a, long b, long mod = MOD)
		{
			return (a * b % mod);
		}

		public static long Pow(long n, long p, long mod)
		{
			long result = 1;
			long b = n;
			while (p > 0)
			{
				if ((p & 1) == 1) result = result * b % mod;
				p >>= 1;
				b = b * b % mod;
			}
			return result;
		}


		public struct Matrix
		{
			public long e11;
			public long e12;
			public long e21;
			public long e22;

			public Matrix(int m11, int m12, int m21, int m22)
			{
				e11 = m11;
				e12 = m12;
				e21 = m21;
				e22 = m22;
			}

			public static Matrix operator *(Matrix m1, Matrix m2)
			{
				Matrix m = new Matrix
				{
					e11 = Add(Mult(m1.e11, m2.e11), Mult(m1.e12, m2.e21)),
					e12 = Add(Mult(m1.e11, m2.e12), Mult(m1.e12, m2.e22)),
					e21 = Add(Mult(m1.e21, m2.e11), Mult(m1.e22, m2.e21)),
					e22 = Add(Mult(m1.e21, m2.e12), Mult(m1.e22, m2.e22))
				};
				return m;
			}

			public static Matrix operator +(Matrix m1, Matrix m2)
			{
				Matrix m = new Matrix
				{
					e11 = Add(m1.e11, m2.e11),
					e12 = Add(m1.e12, m2.e12),
					e21 = Add(m1.e21, m2.e21),
					e22 = Add(m1.e22, m2.e22)
				};
				return m;
			}

			public void Apply(ref long x, ref long y)
			{
				long x2 = Add(Mult(e11, x), Mult(e12, y));
				long y2 = Add(Mult(e21, x), Mult(e22, y));
				x = x2;
				y = y2;
			}

			public Matrix Pow(long p)
			{
				Matrix b = this;
				Matrix result = new Matrix(1, 0, 0, 1);
				while (p != 0)
				{
					if ((p & 1) != 0)
						result *= b;
					p >>= 1;
					b *= b;
				}
				return result;
			}

		}

		public static long Add(long left, long right)
		{
			return (left + right) % MOD;
		}



	}
}