#undef HASH
#define HORIZ
#undef PILE
#define QUERYMAP
#undef CHECKALL

namespace HackerRank.WeekOfCode34.MagicCards
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;
	using static System.Array;
	using static System.Math;
	using static Library;

	class Solution
	{
		private int n, m, qMax;
		private Card[] cards;
		private int[][] queries;
		private long[] answers;
		private long maxResult;
		private int minimumToAll;
		private int[] minAll;
		private int[] allSum;
		private int[] hashes;
		private int[] valueOn;
		private ulong[][] horizBits;
		private long[] sqsum;
		private Card[] pile;

		public unsafe void solve()
		{
			n = Ni();
			m = Ni();
			qMax = Ni();
			maxResult = sumOfSquare(m);
			minimumToAll = MinimumToAll(m);

			cards = new Card[n];
			for (int i = 0; i < n; i++)
				cards[i] = new Card(Ni(Ni()), m);

			queries = new int[qMax][];
			answers = new long[qMax];
			for (int q = 0; q < qMax; q++)
				queries[q] = new[] {q, Ni() - 1, Ni() - 1};

#if QUERYMAP
			var queryMap = new Dictionary<long, int[]>();

#else
		Sort(queries, (a, b) =>
		{
			int cmp = a[1].CompareTo(b[1]);
			if (cmp != 0) return cmp;
			return a[2].CompareTo(b[2]);
		});
#endif


			valueOn = new int[m + 1];
			allSum = new int[n + 1];
			hashes = new int[n];
			int presum = 0;
			for (int i = 0; i < n; i++)
			{
				var len = cards[i].Data.Length;
				allSum[i + 1] = presum = presum + (len == 0 || len == m ? 1 : 0);
			}

			minAll = new int[m + 1];
			for (int i = 0; i < minAll.Length; i++)
				minAll[i] = MinimumToAll(i);

			// Handle duplicates
			var positions = new Dictionary<Card, int>();
			var next = new int[n];
			/*for (var i = 0; i < cards.Length; i++)
			{
				var v = cards[i];
				int pos;
				if (positions.TryGetValue(v, out pos)) next[pos] = i;
				positions[v] = i;
			}*/

			var minUniquePositions = new int[n];
			var minUniquePos = n;
			/*for (int i = cards.Length - 1; i >= 0; i--)
			{
				if (next[i] > 0) minUniquePos = Min(minUniquePos, next[i]);
				minUniquePositions[i] = minUniquePos;
			}*/


#if INTERVALS
		sqsum = new long[m + 2];
		long ssum = 0;
		for (long i = 0; i <= m; i++)
			sqsum[i + 1] = ssum = ssum + i * i;
#endif

#if HORIZ || CHECKALL
			horizBits = BuildHorizontalBits();
#endif

#if HASH
		var cache = new Dictionary<long, long>();
		long prehash = 0;
		for (int i = 0; i < n; i++)
			hashes[i] = cards[i].Hash.GetHashCode();
		var hasher = new SequenceHasher64(hashes);
#endif

#if PILE
		var compare = new CardComparer();
		pile = new Card[100];
#else
			pile = cards;
#endif

			int[] prev = null;
			foreach (var q in queries)
			{
				int index = q[0];
				int left = q[1];
				int right = q[2];
				int length = right - left + 1;

				// Short circuit
				if (
					length >= minimumToAll

					// A set and its complement counts as a duplicate result
					// If there is the max card or zero, then return maxresult
					|| allSum[right + 1] - allSum[left] > 0

					// If there are duplicate values, then return maxresult
					//|| right >= minUniquePositions[left]
				)
				{
					answers[index] = maxResult;
					continue;
				}

#if QUERYMAP
				var queryCode = Combine(left, right);
				if (queryMap.TryGetValue(queryCode, out prev))
				{
					answers[index] = answers[prev[0]];
					continue;
				}
				else
				{
					queryMap[queryCode] = q;
				}
#else
			if (prev != null && prev[1] == left && prev[2] == right)
			{
				answers[index] = answers[prev[0]];
				continue;
			}
			prev = q;
#endif

				long ans;

				if (length == 1)
				{
					answers[index] = Max(cards[left].SumSquare, maxResult - cards[left].SumSquare);
					continue;
				}

#if HASH
			long code = hasher.ComputeBackHash(right, length);
			if (cache.TryGetValue(code, out ans))
			{
				answers[index] = ans;
				continue;
			}
#endif

				if (checkAll(left, right))
				{
					ans = maxResult;
					goto Finish;
				}

#if PILE
			for (int i = left; i <= right; i++) pile[i-left] = cards[i];
			Array.Sort(pile, 0, length, compare);
			ans = dfs(0, right-left);
#elif !HORIZ
			pile = cards;
			ans = dfs(left, right);
#else
				ans = Horiz(left, right);
#endif

				Finish:
				answers[index] = ans;

#if HASH
			cache[code] = ans;
#endif
			}

			foreach (var ans in answers)
				WriteLine(ans);
		}

		private long Combine(long x, long y)
		{
			return (x << 20) + y;
		}


		private Dictionary<long, long> horizMap = new Dictionary<long, long>();

		public long Horiz(int left, int right)
		{
			int length = right - left + 1;
			horizMap.Clear();
			for (int i = 1; i <= m; i++)
			{
				var section = GetSection(horizBits[i], left, length);
				long output;
				horizMap.TryGetValue(section, out output);
				horizMap[section] = output + i * 1L * i;
			}

			// TODO: Check for functional dependencies
			if (length >= minAll[horizMap.Count])
				return maxResult;

			long best = 0;
			foreach (var key in horizMap.Keys)
			{
				long ss = 0;
				foreach (var pair2 in horizMap)
				{
					if (((key ^ ~pair2.Key) & (1 << length) - 1) != 0)
						ss += pair2.Value;
				}

				var ss2 = Max(ss, maxResult - ss);
				if (ss2 > best)
				{
					best = ss2;
					if (best >= maxResult) break;
				}
			}
			return best;
		}

		class CardComparer : IComparer<Card>
		{
			public int Compare(Card a, Card b)
			{
				int cmp = a.Data.Length - b.Data.Length;
				if (cmp != 0) return cmp;
				return a.SumSquare.CompareTo(b.SumSquare);
			}
		}


		bool checkAll(int left, int right)
		{
			int length = right - left + 1;
			int min = m;
			for (int i = left; i <= right; i++)
				min = Min(min, cards[i].Data.Length);
			if (length - 1 >= minAll[min])
				return true;

#if !HORIZ && CHECKALL
		var dict = new HashSet<long>();
		for (int i = 1; i <= m; i++)
		{
			var section = GetSection(horizBits[i], left, length);
			dict.Add(section);
		}

		// TODO: Check for functional dependencies
		if (length >= minAll[dict.Count])
			return true;
#endif

			return false;
		}

		private int shift;

		long dfs(int left, int right)
		{
			shift = 0;
			return dfs(left, right, 0, m);
		}

		long dfs(int left, int right, long sum, int bits)
		{
			int length = right - left + 1;
			if (length <= 0 || sum == maxResult)
				return sum;

			if (length >= minAll[bits])
				return maxResult;

			var card = pile[left];
			var data = card.Data;

			// Try flipped
			long newSum2 = maxResult;
			int newBits2 = 0;
			shift++;
			foreach (var v in data)
				if (--valueOn[v] + shift == 0)
				{
					newSum2 -= v * v;
					newBits2++;
				}

			long result2 = newSum2 != sum ? dfs(left + 1, right, newSum2, newBits2) : 0;

			shift--;
			foreach (var v in data)
				valueOn[v]++;

			if (result2 == maxResult)
				return result2;

			// Try normal

			long newSum = sum;
			int newBits = bits;
			foreach (var v in data)
				if (valueOn[v]++ + shift == 0)
				{
					newSum += v * v;
					newBits--;
				}

			long result1 = newSum != sum || result2 != 0 ? dfs(left + 1, right, newSum, newBits) : 0;

			foreach (var v in data)
				valueOn[v]--;

			return Max(result1, result2);
		}

		static int MinimumToAll(int m)
		{
			int count = 0;
			while (m > 0)
			{
				// We can always get rid of ceil(m/2) with one card
				m >>= 1;
				count++;
			}
			return count;
		}


		class Card
		{
			public readonly int[] Data;
			public readonly ulong[] BitSet;
			public readonly Interval[] Intervals;

			public readonly long SumSquare;
			public readonly long Sum;
			public readonly long Hash;

			public Card(int[] dataOrig, int m)
			{
				Sort(dataOrig);

				int[] data = dataOrig;
				int len = data.Length;
				if (len + len > m || len + len == m && data[len - 1] == 5)
				{
					int newLen = m - len;
					int[] newData = new int[newLen];

					int r = 0;
					int w = 0;
					for (int i = 1; i <= m; i++)
					{
						if (r < len && i >= data[r])
							r++;
						else
							newData[w++] = i;
					}

					data = newData;
					len = newLen;
					Debug.Assert(w == newLen);
				}

				Data = data;
				//BitSet = BuildBitSet(data, m);
#if INTERVALS
			Intervals = BuildIntervals(data);
#endif
				long hash = len;
				foreach (var v in data)
				{
					SumSquare += v * v;
					Sum += v;
					//hash = hash * 1231 + v;
					hash = hash * 9337 + v;
				}

				Hash = hash ^ SumSquare << 20;
			}

			public override int GetHashCode()
			{
				return Hash.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as Card);
			}

			public bool Equals(Card card)
			{
				if (card == this) return true;
				if (card == null) return false;

				return Hash == card.Hash
					   && SumSquare == card.SumSquare
					   && Sum == card.Sum
					   && Data.Length == card.Data.Length;
			}
		}


		ulong[][] BuildHorizontalBits()
		{
			var db = new ulong[m + 1][];
			for (int i = 0; i < db.Length; i++)
				db[i] = BuildBitSet(n);
			for (int i = 0; i < n; i++)
			{
				var card = cards[i];
				foreach (var d in card.Data)
					Set(db[d], i);
			}
			return db;
		}


		static Interval[] BuildIntervals(int[] data)
		{
			Interval[] intervals = null;
			for (int k = 0; k < 2; k++)
			{
				int count = 0;
				for (int i = 0; i < data.Length; i++)
				{
					int start = i;
					while (i + 1 < data.Length && data[i + 1] == 1 + data[i])
						i++;
					if (intervals != null)
						intervals[count] = new Interval {Start = data[start], End = data[i]};
					count++;
				}

				if (k == 0) intervals = new Interval[count];
			}
			return intervals;
		}

		static ulong[] BuildBitSet(int[] data, int m)
		{
			var bitset = BuildBitSet(m);
			foreach (var d in data) bitset[d >> 6] |= 1ul << (d & 63);
			return bitset;
		}

		static ulong[] BuildBitSet(int m)
		{
			var bitset = new ulong[(m + 63 + 64) >> 6];
			return bitset;
		}

		static long GetSection(ulong[] bitset, int index, int length)
		{
			ulong result = 0;
			var end = index + length;
			int startbit = index & 63;
			int startword = index >> 6;
			int startlen = 64 - startbit;
			int endword = end >> 6;


			result = (bitset[startword] >> startbit);
			if (startlen < 64) result &= (1ul << startlen) - 1;
			if (endword > startword) result |= bitset[endword] << 64 - startbit;
			result &= (1ul << length) - 1;
			return (long) result;
		}

		static bool Get(ulong[] bitset, int d) => (bitset[d >> 6] & 1ul << (d & 63)) != 0;

		static void Set(ulong[] bitset, int d) => bitset[d >> 6] |= 1ul << (d & 63);

		static void Clear(ulong[] bitset, int d) => bitset[d >> 6] &= ~(1ul << (d & 63));

		static long sumOfSquare(long n) => n * (n + 1) * (2 * n + 1) / 6;
	}


	public struct Interval
	{
		public int Start;
		public int End;
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

		public SequenceHasher64(int[] s)
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

		public SequenceHasher(int[] s, int hashFactor = 307, long hashMod = int.MaxValue)
		{
			HashMod = hashMod;
			HashFactor = hashFactor;

			long hash = 0;
			_hashes = new long[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				hash = ((hash * HashFactor) + s[i]) % HashMod;
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
			return ((hash * HashFactor) + ch) % HashMod;
		}

		public long Mult(long a, long b)
		{
			return a * b % HashMod;
		}

	}

	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}

	static partial class Library
	{

		#region Common

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		public static void Clear<T>(T[] t, T value = default(T))
		{
			for (int i = 0; i < t.Length; i++)
				t[i] = value;
		}

		public static int BinarySearch<T>(T[] array, T value, int left, int right, bool upper = false)
			where T : IComparable<T>
		{
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				int cmp = value.CompareTo(array[mid]);
				if (cmp > 0 || cmp == 0 && upper)
					left = mid + 1;
				else
					right = mid - 1;
			}
			return left;
		}

		#endregion

		#region  Input

		static System.IO.Stream inputStream;
		static int inputIndex, bytesRead;
		static byte[] inputBuffer;
		static System.Text.StringBuilder builder;
		const int MonoBufferSize = 4096;

		public static void InitInput(System.IO.Stream input = null, int stringCapacity = 16)
		{
			builder = new System.Text.StringBuilder(stringCapacity);
			inputStream = input ?? Console.OpenStandardInput();
			inputIndex = bytesRead = 0;
			inputBuffer = new byte[MonoBufferSize];
		}

		static void ReadMore()
		{
			inputIndex = 0;
			bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
			if (bytesRead <= 0) inputBuffer[0] = 32;
		}

		public static int Read()
		{
			if (inputIndex >= bytesRead) ReadMore();
			return inputBuffer[inputIndex++];
		}

		public static T[] N<T>(int n, Func<T> func)
		{
			var list = new T[n];
			for (int i = 0; i < n; i++) list[i] = func();
			return list;
		}

		public static int[] Ni(int n)
		{
			var list = new int[n];
			for (int i = 0; i < n; i++) list[i] = Ni();
			return list;
		}

		public static long[] Nl(int n)
		{
			var list = new long[n];
			for (int i = 0; i < n; i++) list[i] = Nl();
			return list;
		}

		public static string[] Ns(int n)
		{
			var list = new string[n];
			for (int i = 0; i < n; i++) list[i] = Ns();
			return list;
		}

		public static int Ni()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg)
			{
				c = Read();
			}

			int number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint) d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static long Nl()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg)
			{
				c = Read();
			}

			long number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint) d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static char[] Nc(int n)
		{
			var list = new char[n];
			for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char) c;
			return list;
		}

		public static byte[] Nb(int n)
		{
			var list = new byte[n];
			for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte) c;
			return list;
		}

		public static string Ns()
		{
			var c = SkipSpaces();
			builder.Clear();
			while (true)
			{
				if ((uint) c - 33 >= (127 - 33)) break;
				builder.Append((char) c);
				c = Read();
			}
			return builder.ToString();
		}

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint) c - 33 >= (127 - 33));
			return c;
		}

		#endregion

		#region Output

		static System.IO.Stream outputStream;
		static byte[] outputBuffer;
		static int outputIndex;

		public static void InitOutput(System.IO.Stream output = null)
		{
			outputStream = output ?? Console.OpenStandardOutput();
			outputIndex = 0;
			outputBuffer = new byte[65535];
			AppDomain.CurrentDomain.ProcessExit += delegate { Flush(); };
		}

		public static void WriteLine(object obj = null)
		{
			Write(obj);
			Write('\n');
		}

		public static void WriteLine(long number)
		{
			Write(number);
			Write('\n');
		}

		public static void Write(long signedNumber)
		{
			ulong number = (ulong) signedNumber;
			if (signedNumber < 0)
			{
				Write('-');
				number = (ulong) (-signedNumber);
			}

			Reserve(20 + 1); // 20 digits + 1 extra
			int left = outputIndex;
			do
			{
				outputBuffer[outputIndex++] = (byte) ('0' + number % 10);
				number /= 10;
			} while (number > 0);

			int right = outputIndex - 1;
			while (left < right)
			{
				byte tmp = outputBuffer[left];
				outputBuffer[left++] = outputBuffer[right];
				outputBuffer[right--] = tmp;
			}
		}

		public static void Write(object obj)
		{
			if (obj == null) return;

			var s = obj.ToString();
			Reserve(s.Length);
			for (int i = 0; i < s.Length; i++)
				outputBuffer[outputIndex++] = (byte) s[i];
		}

		public static void Write(char c)
		{
			Reserve(1);
			outputBuffer[outputIndex++] = (byte) c;
		}

		public static void Write(byte[] array, int count)
		{
			Reserve(count);
			Array.Copy(array, 0, outputBuffer, outputIndex, count);
			outputIndex += count;
		}

		static void Reserve(int n)
		{
			if (outputIndex + n <= outputBuffer.Length)
				return;

			Dump();
			if (n > outputBuffer.Length)
				Array.Resize(ref outputBuffer, Math.Max(outputBuffer.Length * 2, n));
		}

		static void Dump()
		{
			outputStream.Write(outputBuffer, 0, outputIndex);
			outputIndex = 0;
		}

		public static void Flush()
		{
			Dump();
			outputStream.Flush();
		}

		#endregion

	}


	public class Program
	{
		public static void Main(string[] args)
		{
			InitInput(Console.OpenStandardInput());
			InitOutput(Console.OpenStandardOutput());
			Solution solution = new Solution();
			solution.solve();
			Flush();
#if DEBUG
			Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
		}
	}
}