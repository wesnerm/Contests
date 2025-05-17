namespace HackerRank.WeekOfCode27
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class TailorShop
	{
		static void Main(String[] args)
		{
			string[] tokens_n = Console.ReadLine().Split(' ');
			int n = Convert.ToInt32(tokens_n[0]);
			var p = long.Parse(tokens_n[1]);
			string[] a_temp = Console.ReadLine().Split(' ');
			var a = Array.ConvertAll(a_temp, long.Parse);

			Array.Sort(a);

			long result = 0;
			long minButtons = 1;
			foreach (var v in a)
			{
				var req = (long)Math.Ceiling((double)v / p);
				var buttons = Math.Max(req, minButtons);
				result += buttons;
				minButtons = buttons + 1;
			}

			Console.WriteLine(result);
		}
	}

}
