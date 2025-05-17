namespace HackerRank.UniversityCodesprint2.QueryingSumsHasher
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
		static int q, k;
		static int[][] pairs = new int[m][];
		static Dictionary<long, QueryFragment>[] hashDict = new Dictionary<long, QueryFragment>[50];

		static SequenceHasher64 hasher;

		#endregion

		public static void Main()
		{
			var input = Array.ConvertAll(ReadLine().Split(), int.Parse);
			m = input[1];
			q = input[2];
			k = input[3];
			s = ReadLine();

			for (int i = 0; i < hashDict.Length; i++)
				hashDict[i] = new Dictionary<long, QueryFragment>(100);

			pairs = new int[m][];
			for (int i = 0; i < m; i++)
			{
				var array = ReadLine().Split();
				pairs[i] = Array.ConvertAll(array, int.Parse);
			}

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

			hasher = new SequenceHasher64(sb);

			foreach (var query in queries)
			{
				if (frags > 10000000)
					Process();

				for (int i = query.A; i <= query.B; i++)
				{
					int left = pairs[i][0];
					int right = pairs[i][1];
					var leftIndex = left + query.WordIndex;
					var rightIndex = right + query.WordIndex;
					int len = right - left + 1;

					var log2 = Log2(len);
					var len2 = len < 8 ? len : 1 << log2;
					var dictId = len < 8 ? 30 + len : log2;

					long hash = hasher.ComputeBackHash(rightIndex, len);
					long code = len == len2 ? hash : hasher.ComputeBackHash(rightIndex, len2);

					QueryFragment next;
					hashDict[dictId].TryGetValue(code, out next);

					bool addf = true;
					for (var cur = next; cur != null && cur.Query == query; /* cur = cur.Next */)
					{
						if (cur.LeftIndex == leftIndex && cur.Length == len
							|| hash == cur.Hash && (len > 16 || StringsEqual(sb, cur.LeftIndex, leftIndex, len)))
						{
							cur.Multiplier++;
							addf = false;
							break;
						}
						break; // TODO: we probably need a counter
					}

					if (addf)
					{
						next = hashDict[dictId][code] = new QueryFragment
						{
							Query = query,
							RightIndex = rightIndex,
							Left = left,
							Length = len,
							Hash = hash,
							Next = next,
						};
						frags++;
					}
				}
			}


			Process();

			foreach (var query in queries)
				WriteLine(query.Answer);
		}


		static int frags;

		public static void Process()
		{

			if (frags == 0)
				return;

			for (int i = 0; i < s.Length; i++)
			{
				for (int j = 1; j <= i + 1 && j <= k; j = j < 8 ? j + 1 : j << 1)
				{
					var dictId = j < 8 ? 30 + j : Log2(j);
					var rollingHash = hasher.ComputeBackHash(i, j);
					if (hashDict[dictId].ContainsKey(rollingHash))
					{
						for (var f = hashDict[dictId][rollingHash]; f != null; f = f.Next)
						{
							int len = f.Length;
							if (i - len + 1 < 0) continue;
							var textHash = hasher.ComputeBackHash(i, len);
							if (f.Hash == textHash)
								f.Query.Answer += f.Multiplier;
						}
					}
				}
			}

			foreach (var t in hashDict)
				t.Clear();
			frags = 0;
		}

		public static bool StringsEqual(StringBuilder sb, int start1, int start2, int len)
		{
			if (start1 == start2) return true;
			for (int i = 0; i < len; i++)
				if (sb[start1++] != sb[start2++])
					return false;
			return true;
		}


		class Query
		{
			public int QueryIndex;
			public int WordIndex;
			public string Word;
			public int A;
			public int B;
			public int Answer;
			public override string ToString()
			{
				return Word;
			}

		}

		class QueryFragment
		{
			public Query Query;
			public int RightIndex;
			public int LeftIndex => RightIndex - Length + 1;
			public int Left;
			public int Length;
			public int Multiplier = 1;
			public long Hash;
			public string Text => Query.Word.Substring(Left, Length);
			public QueryFragment Next;
			public override string ToString()
			{
				return $"{Text} -- {Query.Word} {Left}";
			}

			public int Count
			{
				get
				{
					int count = 0;
					for (var cur = this; cur != null; cur = cur.Next)
						count++;
					return count;
				}
			}
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

#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

	public class SequenceHasher64
	{
		static SequenceHasher lohasher;
		static SequenceHasher hihasher;

		// This gives us the best range of 32-bit values with the benefits of primality
		const long LoPrime = int.MaxValue; // Largest 32-bit prime
		const long HiPrime = int.MaxValue; // 0x7FFFFFED; // 2nd Largest 31-bit prime

		const int LoFactor = 307;
		const int HiFactor = 2309;

		public SequenceHasher64(StringBuilder s)
		{
			lohasher = new SequenceHasher(s, LoFactor, LoPrime);
			hihasher = new SequenceHasher(s, HiFactor, HiPrime);
		}

		public long ComputeBackHash(int rightIndex, int count)
		{
			long lo = lohasher.ComputeBackHash(rightIndex, count);
			long hi = hihasher.ComputeBackHash(rightIndex, count);
			
			// This gives us a 62 bit hash value
			return lo | (hi << 31);
		}

		public long ComputeForwardHash(int leftIndex, int count)
		{
			return ComputeBackHash(leftIndex + count - 1, count);
		}
	}

	public class SequenceHasher
	{
		public readonly long HashFactor;
		public readonly long HashMod;

		static long[] _hashes;
		static long[] _factors;

		// Modular arithmetic does not mix well with unsigned integers 
		// -- Avoid uint and ulong
		// Negative numbers are not congruent to their unsigned counterparts (ie, -k != 2^n - k mod m)

		// Avoid bit shift operations because of high collision rate especially on comp prog test cases

		public SequenceHasher(StringBuilder s, int hashFactor = 307, long hashMod = int.MaxValue)
		{
			HashMod = hashMod;
			HashFactor = hashFactor;

			long hash = 0;
			_hashes = new long[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				hash = ((hash*HashFactor) + s[i]) % HashMod;
				_hashes[i] = hash;
			}

			long factor = 1;
			_factors = new long[s.Length];
			for (int i = 0; i < _factors.Length; i++)
			{
				_factors[i] = factor;
				factor = (factor * HashFactor) % HashMod;
			}

		}

		public long ComputeBackHash(int rightIndex, int count)
		{
			long hashEnd = _hashes[rightIndex];
			long hashStart = rightIndex >= count ? Mult(_hashes[rightIndex - count], _factors[count]) : 0;
			return (hashEnd + HashMod - hashStart) % HashMod;
		}

		public long ComputeForwardHash(int leftIndex, int count)
		{
			return ComputeBackHash(leftIndex + count - 1, count);
		}

		public long Advance(long hash, int ch)
		{
			return ((hash * HashFactor) +ch) % HashMod;
		}

		public long Mult(long a, long b)
		{
			return a * b % HashMod;
		}

	}

}