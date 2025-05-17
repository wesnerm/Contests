namespace HackRank.WorldSprint11.BestMask
{
	// https://www.hackerrank.com/contests/world-codesprint-11/challenges

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;
	using static System.Math;
	using static System.Array;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();
		}

		int[][] lists;
		int n;
		int[] list;


		public void solve()
		{
			n = Ni();
			list = Ni(n);

			//while (true)
			{
				//int ans = Test();

				Sort(list);
				int max = list[n - 1];
				n = list.Length;

				lists = new int[26][];
				for (int i = 0; i < lists.Length && (1 << i) <= max; i++)
					lists[i] = new int[n];

				int result = DivideAndConquer(list, n);

				//var list2 = list.ConvertAll(x => x & ans);
				//Debug.Assert(ans == result);
				WriteLine(result);
			}
		}

		public int Test()
		{
			var random = new Random();

			n = 10;
			for (int i = 0; i < 10; i++)
				list[i] = (random.Next(1, 16384));

			int ans = int.MaxValue;
			for (int i = 1; i < 16384; i++)
			{
				bool good = true;
				for (int j = 0; j < list.Length; j++)
				{
					if ((list[j] & i) == 0)
					{
						good = false;
						break;
					}
				}

				if (good)
				{
					int cmp = BitCount(ans).CompareTo(BitCount(i));
					if (cmp > 0)
						ans = i;
				}
			}

			return ans;
		}



		int DivideAndConquer(int[] listParam, int length, int mask = 0, int exclude = 0, int max = int.MaxValue, int bitcount = 31, int depth = 0)
		{
			var list = lists[depth];
			//if (list == null) list = lists[depth] = new int[n];

			int listCount = 0;
			var prev = -1;
			var and = -1;
			int minBitCount = int.MaxValue;
			int minBitValue = int.MaxValue;
			for (int i = 0; i < length; i++)
			{
				var v = listParam[i] & ~exclude;
				if ((listParam[i] & mask) != 0 || v == prev) continue;
				list[listCount++] = v;
				and &= v;
				prev = v;

				var bc = BitCount(v & ~exclude);
				if (bc <= minBitCount)
				{
					if (bc < minBitCount)
					{
						if (bc == 0)
							return max;

						minBitCount = bc;
						minBitValue = v;
					}
					else
						minBitValue = Min(minBitValue, v);
				}
			}

			if (listCount == 0)
				return 0;

			if (and != 0)
				return and & -and;

			int result = max;
			int mask2 = minBitValue;
			while (mask2 != 0)
			{
				int bit = mask2 & -mask2;
				mask2 -= bit;

				var check = bit | mask;
				int cmp = BitCount(check).CompareTo(bitcount);
				if (cmp > 0 || cmp == 0 && check >= result) continue;

				var tmp = bit | mask | DivideAndConquer(list, listCount, bit | mask, exclude, result, bitcount, depth + 1);
				cmp = BitCount(tmp).CompareTo(bitcount);
				if (cmp < 0 || cmp == 0 && tmp < result)
				{
					result = tmp;
					bitcount = BitCount(tmp);
				}

				exclude |= bit;
			}

			return result;
		}

		public static int BitCount(int x)
		{
			int count;
			var y = unchecked((uint)x);
			for (count = 0; y != 0; count++)
				y &= y - 1;
			return count;
		}

	}

}