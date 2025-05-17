namespace HackerRank.AdInfinitum18.RationalSums
{
	// https://www.hackerrank.com/contests/infinitum18/challenges/rational-sums

	using System;
	using System.Collections.Generic;
	using System.IO;
	using static FastIO;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();
		}

		int[] a;
		int[] b;
		int[] residues;


		public void solve()
		{
			int n = Ni();
			a = Ni(n);
			b = Ni(n - 1);

			residues = new int[n];

			long maxq = 0;

			// Calculate residues
			for (int i = 0; i < n; i++)
			{
				var q = a[i];
				if (q > maxq) maxq = q;
				var qinv = q != 0 ? MOD - q : 0;
				long num = P(qinv);
				long den = Q(qinv);
				residues[i] = (int)Div(num, den);
			}

			long result = 0;
			for (int i = 0; i < n; i++)
			{
				var q = a[i];
				var r = residues[i];
				for (int j = q + 1; j <= maxq; j++)
					result += Inverse(j) * r % MOD;
				result %= MOD;
			}

			WriteLine(result);
		}

		public long P(long n)
		{
			long result = 0;
			for (int i = b.Length - 1; i >= 0; i--)
			{
				result = result * n % MOD + b[i];
				if (result >= MOD)
					result -= MOD;
			}
			return result;
		}

		public long Q(long n)
		{
			long result = 1;
			foreach (var aa in a)
			{
				var f = n + aa;
				if (f >= MOD) f -= MOD;
				if (f != 0) result = result * f % MOD;
			}
			return result;
		}



		#region Mod Math
		public const int MOD = 1000 * 1000 * 1000 + 7;

		static int[] _inverse;
		public static long Inverse(long n)
		{
			long result;

			if (_inverse == null)
				_inverse = new int[10000];

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
				: left * Inverse(divisor) % MOD;
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
			if (_fact == null) _fact = new List<long>(100) { 1 };
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public static long InverseFact(int n)
		{
			if (_ifact == null) _ifact = new List<long>(100) { 1 };
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

		#region Common

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		public static void Clear<T>(T[] t, T value = default(T))
		{
			for (int i = 0; i < t.Length; i++)
				t[i] = value;
		}

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


	}



	public static class Parameters
	{
#if DEBUG
		public const bool Verbose = true;
#else
	public const bool Verbose = false;
#endif
	}

}