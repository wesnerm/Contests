namespace CompetitiveProgramming.HackerRank.RookieRank3.CultureConference
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Solution
	{
		static List<int>[] g;
		static bool[] b;
		static int[] o;
		static Data[] DATA;

		struct Data
		{
			public int MinSelf;
			public int MinSelfHighlighted;
			public int MinChild;
		}

		// MinParent - Highlighted
		// MinParent - Any
		// MinChild - ignores parent


		static int Solve()
		{
			var queue = new Queue<int>();
			queue.Enqueue(0);
			int i = 0;
			while (queue.Count > 0)
			{
				var u = queue.Dequeue();
				o[i++] = u;
				foreach (var v in g[u])
					queue.Enqueue(v);
			}

			while (--i >= 0)
				DATA[o[i]] = getMinimumEmployees(o[i]);

			return DATA[0].MinSelf;
		}

		static Data getMinimumEmployees(int emp)
		{
			var d = new Data();

			var countChildBurned = 0;
			var data = new Data[g[emp].Count];
			for (int i = 0; i < data.Length; i++)
			{
				var c = g[emp][i];
				if (b[c]) countChildBurned++;
				data[i] = DATA[c];
			}

			// Minimize children
			foreach (var child in data)
				d.MinChild += child.MinSelf;

			// Minimize by highlighting
			d.MinSelfHighlighted = 1;
			foreach (var child in data)
				d.MinSelfHighlighted += Math.Min(child.MinSelf, child.MinChild);

			// Minimize by being highlighted by at least one child
			var sumSelf = data.Sum(child => child.MinSelf);
			if (b[emp])
			{
				d.MinSelf = d.MinSelfHighlighted;
				foreach (var child in data)
					d.MinSelf = Math.Min(d.MinSelf, sumSelf - child.MinSelf + child.MinSelfHighlighted);
			}
			else
			{
				d.MinSelf = Math.Min(d.MinSelfHighlighted, sumSelf);
			}

			return d;
		}

		static void Main(String[] args)
		{
			int n = Convert.ToInt32(Console.ReadLine());

			g = new List<int>[n];
			DATA = new Data[n];
			o = new int[n];
			b = new bool[n];

			for (int i = 0; i < n; i++) g[i] = new List<int>();

			for (int i = 1; i < n; i++)
			{
				var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
				g[arr[0]].Add(i);
				b[i] = arr[1] == 0;
			}

			int minimumEmployees = Solve();
			Console.WriteLine(minimumEmployees);
		}
	}

}
