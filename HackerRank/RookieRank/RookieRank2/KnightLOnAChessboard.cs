namespace HackerRank.Contests.RookieRank2
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class KnightLOnAChessboard
	{

		static void Main(String[] args)
		{
			n = Convert.ToInt32(Console.ReadLine());
			grid = new int[n, n];

			for (int i = 1; i < n; i++)
			{
				var list = new List<int>();
				for (int j = 1; j < n; j++)
				{
					dr = i;
					dc = j;
					Clear(n);
					int depth = Search(0, 0, 0, n * n);
					if (depth >= n * n) depth = -1;
					list.Add(depth);
				}
				Console.WriteLine(string.Join(" ", list));
			}
		}

		static void Clear(int n)
		{
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					grid[i, j] = int.MaxValue;
		}

		static int[] dir = new int[] { 1, 1, -1, -1, 1 };

		static int dr;
		static int dc;
		static int n;
		static int Search(int r, int c, int depth, int minDepth)
		{
			//Console.WriteLine($"Search({r},{c},{depth},{minDepth})");
			if (r < 0 || c < 0 || r >= n || c >= n || depth >= minDepth || grid[r, c] <= depth) return short.MaxValue;
			if (r == n - 1 && c == n - 1) return depth;

			grid[r, c] = depth;
			for (int i = 0; i < 4; i++)
			{
				int check = Search(r + dr * dir[i], c + dc * dir[i + 1], depth + 1, minDepth);
				minDepth = Math.Min(check, minDepth);
				if (dr != dc)
				{
					check = Search(r + dc * dir[i], c + dr * dir[i + 1], depth + 1, minDepth);
					minDepth = Math.Min(check, minDepth);
				}
			}
			return minDepth;
		}

		static int[,] grid;

	}

}
