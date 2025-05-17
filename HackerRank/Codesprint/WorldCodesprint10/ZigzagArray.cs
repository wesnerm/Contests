namespace HackerRank.WorldCodeSprint10.ZigZagArray
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class Solution
	{

		static int minimumDeletions(int[] a)
		{

			int count = 0;
			for (int i = 0; i < a.Length; i++)
			{
				bool creasing = i > 0 && i + 1 < a.Length && (a[i - 1] < a[i] && a[i] < a[i + 1] || a[i - 1] > a[i] && a[i] > a[i + 1]);
				if (creasing) count++;
			}
			return count;
		}

		static void Main()
		{
			int n = Convert.ToInt32(Console.ReadLine());
			string[] a_temp = Console.ReadLine().Split(' ');
			int[] a = Array.ConvertAll(a_temp, Int32.Parse);
			int result = minimumDeletions(a);
			Console.WriteLine(result);
		}
	}

}