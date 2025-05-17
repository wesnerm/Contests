namespace HackerRank.WeekOfCode26.TastesLikeWinning
{

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using static System.Math;

	class Solution
	{
		static Stopwatch watch;

		static void Main(String[] args)
		{

			watch = new Stopwatch();
			watch.Start();

			var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
			var sol = new Solution(arr[0], arr[1]);

			if (Verbose == false)
			{
				var result = sol.Precheck();
				if (result >= 0)
				{
					Console.WriteLine(result);
					return;
				}
			}
			else
			{
				Time("Precheck", sol.Precheck);
				Time("BruteForce", sol.BruteForce);
			}

			Time("Compute", sol.Compute);
		}

		public static void Time(string name, Func<long> func)
		{
			if (Verbose)
			{
				var time = watch.Elapsed;
				var result = func();
				time = watch.Elapsed - time;

				Console.WriteLine(name + ":");
				Console.WriteLine($"Result:  {result}");
				Console.WriteLine("Elapsed: " + time);
				Console.WriteLine();
			}
			else
			{
				var result = func();
				Console.WriteLine($"{result}");
			}
		}

		#region Math

		static List<long> _fact = new List<long>(100) {1};
		static List<long> _ifact = new List<long>(100) {1};

		public static long Fact(int n)
		{
			if (n > 100) return Fact(n, n);
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public static long Fact(int n, int k)
		{
			if (k > n) return 0;

			long result = 1;
			for (int i = 0; i < k; i++)
				result = Mult(result, n - i);
			return result;
		}

		public static long InverseFact(int n)
		{
			if (n > 100) return Inverse(Fact(n, n));
			for (int i = _ifact.Count; i <= n; i++)
				_ifact.Add(Div(_ifact[i - 1], i));
			return _ifact[n];
		}

		public static long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
		}

		public const int MOD = 1000 * 1000 * 1000 + 7;


		public static long Gcd(long x, long y, out long xf, out long yf)
		{
			if (x == 0)
			{
				xf = 0;
				yf = 1;
				return y;
			}

			long xf2;
			long gcd = Gcd(y % x, x, out yf, out xf2);
			xf = xf2 - yf * (y / x);
			return gcd;
		}

		public static int ModulusInverse(int x, long mod = MOD)
		{
			long inverse, modFactor;
			Gcd(x, mod, out inverse, out modFactor);
			if (inverse <= 0)
				inverse += (int) (Math.Ceiling(-inverse / (double) mod) * mod);
			return (int) inverse;
		}


		static readonly int[] _inverse = new int[3000];

		public static long Inverse(long n)
		{
			long result;
			if (n < _inverse.Length && (result = _inverse[n]) != 0)
				return result - 1;

			result = ModPow(n, MOD - 2);
			if (n < _inverse.Length)
				_inverse[n] = (int) (result + 1);
			return result;
		}

		public static long Mult(long left, long right)
		{
			return (left * right) % MOD;
		}

		public static long Div(long left, long divisor)
		{
			if (left % divisor == 0)
				return left / divisor;

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

		public static long ModPow(long n, long p, long mod)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result = (result * b) % mod;
				p >>= 1;
				b = (b * b) % mod;
			}
			return result;
		}


		public static long Pow(long n, long p)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result *= b;
				p >>= 1;
				b *= b;
			}
			return result;
		}

		public static bool CheckTimeout()
		{
			if (PreventTimeout && watch.ElapsedMilliseconds > Timeout)
			{
				if (Verbose) Console.WriteLine($"Timed out at {watch.Elapsed}...");
				return true;
			}
			return false;
		}

		#endregion

		int n;
		int m;
		int tmm1;
		private int[] f;


		public Solution(int n, int m)
		{
			this.n = n;
			this.m = m;
			this.tmm1 = ComputeTmm1(m);
			f = new int[n + 1];
			int factor = 0;
			for (int i = 1; i < f.Length; i++)
				f[i] = factor = (factor * i) % MOD;
		}

		public static int ComputeTmm1(int m)
		{
			return (int) Subtract(ModPow(2, m), 1);
		}

		public long Precheck()
		{
			return Precheck(n, m);
		}

		public long Precheck(int n, int m)
		{
			if (n == 0 || m == 0)
				return 0;

			var tmm1 = ComputeTmm1(m);

			if (m < 28)
			{
				// if n > 2^m - 1, 0
				// if n = 2^m - 1, 
				// n%4=0, nimsum=n   --> F(2^m-1)
				// n%4=1, numsum=1   --> F(2^m-1)
				// n%4=2, nimsum=n+1 --> F(2^m-1)
				// n%4=3, nimsum=0   --> 0

				int max = (1 << m) - 1;
				if (max < n) return 0;
				if (max == n)
				{
					if (n % 4 == 3) return 0;
					return Fact(tmm1, n);
				}
			}

			// When n=1, no cancellation
			// When n=2, no cancellation because 2 values are distinct
			switch (n)
			{
				case 1:
				case 2:
					return Fact(tmm1, n);
				case 3:
				case 4:
					return Fact(tmm1, n) - Precheck(n - 1, m);
			}

			// 1<= n, m <= 10^7

			return -1;
		}

		public long BruteForce()
		{
			return BruteForce(tmm1, 0, n);
		}

		public long BruteForce(int max, long xor, int remaining)
		{
			if (remaining == 0)
				return xor != 0 ? 1 : 0;
			long count = 0;
			long addend = 0;
			long msb = xor;
			while (msb > 0 && (msb & ~-msb) != 0) msb &= ~-msb;

			for (; max >= remaining; max--)
			{

				if (max < msb)
				{
					// none of the numbers can set msb to zero
					count += Fact(max, remaining);
					break;
				}

				addend += BruteForce(max - 1, xor ^ max, remaining - 1);
			}

			count += Mult(remaining, addend % MOD);
			return count % MOD;
		}

		int[] cache;

		public long Compute()
		{
			// The game starts with n piles and each pile contains less than 2^m stones.

			// All piles = n*(2^m-1)
			// All piles with different values = F(2^m-1,n)
			// All piles with diff values that win = 
			// How many different ways is there to make an xor zero using n digits from 1...2^m-1

			// Easier to find zeros
			var zeros = ComputeZeros(n);
			var allpiles = Fact(tmm1, n);
			var goodpiles = Subtract(allpiles, zeros);
			return goodpiles;
		}

		public long ComputeZeros(int n)
		{
			if (n <= 2)
				return 0;

			long prev2 = 0;
			long prev1 = 0;
			long factor = tmm1 - 1;
			long fact = tmm1;
			for (int i = 3; i <= n; i++)
			{
				fact = Mult(fact, factor--);
				var case1 = Subtract(fact, prev1);
				var case2 = Mult(prev2, i - 1) * (tmm1 - (i - 2)) % MOD;
				var sum = (case1 - case2) % MOD;
				//Console.WriteLine($"{i}) p={prev1} p2={prev2} c1={case1} c2={case2} sum={sum} {f1} {f2}");

				prev2 = prev1;
				prev1 = sum;
			}

			return prev1;
		}

		static bool Verbose = false;
		const bool PreventTimeout = true;
		const int Timeout = 2800;
		const int TestCase = 1000 * 1000 * 10;
	}
}