namespace HackerRank.WeekOfCode27
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class ZeroMoveNim
	{

		static void Main(String[] args)
		{
			int gameCount = int.Parse(Console.ReadLine());
			var games = new List<long[]>();
			var max = 0L;

			while (gameCount-- > 0)
			{
				Console.ReadLine();
				var p = Array.ConvertAll(Console.ReadLine().Split(), long.Parse);
				games.Add(p);
				max = Math.Max(max, p.Max());
			}

			//  BuildSG(max);
			//  for (int i=0; i<100; i++)
			//  Console.WriteLine($"{i} -> {SG(i)}");

			foreach (var g in games)
			{
				var canWin = CanWin(g);
				Console.WriteLine(canWin ? "W" : "L");
			}
		}

		static bool CanWin(long[] p)
		{
			long xor = 0;
			foreach (var x in p)
				xor ^= SG(x);
			return xor != 0;
		}

		static long SG(long pile)
		{
			if (pile == 0) return 0;
			if (pile % 2 == 0) return pile - 1;
			return pile + 1;
		}

		/*
		static List<int> sg = new List<int> { 0 };

		static void BuildSG(int max)
		{
			var states = new BitArray(max+2);
			for (int i=sg.Count; i<=max; i++)
			{
				var mex = 0;
				states[sg[i-1]] = true;
				while (states[mex] || mex==i) // + zero move
					mex++;
				sg.Add(mex);
			}
		}
		*/

	}
}
