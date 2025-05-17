namespace HackerRank.WorldCodesprint8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class RoadsAndLibraries
	{

		static int clib, croad;
		static int n, m;
		static HashSet<int>[] g;
		static Dictionary<int, HashSet<int>> regions;
		static int[] ds;
		static int[] cc;

		static void Main(String[] args)
		{
			int q = Convert.ToInt32(Console.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				string[] tokens_n = Console.ReadLine().Split(' ');
				n = int.Parse(tokens_n[0]);
				m = int.Parse(tokens_n[1]);
				clib = int.Parse(tokens_n[2]);
				croad = int.Parse(tokens_n[3]);

				ds = new int[n + 1];
				cc = new int[n + 1];
				for (int i = 0; i < ds.Length; i++)
					ds[i] = i;

				g = Enumerable.Range(0, n + 1).Select(k => new HashSet<int>()).ToArray();

				for (int a1 = 0; a1 < m; a1++)
				{
					string[] tokens_city_1 = Console.ReadLine().Split(' ');
					int city_1 = int.Parse(tokens_city_1[0]);
					int city_2 = int.Parse(tokens_city_1[1]);
					g[city_1].Add(city_2);
					g[city_2].Add(city_1);
					Union(ds, city_1, city_2);
				}

				regions = new Dictionary<int, HashSet<int>>();

				int count = 0;
				for (int i = 1; i < ds.Length; i++)
				{
					var root = Find(ds, i);
					if (!regions.ContainsKey(root)) regions[root] = new HashSet<int>();
					regions[root].Add(i);
				}

				long cost = 0;
				foreach (var r in regions.Values)
					cost += Search(r);

				Console.WriteLine(cost);
			}
		}

		public static long Search(HashSet<int> region)
		{
			int count = region.Count;
			if (region.Count == 1) return clib;

			long cost = 0;
			if (croad < clib)
			{
				// All roads connected, 1 library
				cost = clib + (count - 1) * croad;
			}
			else
			{
				// No roads connect, n librarys
				cost = clib * count;
			}

			return cost;
		}

		public static bool Union(int[] ds, int a, int b)
		{
			int ra = Find(ds, a);
			int rb = Find(ds, b);
			if (ra == rb)
				return false;

			if (ra < rb) ds[rb] = ra;
			else ds[ra] = rb;
			return true;
		}

		public static int Find(int[] ds, int a)
		{
			int r = ds[a];
			if (r != a)
				ds[a] = r = Find(ds, r);
			return r;
		}
	}

}