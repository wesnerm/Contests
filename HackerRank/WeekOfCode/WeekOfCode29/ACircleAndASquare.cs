using System;

namespace HackerRank.WeekOfCode29
{
	class ACircleAndASquare
	{
		static void Main(String[] args)
		{
			var input = Array.ConvertAll(Console.ReadLine().Split(), double.Parse);
			double w = input[0], h = input[1];
			input = Array.ConvertAll(Console.ReadLine().Split(), double.Parse);
			double cx = input[0], cy = input[1], r = input[2];
			input = Array.ConvertAll(Console.ReadLine().Split(), double.Parse);
			double x1 = input[0], y1 = input[1], x3 = input[2], y3 = input[3];

			double sx = (x1 + x3) / 2, sy = (y1 + y3) / 2;
			var dx = (x1 - sx) - (y1 - sy);
			var dy = (x1 - sx) + (y1 - sy);
			var C = (x1 - sx) * (x1 - sx) + (y1 - sy) * (y1 - sy);

			for (double y = 0; y < h; y++)
			{
				for (double x = 0; x < w; x++)
					Console.Write((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r
						|| Math.Abs(dx * (x - sx) + dy * (y - sy)) <= C
							&& Math.Abs(-dy * (x - sx) + dx * (y - sy)) <= C
						? '#' : '.');
				Console.WriteLine();
			}
		}
	}
}