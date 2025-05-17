namespace HackerRank.WeekOfCode32.CircularWalk
{
	// https://www.hackerrank.com/contests/w32/challenges/circular-walk

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();
		}

		int[] r;
		long[] dp;
		long n, s, t;
		long r0, g, seed, p;

		public void solve()
		{
			n = Ni();
			s = Ni();
			t = Ni();

			r0 = Ni();
			g = Ni();
			seed = Ni();
			p = Ni();

			r = new int[n];
			r[0] = (int)r0;
			for (int i = 1; i < n; i++)
			{
				long ri = r[i - 1] * g + seed; // BUGGY
				r[i] = (int)(ri % p);
			}

			long left = s;
			long right = s;
			long maxleft = left - r[s];
			long maxright = right + r[s];

			long ans = -1;
			for (int k = 0; k <= n; k++)
			{
				if (Between(left, right, t))
				{
					ans = k;
					break;
				}

				var newLeft = left;
				var newRight = right;

				for (long i = maxleft; i < left; i++)
				{
					var v = r[Fix(i)];
					newLeft = Math.Min(i - v, newLeft);
					newRight = Math.Max(i + v, newRight);
				}

				for (long i = right + 1; i <= maxright; i++)
				{
					var v = r[Fix(i)];
					newLeft = Math.Min(i - v, newLeft);
					newRight = Math.Max(i + v, newRight);
				}

				if (left == maxleft && right == maxright)
					break;

				left = maxleft;
				right = maxright;
				maxleft = newLeft;
				maxright = newRight;
			}

			WriteLine(ans);
		}

		bool Between(long left, long right, long pos)
		{
			var count = right - left;

			var fixleft = Fix(left);
			if (pos < fixleft)
				pos += n;

			return pos - fixleft <= count;
		}


		public long Fix(long x)
		{
			return ((x % n) + n) % n;
		}



	}


}