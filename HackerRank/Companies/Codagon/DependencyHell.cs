namespace HackerRank.Codagon.DependencyHell
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Solution
	{
		static void Main(String[] args)
		{
			var q = int.Parse(Console.ReadLine());
			while (q-- > 0)
			{
				var sol = new Solution();
				sol.Run();
			}
		}

		public void Run()
		{
			var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
			int n = arr[0], m = arr[1];
			dep = new int[n + 1][];
			dep[0] = new int[0];
			for (int i = 1; i <= n; i++)
			{
				arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
				var h = new HashSet<int>(arr.Skip(1));
				if (h.Contains(i))
					h.Remove(i);
				dep[i] = h.ToArray();
			}
			install = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
			var answer = TopSort();
			Console.WriteLine(answer.Length);
			Console.WriteLine(string.Join(" ", answer));
		}


		int[] install;
		int[][] dep;
		List<int>[] rdep;
		int[] curdep;

		int[] TopSort()
		{

			curdep = Enumerable.Range(0, dep.Length).Select(x => dep[x].Length).ToArray();

			rdep = new List<int>[dep.Length];
			for (int i = 0; i < dep.Length; i++)
				rdep[i] = new List<int>();

			for (int i = 0; i < dep.Length; i++)
			{
				foreach (var j in dep[i])
					rdep[j].Add(i);
			}

			var set = new SortedSet<int>(new Comparer() { Sol = this });

			var hashset = new HashSet<int>();
			foreach (var p in install)
				dfs(hashset, p);

			set.UnionWith(hashset);
			var result = new List<int>();
			while (set.Count > 0)
			{
				var i = set.Min;
				if (curdep[i] != 0)
				{
					//Console.WriteLine($"{i}->{curdep[i]}");
					//throw new Exception();
				}
				result.Add(i);
				set.Remove(i);
				foreach (var j in rdep[i])
				{
					if (set.Contains(j))
					{
						set.Remove(j);
						if (curdep[j]-- <= 0)
						{
							throw new Exception();
						}
						set.Add(j);
					}
				}
			}
			return result.ToArray();
		}

		void dfs(HashSet<int> hashset, int p)
		{
			if (hashset.Contains(p)) return;
			hashset.Add(p);
			foreach (var p2 in dep[p])
				dfs(hashset, p2);
		}

		public class Comparer : IComparer<int>
		{
			public Solution Sol;
			public int Compare(int a, int b)
			{
				int cmp = Sol.curdep[a] - Sol.curdep[b];
				if (cmp != 0)
					return cmp;
				return a - b;
			}
		}


	}
}
