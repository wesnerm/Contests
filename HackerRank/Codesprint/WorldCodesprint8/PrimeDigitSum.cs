namespace HackerRank.WorldCodesprint8
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class PrimeDigitSum
	{

		static void Main(String[] args)
		{
			Setup();

			int q = int.Parse(Console.ReadLine());

			var list = Enumerable.Range(0, q).Select(x => int.Parse(Console.ReadLine())).ToArray();
			foreach (var n in list)
			{
				var count = CountDPrimes(n);
				Console.WriteLine(count);
			}
		}

		const int limit = 10000;
		static List<int> threes = new List<int>();
		static List<int> fours = new List<int>();
		static List<int> fives = new List<int>();
		static List<int> fivesPruned = new List<int>();
		static int maxCalced = 0;

		static long[] tables = new long[10000];
		static long[] tablests = new long[10000];
		static long[] scratch = new long[10000];
		static long[] scratchts = new long[10000];
		static long ts = 1;

		static long[] counts = new long[500000];
		public static long CountDPrimes(int nParam)
		{
			for (int n = maxCalced + 1; n <= nParam; maxCalced = n++)
			{
				var tts = ts++;
				long count = 0;
				foreach (var f in fivesPruned)
				{
					var fd10 = f / 10;
					var fml = f % limit;
					var tmp = tablests[fd10] == tts ? tables[fd10] : 0;
					if (scratchts[fml] == ts)
					{
						scratch[fml] = (scratch[fml] + tmp) % MOD;
					}
					else
					{
						scratch[fml] = tmp;
						scratchts[fml] = ts;
					}
					count = (count + tmp) % MOD;
				}
				counts[n] = count % MOD;
				Swap(ref tables, ref scratch);
				Swap(ref tablests, ref scratchts);
			}
			return counts[nParam];
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		static BitArray isDPrime3 = new BitArray(1000);
		static BitArray isDPrime4 = new BitArray(10000);
		static BitArray isDPrime5 = new BitArray(100000);
		public static void Setup()
		{
			int count3_0 = 0, count3_1 = 0;
			int count4_0 = 0, count4_1 = 0;
			int count5_0 = 0, count5_1 = 0;

			var sums3 = new int[1000];
			var sums4 = new int[10000];

			// Threes
			for (int i = 0; i <= 999; i++)
			{
				var dsum = (i % 10) + (i / 100) + ((i / 10) % 10);
				sums3[i] = dsum;
				if (!IsPrime(dsum)) continue;
				isDPrime3[i] = true;
				threes.Add(i);
				if (i < 100) count3_0++; else count3_1++;
			}

			// Fours
			for (int j = 0; j <= 9999; j++)
			{
				var dsum3 = sums3[j / 10];
				var dsum4 = dsum3 + j % 10;
				sums4[j] = dsum4;
				if (!IsPrime(dsum4)
					|| !isDPrime3[j / 10]
					|| !isDPrime3[j % 1000])
					continue;
				isDPrime4[j] = true;
				fours.Add(j);
				if (j < 1000) count4_0++; else count4_1++;
			}

			// Fives
			for (int k = 0; k <= 99999; k++)
			{
				var dsum4 = sums4[k / 10];
				var dsum5 = dsum4 + k % 10;
				if (!IsPrime(dsum5)
				   || !isDPrime4[k / 10]
				   || !isDPrime4[k % 10000])
					continue;
				if (k < 10000) count5_0++;
				else
				{
					count5_1++;
					var kml = k % limit;
					tables[kml]++;
					tablests[kml] = ts;
				}
				isDPrime5[k] = true;
				fives.Add(k);
			}

			var hashSet = new HashSet<int>();
			foreach (var f in fives)
			{
				var start = (f % 10000) * 10;
				int index = fives.BinarySearch(start);
				if (index < 0) index = ~index;
				if (index >= fives.Count || fives[index] > start + 9)
				{
				}
				else
				{
					hashSet.Add(f);
					while (index < fives.Count && fives[index] <= start + 9)
						hashSet.Add(fives[index++]);
				}
			}
			fivesPruned.AddRange(hashSet);
			fivesPruned.Sort();

			var sixes = new List<int>();
			if (Verbose)
			{
				int count6 = 0;
				for (int i = 100000; i <= 999999; i++)
				{
					if (isDPrime5[i / 10] && isDPrime5[i % 100000])
					{
						count6++;
						sixes.Add(i);
					}
				}
				Console.WriteLine($"count6 = {count6}");
			}

			counts[0] = 1;
			counts[1] = 9;
			counts[2] = 90;
			counts[3] = count3_1;
			counts[4] = count4_1;
			counts[5] = count5_1;
			maxCalced = 5;
		}

		private const long PrimesUnder64 = 0
				| 1L << 02 | 1L << 03 | 1L << 05 | 1L << 07
				| 1L << 11 | 1L << 13 | 1L << 17 | 1L << 19
				| 1L << 23 | 1L << 29
				| 1L << 31 | 1L << 37
				| 1L << 41 | 1L << 43 | 1L << 47
				| 1L << 53 | 1L << 59
				| 1L << 61;

		public static bool IsPrime(int n)
		{
			if (n < 0 || n > 62) return false;
			return (PrimesUnder64 & (1L << n)) != 0;
		}

		public const int MOD = 1000 * 1000 * 1000 + 7;
		public const bool Verbose = false;
	}

}