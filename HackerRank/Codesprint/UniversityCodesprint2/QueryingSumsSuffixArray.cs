namespace HackerRank.UniversityCodesprint2.QueryingSumsSuffixArray
{
	using System;
	using System.Collections.Generic;
	using static System.Math;
	using System.Text;
	using System.Diagnostics;
	using static System.Console;

	public class Solution
	{
		#region Variables

		static string s;
		static int m;
		static int[][] pairs ;
		static RangeMinimumQuery rmq;
		static int[] lcps;
		static int[] suffixes;
		static int[] indices;
		static SuffixArray sa;
		#endregion

		public static void Main()
		{
			var input = Array.ConvertAll(ReadLine().Split(), int.Parse);
			m = input[1];
			int q = input[2];
			s = ReadLine();

			pairs = new int[m][];
			for (int i = 0; i < m; i++)
				pairs[i] = Array.ConvertAll(ReadLine().Split(), int.Parse);

			var queries = new List<Query>();

			int concatLength = s.Length + 1;
			for (int a0 = 0; a0 < q; a0++)
			{
				var arr = ReadLine().Split();
				string w = arr[0];
				int a = int.Parse(arr[1]);
				int b = int.Parse(arr[2]);

				var query = new Query { Word = w, A = a, B = b, QueryIndex = a0 };
				queries.Add(query);

				concatLength += w.Length + 1;
			}

			var sb = new StringBuilder(concatLength + 2);
			sb.Append(s);
			sb.Append('\xff');
			foreach (var query in queries)
			{
				query.WordIndex = sb.Length;
				sb.Append(query.Word);
				sb.Append('\x1f');
			}

			sa = new SuffixArray(sb);
			suffixes = sa.Suffixes;
			indices = sa.Indices;
			lcps = sa.LongestCommonPrefixes;

			rmq = new RangeMinimumQuery(lcps);


			int[] presum = new int[sb.Length];
			int sum = 0;
			for (int i = 0; i < presum.Length; i++)
			{
				presum[i] = sum;
				if (suffixes[i] < s.Length) sum++;
			}

			foreach (var query in queries)
			{
				long answer = 0;
				for (int i = query.A; i <= query.B; i++)
				{
					int left = pairs[i][0];
					int right = pairs[i][1];
					int len = right - left + 1;
					int leftSuffix = indices[query.WordIndex + left];
					int rightSuffix = leftSuffix + 1;

					/*
					var leftSuffixOrig = leftSuffix;
					var rightSuffixOrig = rightSuffix;


					var leftSuffix2 = leftSuffix;
					var rightSuffix2 = rightSuffix;
					while (lcps[rightSuffix2] >= len)
						rightSuffix2++;
					while (lcps[leftSuffix2] >= len && leftSuffix2 >= 0)
						leftSuffix2--;
						*/

					Find(lcps, len, ref leftSuffix, ref rightSuffix);

					/*
					Debug.Assert(leftSuffix == leftSuffix2);
					Debug.Assert(rightSuffix == rightSuffix2);
					*/

					var occurrences = presum[rightSuffix] - presum[leftSuffix];
					answer += occurrences;
				}
				WriteLine(answer);
			}
		}

		static void Find(int[] lcps, int len, ref int left, ref int right)
		{
			int point = right;
			if (lcps[right] >= len)
			{
				int dist = 1;
				while (right + dist < lcps.Length && rmq.GetMin(point, right + dist) >= len)
				{
					right += dist;
					dist <<= 1;
				}

				for (; dist > 0; dist >>= 1)
				{
					while (right + dist < lcps.Length && rmq.GetMin(point, right + dist) >= len)
						right += dist;
				}

				if (lcps[right + 1] < len)
					right++;
			}


			point = left;
			if (lcps[left] >= len)
			{
				int dist = 1;
				while (left - dist >= 0 && rmq.GetMin(left - dist + 1, point) >= len)
				{
					left -= dist;
					dist <<= 1;
				}

				for (; dist > 0; dist >>= 1)
				{
					while (left - dist >= 0 && rmq.GetMin(left - dist + 1, point) >= len)
						left -= dist;
				}
			}

		}

		class Query
		{
			public int QueryIndex;
			public int WordIndex;
			public string Word;
			public int A;
			public int B;
		}

#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

	public class RangeMinimumQuery
	{
		readonly int[,] _table;
		readonly int _n;
		readonly int[] _array;

		public RangeMinimumQuery(int[] array)
		{
			_array = array;
			_n = array.Length;

			int n = array.Length;
			int lgn = Log2(n);
			_table = new int[lgn, n];

			_table[0, n - 1] = n - 1;
			for (int j = n - 2; j >= 0; j--)
				_table[0, j] = array[j] <= array[j + 1] ? j : j + 1;

			for (int i = 1; i < lgn; i++)
			{
				int curlen = 1 << i;
				for (int j = 0; j < n; j++)
				{
					int right = j + curlen;
					var pos1 = _table[i - 1, j];
					int pos2;
					_table[i, j] =
						(right >= n || array[pos1] <= array[pos2 = _table[i - 1, right]])
							? pos1
							: pos2;
				}
			}
		}

		public int GetArgMin(int left, int right)
		{
			if (left == right) return left;
			int curlog = Log2(right - left + 1);
			int pos1 = _table[curlog - 1, left];
			int pos2 = _table[curlog - 1, right - (1 << curlog) + 1];
			return _array[pos1] <= _array[pos2] ? pos1 : pos2;
		}

		public int GetMin(int left, int right)
		{
			return _array[GetArgMin(left, right)];
		}


		static int Log2(int value)
		{
			var log = 0;
			if ((uint)value >= (1U << 12))
			{
				log = 12;
				value = (int)((uint)value >> 12);
				if (value >= (1 << 12))
				{
					log += 12;
					value >>= 12;
				}
			}
			if (value >= (1 << 6))
			{
				log += 6;
				value >>= 6;
			}
			if (value >= (1 << 3))
			{
				log += 3;
				value >>= 3;
			}
			return log + (value >> 1 & ~value >> 2);
		}
	}

	public class SuffixArray
	{

		private readonly int length;
		public readonly string String;
		private readonly List<int[]> _commonPrefix = new List<int[]>();
		private Bucket[] _matrix;
		private int[] _suffixArray;

        public SuffixArray(string s)
		{
			String = s;
			length = s.Length;

			_commonPrefix.Add(new int[length]);
			for (int i = 0; i < length; i++)
				_commonPrefix[0][i] = s[i];

			Build();
		}

		public SuffixArray(StringBuilder s)
		{
			length = s.Length;

			_commonPrefix.Add(new int[length]);
			for (int i = 0; i < length; i++)
				_commonPrefix[0][i] = s[i];

			Build();
		}

		public SuffixArray(char[] s)
		{
			length = s.Length;

			_commonPrefix.Add(new int[length]);
			for (int i = 0; i < length; i++)
				_commonPrefix[0][i] = s[i];

			Build();
		}

		void Build()
		{
			_matrix = new Bucket[length];
			buffer = new Bucket[length];

			for (int skip = 1, level = 1; skip < length; skip *= 2, level++)
			{
				_commonPrefix.Add(new int[length]);

				for (int i = 0; i < length; i++)
				{
					int top = _commonPrefix[level - 1][i];
					int bottom = i + skip < length ? _commonPrefix[level - 1][i + skip] : -1000;
					_matrix[i] = new Bucket(top, bottom, i);
				}

				RadixSort(_matrix);
				//Array.Sort(_matrix, (a, b) => a.CompareTo(b));

				for (int i = 0; i < length; i++)
					_commonPrefix[level][_matrix[i].Index] =
						i > 0
						&& _matrix[i].Item1 == _matrix[i - 1].Item1
						&& _matrix[i].Item2 == _matrix[i - 1].Item2
							? _commonPrefix[level][_matrix[i - 1].Index]
							: i;
			}
		}


		public int[] Indices
		{
			get { return _commonPrefix[_commonPrefix.Count - 1]; }
		}


		public int[] Suffixes
		{
			get
			{
				if (_suffixArray == null)
				{
					var array = new int[length];
					for (int i = 0; i < length; i++)
						array[i] = _matrix[i].Index;
					_suffixArray = array;
				}
				return _suffixArray;
			}
		}

		public int[] LongestCommonPrefixes
		{
			get
			{
				var txt = _commonPrefix[0];
				var invSuff = Indices;
				var suffixArr = Suffixes;
				int n = suffixArr.Length;
				int[] lcp = new int[n];

				int k = 0;

				for (int i = 0; i < n; i++)
				{
					if (invSuff[i] == n - 1)
					{
						k = 0;
						continue;
					}

					int j = suffixArr[invSuff[i] + 1];

					while (i + k < n && j + k < n && txt[i + k] == txt[j + k])
						k++;

					lcp[invSuff[i] + 1] = k;

					if (k > 0)
						k--;
				}

				return lcp;
			}
		}

		static Bucket[] buffer;

		public void RadixSort(Bucket[] list)
		{
			RadixSort(list, b => (b.Item2 >> 0) & 0xff, 256);
			if (list.Length >= 1 << 8)
				RadixSort(list, b => (b.Item2 >> 8) & 0xff, 256);
			if (list.Length >= 1 << 16)
				RadixSort(list, b => (b.Item2 >> 16) & 0xff, 256);
			if (list.Length >= 1 << 24)
				RadixSort(list, b => (b.Item2 >> 24) & 0xff, 256);
			RadixSort(list, b => (b.Item1 >> 0) & 0xff, 256);
			if (list.Length >= 1 << 8)
				RadixSort(list, b => (b.Item1 >> 8) & 0xff, 256);
			if (list.Length >= 1 << 16)
				RadixSort(list, b => (b.Item1 >> 16) & 0xff, 256);
			if (list.Length >= 1 << 24)
				RadixSort(list, b => (b.Item1 >> 24) & 0xff, 256);
		}

		public static unsafe void RadixSort(IList<Bucket> list, Func<Bucket, int> func, int buckets)
		{
			var offsets = stackalloc int[buckets + 1];

			for (int i = 0; i < buckets; i++)
				offsets[i] = 0;

			for (int i = 0; i < list.Count; i++)
			{
				var index = func(list[i]);
				offsets[index]++;
			}

			int sum = 0;
			for (int i = 0; i < buckets; i++)
			{
				var newSum = sum + offsets[i];
				offsets[i] = sum;
				sum = newSum;
			}

			for (int i = 0; i < list.Count; i++)
			{
				var index = func(list[i]);
				buffer[offsets[index]++] = list[i];
			}

			for (int i = 0; i < list.Count; i++)
				list[i] = buffer[i];

		}


		public struct Bucket : IComparable<Bucket>
		{
			public int Item1;
			public int Item2;
			public int Index;

			public Bucket(int item1, int item2, int index)
			{
				Item1 = item1;
				Item2 = item2;
				Index = index;
			}

			public int CompareTo(Bucket b)
			{
				int cmp = Item1.CompareTo(b.Item1);
				if (cmp != 0) return cmp;

				cmp = Item2.CompareTo(b.Item2);
				if (cmp != 0) return cmp;

				cmp = Index.CompareTo(b.Index);
				return cmp;
			}

		}

		public int LongestCommonPrefix(int i, int j)
		{
			int len = 0;
			if (i == j)
				return length - i;

			for (int k = _commonPrefix.Count - 1; k >= 0 && i < length && j < length; k--)
			{
				if (_commonPrefix[k][i] == _commonPrefix[k][j])
				{
					i += 1 << k;
					j += 1 << k;
					len += 1 << k;
				}
			}
			return len;
		}
	}
}