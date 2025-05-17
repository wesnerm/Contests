namespace HackerRank.AdInfinitum17
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class BirthdayTriplets
	{

		static void Main(string[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				var array = Array.ConvertAll(Console.ReadLine().Split(), long.Parse);
				f2 = array[0];
				f3 = array[1];
				f4 = array[2];
				long left = array[3];
				long right = array[4];

				List<long[]> candidates127 = new List<long[]>();
				List<long[]> candidates128 = new List<long[]>();
				Find(candidates127, f2, f3, f4, sqrts127);
				Find(candidates128, f2, f3, f4, sqrts128);
				//Console.WriteLine($"candidates127 {candidates127.Count}");
				//Console.WriteLine($"candidates128 {candidates128.Count}");


				List<long[]> options = new List<long[]>();

				foreach (var cand1 in candidates127)
					foreach (var cand2 in candidates128)
						Try(options, cand1[0], cand1[1], cand1[2], cand2);

				//Console.WriteLine($"Sort {options.Count}");
				options.Sort((x, y) =>
				{
					int cmp = x[0].CompareTo(y[0]);
					if (cmp != 0) return cmp;
					cmp = x[1].CompareTo(y[1]);
					if (cmp != 0) return cmp;
					return x[2].CompareTo(y[2]);
				});

				//Console.WriteLine($"Indexers {options.Count}");

				long a, b, c;

				if (options.Count == 0)
				{
					Find(out a, out b, out c, f2, f3, f4);
				}
				else
				{
					a = options[0][0];
					b = options[0][1];
					c = options[0][2];
				}

				var leftSum = Sum(left - 1, a, b, c);
				var rightSum = Sum(right, a, b, c);
				var sum = Subtract(rightSum, leftSum);
				Console.WriteLine(sum);
			}
		}

		static long f2, f3, f4;


		public static void Try(List<long[]> options, long f1, long f2, long f3, long[] g)
		{
			Try2(options, f1, f2, f3, g);
			Try2(options, f1, f3, f2, g);
			Try2(options, f2, f1, f3, g);
			Try2(options, f2, f3, f1, g);
			Try2(options, f3, f1, f2, g);
			Try2(options, f3, f2, f1, g);
		}

		public static void Try2(List<long[]> options,
			long ca1, long ca2, long ca3, long[] g)
		{
			long aa = Calc(ca1, g[0]);
			long bb = Calc(ca2, g[1]);
			long cc = Calc(ca3, g[2]);
			Sort(ref aa, ref bb, ref cc);

			var a2 = aa * aa;
			var b2 = bb * bb;
			var c2 = cc * cc;
			if (aa + bb + cc > 15000
				|| a2 + b2 + c2 != f2
				|| a2 * aa + b2 * bb + c2 * cc != f3
				|| a2 * a2 + b2 * b2 + c2 * c2 != f4)
				return;

			options.Add(new[] { aa, bb, cc });
		}

		public static long Calc(long x, long y)
		{
			return (1L * 128L * x + 127L * 127L * y) % (128L * 127L);
		}

		private static readonly List<int>[] sqrts127 = SquareRoots(127);
		private static readonly List<int>[] sqrts128 = SquareRoots(128);

		public static long Sum(long n, long a, long b, long c)
		{
			long sum = Sum(n, a) + Sum(n, b) + Sum(n, c);
			return Fix(sum);

		}

		public static long Sum(long n, long a)
		{
			if (a == 1)
				return n + 1;

			long sum = Div(ModPow(a, n + 1) - 1, a - 1);
			return sum;
		}

		public static void Find(out long a, out long b, out long c,
			long f2, long f3, long f4)
		{
			const long Max = 15 * 1000;
			b = c = 0;
			for (a = 1; a < Max / 3; a++)
			{
				long a2 = a * a;
				var max = Math.Min((Max - a) / 2, Max - a2 - (a + 2) * (a + 2));
				for (b = a + 1; b <= max; b++)
				{
					long b2 = b * b;
					long aabb = a2 + b2;
					long rem = f2 - aabb;
					if ((b + 1) * (b + 1) > rem)
						break;

					c = (long)Math.Sqrt(rem);
					long c2 = c * c;
					if (aabb + c2 == f2
						&& a2 * a + b2 * b + c2 * c == f3
						&& a2 * a2 + b2 * b2 + c2 * c2 == f4)
						return;
				}
			}

			a = b = c = 0;
		}

		public static List<int>[] SquareRoots(int mod)
		{
			var roots = new List<int>[mod];
			for (int i = 0; i < mod; i++)
				roots[i] = new List<int>();
			for (int i = 0; i < mod; i++)
				roots[i * i % mod].Add(i);
			return roots;
		}

		public static void Find(List<long[]> candidates,
		long f2, long f3, long f4, List<int>[] roots)
		{
			int mod = roots.Length;
			f2 %= mod;
			f3 %= mod;
			f4 %= mod;

			int aceil = mod - 1; // mod / 3;
			int bceil = mod - 1; // mod / 2;

			for (long aa = 0; aa <= aceil; aa++)
			{
				long a2 = aa * aa;
				for (long bb = aa; bb <= bceil; bb++)
				{
					long b2 = bb * bb;
					long aabb = a2 + b2;
					long c2 = ((f2 - aabb) % mod + mod) % mod;

					// f4
					if ((a2 * a2 + b2 * b2 + c2 * c2 - f4) % mod != 0)
						continue;

					foreach (var cc in roots[c2])
					{
						if (cc < bb) continue;
						if ((a2 * aa + b2 * bb + c2 * cc - f3) % mod == 0)
						{
							var cand = new long[] { aa, bb, cc };
							Sort(ref cand[0], ref cand[1], ref cand[2]);
							candidates.Add(cand);
						}
					}
				}
			}
		}


		public static void Sort(ref long a, ref long b, ref long c)
		{
			if (a > c)
				Swap(ref a, ref c);
			if (a > b)
				Swap(ref a, ref b);
			if (c < b)
				Swap(ref b, ref c);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
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


		public static long Fix(long n)
		{
			return ((n % MOD) + MOD) % MOD;
		}


	}
}