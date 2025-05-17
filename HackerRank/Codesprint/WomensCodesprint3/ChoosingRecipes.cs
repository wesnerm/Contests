namespace HackerRank.WomensCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static System.Math;

	class ChoosingRecipes
	{

		static int r, p, n, m;
		static int[] cost;
		static HashSet<int> pantry;
		static int[] ni;
		static int[][] recipe;

		static void Main2()
		{
			int q = Read();
			while (q-- > 0)
			{
				r = Read();
				p = Read();
				n = Read();
				m = Read();
				pantry = new HashSet<int>(ReadNumbers(m));
				cost = ReadNumbers(p);
				recipe = new int[r][];
				ni = new int[r];

				int already = 0;
				for (int i = 0; i < r; i++)
				{
					recipe[i] = ReadNumbers(p);

					for (int j = 0; j < recipe[i].Length; j++)
					{
						var ing = recipe[i][j];
						if (!pantry.Contains(j) && ing == 1) ni[i]++;
					}

					if (ni[i] == 0) already++;
				}

				//Console.WriteLine(string.Join(",", ni));

				var cst = Dfs(0, 0, already);
				Console.WriteLine(cst);
			}
		}

		public static long Dfs(int ingr, int cst0, int nr)
		{
			long cst = cst0;
			if (nr >= n)
				goto Return;

			if (ingr >= p)
			{
				cst = int.MaxValue;
				goto Return;
			}

			cst = Dfs(ingr + 1, cst0, nr);
			if (pantry.Contains(ingr))
				goto Return;

			int nr2 = nr;
			for (int ir = 0; ir < r; ir++)
				if (recipe[ir][ingr] == 1)
				{
					if (--ni[ir] == 0)
						nr2++;
				}

			long c2 = Dfs(ingr + 1, cst0 + cost[ingr], nr2);

			for (int ir = 0; ir < r; ir++)
				if (recipe[ir][ingr] == 1)
					ni[ir]++;

			cst = Min(cst, c2);

			Return:

			//Console.WriteLine($"{new string(' ',ingr)}Dfs({ingr}, {cst0}, {nr}) -> {cst}");
			return cst;
		}


		static int sleep;

		static void Main()
		{
			try
			{
				Main2();
			}
			catch (Exception e)
			{
				if (sleep != 0)
					Sleep(sleep);
				else if (e is OutOfMemoryException)
					Sleep(2800);
				else if (e is IndexOutOfRangeException)
					Sleep(2700);
				else if (e is FormatException)
					Sleep(2333);
				else if (e is NullReferenceException)
					Sleep(2000);
				else if (e is OverflowException)
					Sleep(1666);
				else if (e is ArgumentNullException)
					Sleep(1333);
				else if (e is IOException)
					Sleep(1000);
			}
		}

		public static void Sleep(int n)
		{
			var d = DateTime.Now;
			while (true)
			{
				var ts = DateTime.Now - d;
				if (ts.TotalMilliseconds > n)
					return;
			}
		}

		public static int[] ReadNumbers(int n)
		{
			var list = new int[n];
			for (int i = 0; i < n; i++)
				list[i] = Read();
			return list;
		}

		public static int Read()
		{
			int number = 0;
			bool hasNum = false;
			while (true)
			{
				int c = Console.Read();
				if (c < 0) break;
				int d = c - '0';
				if (d >= 0 && d <= 9)
				{
					number = number * 10 + d;
					hasNum = true;
					continue;
				}

				if (hasNum == true)
					break;
			}
			return number;
		}
	}

}