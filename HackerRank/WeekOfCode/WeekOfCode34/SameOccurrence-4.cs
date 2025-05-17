using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using static System.Array;
using static System.Math;
using static Library;
using static FastIO;

namespace HackerRank.WeekOfCode34.SameOccurence4
{


	class Solution
	{
		private int n, q;
		private int[] arr;
		private Bucket[] buckets;
		private Bucket empty = new Bucket();
		private Query[] queries;

		void test()
		{
			const int period = 1000;
			n = 8000;
			q = 500000;
			arr = new int[n];
			for (int i = 0; i < n; i++)
				arr[i] = (i % period) + 1;
			queries = new Query[q];
			int fx = 1;
			int fy = 1;
			for (int i = 0; i < q; i++)
			{
				queries[i] = new Query {X = fx, Y = fy};
				fy++;
				if (fy > period)
				{
					fy = 1;
					fx++;
					if (fx > period)
					{
						fx = 1;
					}
					;
				}
			}
		}

		void remap()
		{
			var map = new Dictionary<int, int>();
			int id = 1;

			for (int i = 0; i < n; i++)
			{
				var v = arr[i];
				if (!map.ContainsKey(v)) map[v] = id++;
				arr[i] = map[v];
			}

			buckets = new Bucket[id];
			for (int i = 0; i < id; i++)
				buckets[i] = new Bucket {X = i};
			for (int i = 0; i < n; i++)
				buckets[arr[i]].Indices.Add(i);

			foreach (var qq in queries)
			{
				int v;
				qq.X = map.TryGetValue(qq.X, out v) ? v : 0;
				qq.Y = map.TryGetValue(qq.Y, out v) ? v : 0;
				if (qq.X > qq.Y)
					Swap(ref qq.X, ref qq.Y);
			}

		}

		public void solve()
		{
			n = Ni();
			q = Ni();
			arr = Ni(n);

			queries = new Query[q];
			for (int i = 0; i < q; i++)
				queries[i] = new Query {X = Ni(), Y = Ni()};

			//test();
			remap();

			long allRanges = AllRanges(n);
			int time = 0;
			int freqStart = n;
			int[] freq = new int[2 * n + 1];
			int[] timestamp = new int[freq.Length];

			var answers = queries.ToArray();
			Array.Sort(queries, (a, b) =>
			{
				int cmp = a.X.CompareTo(b.X);
				if (cmp != 0) return cmp;
				return a.Y.CompareTo(b.Y);
			});

			Query prevQuery = null;
			foreach (var qq in queries)
			{
				int x = qq.X;
				int y = qq.Y;

				if (prevQuery != null && prevQuery.X == x && prevQuery.Y == y)
				{
					qq.Answer = prevQuery.Answer;
					continue;
				}
				prevQuery = qq;

				if (x == y)
				{
					qq.Answer = allRanges;
					continue;
				}

				Bucket bx = buckets[x], by = buckets[y];

				int i = 0;
				int j = 0;
				int prev = -1;
				int bal = freqStart;
				time++;

				timestamp[bal] = time;
				freq[bal] = 0;

				while (i < bx.Indices.Count || j < by.Indices.Count)
				{
					int v;
					int newBal;
					if (i >= bx.Indices.Count || j < by.Indices.Count && by.Indices[j] < bx.Indices[i])
					{
						v = by.Indices[j++];
						newBal = bal - 1;
					}
					else
					{
						v = bx.Indices[i++];
						newBal = bal + 1;
					}

					if (timestamp[newBal] < time)
					{
						timestamp[newBal] = time;
						freq[newBal] = 0;
					}

					freq[bal] += v - prev;
					prev = v;
					bal = newBal;
				}

				freq[bal] += n - prev;

				int start = freqStart;
				while (start > 0 && timestamp[start - 1] == time) start--;

				long count = 0;
				while (start < freq.Length && timestamp[start] == time)
				{
					long f = freq[start];
					count += f * (f - 1) / 2;
					start++;
				}

				qq.Answer = count;
			}

			foreach (var qq in answers)
				WriteLine(qq.Answer);

			Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
		}


		long AllRanges(long length)
		{
			return length * (length + 1) / 2;
		}

		public class Query
		{
			public long Answer;
			public int X;
			public int Y;
		}

		public class Bucket
		{
			public int X;
			public List<int> Indices = new List<int>();
			public int First => Indices[0];
			public int Last => Indices[Indices.Count - 1];
		}
	}



}