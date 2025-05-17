
namespace HackerRank.NcrCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class SpiralMessage
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			var input = Console.ReadLine().Split().Select(int.Parse).ToArray();
			var rows = input[0];
			var cols = input[1];

			var m = new char[rows][];
			for (int r = 0; r < rows; r++)
				m[r] = Console.ReadLine().ToCharArray();

			int x0 = 0, y0 = 0, x1 = cols - 1, y1 = rows - 1;
			int x = x0, y = y1, dx = 0, dy = -1;

			bool space = true;
			int wordCount = 0;
			while (x >= x0 && x <= x1 && y >= y0 && y <= y1 && m[y][x] != '*')
			{
				do
				{
					var ch = m[y][x];
					if (ch == '*') break;

					//Console.WriteLine("({0},{1}) = '{2}'", x, y, ch);
					if (ch != '#' && space)
						wordCount++;

					space = ch == '#';
					m[y][x] = '*';
					x += dx;
					y += dy;
				}
				while (x >= x0 && x <= x1 && y >= y0 && y <= y1);

				int dx0 = dx, dy0 = dy;
				dx = -dy0;
				dy = dx0;
				x += dx - dx0;
				y += dy - dy0;
			}

			Console.WriteLine(wordCount);
		}
	}
}