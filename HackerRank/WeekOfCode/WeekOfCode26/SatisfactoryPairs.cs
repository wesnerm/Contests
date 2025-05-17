namespace HackerRank.WeekOfCode26
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using static System.Math;

	public class SatisfactoryPairs
	{
		public static void Main()
		{
			int n = int.Parse(Console.ReadLine());

			// var sol = new Solution(n);
			//Time("BruteForce", sol.BruteForce);
			//Time("GcdApproach", sol.GcdApproach);
			//Time("KillerApproach", sol.GcdApproach);

			Time("KillerApproach", () =>
			{
				var sol = new SatisfactoryPairs(n);
				return sol.KillerApproach();
			});
		}

		public static void Time(string name, Func<long> func)
		{
			Console.WriteLine(func());
		}

		public static void Time2(string name, Func<long> func)
		{
			var sw = new System.Diagnostics.Stopwatch();
			sw.Restart();
			var result = func();
			sw.Stop();

			Console.WriteLine(name + ":");
			Console.WriteLine($"Result:  {result}");
			Console.WriteLine("Elapsed: " + sw.Elapsed);
			Console.WriteLine();
		}

		int n = 0;
		int[] factors;

		public SatisfactoryPairs(int n)
		{
			this.n = n;
			this.factors = PrimeFactorsUpTo(n);
		}

		public long BruteForce()
		{
			long count = 0;
			for (long x = 1; x < n; x++)
			{
				for (long y = n - x; y > x; y--)
				{
					long g = gcd(x, y);
					if (n % g == 0 && BruteForce(x, y, g))
					{
						//Console.Write($" ({x},{y})");
						count++;
					}
				}
			}
			//Console.WriteLine();
			return count;
		}

		public bool BruteForce(long x, long y, long g)
		{
			for (long z = (n - y) / x; z >= 1; z--)
			{
				if ((n - z * x) % y == 0)
					return true;
			}
			return false;
		}

		public bool FastCheck(long x, long y)
		{
			long xf, yf;
			// Console.WriteLine($" {xf} * {x} + {yf} * {y} = {g}");
			long g = Gcd(x, y, out xf, out yf);
			if (n % g != 0)
				return false;

			var div = n / g;
			xf *= div;
			yf *= div;

			// long xf0 = xf, yf0 = yf;

			double d = g;
			if (xf <= 0)
			{
				long k = (long)Math.Ceiling((1 - xf) / (y / d));
				xf += (k * y / g);
				yf -= (k * x / g);
			}
			else if (yf <= 0)
			{
				long k = (long)Math.Ceiling((1 - yf) / (x / d));
				xf -= (k * y / g);
				yf += (k * x / g);
			}


			/* else if (BruteForce(x,y,g))
                    {
                        // Console.WriteLine($" ==> {xf0} * {x} + {yf0} * {y} = {xf} * {x} + {yf} * {y} = {n}");
                        count++;
                    } */

			return xf > 0 && yf > 0;
		}

		public long GcdApproach()
		{
			long count = 0;
			for (long x = 1; x < n; x++)
				for (long y = n - x; y > x; y--)
				{
					if (FastCheck(x, y))
						count++;
				}
			return count;
		}

		// 2*3*5*7*11*13*17 = 510510 > 3*10^5

		int[] ts;
		int timestamp = 1;
		public long KillerApproach()
		{
			ts = new int[n];
			long add = 0;
			Action<int> action = delegate (int f)
			{
				if (ts[f] >= timestamp) return;
				add++;
				ts[f] = timestamp;
			};

			// Requirement: x > y
			long cnt = 0;
			// We start at 2 because 1 is smallest positive integer
			for (int x = 2; x < n; x++)
			{
				timestamp++;
				for (int a = (n + x - 1) / x - 1; a > 0; a--)
				{
					int by = n - a * x;

					// Find all factors < x 
					add = 0;
					EnumerateFactors(factors, by, x, action);
					cnt += add;

					//Console.WriteLine($"x={x} a={a} by={by} added {add}");
					//Console.WriteLine(string.Join(" ", Enumerable.Range(0,size).Select(i=>$"{primes[i]}^{counts[i]}")));
				}
			}

			return cnt;
		}

		public static long EnumerateFactors(int[] factors, int n, int max, Action<int> action = null, int f = 1)
		{
			if (max <= f)
				return 0;

			if (n == 1)
			{
				if (action != null) action(f);
				return 1;
			}

			int p = factors[n];
			int c = 1;
			int next = n / p;
			while (next > 1 && factors[next] == p)
			{
				c++;
				next = next / p;
			}

			long result = EnumerateFactors(factors, next, max, action, f);
			while (c-- > 0)
			{
				f *= p;
				result += EnumerateFactors(factors, next, max, action, f);
			}

			return result;
		}

		public long gcd(long x, long y)
		{
			while (x != 0)
			{
				var tmp = y % x;
				y = x;
				x = tmp;
			}
			return y;
		}


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

		public static IEnumerable<int> PrimeFactorsOf(int[] table, int n)
		{
			int prev = 0;
			int k = n;
			while (k > 1)
			{
				int next = table[k];
				if (next != prev) yield return next;
				k /= next;
				prev = next;
			}
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

	}
}
