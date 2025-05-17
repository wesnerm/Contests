using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace HackerRank.WeekOfCode31.CollidingCircles
{
	using static Math;
	using static Console;
	using static FastIO;

	using Number = Double;

	public class Solution
	{
		#region Variables
		int n;
		int k;
		int[] r;
		//int maxk;
		int partitions;
		int maxPartitionSize;
		//int sizeLimit;
		int nLimit;
		#endregion

		public static void Main()
		{
			var sol = new Solution();
			sol.Run();
		}

		void Run()
		{
			n = Ni();
			k = Ni();
			r = Ni(n);
			Array.Sort(r);

			partitions = n - k;
			maxPartitionSize = k + 1;

			var ans = SmartSolve();
			WriteLine(ans * PI);
		}

		long[] naivelist;
		double Naive()
		{
			naivelist = new long[r.Length];
			for (int i = 0; i < naivelist.Length; i++)
				naivelist[i] = r[i];
			return NaiveDfs(n, k, naivelist.Sum(x => x * x));
		}

		double NaiveDfs(int n, int k, double sumsq)
		{
			double ans = 0;

			if (k == 0)
				ans = sumsq;
			else
			{
				double total = 0;
				int count = 0;
				for (int i = 0; i < n; i++)
				{
					long savei = naivelist[i];
					naivelist[i] = naivelist[n - 1];
					var savesquare0 = sumsq;
					sumsq -= savei * savei;
					for (int j = n - 2; j >= i; j--)
					{
						var savej = naivelist[j];
						naivelist[j] = savei + savej;

						var sumsqnew = sumsq + naivelist[j] * naivelist[j] - savej * savej;
						double result = NaiveDfs(n - 1, k - 1, sumsqnew);

						naivelist[j] = savej;
						total += result;
						count++;
					}
					sumsq = savesquare0;
					naivelist[i] = savei;
				}
				ans = total / count;
			}

#if DEBUG
			string listText = string.Join(", ", naivelist.Take(n));
			Console.Error.WriteLine($"{ans * Math.PI:0.000000000000} [{listText}]");
			if (k != 0) Console.Error.WriteLine();
#endif

			return ans;
		}

		double SmartSolve()
		{

			if (k == 0)
				return r.Sum(x => (long)x * x);

			if (partitions == 1)
				return Pow(r.Sum(), 2);

			var sumsquares = r.Sum(x => (double)x * x);
			var squaresum = Pow(r.Sum(x => (double)x), 2);
			var coproducts = squaresum - sumsquares;

			var result = sumsquares;
			var multiplier = 0.0;
			var probability = 1.0;

			long nn = n;
			for (int i = 0; i < k; i++)
			{
				var phit = 2.0 / ((nn - 1) * nn);
				multiplier += probability * phit;
				probability *= (1.0 - phit);
				nn--;
			}

			result += multiplier * coproducts;
			return result;
		}

		double Solve()
		{

			if (k == 0)
				return r.Sum(x => x * x);

			if (partitions == 1)
				return Pow(r.Sum(), 2);


			var sums = new Number[maxPartitionSize + 1];
			var squares = new Number[maxPartitionSize + 1];
			var counts = new long[maxPartitionSize + 1];

			double ysums = 0;
			double ysquares = 0;
			for (int i = 0; i < n; i++)
			{
				Number a = r[i];
				ysums += a;
				ysquares += a * a;
			}

			counts[0] = 1;
			for (int j = 1; j <= maxPartitionSize; j++)
			{
				Number x = sums[j - 1];
				var x2 = squares[j - 1];
				var cnts = counts[j - 1];
				var xa = n * x + cnts * ysums;
				var xa2 = n * x2 + 2 * ysums * x + cnts * ysquares;
				counts[j] += n * cnts;
				sums[j] += xa;
				squares[j] += xa2;
			}

			Number instanceSum = 0;
			Number c = 0;
			for (int i = 1; i <= maxPartitionSize; i++)
			{
				// Multiply the instances by the square of all possible combinations of 
				// Number of instances of each subset among all partitions
				// var stirling = Stirling(n - i, partitions - 1);
				var picked = Picked(k, i);
				var instances = picked * squares[i];
				c += picked * counts[i];
				instanceSum += instances;
			}

			// Number of partitions of size n-paritions of set r
			var totalPartitions = c / partitions;
			//var totalPartitions2 = Stirling(n, partitions);

			Console.Error.WriteLine($"instancesum: {instanceSum}");
			Console.Error.WriteLine($"totalPartitions: {totalPartitions}");

			double expected = (double)instanceSum / totalPartitions;
			return expected;
		}


		double Picked(int k, int size)
		{
			// Picked(4,2,1) = 12/4 = 3
			// Picked(4,2,2) = 12/6 = 2
			// Picked(4,2,3) = 12/4 = 3

			// Picked(5,2,1) = 90/5 = 18
			// Picked(5,2,2) = 60/10 = 6
			// Picked(5,2,3) = 30/10 = 3

			// divisor is comb(n, size)

			// 5 numbers
			// 2 possible choices
			// how manys to choose all but 1
			// Fact(4,2) or Comb(4,2)
			// how many ways to choose all
			// Fact(5,2) or Fact(4,2)

			int n = this.n + (k - this.k);
			if (k <= 0) return size == 1 ? 1 : 0;
			if (k + 1 < size) return 0;
			if (size < 1) return 0;

			double result;
			if (size <= 1)
			{
				result = 1;
				for (; k > 0; k--, n--)
				{
					var allpos = n * (n - 1) / 2;
					var hit = n - 1;
					result *= (allpos - hit);
				}
			}
			else
			{
				result = 0;
				var rest = n - size;
				if (rest > 1)
				{
					var pickedOther = Picked(k - 1, size);
					result += rest * (rest - 1) * pickedOther;
				}

				var pickedDoubles = Picked(k - 1, size - 1);
				var doubles = (double)size * (size - 1) * pickedDoubles;
				result += doubles;
				result /= 2.0;
			}

			return result;
		}

		static List<Number> _fact = new List<Number>(100001) { 1 };

		static Number Fact(int n)
		{
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(_fact[i - 1] * i);
			return _fact[n];
		}

		static Number Comb(int n, int k)
		{
			return k <= 1
				? (k == 1 ? n : k == 0 ? 1 : 0)
				: (k + k > n
					? Comb(n, n - k)
					: Fact(n) / Fact(k) / Fact(n - k));
		}
	}

	public static class Parameters
	{

		#region  Parameters

#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif

		#endregion
	}

}