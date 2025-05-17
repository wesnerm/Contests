namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class HackerlandRadioTransmitters
	{

		static void Main(String[] args)
		{
			var tokens_n = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
			int n = tokens_n[0];
			int k = tokens_n[1];
			var x = Console.ReadLine().Split().Select(int.Parse).ToArray();

			Array.Sort(x);

			int[] fore = new int[n];
			for (int right = n - 1, left = n - 1; left >= 0; left--)
			{
				while (left < right & x[left] + k < x[right]) right--;
				fore[left] = right;
			}

			int min = 0;
			int curr = -1;
			while (curr < n - 1)
			{
				curr = fore[fore[curr + 1]];
				min++;
			}
			Console.WriteLine(min);
		}
	}

}
