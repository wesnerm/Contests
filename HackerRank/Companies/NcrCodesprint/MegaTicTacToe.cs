
namespace HackerRank.NcrCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class MegaTicTacToe
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			int g = int.Parse(Console.ReadLine());

			for (int i = 0; i < g; i++)
			{
				var input = Console.ReadLine().Split().Select(int.Parse).ToArray();
				int n = input[0];
				int m = input[1];
				int k = input[2];

				bool xgood = false;
				bool ogood = false;

				int[] diag1 = new int[m];
				int[] diag2 = new int[m];
				int[] vert = new int[m];

				for (int j = 0; j < n; j++)
				{
					var row = Console.ReadLine();
					int horz = 0;
					int diag1prev = 0;

					for (int v = 0; v < m; v++)
					{
						var ch = row[v];
						var sn = ch == 'X' ? -1 : ch == 'O' ? 1 : 0;

						int d = diag1prev;
						diag1prev = diag1[v];

						if (sn == 0)
						{
							horz = diag1[v] = diag2[v] = vert[v] = 0;
							continue;
						}

						diag1[v] = (d * sn >= 0 ? d : 0) + sn;
						diag2[v] = (v + 1 < m && diag2[v + 1] * sn >= 0 ? diag2[v + 1] : 0) + sn;
						vert[v] = (vert[v] * sn >= 0 ? vert[v] : 0) + sn;
						horz = (horz * sn >= 0 ? horz : 0) + sn;

						//Console.WriteLine("({0},{1}) d1={2} d2={3} v={4} h={5}", j,v,diag1[v],diag2[v],vert[v],horz);

						if (diag1[v] == -k || diag2[v] == -k || vert[v] == -k || horz == -k) xgood = true;
						if (diag1[v] == k || diag2[v] == k || vert[v] == k || horz == k) ogood = true;
					}
				}

				Console.WriteLine(xgood == ogood ? "NONE" : xgood ? "LOSE" : "WIN");
			}
		}
	}
}