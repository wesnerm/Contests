namespace HackerRank.WeekOfCode26.HardHomework
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using static System.Math;

	class Solution
	{

		static Stopwatch watch;

		static void Main(String[] args)
		{

			watch = new Stopwatch();
			watch.Start();

			var n = int.Parse(Console.ReadLine());
			var sol = new Solution(n);

			//Time("BruteForce", sol.BruteForce);
			//Time("SmartForce", sol.SmartForce);
			Time("SmartSearch", sol.SmartSearch);
		}


		public static void Time(string name, Func<double> func)
		{
			if (Verbose)
			{
				var time = watch.Elapsed;
				var result = func();
				time = watch.Elapsed - time;

				Console.WriteLine(name + ":");
				Console.WriteLine($"Result:  {result:F9} = {result}");
				Console.WriteLine("Elapsed: " + time);
				Console.WriteLine();
			}
			else
			{
				var result = func();
				Console.WriteLine($"{result:F9}");
			}
		}

		int n;
		double[] sin;

		public Solution(int n)
		{
			this.n = n;
			this.sin = new double[n];
			for (int i = 0; i < n; i++)
				sin[i] = Sin(i);
		}

		public double BruteForce()
		{
			double max = double.MinValue;

			for (int i = n - 2; i > 0; i--)
			{
				var max2 = double.MinValue;
				int end = n - i - 1;
				for (int j = i; j <= end; j++)
				{
					var k = n - i - j;
					if (k >= 0) max2 = Max(max2, sin[j] + sin[k]);
				}
				max = Max(max, max2 + sin[i]);
			}

			return max;
		}

		public double SmartForce()
		{
			double max = -3;

			var sorted = new int[n];
			for (int i = 0; i < n; i++)
				sorted[i] = i;

			Array.Sort(sorted, (a, b) => sin[b].CompareTo(sin[a]));

			// Go through x,y,z from highest value to lowest where sx>=sy>=sz

			for (int i = 0; i < n; i++)
			{
				var x = sorted[i];
				var sx = sin[x];
				if (max >= sx * 3) break;

				var max2 = max - sx;
				for (int j = i; j < n; j++)
				{
					var y = sorted[j];
					var sy = sin[y];
					if (max2 >= sy * 2) break;

					var z = n - x - y;
					if (z <= 0) continue;
					var sz = sin[z];
					if (sz > sy) continue; // Already seen

					max2 = Max(max2, sy + sz);
				}

				var newMax = max2 + sx;
				if (newMax > max)
					max = newMax;

				if (watch.ElapsedMilliseconds > Timeout)
					break;
			}

			return max;
		}


		public double SmartSearch()
		{
			if (n <= BruteForceThreshold)
				return BruteForce();
			if (n <= SmartForceThreshold)
				return SmartForce();

			int r = n % Period;

			// Create buckets
			var buckets = new List<Bucket>(Period * (Period + 1) / 2);
			for (int i = 0; i < Period; i++)
			{
				var pni = 2 * Period + r - i;
				var sini = sin[i];
				for (int j = 0; j <= i; j++)
				{
					var k = (pni - j) % Period;
					if (k > j) continue;  // i >= j >= k to eliminate redundancy
					var v = sini + sin[j] + sin[k];
					buckets.Add(new Bucket { i = (short)i, j = (short)j, k = (short)k, sin = v });
				}
			}
			buckets.Sort((a, b) => b.CompareTo(a));

			double max = -3;
			int current = 1;
			int count = SmartSearchCount;

			foreach (var b in buckets)
			{
				var prevMax = max;
				var m = (int)Math.Ceiling(n / (double)Period);
				for (int i = 0; i < m; i++)
				{
					var x = i * Period + b.i;
					if (x >= n) continue;

					for (int j = 0; j < m; j++)
					{
						var y = j * Period + b.j;
						var z = n - x - y;
						if (y >= n || z < 0) continue;

						var v = sin[x] + sin[y] + sin[z];
						if (v > max)
							max = v;
					}
				}

				if (Verbose && current > 1 && prevMax != max)
				{
					Console.WriteLine($"At #{current} Max {prevMax} --> {max}");
				}

				if (current++ >= count) break;

				if (PreventTimeout && watch.ElapsedMilliseconds > Timeout)
				{
					if (Verbose) Console.WriteLine($"Timed out before iteration #{current} at {watch.Elapsed}...");
					break;
				}
			}

			if (Verbose)
			{
				for (int i = 0; i < 20; i++)
					Console.WriteLine($"{i}) {buckets[i]}");
			}

			return max;
		}

		public struct Bucket : IComparable<Bucket>
		{
			public double sin;
			public short i;
			public short j;
			public short k;

			public int CompareTo(Bucket bucket)
			{
				int cmp = sin.CompareTo(bucket.sin);
				if (cmp == 0)
					cmp = i.CompareTo(bucket.i);
				if (cmp == 0)
					cmp = j.CompareTo(bucket.j);
				return cmp;
			}

			public override string ToString()
			{
				return $"{sin} ({i},{j},{k})";
			}
		}

		const bool Verbose = false;
		const int SmartSearchCount = 200;
		const int Period = 710;
		const int BruteForceThreshold = 50000;
		const int SmartForceThreshold = 50000;
		const bool PreventTimeout = true;
		const int Timeout = 2800;
	}
}