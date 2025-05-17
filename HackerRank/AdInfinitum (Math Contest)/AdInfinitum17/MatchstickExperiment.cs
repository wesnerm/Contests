namespace HackerRank.AdInfinitum17
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;


	class MatchstickExperiment
	{

		static void Main(String[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				string[] tokens_n = Console.ReadLine().Split();
				int n = Convert.ToInt32(tokens_n[0]);
				int m = Convert.ToInt32(tokens_n[1]);
				var p = Convert.ToDouble(tokens_n[2]);

				if (m > n)
				{
					int tmp = m;
					m = n;
					n = tmp;
				}


				var answer = CalculateCell(m, n, p);
				Console.WriteLine(answer);

			}
		}

		static Searcher searcher = new Searcher();
		private static Dictionary<double, double[,]> dict = new Dictionary<double, double[,]>();

		static double CalculateCell(long m, long n, double p)
		{
			if (m > n) return CalculateCell(n, m, p);

			double mn = m * n;
			double result = 0;

			if (n < 7)
			{
				var dp = new double[m, n];
				for (int i = 0; i < m; i++)
					for (int j = 0; j < n; j++)
					{
						var tmp = searcher.Search(i, j, m, n, p);
						dp[i, j] = tmp;
						result += tmp;
					}
			}
			else if (m >= 7)
			{
				// n >= m >= 7
				double[,] dp;
				if (!dict.TryGetValue(p, out dp))
				{
					dp = new double[4, 4];
					for (int i = 0; i < 4; i++)
						for (int j = 0; j < 4; j++)
							dp[i, j] = searcher.Search(i, j, 7, 7, p);
					dict[p] = dp;
				}

				// Corners
				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++)
						result += 4 * dp[i, j];

				// Center
				result += (m - 6) * (n - 6) * dp[3, 3];

				// Sides
				for (int i = 0; i < 3; i++)
					result += 2 * dp[i, 3] * (n + m - 12);
			}
			else
			{
				// m < 7 && n > 10
				var dp = new double[m, 4];
				for (int i = 0; i < m; i++)
					for (int j = 0; j < 4; j++)
						dp[i, j] = searcher.Search(i, j, m, 7, p);

				// Sides
				for (int i = 0; i < m; i++)
					for (int j = 0; j < 3; j++)
						result += 2 * dp[i, j];

				// Center
				for (int i = 0; i < m; i++)
					result += (n - 6) * dp[i, 3];

				//Dump(dp);
			}

			return result / mn;
		}


		public static void Dump(double[,] dp)
		{
			for (int i = 0; i < dp.GetLength(0); i++)
			{
				Console.WriteLine($"{i}) ");
				for (int j = 0; j < dp.GetLength(1); j++)
					Console.Write($" {dp[i, j],9:F6}");
				Console.WriteLine();
			}
		}


		public class Searcher
		{
			public bool[,] visited = new bool[10, 10];
			public int[,] path = new int[3, 2];
			public int pathSize;
			public long m, n;
			public double p;
			public int length;


			public Searcher()
			{
			}

			public double Search(int x, int y, long m, long n, double p)
			{
				this.p = p;
				this.m = m;
				this.n = n;
				var result1 = Search(x, y, 1) / 1.0;
				var result2 = Search(x, y, 2) * p / 2.0;
				var result3 = Search(x, y, 3) * p * p / 3.0;
				var result = result1 + result2 + result3;
				return result;
			}

			public double Search(int x, int y, int length)
			{
				this.length = length;
				return Dfs(x, y);
			}

			double Dfs(int x, int y)
			{
				if (x < 0 || y < 0 || x >= m || y >= n || visited[x, y])
					return 0;

				visited[x, y] = true;
				path[pathSize, 0] = x;
				path[pathSize, 1] = y;
				pathSize++;

				double result = 0;
				if (pathSize == length)
					result = CalcWalls();
				else
				{
					result += Dfs(x - 1, y);
					result += Dfs(x, y - 1);
					result += Dfs(x + 1, y);
					result += Dfs(x, y + 1);

					if (pathSize == 2)
					{
						var xx = path[0, 0];
						var yy = path[0, 1];
						var tmp = 0.0;
						tmp += Dfs2(xx - 1, yy);
						tmp += Dfs2(xx, yy - 1);
						tmp += Dfs2(xx + 1, yy);
						tmp += Dfs2(xx, yy + 1);
						result += tmp;
					}
				}

				pathSize--;
				visited[x, y] = false;
				return result;
			}

			double Dfs2(int x, int y)
			{
				var xx = path[pathSize - 1, 0];
				var yy = path[pathSize - 1, 1];
				if (x > xx || x == xx && y >= yy)
					return 0;
				return Dfs(x, y);
			}

			double CalcWalls()
			{
				double result = 1;
				for (int i = 0; i < pathSize; i++)
				{
					var x = path[i, 0];
					var y = path[i, 1];
					result *= Check(x - 1, y);
					result *= Check(x + 1, y);
					result *= Check(x, y - 1);
					result *= Check(x, y + 1);
				}
				return result;
			}

			double Check(int x, int y)
			{
				return x < 0 || y < 0 || x >= m || y >= n || visited[x, y] ? 1 : 1 - p;
			}

		}


	}
}