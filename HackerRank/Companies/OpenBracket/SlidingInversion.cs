#define BIT


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;
using STType = System.Int64;

class Solution
{
	private int m, n;
	private int[] arr, ind;

	public void solve()
	{
		n = Ni();
		m = Ni();
		var array = Ni(n);
		arr = (int[]) array.Clone();

		var values = CompressX(arr);
		for (int i = 0; i < arr.Length; i++)
			arr[i]++;

		long result = 0;

#if BIT
		var bit = new FenwickTree(n+3);
#else
		var bit = new LazySegmentTree(n);
#endif
		long inversions = 0;
		for (int i = 0; i < n; i++)
		{
			if (i >= m)
			{
				bit.Add(arr[i-m], -1);
				inversions -= bit.SumInclusive(0, arr[i-m] - 1);
			}


			inversions += bit.SumInclusive(arr[i] + 1, n+2);
			bit.Add(arr[i], 1);

			if (i >= m - 1)
				result += inversions;
		}

		WriteLine(result);
	}

	public static int[] CompressX(int[] a)
		{
			int n = a.Length;
			var ret = (int[])a.Clone();
			int[] ind = new int[n];
			for (int i = 0; i < n; i++)
				ind[i] = i;

			Array.Sort(ret, ind);

			int p = 0;
			for (int i = 0; i < n; i++)
			{
				if (i == 0 || ret[i] != ret[i - 1]) ret[p++] = ret[i];
				a[ind[i]] = p - 1;
			}

			Array.Resize(ref ret, p);
			return ret;
		}

	public class FenwickTree
	{
		public readonly long[] Array;

		/*public Fenwick(int[] a) : this(a.Length)
        {
            for (int i = 0; i < a.Length; i++)
                Add(i, a[i]);
        }*/

		public FenwickTree(long[] a) : this(a.Length)
		{
			int n = a.Length;
			System.Array.Copy(a, 0, Array, 1, n);
			for (int k = 2, h = 1; k <= n; k *= 2, h *= 2)
			{
				for (int i = k; i <= n; i += k)
					Array[i] += Array[i - h];
			}
		}

		public FenwickTree(long size)
		{
			Array = new long[size + 1];
		}

		// Increments value		
		/// <summary>
		/// Adds val to the value at i
		/// </summary>
		/// <param name="i">The i.</param>
		/// <param name="val">The value.</param>
		public void Add(int i, long val)
		{
			if (val == 0) return;
			for (i++; i < Array.Length; i += (i & -i))
				Array[i] += val;
		}

		// Sum from [0 ... i]
		public long SumInclusive(int i)
		{
			long sum = 0;
			for (i++; i > 0; i -= (i & -i))
				sum += Array[i];
			return sum;
		}

		public long SumInclusive(int i, int j)
		{
			return SumInclusive(j) - SumInclusive(i - 1);
		}

		// get largest value with cumulative sum less than or equal to x;
		// for smallest, pass x-1 and add 1 to result
		public int GetIndexGreaterEqual(long x)
		{
			int i = 0, n = Array.Length - 1;
			for (int bit = HighestOneBit(n); bit != 0; bit >>= 1)
			{
				int t = i | bit;
				if (t <= n && Array[t] < x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return i;
		}

		// get largest value with cumulative sum less than or equal to x;
		// for smallest, pass x-1 and add 1 to result
		public int GetIndexGreater(long x)
		{
			int i = 0, n = Array.Length - 1;
			for (int bit = HighestOneBit(n); bit != 0; bit >>= 1)
			{
				int t = i | bit;
				if (t <= n && Array[t] <= x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return i;
		}

		public int FindGFenwick(long x)
		{
			int i = 0;
			int n = Array.Length;
			for (int b = HighestOneBit(n); b != 0 && i < n; b >>= 1)
			{
				int t = i + b;
				if (t < n && Array[t] <= x)
				{
					i = t;
					x -= Array[t];
				}
			}
			return x != 0 ? -(i + 1) : i - 1;
		}

		public long[] GetArray()
		{
			int n = Array.Length - 1;
			long[] ret = new long[n];
			for (int i = 0; i < n; i++)
				ret[i] = SumInclusive(i);

			for (int i = n - 1; i >= 1; i--)
				ret[i] -= ret[i - 1];
			return ret;
		}

		public static int HighestOneBit(int n)
		{
			return n != 0 ? 1 << Log2(n) : 0;
		}

		public static int Log2(int value)
		{
			// TESTED
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

	public class RangeFenwick
	{
		FenwickTree bit1;
		FenwickTree bit2;

		public RangeFenwick(int n)
		{
			bit1 = new FenwickTree(n);
			bit2 = new FenwickTree(n);
		}

		public void AddRangeInclusive(int j, long v)
		{
			bit1.Add(j + 1 * 0, -v);
			bit2.Add(j + 1 * 0, v * (j + 1));
			bit1.Add(0, v);
		}

		public void AddRangeInclusive(int i, int j, long v)
		{
			if (i > j) return;
			bit1.Add(j + 1 * 0, -v);
			bit2.Add(j + 1 * 0, v * (j + 1));
			bit1.Add(i, v);
			bit2.Add(i, -v * i);
		}

		//public void AddProgressionInclusive(int i, int j, int diff)
		//{
		//	if (i > j) return;
		//	bit1.Add(j + 1 * 0, -v);
		//	bit2.Add(j + 1 * 0, v * (j + 1));
		//	bit1.Add(i, v);
		//	bit2.Add(i, -v * i);
		//}

		// Range query: Returns the sum of all elements in [1...b]
		public long QueryRangeInclusive(int i)
		{
			if (i < 0) return 0;
			return bit2.SumInclusive(i) - bit1.SumInclusive(i) * (i + 1);
		}

		// Range query: Returns the sum of all elements in [i...j]
		public long QueryRangeInclusive(int i, int j)
		{
			return QueryRangeInclusive(j) - QueryRangeInclusive(i - 1);
		}

		public long[] Table()
		{
			int n = bit1.Array.Length - 1;
			long[] result = new long[n];
			for (int i = 0; i < n; i++)
				result[i] = QueryRangeInclusive(i);
			for (int i = n - 1; i >= 1; i--)
				result[i] -= result[i - 1];
			return result;
		}
	}

	public class LazySegmentTree
	{
		public STType Min;
		public STType Max;
		public STType Sum;
		public STType LazyAdd;
		public int Start;
		public int End;
		public LazySegmentTree Left;
		public LazySegmentTree Right;
		public bool Covering;
		public int Length => End - Start + 1;

		public LazySegmentTree(int n)
			: this(null, 0, n - 1)
		{
		}

		LazySegmentTree(STType[] array, int start, int end)
		{
			Start = start;
			End = end;

			if (end > start)
			{
				int mid = (start + end) / 2;
				Left = new LazySegmentTree(array, start, mid);
				Right = new LazySegmentTree(array, mid + 1, end);
				UpdateNode();
			}
			else
			{
				var v = array == null ? 0 : array[start];
				Min = v;
				Max = v;
				Sum = v;
			}
		}

		public long QueryRangeInclusive(int start, int end)
		{
			if (Start >= start && End <= end)
				return Sum;
			if (start > End || end < Start)
				return 0;

			LazyPropagate();
			return Left.QueryRangeInclusive(start, end) + Right.QueryRangeInclusive(start, end);
		}

		public long GetMinInclusive(int start, int end)
		{
			if (Start >= start && End <= end)
				return Min;
			if (start > End || end < Start)
				return long.MaxValue;

			LazyPropagate();
			return Min(Left.GetMinInclusive(start, end), Right.GetMinInclusive(start, end));
		}


		public void AddRangeInclusive(int start, int end, STType value)
		{
			if (start > End || end < Start)
				return;

			if (Start >= start && End <= end)
			{
				Add(value);
				return;
			}

			LazyPropagate();
			Left.AddRangeInclusive(start, end, value);
			Right.AddRangeInclusive(start, end, value);
			UpdateNode();
		}

		void Add(STType value)
		{
			Sum += value * Length;
			Min += value;
			Max += value;
			LazyAdd += value;
		}

		void LazyPropagate()
		{
			if (Start == End)
				return;

			if (Covering)
			{
				Left.Cover(Min);
				Right.Cover(Min);
				LazyAdd = 0;
				Covering = false;
				return;
			}

			if (LazyAdd != 0)
			{
				var value = LazyAdd;
				LazyAdd = 0;
				Left.Add(value);
				Right.Add(value);
			}
		}

		void UpdateNode()
		{
			var left = Left;
			var right = Right;
			Sum = left.Sum + right.Sum;
			Min = Min(left.Min, right.Min);
			Max = Max(left.Max, right.Max);
		}

		public void CoverInclusive(int start, int end, STType value)
		{
			if (start > End || end < Start)
				return;

			if (Start >= start && End <= end)
			{
				Cover(value);
				return;
			}

			LazyPropagate();
			Left.CoverInclusive(start, end, value);
			Right.CoverInclusive(start, end, value);
			UpdateNode();
		}

		void Cover(STType value)
		{
			Min = value;
			Max = value;
			LazyAdd = 0;
			Sum = value * Length;
			Covering = true;
		}

		// TODO: 
		// SOURCE: http://e-maxx.ru/algo/segment_tree
		/*
			int FindKth(int v, int tl, int tr, int k)
			{
				if (k > t[v])
					return -1;
				if (tl == tr)
					return tl;
				int tm = (tl + tr) / 2;
				if (t[v * 2] >= k)
					return FindKth(v * 2, tl, tm, k);
				else
					return FindKth(v * 2 + 1, tm + 1, tr, k - t[v * 2]);
			}
		*/
	}
}class CaideConstants {
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

	public static char[] Nc(int n)
	{
		var list = new char[n];
		for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
		return list;
	}

	public static byte[] Nb(int n)
	{
		var list = new byte[n];
		for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte)c;
		return list;
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