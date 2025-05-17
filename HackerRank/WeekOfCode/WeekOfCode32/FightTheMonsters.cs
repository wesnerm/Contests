namespace HackerRank.WeekOfCode32.FightTheMonsters
{
	// https://www.hackerrank.com/contests/w32/challenges/fight-the-monsters

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();

#if DEBUG
			Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
#endif
		}

		public void solve()
		{
			long n = Ni(), hit = Ni(), t = Ni();
			long[] h = Nl((int)n);

			Array.Sort(h);

			int i = 0;
			while (t > 0 && i < n)
			{
				long div = (h[i] + hit - 1) / hit;
				t -= div;
				if (t < 0) break;
				i++;
			}

			WriteLine(i);
		}



	}

}