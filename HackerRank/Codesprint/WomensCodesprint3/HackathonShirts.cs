namespace HackerRank.WomensCodesprint
{
	using System;
	using System.Threading;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class hackathonShirts
	{

		static void Main()
		{
			long q = long.Parse(Console.ReadLine().Trim());

			var found = new int[50005];
			var list = new List<int>();
			var list2 = new List<int>();
			while (q-- > 0)
			{

				long n = long.Parse(Console.ReadLine().Trim());

				ReadNumbers(list, Console.ReadLine());
				//var sizes = Array.ConvertAll( Console.ReadLine().Split(),int.Parse);

				list.Sort();
				var sizes = list.ToArray();

				long v = long.Parse(Console.ReadLine().Trim());
				for (long a1 = 0; a1 < v; a1++)
				{
					ReadNumbers(list2, Console.ReadLine());
					int smallest = list2[0];
					int largest = list2[1];
					long a = BinarySearch(sizes, smallest, 0, n - 1, false);
					long b = BinarySearch(sizes, largest, 0, n - 1, true);
					found[a]++;
					found[b]--;
				}

				long sum = 0;
				long count = 0;
				for (long i = 0; i < n; i++)
				{
					sum += found[i];
					if (sum > 0) count++;
					found[i] = 0;
				}
				found[n] = 0;

				Console.WriteLine(count);
			}
		}

		public static void ReadNumbers(List<int> list, string text)
		{
			list.Clear();
			long number = 0;
			bool hasNum = false;
			foreach (var c in text)
			{
				int d = c - '0';
				if (d >= 0 && d <= 9)
				{
					hasNum = true;
					number = 10 * number + d;
					continue;
				}
				if (hasNum) list.Add((int)number);
				number = 0;
				hasNum = false;
			}
			if (hasNum) list.Add((int)number);
		}

		public static long BinarySearch<T>(T[] array, T value, long left, long right, bool upper = false)
			where T : IComparable<T>
		{
			while (left <= right)
			{
				long mid = left + (right - left) / 2;
				int cmp = value.CompareTo(array[mid]);
				if (cmp > 0 || cmp == 0 && upper)
					left = mid + 1;
				else
					right = mid - 1;
			}
			return left;
		}
	}

}