namespace HackerRank.WeekOfCode26
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static System.Math;

	class MusicOnTheStreet
	{
		public const long Infinity = int.MaxValue;
		public const long NotFound = int.MinValue;

		public static void Main(String[] args)
		{
			int n = int.Parse(Console.ReadLine());
			var nums = Console.ReadLine().Split().Select(long.Parse).ToArray();
			var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
			long m = arr[0];
			long hmin = arr[1];
			long hmax = arr[2];

			var areas = new Area[n + 1];
			long start = nums[0] - hmax;
			for (int i = 0; i < areas.Length; i++)
			{
				areas[i] = new Area
				{
					Start = start,
					End = start = i < nums.Length ? nums[i] : start + hmax,
				};
			}

			long found = NotFound;
			int right = 0;
			for (int left = 0; left < areas.Length && found == NotFound; left++)
			{
				var area = areas[left];
				if (area.Start < area.End - hmax)
					area.Start = area.End - hmax;

				for (right = Math.Max(left, right); right < areas.Length; right++)
				{
					var area2 = areas[right];
					if (area2.End - area2.Start < hmin) // Too small
					{
						left = right;
						break;
					}

					long dist = area2.End - area.Start;
					if (dist >= m)
					{
						var minStart = area.End - hmax;
						var maxStart = area.End - hmin;
						var maxEnd = area2.Start + hmax;
						var minEnd = area2.Start + hmin;
						var minStart2 = minEnd - m;
						var maxStart2 = maxEnd - m;

						var minStart3 = Max(minStart, minStart2);
						var maxStart3 = Min(maxStart, maxStart2);
						if (minStart3 <= maxStart3)
						{
							found = minStart3;
							break;
						}
						break;
					}

					if (area2.End - area2.Start > hmax) // Too big to be in the middle
					{
						left = right;
						break;
					}
				}
			}

			Console.WriteLine(found);
		}
	}

	public class Area
	{
		public long Start;
		public long End;
	}

}