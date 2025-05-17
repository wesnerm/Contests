namespace HackerRank.WorldCodeSprint10.MaximalAndSubsequence
{
	using System;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;
	using System.Numerics;

// Powered by caide (code generator, tester, and library code inliner)

	class Solution
	{

		const long MOD = 1000L * 1000L * 1000L + 7;

		static long[] solve(int n, int k, long[] a)
		{
			var list = new List<long>(a);

			// Find maximum bit diff
			var bitsAnd = -1L;
			var bitsOr = 0L;

			foreach (var v in list)
			{
				bitsAnd &= v;
				bitsOr |= v;
			}

			var bitsXor = bitsAnd ^ bitsOr;
			var maxAnd = bitsAnd;

			// Filter out list
			while (list.Count >= k)
			{
				var highestBit = HighestBit(bitsXor);
				if (highestBit == 0)
					break;

				var count = 0;
				foreach (var v in list)
				{
					if ((v & highestBit) != 0)
						count++;
				}

				// bit needs to be on 
				if (count >= k)
				{
					list.RemoveAll(x => (x & highestBit) == 0);
					maxAnd |= highestBit;
				}

				bitsXor &= ~highestBit;
			}

			bitsAnd = -1L;
			bitsOr = 0L;

			foreach (var v in list)
			{
				bitsAnd &= v;
				bitsOr |= v;
			}

			bitsXor = bitsAnd ^ bitsOr;
			maxAnd = bitsAnd;

			// Return result
			var countAnd = Choose(list.Count, k);
			;
			return new long[] {maxAnd, countAnd};
		}

		static long Choose(long n, long k)
		{
			if (k + k > n)
				k = n - k;

			BigInteger result = 1;

			for (long i = 0; i < k; i++)
				result = result * (n - i) / (i + 1);
			return (long) (result % MOD);
		}


		static long HighestBit(long v)
		{
			while (v - (v & -v) != 0)
				v -= (v & -v);
			return v;
		}


		public void solve(TextReader input, TextWriter output)
		{
			string[] tokens_n = input.ReadLine().Split(' ');
			int n = Convert.ToInt32(tokens_n[0]);
			int k = Convert.ToInt32(tokens_n[1]);
			long[] a = new long[n];
			for (int a_i = 0; a_i < n; a_i++)
			{
				a[a_i] = Convert.ToInt64(input.ReadLine());
			}
			long[] result = solve(n, k, a);
			output.WriteLine(String.Join("\n", result));
		}
	}
}