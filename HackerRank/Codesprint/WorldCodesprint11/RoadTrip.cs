namespace HackerRank.WorldCodeSprint11.RoadTrip
{
	//namespace HackerRank.WeekOfCode31.Problem
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using static FastIO;
	using static System.Math;

	// https://www.hackerrank.com/contests/world-codesprint-11/challenges/road-trip-1

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			solve();
			Flush();
		}

		int n;
		int q;
		Query[] queries;
		static long[] answers;

		public void Test()
		{
			n = 12;
			q = n * (n + 1) / 2;

			queries = new Query[q];
			answers = new long[q];
			int qq = 0;
			for (int i = 0; i < n; i++)
				for (int j = i; j < n; j++)
					queries[qq] = new Query { Index = qq++, Left = i, Right = j };

			var r = new Random();
			ws = new long[n - 1];
			gs = new long[n];
			ps = new long[n];

			for (int i = 0; i < n; i++)
			{
				if (i + 1 < n)
					ws[i] = r.Next(0, 10);
				gs[i] = r.Next(0, 10);
				ps[i] = r.Next(0, 10);
			}

		}

		public void solve()
		{
			ReadData();

			//while (true)
			Process();
		}

		void Process()
		{
			bool test = ws[0] != 1000000;

			if (test)
				PrepNaive();
			else
				PrepOptimized();

			Array.Sort(queries, (a, b) =>
		 {
			 int cmp = a.Left.CompareTo(b.Left);
			 if (cmp != 0) return cmp;
			 cmp = a.Right.CompareTo(b.Right);
			 return cmp;
		 });


			foreach (var query in queries)
			{
				int left = query.Left;
				int right = query.Right;
				if (left < right)
				{
					query.Answer = test ? ProcessNaive(query) : ProcessOptimized(query);
				}
			}

			for (int i = 0; i < q; i++)
				WriteLine(answers[i]);
		}

		RangeMinimumQuery rmqNeeded;
		RangeMinimumQuery rmqSupplied;
		int[,] minPriceRange;
		long[,] minCosts;
		long[] needed;
		long[] supplied;
		long[] gs;
		long[] ps;
		long[] ws;
		int[] mins;
		int[] minIndexes;

		long ProcessOptimized(Query q)
		{
			int left = q.Left;
			int right = Min(q.Right, minRight[q.Right]);
			long cost = 0;
			while (left < right)
			{
				int mp = minPriceRange[0, left];

				long supplied = Supplied(q.Left, left - 1);
				if (supplied == 0 && mp < right)
				{
					// Big Jump
					int j;
					for (j = MaxLevels - 1; j > 0; j--)
					{
						if (minPriceRange[j, left] < right)
							break;
					}

					cost += minCosts[j, left];
					left = minPriceRange[j, left];
					continue;
				}

				int nextPriceMin = Min(right, mp);
				long need1 = Needed(left, nextPriceMin);
				long need2 = Max(0, need1 - supplied);
				cost += ps[left] * need2;
				left = nextPriceMin;
			}

			return cost;
		}

		const int MaxLevels = 28;
		int[] minRight;

		public void PrepOptimized()
		{
			needed = new long[n];
			supplied = new long[n];
			minRight = new int[n];

			mins = new int[n + 2];
			minIndexes = new int[n + 2];
			var minCount = 1;
			mins[0] = int.MinValue;
			minIndexes[0] = n;

			minPriceRange = new int[MaxLevels, n];
			minCosts = new long[MaxLevels, n + 1];

			for (int i = n - 1; i >= 0; i--)
			{
				var v = (int)ps[i];
				// NOTE: Really cool trick to get the previous and next element in the sequence
				int index = BinarySearch(mins, v, 0, minCount - 1);
				minPriceRange[0, i] = minIndexes[index - 1];
				mins[index] = v;
				minIndexes[index] = i;
				minCount = index + 1;
			}

			for (int i = n - 2; i >= 0; i--)
			{
				needed[i] = Max(0, ws[i] - gs[i] + needed[i + 1]);
			}

			int minr = n;
			for (int i = n - 1; i >= 0; i--)
			{
				if (needed[i] == 0) minr = i;
				minRight[i] = minr;
			}

			long supp = 0;
			for (int i = 0; i < n; i++)
				supplied[i] = supp = Math.Max(0, supp + gs[i] - (i + 1 < n ? ws[i] : 0));

			rmqSupplied = new RangeMinimumQuery(supplied);
			rmqNeeded = new RangeMinimumQuery(needed);

			int firstNonZero = n - 1;
			for (int i = n - 1; i >= 0; i--)
			{
				int t;
				if (needed[i] != 0)
					firstNonZero = i;
				else
				{
					t = minPriceRange[0, i];
					if (t < firstNonZero && minPriceRange[0, t] < firstNonZero)
						minPriceRange[0, i] = minPriceRange[0, t];
				}

				minCosts[0, i] = ps[i] * Needed(i, minPriceRange[0, i]);
			}

			for (int k = 1; k < MaxLevels; k++)
			{
				for (int i = 0; i < n; i++)
				{
					int t = minPriceRange[k, i] = minPriceRange[k - 1, i];
					minCosts[k, i] = minCosts[k - 1, i];

					if (t < n && Supplied(i, t - 1) == 0)
					{
						minPriceRange[k, i] = minPriceRange[k - 1, t];
						long cost = minCosts[k - 1, i];
						//long need1 = Needed(i, t);
						//if (need1 > 0) cost += ps[i] * need1;
						cost += minCosts[k - 1, t];
						minCosts[k, i] = cost;
					}
				}
			}

		}

		long Needed(int left, int right)
		{
			if (left >= right) return 0;
			if (right >= n) right = n - 1;
			long need = needed[left];
			if (need == 0) return 0;
			var min = rmqNeeded.GetMin(left, right);
			return need - min;
		}

		long Supplied(int left, int right)
		{
			if (left >= right) return left == right && right + 1 < n ? Max(0, gs[right] - ws[right]) : 0;
			long supp = supplied[right];
			if (supp == 0 || left <= 0) return supp;
			var min = left > 0 ? rmqSupplied.GetMin(left - 1, Min(right, n - 1)) : 0;
			return supp - min;
		}

		public class Query
		{
			public int Index;
			public int Left;
			public int Right;
			public long Answer
			{
				get { return (answers != null && Index < answers.Length) ? answers[Index] : 0; }
				set { answers[Index] = value; }
			}

			public override string ToString()
			{
				return $"#{Index} {Left}-{Right}";
			}
		}

		Record dp;
		int prevLeft = -1;
		int prevRight;

		Record[] records;

		void PrepNaive()
		{
			records = new Record[n];

			for (int i = 0; i < n; i++)
			{
				records[i].W = i + 1 < n ? ws[i] : 0;
				records[i].G = gs[i];
				records[i].P = ps[i];
				records[i].Left = i;
				records[i].Right = i;

				long d = Min(records[i].G, records[i].W);
				records[i].W -= d;
				records[i].G -= d;
			}

		}

		long ProcessNaive(Query q)
		{
			int left = q.Left;
			int right = q.Right;

			if (prevLeft != left)
			{
				dp = records[left];
				prevLeft = left;
				prevRight = left;
			}

			while (prevRight < right)
			{
				prevRight++;
				Combine(ref dp, dp, records[prevRight]);
			}

			return dp.Cost;
		}

		public static void Combine(ref Record record, Record record1, Record record2)
		{
			long d;
			record = record2;

			record.P = Min(record1.P, record.P);
			if (record1.P * record.Quantity < record.Cost)
				record.Cost = record1.P * record.Quantity;

			if (record1.W > 0)
			{
				record.Cost += record1.P * record1.W;
				record.Quantity += record1.W;
			}

			record.Cost += record1.Cost;
			record.Quantity += record1.Quantity;

			record.G += record1.G;
			d = Min(record.G, record.W);
			record.G -= d;
			record.W -= d;
		}

		public static int BinarySearch<T>(T[] array, int value, int left, int right, bool upper = false)
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

		void ReadData()
		{
			n = Ni();
			q = Ni();
			queries = new Query[q];
			answers = new long[q];

			ws = Nl(n - 1);
			gs = new long[n];
			ps = new long[n];
			for (int i = 0; i < n; i++)
			{
				gs[i] = Nl();
				ps[i] = Nl();
			}

			/*		for (int i = n - 2; i >= 0; i--)
					{
						long d = Min(gs[i], ws[i]);
						gs[i] -= d;
						ws[i] -= d;
					}
			*/
			for (int i = 0; i < q; i++)
			{
				int left = Ni() - 1;
				int right = Ni() - 1;
				queries[i] = new Query { Left = left, Right = right, Index = i };
			}

		}


	}

	public struct Record
	{
		public static Record Empty = new Record { P = 1000000 };

		public long W;
		public long G;
		public long P;
		public long Cost;
		public long Quantity;
		public int Left;
		public int Right;

		public override string ToString()
		{
			return $"{Left}-{Right} W={W} G={G} P={P} Cost={Cost} Q={Quantity}";
		}
	}



	public class RangeMinimumQuery
	{
		readonly int[,] _table;
		readonly int _n;
		readonly long[] _array;

		public RangeMinimumQuery(long[] array)
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

		public long GetMin(int left, int right)
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

	public static class FastIO
	{
		#region  Input
		static System.IO.Stream inputStream;
		static int inputIndex, bytesRead;
		static byte[] inputBuffer;
		static System.Text.StringBuilder builder;
		const int MonoBufferSize = 4096;
		public static bool EOF;

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
			if (bytesRead <= 0)
			{
				inputBuffer[0] = 32;
				EOF = true;
			}
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

		public static long[] Nl(int n)
		{
			var list = new long[n];
			for (int i = 0; i < n; i++) list[i] = Nl();
			return list;
		}

		public static int Ni()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg) { c = Read(); }

			int number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint)d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static long Nl()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg) { c = Read(); }

			long number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint)d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static string Ns()
		{
			var c = SkipSpaces();
			builder.Clear();
			while (true)
			{
				if ((uint)c - 33 >= (127 - 33)) break;
				builder.Append((char)c);
				c = Read();
			}
			return builder.ToString();
		}

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint)c - 33 >= (127 - 33));
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
			ulong number = (ulong)signedNumber;
			if (signedNumber < 0)
			{
				Write('-');
				number = (ulong)(-signedNumber);
			}

			Reserve(20 + 1); // 20 digits + 1 extra
			int left = outputIndex;
			do
			{
				outputBuffer[outputIndex++] = (byte)('0' + number % 10);
				number /= 10;
			}
			while (number > 0);

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
				outputBuffer[outputIndex++] = (byte)s[i];
		}

		public static void Write(char c)
		{
			Reserve(1);
			outputBuffer[outputIndex++] = (byte)c;
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

	public static class Parameters
	{
#if DEBUG
		public const bool Verbose = true;
#else
	public const bool Verbose = false;
#endif
	}

	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}
	public class Program
	{
		public static void Main(string[] args)
		{
			Solution solution = new Solution();
			solution.solve(Console.OpenStandardInput(), Console.OpenStandardOutput());

#if DEBUG
			Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
		}
	}


}