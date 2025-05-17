namespace HackerRank.WeekOfCode34.MaximumGcdAndSum
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static FastIO;

	class Solution
	{

		static int maximumGcdAndSum(int[] A, int[] B)
		{

			var table = new byte[1000001];
			foreach (var a in A) table[a] |= 1;
			foreach (var b in B) table[b] |= 2;

			int maxsum = 0;
			for (int i = table.Length - 1; i >= 1; i--)
			{
				int maxa = 0;
				int maxb = 0;
				for (int j = i; j < table.Length; j += i)
				{
					var v = table[j];
					if ((v & 1) != 0) maxa = j;
					if ((v & 2) != 0) maxb = j;
				}

				if (maxa != 0 && maxb != 0)
				{
					maxsum = maxa + maxb;
					break;
				}
			}

			return maxsum;
		}

		static void Main(String[] args)
		{
			int n = Ni();
			int[] A = Ni(n);
			int[] B = Ni(n);
			int res = maximumGcdAndSum(A, B);
			Console.WriteLine(res);
		}
	}



}