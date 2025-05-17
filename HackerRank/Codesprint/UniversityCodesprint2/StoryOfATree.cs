
namespace HackerRank.UniversityCodesprint2.StoryOfATree
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using static System.Console;

	public class Solution
	{
		#region Variables

		List<int>[] g;
		List<int>[] guesses;
		Info[] infos;
		int[] dist;
		int mink;

		#endregion

		#region Main

		public static void Main()
		{
			int q = int.Parse(ReadLine());
			for (int i = 0; i < q; i++)
			{
				var sol = new Solution();
				sol.Run();
			}
		}

		void Run()
		{
			int n = int.Parse(ReadLine());
			g = new List<int>[n + 1];
			guesses = new List<int>[n + 1];
			dist = new int[n + 1];
			infos = new Info[n + 1];

			for (int i = 0; i < g.Length; i++)
			{
				g[i] = new List<int>();
				guesses[i] = new List<int>();
			}

			for (int i = 1; i < n; i++)
			{
				var a = ReadLine().Split();
				var u = int.Parse(a[0]);
				var v = int.Parse(a[1]);
				if (u == v) continue;
				g[u].Add(v);
				g[v].Add(u);
			}

			var input = ReadLine().Split();
			var guessCount = int.Parse(input[0]);
			mink = int.Parse(input[1]);

			for (int i = 0; i < guessCount; i++)
			{
				input = ReadLine().Split();
				var u = int.Parse(input[0]);
				var v = int.Parse(input[1]);
				guesses[u].Add(v);
				//guesses[v].Add(-u);
			}

			int root = 1;

			Dfs(root, -1, 0);
			Dfs2(root);
			Dfs3(root);

			int answer = Solve(1);

			var num = answer;
			var den = n;
			var gcd = Gcd(num, den);
			num /= gcd;
			den /= gcd;
			WriteLine($"{num}/{den}");
		}


		int Solve(int u, int p = -1)
		{
			var answer = 0;

			foreach (var child in g[u])
			{
				if (child == p) continue;
				answer += Solve(child, u);
			}

			var guessesRight = infos[u].GuessesAsParent + infos[u].GuessesAsChild;
			if (guessesRight >= mink)
				answer++;

			return answer;
		}

		void Dfs3(int u, int p = -1)
		{
			foreach (var child in g[u])
			{
				if (child == p) continue;
				infos[child].GuessesAsChild +=
					(infos[u].GuessesAsParent
					- infos[child].P2Self
					- infos[child].GuessesAsParent)
					+ infos[u].GuessesAsChild;
			}

			foreach (var child in g[u])
			{
				if (child == p) continue;
				Dfs3(child, u);
			}
		}

		Info Dfs2(int u, int p = -1)
		{
			foreach (var child in guesses[u])
			{
				if (child != p)
				{
					infos[child].P2Self++;
					infos[u].GuessesAsParent++;
				}
				else
				{
					infos[u].Self2P++;
					infos[u].GuessesAsChild++;
				}
			}

			foreach (var child in g[u])
			{
				if (child == p) continue;
				var childInfo = Dfs2(child, u);
				infos[u].GuessesAsParent += childInfo.GuessesAsParent;
			}

			return infos[u];
		}


		public int Dfs(int u, int p, int d)
		{
			dist[u] = d;

			int count = 1;
			foreach (var child in g[u])
			{
				if (child == p) continue;
				count += Dfs(child, u, d + 1);
			}

			return count;
		}

		#endregion

		#region Helpers

		struct Info
		{
			public int GuessesAsChild;
			public int GuessesAsParent;
			public int P2Self;
			public int Self2P;
			public override string ToString()
			{
				return $"AsParent={GuessesAsParent}  AsChild={GuessesAsChild}  " +
					   $"P2Self={P2Self} Self2P={Self2P}";
			}
		}

		int Gcd(int a, int b)
		{
			if (a == 0) return b;
			return Gcd(b % a, a);
		}

		#endregion

#if DEBUG
		public static bool Verbose = true;
#else
        public static bool Verbose = false;
#endif
	}
}