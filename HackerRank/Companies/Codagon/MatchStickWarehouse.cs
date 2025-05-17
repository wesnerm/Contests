namespace HackerRank.Codagon
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class MatchstickWarehouse
	{

		static void Main(String[] args)
		{
			string[] tokens_n = Console.ReadLine().Split(' ');
			int n = Convert.ToInt32(tokens_n[0]);
			int c = Convert.ToInt32(tokens_n[1]);
			var size = new int[c];
			var freq = new int[c];
			for (int i = 0; i < c; i++)
			{
				var arr = Array.ConvertAll(Console.ReadLine().Split(), Int32.Parse);
				freq[i] = arr[0];
				size[i] = arr[1];
			}

			Array.Sort(size, freq);

			long answer = 0;
			int cap = n;
			for (int i = c - 1; i >= 0; i--)
			{
				var take = Math.Min(cap, freq[i]);
				cap -= take;
				answer += take * size[i];
			}

			Console.WriteLine(answer);
		}
	}
}
