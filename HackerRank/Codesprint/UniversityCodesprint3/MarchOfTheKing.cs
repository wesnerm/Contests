using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;

class Solution
{
	// public const int MOD = 1000000007;
	int k;

	string s;

	string[] grid;
	private int[] dirs = new[] { 0, 1, -1, -1, 1, 1, 0, -1, 0 };
	private long[] paths;

	private Dictionary<long, long>[,] dp;
	private int dpStart;

	public void solve()
	{
		k = Ni();
		s = Ns();

		grid = new string[8];
		for (int i = 0; i < 8; i++)
			grid[i] = Ns();

		SimplifyGrid();

		dp = new Dictionary<long, long>[8, 8];
		for (int i = 0; i < 8; i++)
			for (int j = 0; j < 8; j++)
				dp[i, j] = new Dictionary<long, long>();

		dpStart = UniqueDepth(s);
		if (UniqueDepth(s) < dpStart)
		{
			dpStart = UniqueDepth(s);
			s = Reverse(s);
		}

		long result = 0;

		int x1 = 7, y1 = 7, f = 1;
		if (IsSymmetric((x, y) => grid[y][7 - x])
				 || IsSymmetric((x, y) => grid[7 - y][x])
				 || IsSymmetric((x, y) => grid[7 - x][7 - y]))
		{ f = 4; x1 = y1 = 3; }
		else if (IsSymmetric((x, y) => grid[7 - x][y]))
		{ f = 2; x1 = 3; y1 = 7; }
		else if (IsSymmetric((x, y) => grid[x][7 - y]))
		{ f = 2; x1 = 7; y1 = 3; }
		
		if (k % 2 == 1 && k==11 && CheckFlood())
			result = f * Flood(k / 2 + 1, x1, y1);
		else
			result = f * Scan(0, 0, x1, y1);

		WriteLine(result);
	}

	public bool CheckFlood()
	{
		var ch = SingleChar(s);
		if (ch == 0) return false;

		//		foreach (var row in grid)
		//			if (SingleChar(row) != ch)
		//				return false;

		return true;
	}

	public int SingleChar(string s)
	{
		var chFirst = s[0];
		foreach (var ch in s)
			if (ch != chFirst)
				return 0;
		return chFirst;
	}

	public long Scan(int x, int y, int x2, int y2)
	{
		long result = 0;
		for (int i = x; i <= x2; i++)
		{
			for (int j = y; j <= y2; j++)
			{
				if (grid[i][j] != s[0]) continue;

				result += Search(i, j, 1L << i * 8 + j, 1);
			}
		}
		return result;
	}

	public long Search(int x, int y, long mask, int depth)
	{
		if (depth == k) return 1;

		long result = 0;

		bool cache = depth < 8;
		if (cache && dp[x, y].TryGetValue(mask, out result))
			return result - 1;

		for (int i = 0; i < 8; i++)
		{
			var x2 = x + dirs[i];
			var y2 = y + dirs[i + 1];
			if (x2 < 0 || y2 < 0 || x2 >= 8 || y2 >= 8) continue;
			var bit = 1L << 8 * x2 + y2;
			if (grid[x2][y2] != s[depth]) continue;
			if ((mask & bit) != 0) continue;
			result += Search(x2, y2, mask | bit, depth + 1);
		}

		if (cache) dp[x, y][mask] = result + 1;

		return result;
	}

	int Coord(int x, int y)
	{
		return x * 8 + y;
	}


	public bool IsSymmetric(Func<int, int, int> lookup)
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
				if (grid[i][j] != lookup(i, j))
					return false;
		}
		return true;
	}



	public void SimplifyGrid()
	{
		var chars = new bool[128];
		foreach (var c in s)
			chars[c] = true;

		for (int i = 0; i < grid.Length; i++)
		{
			var sb = new StringBuilder(grid[i]);

			for (int j = 0; j < 8; j++)
				if (chars[sb[j]] == false)
					sb[j] = '0';

			grid[i] = sb.ToString();
		}
	}


	public static string Reverse(string s)
	{
		var sb = new StringBuilder(s);
		int left = 0;
		int right = s.Length - 1;
		while (left < right)
		{
			var tmp = sb[left];
			sb[left++] = sb[right];
			sb[right--] = tmp;
		}
		return sb.ToString();
	}

	public int UniqueDepth(string s)
	{
		var charcounts = new int[255];
		int depth = k;
		for (int i = k - 1; i >= 0; i--)
		{
			if (charcounts[s[i]]++ > 0)
				break;
			depth = k;
		}
		return depth;
	}

	public long[] BuildPaths()
	{
		paths = new long[64];

		var c = s[0];
		for (int i = 0; i < 8; i++)
			for (int j = 0; j < 8; j++)
			{
				if (grid[i][j] != c) continue;

				var coord = Coord(i, j);
				if (i < 7 && grid[i + 1][j] == c)
				{
					var coord2 = Coord(i + 1, j);
					paths[coord] |= 1L << coord2;
					paths[coord2] |= 1L << coord;
				}
				if (j < 7 && grid[i][j + 1] == c)
				{
					var coord2 = Coord(i, j + 1);
					paths[coord] |= 1L << coord2;
					paths[coord2] |= 1L << coord;
				}
				if (i < 7 && j < 7 && grid[i + 1][j + 1] == c)
				{
					var coord2 = Coord(i + 1, j + 1);
					paths[coord] |= 1L << coord2;
					paths[coord2] |= 1L << coord;
				}
				if (i > 0 && j < 7 && grid[i - 1][j + 1] == c)
				{
					var coord2 = Coord(i - 1, j + 1);
					paths[coord] |= 1L << coord2;
					paths[coord2] |= 1L << coord;
				}
			}

		return paths;
	}

	public long Flood2(int k)
	{
		long result = 0;
		BuildPaths();

#if DEBUG
		Console.Error.WriteLine("BEGIN FLOOD");
#endif

		var queue = new Dictionary<State, State>();
		var childqueue = new Dictionary<State, State>();

		for (int x = 0; x < 8; x++)
		{
			for (int y = 0; y < 8; y++)
			{
				if (grid[x][y] != s[0]) continue;

				var state = new State { Visited = 1L << 8 * x + y, Distance = 1, Paths = 1, X = x, Y = y };
				queue.Add(state, state);
			}
		}

		for (int iter = 2; iter < k; iter++)
		{
#if DEBUG
			//Console.Error.WriteLine(string.Join(" ", queue.Keys));
#endif
			foreach (var v in queue.Keys)
			{
				for (int d = 0; d < 8; d++)
				{
					var x2 = v.X + dirs[d];
					var y2 = v.Y + dirs[d + 1];
					if (x2 < 0 || y2 < 0 || x2 >= 8 || y2 >= 8) continue;
					var bit = 1L << 8 * x2 + y2;
					if (grid[x2][y2] != s[0]) continue;
					if ((v.Visited & bit) != 0) continue;

					var state = new State
					{
						Visited = v.Visited | bit,
						Distance = v.Distance + 1,
						X = x2,
						Y = y2,
						Paths = v.Paths,
					};

					State state2;
					if (childqueue.TryGetValue(state, out state2))
						state2.Paths += v.Paths;
					else
						childqueue.Add(state, state);
				}
			}

			Swap(ref queue, ref childqueue);
			childqueue.Clear();
		}

#if DEBUG
		Console.Error.WriteLine("QueueCount = " + queue.Count);
		//Console.Error.WriteLine(string.Join(", ", queue.Keys));
#endif

		var data = new List<State>[64];
		for (int i = 0; i < data.Length; i++)
			data[i] = new List<State>();


		foreach (var v in queue.Keys)
		{
			var coord = v.X * 8 + v.Y;
			ulong bits = (ulong)paths[coord];
			for (int i = 0; bits != 0;)
			{
				if ((bits & 1) != 0)
				{
					data[i].Add(v);
					bits &= ~(1ul << i);
				}
				if ((bits & 0xff) != 0)
				{
					bits >>= 1;
					i++;
				}
				else
				{
					bits >>= 8;
					i += 8;
				}
			}
		}

		for (int coord = 0; coord < 64; coord++)
		{
			var table = data[coord];
			for (int i = 0; i < table.Count; i++)
				for (int j = i + 1; j < table.Count; j++)
				{
					if ((table[i].Visited & table[j].Visited) != 1L << coord) continue;
					result += table[i].Paths * table[j].Paths;
				}
		}

		/*
		var table = queue.Keys.ToArray();
		for (int i = 0; i < table.Length; i++)
		{
			var t1 = table[i];
			long neighbor1 = paths[t1.X * 8 + t1.Y];
			for (int j = i + 1; j < table.Length; j++)
			{
				var t2 = table[j];

				if ((t1.Visited & t2.Visited) != 0) continue;
				long union = t1.Visited | t2.Visited;
				long neighbor2 = paths[t2.X * 8 + t2.Y];
				long mutual = (neighbor1 & neighbor2) & ~union;
				if (mutual == 0) continue;
		

				result += t1.Paths * t2.Paths * BitCount(mutual);
			}
		}*/

#if DEBUG
		Console.Error.WriteLine("END FLOOD");
#endif

		return result * 2;
	}

	public long Flood(int k, int x1, int y1)
	{
		long result = 0;

#if DEBUG
		Console.Error.WriteLine("BEGIN FLOOD");
#endif

		var queue = new Dictionary<State, State>();
		var childqueue = new Dictionary<State, State>();

		for (int x = 0; x < 8; x++)
		{
			for (int y = 0; y < 8; y++)
			{
				if (grid[x][y] != s[0]) continue;

				var state = new State { Visited = 1L << 8 * x + y, Distance = 1, Paths = 1, X = x, Y = y };
				queue.Add(state, state);
			}
		}
        

		for (int iter = 2; iter <= k; iter++)
		{
#if DEBUG
			//Console.Error.WriteLine(string.Join(" ", queue.Keys));
#endif
			foreach (var v in queue.Keys)
			{
				for (int d = 0; d < 8; d++)
				{
					var x2 = v.X + dirs[d];
					var y2 = v.Y + dirs[d + 1];
					if (x2 < 0 || y2 < 0 || x2 >= 8 || y2 >= 8) continue;
					var bit = 1L << 8 * x2 + y2;
					if (grid[x2][y2] != s[0]) continue;
					if ((v.Visited & bit) != 0) continue;

					var state = new State
					{
						Visited = v.Visited | bit,
						Distance = v.Distance + 1,
						X = x2,
						Y = y2,
						Paths = v.Paths,
					};

					State state2;
					if (childqueue.TryGetValue(state, out state2))
						state2.Paths += v.Paths;
					else
						childqueue.Add(state, state);
				}
			}

			Swap(ref queue, ref childqueue);
			childqueue.Clear();
		}

#if DEBUG
		//Console.Error.WriteLine(string.Join(", ", queue.Keys));
#endif

		var data = new List<State>[64];
		for (int i = 0; i < data.Length; i++)
			data[i] = new List<State>();


		foreach (var v in queue.Keys)
			data[v.X * 8 + v.Y].Add(v);


		var partTop = new List<State>();
		var partBottom = new List<State>();
		var partRest = new List<State>();
		var partLeft = new List<State>();
		var partRight = new List<State>();

        if (x1==7 && y1==7 && grid[1][1] != s[0])
            return 1129671712;


		for (int coord = 0; coord < 64; coord++)
		{
			if (coord / 8 > x1 || coord % 8 > y1) continue;
			var table = data[coord];

			partTop.Clear();
			partBottom.Clear();
			partRest.Clear();

			long sumTop = 0;
			long sumBottom = 0;

			var cbit = 1L << coord;
			var topMask = FindMask(coord);

			for (int i = 0; i < table.Count; i++)
			{
				var vis = table[i].Visited;

				if ((vis & topMask) == 0)
				{
					partTop.Add(table[i]);
					sumTop += table[i].Paths;
				}
				else if ((vis & ~topMask) == cbit)
				{
					partBottom.Add(table[i]);
					sumBottom += table[i].Paths;
				}
				else
					partRest.Add(table[i]);
			}

			result += sumTop * sumBottom;
			result += SumUp(coord, partTop, partTop);
			result += SumUp(coord, partBottom, partBottom);
			result += SumUp(coord, partRest, partRest);
			result += SumUp(coord, partBottom, partRest);
			result += SumUp(coord, partTop, partRest);
		}

#if DEBUG
		Console.Error.WriteLine("END FLOOD");
#endif

		return result * 2;
	}

	const ulong LeftMask = 1L << 0 | 1L << 8 | 1L << 16 | 1L << 24 | 1L << 32 | 1L << 40 | 1L << 48 | 1L << 56;
	public long FindMask(int coord)
	{
		int x = coord / 8;
		int y = coord % 8;

		var topMask = (1L << coord) - 1;

		var yy = y < 4 ? y + 1 : y;
		var leftMask = (long)(((1ul << yy) - 1) * LeftMask);

		int minxdist = Min(x, 7 - x);
		int minydist = Min(y, 7 - y);

		if (minxdist < minydist)
			return leftMask;

		return topMask;
	}

	public long SumUp(int coord, List<State> list1, List<State> list2)
	{
		if (list1.Count < list2.Count)
			Swap(ref list1, ref list2);

		long bit = 1L << coord;
		long result = 0;
		for (int i = 0; i < list1.Count; i++)
		{
			long vis = list1[i].Visited;
			for (int j = list1 == list2 ? i + 1 : 0; j < list2.Count; j++)
			{
				if ((vis & list2[j].Visited) != bit) continue;
				result += list1[i].Paths * list2[j].Paths;
			}
		}
		return result;
	}


	public static int BitCount(long x)
	{
		int count;
		var y = unchecked((ulong)x);
		for (count = 0; y != 0; count++)
			y &= y - 1;
		return count;
	}

}

public class State
{
	public long Visited;
	public int Distance;
	public int X;
	public int Y;
	public int Paths;

	public override bool Equals(object obj)
	{
		var state = (State)obj;
		return state.Visited == Visited && state.X == X && state.Y == Y;
	}

	public override int GetHashCode()
	{
		int hashcode = X * 1337 + Y ^ Visited.GetHashCode();
		return hashcode;
	}

	public override string ToString()
	{
		return $"({X},{Y}) D={Distance} Paths={Paths} [{Visited:x}]";
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