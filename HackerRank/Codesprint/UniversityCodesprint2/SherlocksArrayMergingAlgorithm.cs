namespace HackerRank.UniversityCodesprint2.SherlocksArrayMergingAlgorithm
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using static System.Math;
	using static System.Console;
	using static HackerRankUtils;

	public class Solution
	{
		#region Variables


		#endregion

		public static void Main()
		{
			var sol = new Solution();
			sol.Run();
		}

		int[] array;
		int[] longestLength;
		int[,,] dp;

		static int Slack = 60;

		void Run()
		{
			int n = int.Parse(ReadLine());
			array = Array.ConvertAll(ReadLine().Split(), int.Parse);
			longestLength = new int[n];

			for (int len = 0, index = n - 1; index >= 0; index--)
			{
				if (len > 0 && array[index] > array[index + 1])
					len = 0;
				longestLength[index] = ++len;
			}


			int maxParts = longestLength[0];
			Slack = Min(maxParts, Slack);

			dp = new int[array.Length + 1, maxParts + 1, Slack];

			long answer = Compute(n, maxParts);
			WriteLine(answer);
		}

		long Compute(int length, int partitionsParam)
		{
			var partitions = partitionsParam;
			if (length == 0) return 1;

			int index = array.Length - length;
			partitions = Min(partitions, longestLength[index]);

			if (partitions == 1) return partitionsParam;

			var slack = partitionsParam - partitions;
			if (slack < Slack)
			{
				if (dp[index, partitions, slack] != 0)
					return dp[index, partitions, slack] - 1;
			}

			long answer = 0;
			for (int i = partitions; i >= 1; i--)
			{
				var computedAnswer = Compute(length - i, i);
				var multiplied = computedAnswer;
				if (index > 0 && partitionsParam > 1)
					multiplied = Mult(multiplied, Fact(partitionsParam, i));
				answer += multiplied;
			}

			answer %= MOD;

			if (slack < Slack)
				dp[index, partitions, slack] = (int)answer + 1;

			//Console.WriteLine($"C({length},{partitionsParam})->C({length},{partitions})->{answer}");
			return answer;
		}


#if DEBUG
		public static bool Verbose = true;
#else
        public static bool Verbose = false;
#endif
	}


	public static class HackerRankUtils
	{
		#region Mod Math
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
				_inverse[n] = (int)(result + 1);
			return result;
		}

		public static long Mult(long left, long right)
		{
			return (left * right) % MOD;
		}

		public static long Div(long left, long divisor)
		{
			return left % divisor == 0
				? left / divisor
				: Mult(left, Inverse(divisor));
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
			if (_fact == null) _fact = new List<long>(1200) { 1 };
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public static long Fact(int n, int m)
		{
			var fact = Fact(n);
			if (m < n)
				fact = Mult(fact, InverseFact(n - m));
			return fact;
		}


		public static long InverseFact(int n)
		{
			if (_ifact == null) _ifact = new List<long>(1200) { 1 };
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

		#endregion
	}

}