namespace HackerRank.WeekOfCode26
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Twins
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */
			var a2 = Console.ReadLine().Split();
			var a = a2.Select(int.Parse).ToArray();
			int lo = a[0];
			int hi = a[1];

			int cnt = 0;
			var primes = GetPrimeSet(lo, hi);

			if (lo <= 3 && hi >= 5)
				cnt++;

			for (int i = lo + 6 - (lo % 6); i < hi; i += 6)
			{
				if (primes[i - lo - 1] && primes[i - lo + 1])
					cnt++;
			}
			Console.WriteLine(cnt);
		}

		public static BitArray GetPrimeSet(int max)
		{
			var isPrime = new BitArray(max + 1, true);

			isPrime[0] = false;
			isPrime[1] = false;

			for (int i = 2; i <= max; i += 2)
				isPrime[i] = false;

			for (int i = 3; i <= max; i += 2)
			{
				if (isPrime[i])
					for (long j = (long)i * i; j <= max; j += i + i)
						isPrime[(int)j] = false;
			}

			return isPrime;
		}

		public static BitArray GetPrimeSet(int lo, int hi)
		{
			if (lo == 0) return GetPrimeSet(hi);

			var check = new BitArray(hi - lo + 1, true);

			// Mark all numbers less than 2 as composite
			for (int i = lo; i < 2; i++)
				check[i - lo] = false;

			// Mark all even numbers as composite
			for (long i = Math.Max(lo + (lo & 1), 4); i <= hi; i += 2)
				check[(int)(i - lo)] = false;

			int sqrt = (int)Math.Ceiling(Math.Sqrt(hi));
			var primes = GetPrimeSet(sqrt);
			for (int i = 3; i <= sqrt; i += 2)
			{
				if (primes[i])
				{
					long start = Math.Max(lo, i);
					for (long j = start + i - (start % i); j <= hi; j += i)
						check[(int)(j - lo)] = false;
				}
			}
			return check;
		}

	}

}