namespace HackerRank.WomensCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static System.Math;
	using static HackerRankUtils;

	class SpringDecorating
	{
		static int[,] rgb;
		static int n;
		static int[,,,,,] dp = new int[11, 11, 11, 11, 11, 11];

		static void Main()
		{
			n = Read();
			int rb = Read();
			int rg = Read();
			int br = Read();
			int bg = Read();
			int gr = Read();
			int gb = Read();
			;

			rgb = new int[3, 3] {{0, rg, rb}, {gr, 0, gb}, {br, bg, 0}};

			long result = Dfs(0);
			if (rg == rb && gr == gb && br == bg && rg == gr && gb == bg)
				result *= 3;
			else
				result = Dfs(0) + Dfs(1) + Dfs(2);


			result %= MOD;
			Console.WriteLine(result);
		}

		static long[] cache = new long[100];

		static int Succ(int n) => n < 2 ? n + 1 : 0;
		static int Pred(int n) => n > 0 ? n - 1 : 2;

		static long Dfs(int prev, int c = 1)
		{
			if (c >= n) return 1;

			int succ = Succ(prev);
			int pred = Pred(prev);

			//if (rgb[prev,pred] > rgb[prev,succ])
			//    Swap(ref pred, ref succ);

			long count =
				dp[rgb[prev, pred], rgb[prev, succ], rgb[pred, prev], rgb[pred, succ], rgb[succ, prev], rgb[succ, pred]] - 1;
			if (count >= 0)
				return count;

			count = cache[c];
			if (count == 0)
				cache[c] = count = Comb2(n - 1, c - 1);

			for (int i = 0; i < 3; i++)
			{
				if (rgb[prev, i] <= 0) continue;
				rgb[prev, i]--;
				count += Dfs(i, c + 1);
				rgb[prev, i]++;
			}

			count = (count < MOD) ? count : count % MOD;
			dp[rgb[prev, pred], rgb[prev, succ], rgb[pred, prev], rgb[pred, succ], rgb[succ, prev], rgb[succ, pred]] =
				(int) (count + 1);
			return count;
		}
	}

	public static class HackerRankUtils
	{
		#region Mod Math

		public const int MOD = 1000 * 1000 * 1000 + 7;

		public static long Inverse(long n, long p = MOD)
		{
			return ModPow(n, p - 2);
		}

		public static long Mult(long left, long right)
		{
			return (left * right) % MOD;
		}

		public static long Subtract(long left, long right)
		{
			return (left + (MOD - right)) % MOD;
		}

		public static long Fix(long n)
		{
			return ((n % MOD) + MOD) % MOD;
		}

		public static long ModPow(long n, long p, long mod = MOD)
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

		#endregion

		#region Combinatorics

		static List<long> _fact;
		static List<long> _ifact;

		public static long Fact(int n)
		{
			if (_fact == null) _fact = new List<long>(100001) {1};
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public static long InverseFact(int n)
		{
			if (_ifact == null) _ifact = new List<long>(100001) {1};

			if (_ifact.Count <= n)
			{
				Fact(n);
				for (int i = _ifact.Count; i <= n; i++)
					_ifact.Add(Inverse(_fact[i]));
			}
			return _ifact[n];
		}

		public static long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
		}

		public static long Comb2(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb2(n, n - k);
			long result = 1;
			long den = 1;
			for (int i = 0; i < k; i++)
			{
				result = result * (n - i) % MOD;
				den = den * (i + 1) % MOD;
			}
			return result * Inverse(den) % MOD;
		}

		#endregion

		#region Input

		public static string[] ReadArray()
		{
			int n = int.Parse(Console.ReadLine());
			string[] array = new string[n];
			for (int i = 0; i < n; i++)
				array[i] = Console.ReadLine();
			return array;
		}

		public static int[] ReadInts()
		{
			return Console.ReadLine().Split().Select(int.Parse).ToArray();
		}

		#endregion

		#region Arrays

		public static int BinarySearch<T>(T[] array, T value, int left, int right, bool upper = false)
			where T : IComparable<T>
		{
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				int cmp = value.CompareTo(array[mid]);
				if (cmp > 0 || cmp == 0 && upper)
					left = mid + 1;
				else
					right = mid - 1;
			}
			return left;
		}

		#endregion

		#region Common

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		#endregion


		public static int[] ReadNumbers(int n)
		{
			var list = new int[n];
			for (int i = 0; i < n; i++)
				list[i] = Read();
			return list;
		}

		public static int Read()
		{
			int number = 0;
			bool hasNum = false;
			while (true)
			{
				int c = Console.Read();
				if (c < 0) break;
				int d = c - '0';
				if (d >= 0 && d <= 9)
				{
					number = number * 10 + d;
					hasNum = true;
					continue;
				}

				if (hasNum == true)
					break;
			}
			return number;
		}

	}
}