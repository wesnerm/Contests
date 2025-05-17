using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HackerRank.WeekOfCode29.AlmostIntegerRockGarden
{
    using static Math;
	using static Console;

	public class Solution
	{

		static void Main()
		{
			var sw = new Stopwatch();
			sw.Start();
			string[] tokens_x = ReadLine().Split(' ');
			int x = Convert.ToInt32(tokens_x[0]);
			int y = Convert.ToInt32(tokens_x[1]);
			Precompute();
			Search2(x, y);
#if DEBUG
			WriteLine(sw.Elapsed);
#endif

			Dump();
		}

		static Dictionary<long, Candidate>[] set = new Dictionary<long, Candidate>[5];
		static Dictionary<long, List<Coord>> coords = new Dictionary<long, List<Coord>>();


		public static void Dump()
		{
			var list = new List<Candidate>();
			foreach (var v in set[4].Values)
				Scan(v, list);

			WriteLine("\nTHE DATA");
			var candidates = set[4].Values.ToList();
			candidates.Sort((a, b) => (Abs(a.Value).CompareTo(Abs(b.Value))));

			var found = new HashSet<long>();
			foreach (var kvp in coords)
			{
				var c = kvp.Value.First();
				if (found.Contains(kvp.Key))
					continue;

				foreach (var cd in candidates)
					Untag(cd);

				var hashSet = Search(c.X, c.Y);
				if ( hashSet == null )
				{
					WriteLine($"Could not find {c}");
					continue;
				}


				var join = string.Join(", ", hashSet.Select(x=>$"{{ {x.X}, {x.Y} }}"));

				var score = hashSet.Sum(x => x.Value);

				WriteLine($"{{ {join} }},  // {score}");

				foreach (var v in hashSet)
					found.Add(v.Code);
			}
			WriteLine();


			WriteLine($"{list.Count} Coords");
			foreach (var kvp in coords)
			{
				var k = kvp.Key;
				foreach (var c in kvp.Value)
					WriteLine($"AddCoord({k}, {c.X}, {c.Y});");
			}

		}

		public static void Scan(Candidate candidate, List<Candidate> list)
		{
			for (var current = candidate; current != null; current = current.Next)
			{
				if (current.Index != 0) break;
				list.Add(current);
				current.Index = list.Count;
				Scan(current.Cand1, list);
				Scan(current.Cand2, list);
			}
		}



		public static HashSet<Coord> Search(int x, int y)
		{

			var candidates = set[4].Values.ToList();
			candidates.Sort((a, b) => (Abs(a.Value).CompareTo(Abs(b.Value))));

			var coord = new Coord { X = x, Y = y };
			var d = coord.Code; 
			foreach (var c in candidates)
			{
				if (Tag(c, d) == -1) continue;
				var hashSet = new HashSet<Coord>();
				var stack = new List<Coord>();

				hashSet.Add(coord);
				stack.Add(coord);

				if (Attempt(stack, hashSet, c, d, true))
					return hashSet;
			}

			return null;
		}

		public static void Search2(int x, int y)
		{
			var hashSet = Search(x, y);
			if (hashSet == null) return;

			var c = new Coord(x, y);
			hashSet.Remove( c);
			var score = c.Value;
			foreach (var c2 in hashSet)
			{
				WriteLine($"{c2.X} {c2.Y}");
				score += c2.Value;
			}
#if DEBUG
			WriteLine(score);
#endif

		}

		public static bool Attempt(List<Coord> list, HashSet<Coord> set, Candidate cand, long d, bool find = false)
		{
			int listCount = list.Count;

			for (var c = cand; c != null; c = c.Next)
			{
				if (find && c.Tag != 1) continue;

				// For backtracking
				for (int j = list.Count - 1; j >= listCount; j--)
				{
					set.Remove(list[j]);
					list.RemoveAt(j);
				}


				if (c.Cand1 == null)
				{
					if (find) return true;
					var coord = new Coord(0, 0);

					foreach (var crd in coords[c.Value])
					{
						coord.X = crd.X;
						coord.Y = crd.Y;
						for (int k = 0; k < 2; k++)
						{
							for (int i = 0; i <= 3; i++)
							{
								coord.X = Abs(coord.X) * ((i & 1) == 0 ? 1 : -1);
								coord.Y = Abs(coord.Y) * ((i & 2) == 0 ? 1 : -1);
								if (!set.Contains(coord))
								{
									set.Add(coord);
									return true;
								}
							}
							coord.X = crd.Y;
							coord.Y = crd.X;
						}

					}
					continue;
				}

				if (!find)
				{
					if (Attempt(list, set, c.Cand1, d, false)
						&& Attempt(list, set, c.Cand2, d, false))
						return true;
					continue;
				}

				if (c.Cand1.Tag == 1)
				{
					if (Attempt(list, set, c.Cand1, d, true)
						&& Attempt(list, set, c.Cand2, d, false))
						return true;
				}

				if (c.Cand2.Tag == 1)
				{
					if (Attempt(list, set, c.Cand2, d, true)
						&& Attempt(list, set, c.Cand1, d, false))
						return true;
				}
			}

			// For backtracking
			for (int j = list.Count - 1; j >= listCount; j--)
			{
				set.Remove(list[j]);
				list.RemoveAt(j);
			}
			return false;
		}

		public static int Tag(Candidate cand, long d)
		{
			if (cand.Tag != 0)
				return cand.Tag;

			int tag = -1;
			for (var cur = cand; cur != null; cur = cur.Next)
			{
				if (cur.Cand1 == null)
					cur.Tag = cur.Value == d ? 1 : -1;
				else
				{
					var tag1 = Tag(cur.Cand1, d);
					var tag2 = Tag(cur.Cand2, d);
					if (tag1 == 1 || tag2 == 1)
						cur.Tag = 1;
				}

				if (cur.Tag == 1)
					tag = 1;
			}

			return tag;
		}

		public static void Untag(Candidate cand)
		{
			if (cand==null)
				return;

			for (var cur = cand; cur != null; cur = cur.Next)
			{
				Untag(cur.Cand1);
				Untag(cur.Cand2);
				cur.Tag = 0;
			}
		}


		public static void RadixSort(List<Candidate> list,
			Candidate[] buffer = null,
			long maxValueParam = int.MaxValue)
		{
			int shift0 = 16;
			uint buckets0 = 1u << shift0;
			uint mask = buckets0 - 1;
			ulong maxValue = (ulong)maxValueParam;

			var offsets = new int[buckets0];

			if (buffer == null || buffer.Length < list.Count)
				buffer = new Candidate[list.Count];

			for (int shift = 0; shift < 64; shift += shift0)
			{
				int buckets = (int)Min(buckets0, (maxValue >> shift) + 1);
				for (int i = 0; i < buckets; i++)
					offsets[i] = 0;

				for (var i = 0; i < list.Count; i++)
				{

					var x = ((ulong)list[i].Value + (1ul << 63) >> 1);
					var radix = x >> shift & mask;
					offsets[radix]++;
				}

				var sum = 0;
				for (var i = 0; i < buckets; i++)
				{
					var newSum = sum + offsets[i];
					offsets[i] = sum;
					sum = newSum;
				}

				for (var i = 0; i < list.Count; i++)
				{
					var x = ((ulong)list[i].Value + (1ul << 63) >> 1);
					var radix = x >> shift & mask;
					buffer[offsets[radix]++] = list[i];
				}

				for (var i = 0; i < list.Count; i++)
					list[i] = buffer[i];
			}
		}


		static void Precompute()
		{
			for (int i = 0; i < set.Length; i++)
				set[i] = new Dictionary<long, Candidate>();

			for (int i = 1; i <= 12; i++)
				for (int j = i; j <= 12; j++)
				{
					var coord = new Coord { X = i, Y = j };
					var d = DoubleToLong(Sqrt(i * i + j * j));
					if (d == 0) continue;
					if (!coords.ContainsKey(d))
						coords[d] = new List<Coord>();
					coords[d].Add(coord);
					Push(set[0], d);
				}

			foreach (Candidate j in set[0].Values)
				foreach (Candidate k in set[0].Values)
				{
					if (j.Value > k.Value)
						continue;
					long d = j.Value + k.Value;
					Push(set[1], d, j, k);
				}

			Combine(set[1], set[0], set[2], 0, true);
			for (int i = 3; i < 5; i++)
				Combine(set[i - 1], set[i-1], set[i], i == 3 ? 1e-2 : 1e-13, false);
		}

		public static void Combine(
			Dictionary<long, Candidate> set1,
			Dictionary<long, Candidate> set2,
			Dictionary<long, Candidate> set3,
			double epsd, bool full)
		{
			var cands = set1.Values.ToList();
			cands.Sort((a, b) => a.Value.CompareTo(b.Value));

			int count = cands.Count;
			var eps = epsd == 0 ? long.MaxValue : DoubleToLong(epsd);
			foreach(var cand in set2.Values)
			{
				var d = cand.Value;
				var target = -d - eps;
				int left = 0;
				int right = count - 1;
				while (left <= right)
				{
					int mid = (left + right) / 2;
					if (target > cands[mid].Value)
						left = mid + 1;
					else
						right = mid - 1;
				}

				if (left == count)
					left = 0;

				int index, k;

				if (full)
				{
					for (index = left, k = 0;
						k < count;
						k++, index = index > 0 ? index - 1 : count - 1)
					{
						var d2 = cands[index].Value;
						var frac = d + d2;
						var diff = Abs(frac);
						if (diff > eps) break;
						left = index;
					}
				}

				for (index = left, k = 0;
					k < count;
					k++, index = index + 1 < count ? index + 1 : 0)
				{
					var d2 = cands[index].Value;
					var frac = d + d2;
					var diff = Abs(frac);
					if (diff > eps) break;
					Push(set3, frac, cand, cands[index]);
				}
			}
		}

		public static void Push(Dictionary<long, Candidate> dict, long v, Candidate c1 = null, Candidate c2 = null)
		{
			Candidate next = null;
			if (dict.ContainsKey(v))
				next = dict[v];

			dict[v] = new Candidate { Value = v, Next = next, Cand1 = c1, Cand2 = c2 };

		}


		public static long DoubleToLong(double d)
		{
			var d2 = d - (long)d;
			d2 *= 1L << 62;
			long value = (long)(d2) << 2;
			return value;
		}

		public class Candidate
		{
			public long Value;
			public Candidate Next;
			public Candidate Cand1;
			public Candidate Cand2;
			public int Tag;
			public int Index;
		}

		public class Coord
		{
			public int X;
			public int Y;
			public double Value => Sqrt(X * X + Y * Y);
			public long Code => DoubleToLong(Value);

			public Coord()
			{

			}

			public Coord(int x, int y)
			{
				X = x;
				Y = y;
			}
			public override int GetHashCode()
			{
				return X.GetHashCode() ^ Y.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var coord = obj as Coord;
				return coord != null && coord.X == X && coord.Y == Y;
			}

			public override string ToString()
			{
				return $"{X} {Y}";
			}

		}

		public static double Frac(double f)
		{
			return f - Floor(f);
		}


#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

}