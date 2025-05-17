namespace HackerRank.WorldCodeSprint8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;
	class DecibinaryNumbers
	{

		static long[] answers;
		static long[] numbers;
		static long max;

		static void Main(String[] args)
		{
			var q = int.Parse(Console.ReadLine());
			numbers = Enumerable.Range(0, q).Select(x => long.Parse(Console.ReadLine())).ToArray();
			max = Math.Max(10000000, numbers.Max());

			if (Verbose)
			{
				answers = new long[max];
			}

			Setup();
			foreach (var x in numbers)
			{
				var result = GetNumberDP(x - 1);

				if (Verbose)
				{
					var result2 = GetNumber(x - 1);
					Console.WriteLine($"{result} (compare {result2})");
				}
				else
				{
					Console.WriteLine(result);
				}
			}
		}

		static void Setup()
		{
			WaysToRepresentDP();


			if (Verbose)
			{
				var offsets = new int[max + 1];
				var values = new long[max + 1];

				for (int i = 0; i < max; i++)
				{
					var db = values[i] = DB(i);
					if (db > max) db = max;
					offsets[db]++;
				}

				int sum = 0;
				for (int i = 0; i <= max; i++)
				{
					int oldSum = sum;
					sum += offsets[i];
					offsets[i] = oldSum;
				}

				for (int i = 0; i < max; i++)
				{
					var db = values[i];
					if (db >= max) db = max;
					answers[offsets[db]++] = i;
				}

				/*
                for (int i = 0; i < 100; i++)
                    Console.WriteLine($"{i+1}) {answers[i]}");
                */

				for (int i = 0; i < 50; i++)
					Console.WriteLine(
						$"{i}-> {offsets[i]} +{offsets[i + 1] - offsets[i]} {DPLE(i)} +{DPLE(i + 1) - DPLE(i)}");
			}
		}

		static int Exponent = 50;
		static long[,] dp = new long[300001, Exponent];

		public static void WaysToRepresentDP()
		{
			int limit = dp.GetLength(0);
			for (int i = 0; i < limit; i++)
				dp[i, 0] = Math.Min(i, 9);

			long bits = 1;
			long nextPo2 = 2;

			int len = dp.GetLength(0);
			for (int i = 2; i < len; i++)
			{
				while (i >= nextPo2)
				{
					bits++;
					nextPo2 <<= 1;
				}

				for (int j = 0; j <= 9; j++)
				{
					var k = Math.Max(0, i - j);
					var halfk = k >> 1;
					for (int s = 1; s < bits; s++)
						dp[i, s] += dp[halfk, s - 1];
				}

				// if (i%50000 == 0) Console.WriteLine($"{i}->{DPLE(i)}");
			}

			if (Verbose)
			{

				Console.WriteLine();
				for (int j = -1; j < 25; j++)
				{

					Console.Write(" {0,8} |", j < 0 ? "" : j.ToString());

					for (int k = 0; k < 8; k++)
						Console.Write(" {0,8}", j < 0 ? k : dp[j, k]);

					Console.WriteLine();

					if (j == -1)
						Console.WriteLine(new String('-', 90));
				}
				Console.WriteLine();
			}
		}

		static long DPLE(int x)
		{
			long result = 0;
			for (int i = 0; i < Exponent; i++)
				result += dp[x, i];
			return result;
		}

		static long DB(long x)
		{
			long result = 0;
			long b = 1;
			while (x > 0)
			{
				result += (x % 10) * b;
				b *= 2;
				x /= 10;
			}
			return result;
		}

		static long GetNumber(long x)
		{
			if (x < 3) return x;
			if (x >= answers.Length) return -1;
			return answers[x];
		}

		static BigInteger GetNumberDP(long xParam)
		{
			long x = xParam;
			// 0, 1, 2 return as themselves
			if (x < 3) return x;

			// We remove zero because it is not recorded in the sums
			x--;

			// binary search
			int left = 0;
			int right = dp.GetLength(0) - 1;
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				if (x >= DPLE(mid))
					left = mid + 1;
				else
					right = mid - 1;
			}

			var value = left;
			long index = x - (right >= 0 ? DPLE(right) : 0);

			// Get the lexicographically ith string of value

			var result = GetValueString(value, index);
			return result;
		}

		public static BigInteger GetValueString(long value, long indexParam)
		{
			if (value < 2) return value;

			// Step 1: Figure out the number of digits

			long index = indexParam;
			int digits = 1;

			while (true)
			{
				var count = GetCountOfValues(value, digits);
				if (index < count)
					break;
				index -= count;
				digits++;
			}

			if (digits == 1)
				return value;

			// Step 2: Reconstruct the value recursively

			var b2 = 1;
			BigInteger b10 = 1;
			for (int i = 1; i < digits; i++)
			{
				b2 *= 2;
				b10 *= 10;
			}

			BigInteger result = 0;
			for (int d = 1; d <= 9; d++)
			{
				// How many are there with first digit d
				var remainder = value - d * b2;

				long dcount = 0;
				for (int i = 1; i < digits; i++)
					dcount += GetCountOfValues(remainder, i);

				if (index < dcount)
				{
					result = b10 * d + GetValueString(remainder, index);
					return result;
				}
				index -= dcount;
			}

			//throw new Exception();
			return 0;
		}

		public static long GetCountOfValues(long value, int digits)
		{
			if (value < 0) return 0;
			if (value == 0) return 1; // POTENTIAL BUG

			int k = digits - 1;
			long count = dp[value, k] - dp[value - 1, k];
			return count;
		}

		public const bool Verbose = false;
	}

}
