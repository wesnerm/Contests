
namespace HackerRank.AdInfinitum17
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;

	class DivisorExplorationII
	{

		static void Main(String[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			primes = PrimesList(200000);

			long backproduct = 1;
			for (int i = 1; i <= 100000; i++)
			{
				long p = primes[i - 1];
				backproduct = Mult(backproduct, Mult(p - 1, p - 1));
				ip2[i] = (int)Inverse(backproduct);
			}

			for (int a0 = 0; a0 < q; a0++)
			{
				int[] array = Console.ReadLine().Split().Select(int.Parse).ToArray();
				int m = array[0];
				int a = array[1];

				var answer = Solve(m, a);
				Console.WriteLine(answer);
			}
		}

		private static List<int> primes;
		private static int[] ip2 = new int[100001];


		static long Solve(long m, long a)
		{
			long result = 1;

			for (int i = 1; i <= m; i++)
			{
				var p = primes[i - 1];
				// int pow = a + i;
				// long f = Div(Pow(p, pow+1) - 1, p - 1);
				long f = Calc(p, a + i);
				result = Mult(result, f);
			}

			//return Fix(result);
			return Mult(result, ip2[m]);
		}

		static long Calc(long p, long k)
		{
			long sum = Fix(1L + k);
			sum = Subtract(sum, Mult(k + 2, p));
			sum = Fix(sum + ModPow(p, k + 2L));
			//var p2 = Fix((p - 1) * (p - 1));
			//sum = Div(sum, p2);
			return sum;
		}

		public static long Fix(long n)
		{
			return ((n % MOD) + MOD) % MOD;
		}

		public static List<int> PrimesList(int size)
		{
			List<int> result = new List<int>();
			int max = 10 * 1000 * 1000;
			var factors = PrimeFactorsUpTo(max);

			for (int i = 2; i < max; i++)
			{
				if (factors[i] == i)
				{
					result.Add(i);
					if (result.Count == size)
						break;
				}
			}

			return result;
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

		public const int MOD = 1000 * 1000 * 1000 + 7;

		static readonly int[] _inverse = new int[1000000];
		public static long Inverse(long n)
		{
			if (n < 0) { n = Fix(n); }

			long result;
			if (n < _inverse.Length && (result = _inverse[n]) != 0)
				return result - 1;

			result = ModPow(n, MOD - 2);
			if (n < _inverse.Length)
				_inverse[n] = (int)(result + 1);
			return result;
		}

		public static int Mult(long left, long right)
		{
			return (int)((left * right) % MOD);
		}

		public static int Div(long left, long divisor)
		{
			if (left < 0) { left = Fix(left); }

			if (left % divisor == 0)
				return (int)(left / divisor);

			return Mult(left, Inverse(divisor));
		}

		public static long Subtract(long left, long right)
		{
			return (left + (MOD - right)) % MOD;
		}

		public static long ModPow(long n, long p)
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
			return result;
		}

	}

}