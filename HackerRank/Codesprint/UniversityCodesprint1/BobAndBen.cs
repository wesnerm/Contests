namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class BobAndBen
	{
		static void Main(String[] args)
		{
			int g = int.Parse(Console.ReadLine());
			for (int i = 0; i < g; i++)
			{
				int n = int.Parse(Console.ReadLine());
				var a = Enumerable.Range(0, n).Select(x => Console.ReadLine().Split().Select(int.Parse).ToArray()).ToArray();
				var bobWin = CanWin(a);
				Console.WriteLine(bobWin ? "BOB" : "BEN");
			}
		}

		static bool CanWin(int[][] a)
		{
			int score = 0;
			for (int i = 0; i < a.Length; i++)
				score ^= Game(a[i][0], a[i][1]);
			return score != 0;
		}

		static int Game(int m, int k)
		{
			switch (m)
			{
				case 0: return 0;
				case 1: return 1;
				case 2: return 0;
				default: return m % 2 == 1 ? 1 : 2;
			}
		}
	}
}
