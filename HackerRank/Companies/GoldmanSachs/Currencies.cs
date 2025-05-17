using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using static System.Array;
using static System.Math;
using T = Solution.Number;
using static Library;

class Solution
{
	private int n, x, s, f, m;

	private const int MOD = 1000 * 1000 * 1000 + 7;


	public void solve()
	{
		n = Ni();
		x = Ni();
		s = Ni();
		f = Ni();
		m = Ni();

		var A = new T[n, n];
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n; j++)
				A[i, j] = Ni();
		}

		var R = Pow(A, m);
		//		Console.Error.WriteLine(Text(R));
		WriteLine(x * R[s, f].Value % MOD);
	}

	public static string Text(T[,] mat)
	{
		StringBuilder sb = new StringBuilder();
		if (mat == null)
			return null;

		int n = mat.GetLength(0);
		int m = mat.GetLength(1);

		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < m; j++)
			{
				if (j > 0) sb.Append(' ');
				sb.Append(mat[i, j]);
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	public T[,] Mult(T[,] A, T[,] B)
	{
		T[,] c = new T[n, n];
		Mult(A, B, c);
		return c;
	}

	public void Mult(T[,] A, T[,] B, T[,] c)
	{
		for (int i = 0; i < n; i++)
		for (int j = 0; j < n; j++)
			c[i, j] = 0;

		for (int i = 0; i < n; i++)
		for (int j = 0; j < n; j++)
		{
			for (int k = 0; k < n; k++)
			{
				T trial = A[i, k] * B[k, j];
				if (c[i, j] < trial) c[i, j] = trial;
			}
		}
	}

	public T[,] Pow(T[,] a, int p)
	{
		int n = a.GetLength(0);
		var tmp = new T[n, n];
		var result = Diagonal(n, 1);
		var b = Clone(a);

		int i = 0;
		/*		Console.Error.WriteLine("Iter {0}: m = ");
				Console.Error.WriteLine(Text(result));
				Console.Error.WriteLine("b = ");
				Console.Error.WriteLine(Text(b));
		*/
		while (p > 0)
		{
			Console.Error.WriteLine($"Iter {i}");
			if ((p & 1) != 0)
			{
				Mult(result, b, tmp);
				Assign(result, tmp);
				//				Console.Error.WriteLine(Text(result));
			}
			p >>= 1;
			Mult(b, b, tmp);
			Assign(b, tmp);
			//			Console.Error.WriteLine("b = ");
			//			Console.Error.WriteLine(Text(b));
		}
		return result;
	}

	public T[,] Clone(T[,] m)
	{
		return (T[,]) m.Clone();
	}


	public void Assign(T[,] dest, T[,] src)
	{
		Array.Copy(src, dest, src.Length);
	}

	public T[,] Diagonal(int n, T d)
	{
		var id = new T[n, n];
		for (int i = 0; i < n; i++)
			id[i, i] = d;
		return id;
	}


	public struct Number
	{
		public long Value;
		public double Real;
		public double LogReal;

		public static implicit operator Number(long n)
		{
			return new Number() {Value = n, Real = n, LogReal =Log(n)};
		}

		//public static Number operator +(Number n, Number n2)
		//{
		//	return new Number()
		//	{
		//		Value = (n.Value + n2.Value) % MOD,
		//		Real = n.Real + n2.Real
		//	};
		//}

		public static Number operator *(Number n, Number n2)
		{
			if (n.Real == 0 || n2.Real == 0)
				return new Number();

			return new Number()
			{
				Value = (n.Value * n2.Value) % MOD,
				Real = n.Real + n2.Real,
				LogReal = n.LogReal + n2.LogReal
			};
		}

		public static bool operator <(Number n, Number n2)
		{
			if (n.Real < 1e1 || n2.Real < 1e1)
				return n.Real < n2.Real;

			return n.LogReal < n2.LogReal;
		}

		public static bool operator >(Number n, Number n2)
		{
			return n2 > n;
		}

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