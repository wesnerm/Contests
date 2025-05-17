using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using static System.Array;
using static System.Math;
using static Library;
using static FastIO;

namespace HackerRank.WeekOfCode34.SameOccurence1
{

	class Solution
	{
		private int n, q;
		private int[] arr;
		private Dictionary<int, Bucket> buckets = new Dictionary<int, Bucket>();
		private Bucket empty = new Bucket();

		public void solve()
		{
			n = Ni();
			q = Ni();
			arr = Ni(n);

			for (int i = 0; i < n; i++)
			{
				var v = arr[i];
				if (buckets.ContainsKey(v) == false)
					buckets[v] = new Bucket() {X = v};
				buckets[v].Indices.Add(i);
			}

			long allRanges = AllRanges(n);
			int time = 0;
			int freqStart = n;
			int[] freq = new int[2 * n + 1];
			int[] timestamp = new int[freq.Length];

			var queries = new Query[q];

			for (int a0 = 0; a0 < q; a0++)
			{
				var qq = queries[a0] = new Query
				{
					X = Ni(),
					Y = Ni(),
				};

				if (buckets.ContainsKey(qq.X) == false) qq.X = 0;
				if (buckets.ContainsKey(qq.Y) == false) qq.Y = 0;
				if (qq.X > qq.Y) Swap(ref qq.X, ref qq.Y);
			}

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

				Bucket bx, by;
				buckets.TryGetValue(x, out bx);
				buckets.TryGetValue(y, out by);

				if (bx == by)
				{
					qq.Answer = allRanges;
					continue;
				}

				if (bx == null)
					Swap(ref bx, ref by);

				if (by == null)
				{
					long emptyCount = AllRanges(bx.First - 0) + AllRanges(n - (bx.Last + 1));
					int length = bx.Last - bx.First + 1;
					if (bx.Indices.Count == 2)
					{
						qq.Answer = emptyCount + AllRanges(bx.Last - bx.First - 1);
						continue;
					}
					else if (length - bx.Indices.Count < 2)
					{
						qq.Answer = emptyCount + (length - bx.Indices.Count);
						continue;
					}

					by = empty;
				}

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


	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}


}