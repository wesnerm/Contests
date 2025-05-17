
namespace HackerRank.NcrCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class CoconutPlantation
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			var pcnt = int.Parse(Console.ReadLine());
			for (int i = 0; i < pcnt; i++)
			{
				var plant = new Plantation();
				var result = plant.Solve();
				Console.WriteLine(result);
			}
		}

		public class Plantation
		{
			int[,] g;
			int rows;
			int cols;
			int n;
			int m;
			int minWorker;

			public int Solve()
			{
				var input = Console.ReadLine().Split().Select(int.Parse).ToArray();
				rows = input[0];
				cols = input[1];
				n = input[2];
				m = input[3];
				g = new int[rows, cols + 1];

				for (int j = 0; j < n; j++)
				{
					var rec = Console.ReadLine().Split().Select(int.Parse).ToArray();
					int x0 = rec[0];
					int y0 = rec[1];
					int x1 = rec[2];
					int y1 = rec[3];

					for (int x = x0; x <= x1; x++)
					{
						g[x, y0]++;
						g[x, y1 + 1]--;
					}
				}

				for (int x = 0; x < rows; x++)
				{
					int sum = 0;
					for (int y = 0; y < cols; y++)
					{
						sum += g[x, y];
						g[x, y] = sum;
					}
				}

				// Mark off all the dead trees
				for (int x = 0; x < rows; x++)
				{
					for (int y = 0; y < cols; y++)
						if (g[x, y] < m)
							g[x, y] = 0;
				}

				var paths = new List<Path>();
				var vpath = new Path[cols];

				for (int r = 0; r < rows; r++)
				{
					Path hpath = null;
					for (int c = 0; c < cols; c++)
					{
						if (g[r, c] == 0)
						{
							hpath = null;
							vpath[c] = null;
							continue;
						}

						bool moreR = r + 1 < rows && g[r + 1, c] != 0;
						bool moreC = c + 1 < cols && g[r, c + 1] != 0;

						if (vpath[c] != null)
							vpath[c].d++;
						else if (moreR)
						{
							vpath[c] = new Path { x = r, y = c, useX = true };
							paths.Add(vpath[c]);
						}

						if (hpath != null)
							hpath.d++;
						else if (moreC || vpath[c] == null)
						{
							hpath = new Path { x = r, y = c, useX = false };
							paths.Add(hpath);
						}

						var cnt = 0;
						if (vpath[c] != null) cnt++;
						if (hpath != null) cnt++;

						// If a cell is only covered by one worker, make the worker required
						if (cnt == 1)
						{
							var p = hpath ?? vpath[c];
							p.required = true;
						}
						else if (cnt == 2)
						{
							hpath.overlapped = true;
							vpath[c].overlapped = true;
						}
					}
				}

				paths.RemoveAll(p =>
				{
					if (!p.required && p.overlapped)
						return false;
					AddWorker(p);
					return true;
				});

				foreach (var p in paths)
					p.wt = p.d;

				var set = new SortedSet<Path>(paths);
				while (set.Count > 0)
				{
					var p = set.Max;
					set.Remove(p);
					int d = ActualWeight(p);
					if (d == 0)
						continue;
					if (d >= p.wt)
						AddWorker(p);
					else
					{
						p.wt = d;
						set.Add(p);
					}
				}

				return minWorker;
			}

			public int ActualWeight(Path p)
			{
				int x = p.x;
				int y = p.y;
				int d = p.d;
				int wt = 0;
				for (int i = 0; i < d; i++)
				{
					if (g[x, y] != 0)
						wt++;
					if (p.useX) x++; else y++;
				}

				return wt;
			}

			public void AddWorker(Path p)
			{
				var x = p.x;
				var y = p.y;
				for (int i = 0; i < p.d; i++)
				{
					g[x, y] = 0;
					if (p.useX) x++; else y++;
				}
				//Console.WriteLine($"{p}");
				minWorker++;
			}

			public class Path : IComparable<Path>
			{
				static int idCounter;
				public int id = idCounter++;
				public bool useX;
				public int x;
				public int y;
				public int d = 1;
				public int wt = 1;
				public bool overlapped;
				public bool required;

				public int CompareTo(Path b)
				{
					var a = this;
					if (a == b) return 0;
					if (b == null) return 1;
					int cmp = a.wt.CompareTo(b.wt);
					if (cmp == 0) cmp = b.useX.CompareTo(a.useX);
					if (cmp == 0) cmp = a.id.CompareTo(b.id);
					return cmp;
				}

				public override string ToString()
				{
					var u = useX ? 'v' : 'h';
					var r = required ? "required" : "";

					return $"({x},{y}) {d}{u} {r}";
				}
			}
		}
	}
}