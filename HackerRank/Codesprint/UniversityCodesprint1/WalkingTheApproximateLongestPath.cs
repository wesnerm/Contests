namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Collections;
	using System.Linq;
	using System.Numerics;

	class WalkingTheApproximateLongestPath
	{

		Dictionary<int, HashSet<int>> dict;
		BitArray visited;
		NodeDegree[] degrees;
		SortedSet<NodeDegree> set = new SortedSet<NodeDegree>();
		List<int> bestPath = new List<int>();
		int target;

		static void Main(String[] args)
		{
			new WalkingTheApproximateLongestPath().Run();
		}

		void Run()
		{
			var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
			int n = a[0];
			int m = a[1];
			dict = Enumerable.Range(1, n).ToDictionary(x => x, x => new HashSet<int>());
			for (int i = 0; i < m; i++)
			{
				a = Console.ReadLine().Split().Select(int.Parse).ToArray();
				dict[a[0]].Add(a[1]);
				dict[a[1]].Add(a[0]);
			}

			degrees = new NodeDegree[n + 1];
			foreach (var e in dict)
			{
				var nd = new NodeDegree(e.Key, e.Value.Count);
				set.Add(nd);
				degrees[e.Key] = nd;
			}

			target = (int)(Math.Sqrt(n < 50 ? 1 : n < 500 ? .95 : .85) * n);

			var cand = new List<int>(set.Take(7).Select(x => x.vertex));

			visited = new BitArray(n + 1);
			foreach (var c in cand)
			{
				visited[c] = true;
				BuildPath(new List<int> { c });
				visited[c] = false;
			}

			//var path = new List<int>();
			Console.WriteLine(bestPath.Count);
			Console.WriteLine(string.Join(" ", bestPath));
		}

		void BuildPath(List<int> path)
		{
			const int branch = 3;
			var list = new List<int>();

			if (path.Count == 0)
				list.AddRange(set.Select(x => x.vertex).Take(8));
			else
			{
				int last = path[path.Count - 1];
				list.AddRange(dict[last].Where(x => !visited[x]));
				list.Sort((a, b) => degrees[a].degree.CompareTo(degrees[b].degree));
				if (list.Count > branch)
					list.RemoveRange(branch, list.Count - branch);
			}

			int climit = dict.Count < 1000 ? 50 : 15;
			foreach (var c in list)
			{
				visited[c] = true;
				path.Add(c);
				set.Remove(degrees[c]);
				bool disconnected = false;

				if (dict[c].Count < climit)
					foreach (var e in dict[c])
					{
						if (visited[e]) continue;
						dict[e].Remove(c);
						set.Remove(degrees[e]);
						if (--degrees[e].degree == 0)
							disconnected = true;
						else
							set.Add(degrees[e]);
					}

				if (path.Count > bestPath.Count)
				{
					bestPath.Clear();
					bestPath.AddRange(path);
				}

				if (!disconnected)
					BuildPath(path);

				if (dict[c].Count < climit)
					foreach (var e in dict[c])
					{
						if (visited[e]) continue;
						dict[e].Add(c);
						set.Remove(degrees[e]);
						degrees[e].degree++;
						set.Add(degrees[e]);
					}

				set.Add(degrees[c]);
				path.RemoveAt(path.Count - 1);
				visited[c] = false;


				if (bestPath.Count >= target)
					return;
			}
		}

		public class NodeDegree : IComparable<NodeDegree>
		{
			public int vertex;
			public int degree;
			public NodeDegree(int v, int d)
			{
				vertex = v;
				degree = d;
			}
			public int CompareTo(NodeDegree n)
			{
				int cmp = degree.CompareTo(n.degree);
				if (cmp == 0)
					cmp = vertex.CompareTo(n.vertex);
				return cmp;
			}
		}
	}
}
