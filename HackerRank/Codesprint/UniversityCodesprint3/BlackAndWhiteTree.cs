using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;

class Solution
{
	// public const int MOD = 1000000007;

	private int n;
	private List<int>[] g;
	private TreeGraph tree;

	public void solve()
	{
		int n = Ni();
		var colors = Ni(n);

		g = new List<int>[n];
		for (int i = 0; i < n; i++)
			g[i] = new List<int>();

		for (int i = 1; i < n; i++)
		{
			int u = Ni() - 1;
			int v = Ni() - 1;
			g[u].Add(v);
			g[v].Add(u);
		}

		tree = new TreeGraph(g, 0);

		var maxblacks = new int[n];
		var maxwhites = new int[n];


		var order = tree.Queue;
		for (int iv = tree.TreeSize - 1; iv >= 0; iv--)
		{
			int v = order[iv];
			var p = tree.Parents[v];

			if (colors[v] == 1)
			{
				maxblacks[v] = 1;
				maxwhites[v] = -1;
			}
			else
			{
				maxwhites[v] = 1;
				maxblacks[v] = -1;
			}

			foreach (var c in g[v])
			{
				if (c == p) continue;
				if (maxblacks[c] > 0) maxblacks[v] += maxblacks[c];
				if (maxwhites[c] > 0) maxwhites[v] += maxwhites[c];
			}
		}

		bool blackMaxed = false;
		int vMaxed = -1;
		int maxStrange = -1;

		for (int i = 0; i < tree.TreeSize; i++)
		{
			if (maxblacks[i] >= maxStrange)
			{
				blackMaxed = true;
				vMaxed = i;
				maxStrange = maxblacks[i];
			}
			if (maxwhites[i] >= maxStrange)
			{
				blackMaxed = false;
				vMaxed = i;
				maxStrange = maxwhites[i];
			}
		}

		if (blackMaxed)
			Swap(ref maxblacks, ref maxwhites);

		var results = new List<int>(n);
		var queue = new Queue<int>(n);
		queue.Enqueue(vMaxed);
		while (queue.Count > 0)
		{
			var v = queue.Dequeue();
			if (maxwhites[v] <= 0) continue;
			results.Add(v + 1);
			var p = tree.Parents[v];
			foreach (var c in g[v])
			{
				if (c == p) continue;
				queue.Enqueue(c);
			}
		}

		results.Sort();
		WriteLine(maxwhites[vMaxed]);
		WriteLine(results.Count);
		WriteLine(string.Join(" ", results));
	}

	public class TreeGraph
	{
		#region Variables
		public int[] Parents;
		public int[] Queue;
		public int[] Depths;
		public int[] Sizes;
		public IList<int>[] Graph;
		public int Root;
		public int TreeSize;
		public int Separator;

		bool sizesInited;
		#endregion

		#region Constructor
		public TreeGraph(IList<int>[] g, int root = 0, int avoid = -1)
		{
			Graph = g;
			if (root >= 0)
				Init(root, avoid);
		}
		#endregion

		#region Methods
		public void Init(int root, int avoid = -1)
		{
			var g = Graph;
			int n = g.Length;
			Root = root;
			Separator = avoid;

			Queue = new int[n];
			Parents = new int[n];
			Depths = new int[n];

			for (int i = 0; i < Parents.Length; i++)
				Parents[i] = -1;

			Queue[0] = root;

			int treeSize = 1;
			for (int p = 0; p < treeSize; p++)
			{
				int cur = Queue[p];
				var par = Parents[cur];
				foreach (var child in g[cur])
				{
					if (child != par && child != avoid)
					{
						Queue[treeSize++] = child;
						Parents[child] = cur;
						Depths[child] = Depths[cur] + 1;
					}
				}
			}

			TreeSize = treeSize;
		}

		public void InitSizes()
		{
			if (sizesInited)
				return;

			if (Sizes == null)
				Sizes = new int[Graph.Length];
			sizesInited = true;

			Sizes[Separator] = 0;
			for (int i = TreeSize - 1; i >= 0; i--)
			{
				int current = Queue[i];
				Sizes[current] = 1;
				foreach (int e in Graph[current])
					if (Parents[current] != e)
						Sizes[current] += Sizes[e];
			}
		}
		#endregion


	}


}

class CaideConstants {
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
	        if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c=='-';
        if (neg) { c = Read(); }

        long number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if ((uint)d > 9) break;
            number = number * 10 + d;
	        if (number < 0) throw new FormatException();
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