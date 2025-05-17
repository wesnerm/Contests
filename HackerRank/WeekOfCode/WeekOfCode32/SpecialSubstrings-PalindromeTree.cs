namespace HackerRank.WeekOfCode32.SpecialSubstrings
{
	// https://www.hackerrank.com/contests/w32/challenges/special-substrings

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;

	using Trie = SuffixAutomaton.Node;


	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
#if DEBUG
			var process = Process.GetCurrentProcess();
			var startTime = process.TotalProcessorTime;
#endif
			solve();
			Flush();

#if DEBUG
			Console.Error.WriteLine(process.TotalProcessorTime - startTime);
#endif
		}

		char[] ca;

		public int Test()
		{
			int n = 50000;

			ca = new char[n];

			int left = 0;
			int right = n - 1;
			int i = 0;
			while (left <= right)
			{
				char ch = (char)('a' + i % 26);
				ca[left++] = ch;
				ca[right--] = ch;
				i++;
			}

			return n;
		}

		public void solve()
		{
			int n = Ni();
			ca = Nc(n);

			//n = Test();

			var pt = new PalindromeTree(n);
			var sa = new SuffixAutomaton(ca);

			//var occurrences = sa.Occurrences();
			//sa.GetNodes();
			var visited = new Dictionary<Pair, Info>(sa.NodeCount);
			var empty = new Info { Length = 0, Index = -1 };

			long prefixes = 0;
			for (var ich = 0; ich < ca.Length; ich++)
			{
				var ch = ca[ich];
				var node = pt.Extend(ch);

				if (node != null)
				{
					var trie = sa.Start;
					var suffix = node.SuffixLink;
					int suffixLength = 0;
					int i;
					if (suffix != null && suffix.TrieLength >= 1)
					{
						trie = suffix.Trie;
						suffixLength = suffix.TrieLength;
						if (suffixLength < suffix.Length)
						{
							for (i = suffixLength; i < suffix.Length; i++)
								trie = trie[ca[ich - i]];
							suffix.Trie = trie;
							suffix.TrieLength = suffixLength = suffix.Length;
						}
					}

					int prefixLength = 0;
					var left = ich - node.Length + 1;
					var right = ich;

					for (i = suffixLength; i < node.Length; i++)
					{
						var c = ca[ich - i];
						var prev = trie;
						trie = trie[c];

						// Check if single occurrence then bail
						/*if (occurrences[trie.Index] == 1)
						{
							prefixLength += node.Length - i;
							i++;
							break;
						}*/

						// Check if seen before
						var pair = new Pair { Node = trie.Index, Length = i };

						Info info;
						if (!visited.TryGetValue(pair, out info))
						{
							visited[pair] = new Info
							{
								Index = left + i + 1,
								Length = node.Length - i - 1,
							};
							prefixLength += node.Length - i;
							i++;
							break;
						}

						if (info.Length != 0)
						{
							visited[pair] = empty;
							if (info.Length > 0)
							{
								var t = trie[ca[info.Index]];
								var pair2 = new Pair { Node = t.Index, Length = i + 1 };
								info.Index++;
								info.Length--;
								visited[pair2] = info;
							}
						}
					}
					prefixes += prefixLength;
					node.Trie = trie;
					node.TrieLength = i;
				}

				long ans = prefixes;
				WriteLine(ans);
			}
		}
	}

	public class Pair : IEquatable<Pair>
	{
		public int Node;
		public int Length;

		public override string ToString()
		{
			return $"{Node} {Length}";
		}

		public bool Equals(Pair other)
		{
			return Node == other.Node && Length == other.Length;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Pair && Equals((Pair)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Node * 397) ^ Length;
			}
		}
	}

	public struct Info
	{
		public int Index;
		public int Length;
	}


	public class PalindromeTree
	{
		public StringBuilder Text;
		public List<Node> Nodes;
		public Node Root;
		public Node Empty;
		Node suffix;

		public PalindromeTree(int length)
		{
			Text = new StringBuilder(length);
			Root = new Node { Length = -1 };
			Empty = new Node { Length = 0, SuffixLink = Root };
			Root.SuffixLink = Root;

			suffix = Empty;

			Nodes = new List<Node>(length)
			{
				Root,
				Empty,
			};
		}

		public Node Extend(char ch)
		{
			int pos = Text.Length;
			Text.Append(ch);

			Node current;
			int pos2;
			for (current = suffix; ; current = current.SuffixLink)
			{
				pos2 = pos - 1 - current.Length;
				if (pos2 >= 0 && Text[pos2] == ch)
					break;
			}

			int let = ch - 'a';
			if (current.Next[let] != null)
			{
				// We found an existing palindrome
				suffix = current.Next[let];
				return null;
			}

			var node = suffix = new Node { Length = current.Length + 2 };
			current.Next[let] = node;
			Nodes.Add(node);

			if (node.Length == 1)
			{
				// Single character palindrome
				node.SuffixLink = Empty;
				node.Count = 1;
				return node;
			}

			do
			{
				current = current.SuffixLink;
				pos2 = pos - 1 - current.Length;
			}
			while (pos2 < 0 || Text[pos2] != ch);

			node.SuffixLink = current.Next[let];
			node.Count = 1 + node.SuffixLink.Count;
			return node;
		}


		public class Node
		{
			public Node[] Next = new Node[26];
			public Node SuffixLink;
			public int Length;
			public int Count;

			public Trie Trie;
			public int TrieLength;
		}
	}

	public class SuffixAutomaton
	{
		public Node Start;
		public Node End;
		public int NodeCount;
		public IEnumerable<char> Text;

		Node[] _nodes;
		SummarizedState[] _summary;

		private SuffixAutomaton()
		{
			Start = new Node();
			End = Start;
			NodeCount = 1;
		}

		/// <summary>
		/// Constructs an automaton from the string
		/// </summary>
		/// <param name="s"></param>
		public SuffixAutomaton(IEnumerable<char> s) : this()
		{
			Text = s;

			foreach (var c in s)
				Extend(c);

			for (var p = End; p != Start; p = p.Link)
				p.IsTerminal = true;
		}

		/// <summary>
		/// Extends an automaton by one character
		/// </summary>
		/// <param name="c"></param>
		public void Extend(char c)
		{
			var node = new Node
			{
				Key = c,
				Len = End.Len + 1,
				Link = Start,
				Index = NodeCount,
			};
			NodeCount++;

			Node p;
			for (p = End; p != null && p[c] == null; p = p.Link)
				p[c] = node;
			End = node;

			if (p == null) return;

			var q = p[c];
			if (p.Len + 1 == q.Len)
				node.Link = q;
			else
			{
				var clone = q.Clone();
				clone.Len = p.Len + 1;
				clone.Index = NodeCount;
				NodeCount++;

				for (; p != null && p[c] == q; p = p.Link)
					p[c] = clone;

				q.Link = node.Link = clone;
			}
		}

		public Node[] GetNodes()
		{
			if (_nodes != null && NodeCount == _nodes.Length)
				return _nodes;

			var nodes = _nodes = new Node[NodeCount];
			int stack = 0;
			int idx = NodeCount;

			nodes[stack++] = Start;
			while (stack > 0)
			{
				var current = nodes[--stack];

				if (current.Index > 0)
					current.Index = 0;

				current.Index--;
				var index = current.NextCount + current.Index;
				if (index >= 0)
				{
					stack++;

					var child = current.Next[index];
					if (child.Index >= -child.NextCount)
						nodes[stack++] = current.Next[index];
				}
				else if (index == -1)
				{
					nodes[--idx] = current;
				}
				Debug.Assert(idx >= stack);
			}

			if (idx != 0)
			{
				Debug.Assert(idx == 0, "NodeCount smaller than number of nodes");
				NodeCount -= idx;
				_nodes = new Node[NodeCount];
				Array.Copy(nodes, idx, _nodes, 0, NodeCount);
			}

			UpdateNodeIndices();
			return _nodes;
		}

		public int[] Occurrences()
		{
			var counts = new int[NodeCount];
			foreach (var n in NodesBottomUp())
			{
				int count = 0;
				for (int i = 0; i < n.NextCount; i++)
				{
					var child = n.Next[i];
					count += counts[child.Index];
				}
				if (n.IsTerminal)
					count++;
				counts[n.Index] = count;
			}

			return counts;
		}

		/// <summary>
		/// Iterates through nodes in bottom-up fashion
		/// </summary>
		public IEnumerable<Node> NodesBottomUp()
		{
			var nodes = GetNodes();
			for (int i = NodeCount - 1; i >= 0; i--)
				yield return nodes[i];
		}

		void UpdateNodeIndices()
		{
			var nodes = _nodes;
			for (int i = 0; i < NodeCount; i++)
				nodes[i].Index = i;
		}

		/// <summary>
		/// Goes through a node given a string
		/// </summary>
		/// <param name="pattern">string to search for</param>
		/// <param name="index">start of substring in pattern to search for</param>
		/// <param name="count">length of substring</param>
		/// <returns>returns node representing string or null if failed</returns>

		public Node FindNode(string pattern, int index, int count)
		{
			var node = Start;
			for (int i = 0; i < count; i++)
			{
				node = node[pattern[index + i]];
				if (node == null) return null;
			}
			return node;
		}

		public Node FindNode(string pattern)
		{
			return FindNode(pattern, 0, pattern.Length);
		}


		/// <summary>
		/// A state in the compressed automaton
		/// </summary>
		public struct SummarizedState
		{
			/// <summary> the end node of a labeled multicharacter edge </summary>
			public Node Node;
			/// <summary> the number of characters to advance to reach the state </summary>
			public int Length;
			public override string ToString() => $"Node={Node?.Index} Length={Length}";
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var n in GetNodes())
			{
				sb.Append($"{{id:{0}, len:{n.Len}, link:{n.Link?.Index ?? -1}, cloned:{n.IsCloned}, Next:{{");
				sb.Append(string.Join(",", n.Children.Select(c => c.Key + ":" + c.Index)));
				sb.AppendLine("}}");
			}
			return sb.ToString();
		}

		public class Node : TrieNode<Node>
		{
			public int Len;
			public int Index;
			public Node Link;
			public Node Original;

			public int FirstOccurrence => Original?.Len ?? this.Len;

			public bool IsCloned => Original != null;

			public new Node Clone()
			{
				var node = base.Clone();
				node.Original = Original ?? this;
				node.Next = (Node[])node.Next.Clone();
				return node;
			}
		}
	}

	public class TrieNode<T> where T : TrieNode<T>
	{
		public char Key;
		public bool IsTerminal;
		public byte NextCount;
		int KeyMask;
		public T[] Next;
		public static T[] ArrayEmpty = new T[0];

		public TrieNode()
		{
			Next = ArrayEmpty;
		}

		public T this[char ch]
		{
			get
			{
				if ((KeyMask << ~ch) < 0)
				{
					int left = 0;
					int right = NextCount - 1;
					while (left <= right)
					{
						int mid = (left + right) >> 1;
						var val = Next[mid];
						int cmp = val.Key - ch;
						if (cmp < 0)
							left = mid + 1;
						else if (cmp > 0)
							right = mid - 1;
						else
							return val;
					}
				}
				return null;
			}
			set
			{
				int left = 0;
				int right = NextCount - 1;
				while (left <= right)
				{
					int mid = (left + right) >> 1;
					var val = Next[mid];
					int cmp = val.Key - ch;
					if (cmp < 0)
						left = mid + 1;
					else if (cmp > 0)
						right = mid - 1;
					else
					{
						Next[mid] = value;
						return;
					}
				}

				if (NextCount >= Next.Length)
					Array.Resize(ref Next, Math.Max(2, NextCount * 2));
				if (NextCount > left)
					Array.Copy(Next, left, Next, left + 1, NextCount - left);
				NextCount++;
				Next[left] = value;
				KeyMask |= 1 << ch;
			}
		}

		/// <summary>
		/// Return child nodes
		/// </summary>
		public IEnumerable<T> Children
		{
			get
			{
				for (int i = 0; i < NextCount; i++)
					yield return Next[i];
			}
		}

		/// <summary>
		/// Clones an node
		/// </summary>
		/// <returns></returns>
		public T Clone()
		{
			var node = (T)MemberwiseClone();
			node.Next = (T[])node.Next.Clone();
			return node;
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