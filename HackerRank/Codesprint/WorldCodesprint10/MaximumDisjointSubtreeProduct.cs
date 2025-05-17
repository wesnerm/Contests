namespace HackerRank.WorldCodeSprint10.MaximumDisjointSubtreeProduct
{
	using System;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;
	using static System.Console;

	// Powered by caide (code generator, tester, and library code inliner)

	class Solution
	{

		List<int>[] graph;
		long[] nodeSum;
		long[] invSum;
		long[] nodeMax;
		long[] invMax;
		int[] depth;
		int[] w;

		public static void Main()
		{
			new Solution().solve();
		}

		public void solve()
		{
			int n = Convert.ToInt32(FastIO.Ni());
			w = FastIO.Ni(n);

			graph = new List<int>[n];
			for (int i = 0; i < graph.Length; i++)
				graph[i] = new List<int>();


			for (int a0 = 0; a0 < n - 1; a0++)
			{
				int u = FastIO.Ni() - 1;
				int v = FastIO.Ni() - 1;
				graph[u].Add(v);
				graph[v].Add(u);
			}

			long result = Solve(n);

			//Error.WriteLine("----");
			for (int i = 0; i < w.Length; i++)
				w[i] = -w[i];

			result = Math.Max(Solve(n), result);

			Console.WriteLine(result);
		}

		public long Solve(int n)
		{
			nodeSum = new long[n];
			invSum = new long[n];
			nodeMax = new long[n];
			invMax = new long[n];
			depth = new int[n];

			for (int i = 0; i < n; i++)
			{
				invMax[i] = w[i];
				nodeMax[i] = w[i];
			}

			Dfs(0);
			Pairwise(0);

			return ans;
		}

		public void Dfs(int v, int p = -1, int d = 0)
		{
			long sum = w[v];
			long max = long.MinValue;

			foreach (var u in graph[v])
			{
				if (u == p) continue;
				Dfs(u, v, d + 1);
				if (nodeSum[u] >= 0) sum += nodeSum[u];
				max = Math.Max(nodeMax[u], max);
			}

			nodeSum[v] = sum;
			nodeMax[v] = Math.Max(sum, max);
			depth[v] = d;
		}

		long ans = 0;
		public void Pairwise(int v, int p = -1, long iv = 0, long ivMax = 0)
		{
			long a = 0;
			long b = 0;
			ans = Math.Max(ans, ivMax * nodeMax[v]);

			//Error.WriteLine($"Pairwise({v+1},{p+1},ns={iv},ivMax={ivMax}) with {ivMax}*{nodeMax[v]} = {ivMax*nodeMax[v]}");

			iv = Math.Max(iv, 0) + nodeSum[v];

			foreach (var u in graph[v])
			{
				if (u == p) continue;
				var ivNew = iv - Math.Max(0, nodeSum[u]);
				Pairwise(u, v, ivNew, Math.Max(ivNew, ivMax));
				if (nodeMax[u] > b)
				{
					b = nodeMax[u];
					if (b > a) { var tmp = a; a = b; b = tmp; }
				};
			}

			ans = Math.Max(ans, b * a);
		}
	}

}