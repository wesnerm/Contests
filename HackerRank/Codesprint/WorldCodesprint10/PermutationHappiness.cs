namespace HackerRank.WorldCodeSprint10.PermutationHappiness
{
	using System;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;

// Powered by caide (code generator, tester, and library code inliner)

	class Solution
	{
		const int maxsize = 3000;
		int[,] cache = new int[maxsize + 1, maxsize + 1];

		long happy(int n, int k)
		{
			if (k == 0) return n <= 1 ? 1 : 0;
			if (k >= n) return 0;
			if (k < (n >> 1)) return 0;

			var result = cache[n, k] - 1;
			if (result >= 0) return result;

			long count = 0;

			for (int i = 1; i <= n; i++)
			{
				long count2 = 0;
				for (int j = 0; j < k; j++)
				{
					var lhsCount = happy(i - 1, (k - 1) - j);
					if (lhsCount == 0) continue;

					var rhsCount = happy(n - i, j);
					if (rhsCount == 0) continue;

					count2 += lhsCount * rhsCount % MOD;
				}

				count += count2 % MOD * Comb(n - 1, i - 1) % MOD;
			}

			result = (int) (count % MOD);
			cache[n, k] = result + 1;
			return result;
		}


		long query(int n, int k)
		{
			if (k >= n) return 0;

			long result = 0;
			for (int i = k; i <= n; i++)
				result += happy(n, i);

			return result % MOD;
		}


		void Swap(ref int a, ref int b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}


		public const int MOD = 1000 * 1000 * 1000 + 7;

		static int[] _inverse;

		public static long Inverse(long n)
		{
			long result;

			if (_inverse == null)
				_inverse = new int[3000];

			if (n < _inverse.Length && (result = _inverse[n]) != 0)
				return result - 1;

			result = ModPow(n, MOD - 2);
			if (n < _inverse.Length)
				_inverse[n] = (int) (result + 1);
			return result;
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

		static List<long> _fact;
		static List<long> _ifact;

		public static long Fact(int n)
		{
			if (_fact == null) _fact = new List<long>(3000) {1};
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add((_fact[i - 1] * i) % MOD);
			return _fact[n];
		}

		public static long InverseFact(int n)
		{
			if (_ifact == null) _ifact = new List<long>(3000) {1};
			for (int i = _ifact.Count; i <= n; i++)
			{
				long left = _ifact[i - 1];
				_ifact.Add(left % (long) i == 0 ? left / i : (left * Inverse(i)) % MOD);
			}
			return _ifact[n];
		}

		public static long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return ((Fact(n) * InverseFact(k)) % MOD * InverseFact(n - k)) % MOD;
		}


		public void solve(TextReader input, TextWriter output)
		{
			int q = Convert.ToInt32(input.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				string[] tokens_n = input.ReadLine().Split();
				int n = Convert.ToInt32(tokens_n[0]);
				int k = Convert.ToInt32(tokens_n[1]);

				var result = query(n, k);
				output.WriteLine(result);
			}
		}
	}

	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			Solution solution = new Solution();
			using (System.IO.TextReader input =
				CaideConstants.InputFile == null ? System.Console.In : new System.IO.StreamReader(CaideConstants.InputFile))
			using (System.IO.TextWriter output =
				CaideConstants.OutputFile == null ? System.Console.Out : new System.IO.StreamWriter(CaideConstants.OutputFile))

				solution.solve(input, output);
		}
	}

}