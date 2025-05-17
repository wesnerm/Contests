namespace HackerRank.WorldCodeSprint8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class ReturnOfNim
	{

		static void Main(String[] args)
		{
			int a0 = Convert.ToInt32(Console.ReadLine());
			while (a0-- > 0)
			{
				Console.ReadLine();
				var piles = Console.ReadLine().Split().Select(int.Parse).ToArray();
				Array.Sort(piles);

				// Win same piles by reducing them all to zero
				if (piles[0] == piles[piles.Length - 1])
				{
					ReportWin(true);
					continue;
				}

				// Pile game of size 2
				if (piles.Length == 2)
				{
					ReportWin(WythoffsGame(piles));
					continue;
				}

				var xor = piles.Aggregate((a, b) => a ^ b);
				ReportWin(xor != 0);
			}
		}

		public class Solver
		{
			int[] ranks;
			int size;
			int[] piles;
			//Dictionary<long, int> dict = new Dictionary<long, int>();

			static byte[] g = new byte[10000000];

			public Solver(int[] pilesParam)
			{
				piles = /*Squeeze*/(pilesParam);
				ranks = (int[])piles.Clone();
				size = piles.Length;

				long code = MaxCode();
				Array.Clear(g, 0, g.Length);
				// g = new byte[code];
			}


			int depth = 0;
			public bool CanWin()
			{
				depth++;
				/*if (depth > 300)
                    return true; */

				long code = Code(piles, size);

				if (code < g.Length && code >= 0 && g[code] != 0)
					return g[code] == 1 ? true : false;

				bool canWin = false;

				// Remove a single pile or all piles to achieve nimsum
				int min = piles[0];
				int xor = 0;
				int xor2 = 0;
				for (int i = 0; i < size; i++)
				{
					xor ^= piles[i];
					xor2 ^= piles[i] - min;
				}

				if (xor2 == 0)
					canWin = true;
				else
				{
					for (int i = 0; i < size; i++)
						if (piles[i] == xor)
						{
							canWin = true;
							break;
						}
				}

				// Remove amount j=1..min-1 from all pile
				for (int j = min - 1; j >= 1 && !canWin; j--)
				{
					for (int i = 0; i < size; i++) piles[i] -= j;
					canWin = !CanWin();
					for (int i = 0; i < size; i++) piles[i] += j;
				}

				// Remove amount from single pile
				for (int i = 0; i < size && !canWin; i++)
				{
					for (int j = piles[i] - 1; j >= 1 && !canWin; j--)
					{
						int k = Bump(piles, i, -j, size);
						canWin = !CanWin();
						Bump(piles, k, +j, size);
					}
				}

				if (code < g.Length && code >= 0)
					g[code] = (byte)(canWin ? 1 : 2);

				depth--;
				return canWin;
			}


			public long MaxCode()
			{
				long factor = 1;
				for (int i = 0; i < ranks.Length; i++)
					factor *= ranks[i];
				return factor;
			}

			public long Code(int[] piles, int size)
			{
				long value = 0;
				long factor = 1;
				for (int i = 0; i < size; i++)
				{
					value = value * factor + piles[size - 1 - i] - 1;
					factor *= ranks[ranks.Length - 1 - i];
				}
				return value;
			}
		}

		public static int[] Squeeze(int[] piles)
		{
			var list = new List<int>();
			for (int read = 0; read < piles.Length; read++)
			{
				// Remove duplicates
				if (read + 1 < piles.Length && piles[read + 1] == piles[read])
				{
					read++;
					continue;
				}
				list.Add(piles[read]);
			}
			return list.ToArray();
		}

		public static int Bump(int[] piles, int index, int amount, int size)
		{
			int oldValue = piles[index];
			int newValue = oldValue + amount;

			if (amount < 0)
			{
				while (index > 0 && piles[index - 1] >= newValue)
				{
					piles[index] = piles[index - 1];
					index--;
				}
			}
			else
			{
				while (index + 1 < size && piles[index + 1] < newValue)
				{
					piles[index] = piles[index + 1];
					index++;
				}
			}

			piles[index] = newValue;
			return index;
		}

		public static void ReportWin(bool win)
		{
			if (win == true)
				Console.WriteLine("Sherlock");
			else
				Console.WriteLine("Watson");
		}

		public static bool WythoffsGame(int[] piles)
		{
			var gr = (1 + Math.Sqrt(5)) / 2;
			var m0 = piles[0];
			var m1 = piles[1];

			for (int n = 0; true; n++)
			{
				long c0 = (long)(Math.Floor(n * gr));
				long c1 = (long)(Math.Floor(n * gr * gr));
				if (m0 < c0) break;
				if (m0 == c0 && m1 == c1) return false;
			}

			return true;
		}


	}

}
