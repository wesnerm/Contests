
namespace HackerRank.NcrCodesprint
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class GameOfNumbers
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			var q = int.Parse(Console.ReadLine());
			for (int qq = 0; qq < q; qq++)
			{
				var input = Console.ReadLine().Split().Select(int.Parse).ToArray();
				int l = input[0];
				int r = input[1];
				int k = input[2];

				var result = CanWin(l, r, k);
				Console.WriteLine(result ? "Alice" : "Bob");
			}
		}

		static bool CanWin(int left, int right, int k)
		{
			if (k <= 0) return false;
			if (k <= right) return true;
			int kk = k - (right + 1);
			kk %= left + right; // right + 1
			return kk >= left;
		}

		/*
		static bool CanWin(int left, int right, int k)
		{
			if (k <= 0) return false;
			if (k <= right) return true;
			int d = right-left+1;

			var canWin = new BitArray(2*(k+1));
			return CanWin(left, right, k, canWin);
		}

		static bool CanWin(int left, int right, int k, BitArray canWin)
		{
			if (k <= 0) return false;
			if (k <= right) return true;

			if (canWin[2*k]) return canWin[2*k+1];

			int d = right-left+1;
			bool winnable = false;
			for (int i =left; i<=right; i++)
				if (!CanWin(left, right, k-i))
				{
					winnable = true;
					break;
				}

			canWin[2*k] = true;
			canWin[2*k+1] = winnable;
			return winnable;
		}
		*/

		/*
		static bool CanWin(int left, int right, int k)
		{
			if (k <= 0) return false;
			if (k <= right) return true;
			int d = right-left+1;

			var canWin = new BitArray(d, true);
			int winStates = d;
			int pos = 0;

			for (int i=right+1; i<k; i++)
			{
				bool cw = winStates!=d;
				Console.WriteLine("i={0} cw={1} canWin[{2})", i, cw, canWin[pos]);
				winStates -= canWin[pos] ? 1 : 0;
				canWin[pos] = cw;
				winStates += cw ? 1 : 0;

				pos += 1;
				if (pos >= d) pos = 0;
			}

			return winStates!=d;
		}

		static bool CanWin(int left, int right, int k)
		{
			if (k <= 0) return false;
			if (k <= right) return true;
			int kk = k-(right+1);
			kk %= right+1;
			return kk >= left;
		}
		*/

		/*
		static bool CanWin(int left, int right, int k)
		{
			if (k <= 0) return false;
			if (k <= right) return true;
			int d = right-left+1;

			var canWin = new BitArray(d, true);
			int winStates = d;
			int pos = 0;

			for (int i=k-right-1; i>0; i--)
			{
				bool cw = winStates!=d;
				//Console.WriteLine("i={0} cw={1} canWin[{2})", i, cw, canWin[pos]);
				winStates -= canWin[pos] ? 1 : 0;
				canWin[pos] = cw;
				winStates += cw ? 1 : 0;

				pos += 1;
				if (pos >= d) pos = 0;
			}

			return winStates!=d;
		}
		*/


	}
}