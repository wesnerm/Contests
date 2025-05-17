namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class KindergartenAdventures
	{
		static void Main(String[] args)
		{
			int n = int.Parse(Console.ReadLine());
			var a = Console.ReadLine().Split().Select(int.Parse).ToArray();

			var tree = new SegmentTree(n);
			for (int i = 0; i < n; i++)
			{
				int v = a[i];
				if (v >= a.Length) continue;
				int start = (i + 1) % a.Length;
				int end = (i + a.Length - v) % a.Length;
				if (start <= end)
					tree.Modify(start, end - start + 1, 1);
				else
				{
					tree.Modify(start, a.Length - start, 1);
					tree.Modify(0, end + 1, 1);
				}
			}

			int bestIndex = 1;
			int best = 0;
			for (int i = 1; i <= a.Length; i++)
			{
				var v = tree.Query(i - 1);
				//Console.WriteLine($"{i}->{v}");
				if (v > best)
				{
					best = v;
					bestIndex = i;
				}
			}

			Console.WriteLine(bestIndex);
		}

		public class SegmentTree
		{
			private readonly int[] _tree;

			public SegmentTree(int size)
			{
				_tree = new int[size * 2];
			}

			public void Modify(int start, int count, int value)
			{
				int size = _tree.Length / 2;
				int left = start + size;
				int right = start + count + size; // open border
				for (; left < right; left >>= 1, right >>= 1)
				{
					if (left % 2 == 1) _tree[left++] += value;
					if (right % 2 == 1) _tree[--right] += value;
				}
			}

			public int Query(int index)
			{
				int res = 0;
				int i = index + _tree.Length / 2;
				for (; i > 0; i >>= 1)
					res += _tree[i];
				return res;
			}

		}
	}
}
