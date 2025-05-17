using System.Runtime.InteropServices;

namespace HackerRank.WeekOfCode28.SuffixRotation
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using static System.Math;

	public class Solution
	{
#if VERBOSE
	public static bool Verbose = true;
#else
	public static bool Verbose = false;
#endif	
		static Stopwatch watch;

		static void Main()
		{
			watch = new Stopwatch();
			watch.Start();

			int tests = int.Parse(Console.ReadLine());
			for (int t = 0; t < tests; t++)
			{
				var s = Console.ReadLine();

				Time("BruteForce", () =>
				{
					var brute = Solve2(s);
					return brute;
				}, true);

				Time("Partition", () =>
				{
					var result = Partition(s);
					return result;
				}, false);

			}
		}

		public static string Fix(string s)
		{
			var sb = new StringBuilder(s);
			RemoveDuplicates(sb);
			Normalize(sb);
			return sb.ToString();
		}
		public static void Generate(List<string> strings, StringBuilder builder,  char max, int depth)
		{
			if (depth == 0)
			{
				strings.Add(builder.ToString());
				return;
			}

			for (char alpha = 'a'; alpha <= max; alpha++)
			{
				builder[depth - 1] = alpha;
				Generate(strings, builder, max, depth-1);
			}
		}


		public static void Time2(string name, Func<int> func, bool test = false)
		{

		}

		public static void Time(string name, Func<int> func, bool test = false)
		{
			if (Verbose)
			{
				var time = watch.Elapsed;
				var result = func();
				time = watch.Elapsed - time;

				Console.WriteLine(name + ":");
				Console.WriteLine($"Result:  {result}");
				Console.WriteLine("Elapsed: " + time);
				Console.WriteLine();
			}
			else if (!test)
			{
				var result = func();
				Console.WriteLine($"{result}");
			}
		}

		public int Result;

		protected int n;
		public string mainString;
		public StringStats s;


		public static int Check(string s, int i)
		{
			return s[i] - s[(i + s.Length - 1) % s.Length];
			/*
			char ch = s[i];

			int len = s.Length;

			for (int j = 1; j < len; j++)
			{
				char chPrev = s[(i + len - j) % len];
				if (ch < chPrev) return -1;
				if (ch == chPrev + 1) return 1;
			}
			return 1;*/
		}

		public static int Partition(string s)
		{
			s = Fix(s);

			var isGreater = new bool[27];
			var isLesser = new bool[27];
			for (int i = 0; i < s.Length; i++)
			{
				char ch = s[i];
				if (!isGreater[ch - 'a'])
				{
					int cmp = Check(s, i);
					isGreater[ch - 'a'] |= cmp > 0;
					isLesser[ch - 'a'] |= cmp < 0;
				}
			}

			var list = new List<int> {0};
			for (int i=1; i<25;i++)
			{
				if (isLesser[i] && !isGreater[i])
					list.Add(i);
			}
			list.Add(26);

			var strings = new List<string>();
			for (var i = 0; i < list.Count-1; i++)
			{
				var lo = list[i];
				var hi = list[i + 1];
				var sb = new StringBuilder();

				foreach (var ch in s)
				{
					var c = ch - 'a';
					if (c < lo) continue;
					if (c >= hi)
					{
						if (sb.Length ==0 || sb[sb.Length-1] != 'z')
							sb.Append('z');
						continue;
					}
					sb.Append(ch);
				}

				if (i>0 && sb.Length > 0 && sb[0] == lo + 'a')
				{
					sb.Append(sb[0]);
					sb.Remove(0, 1);
				}

				strings.Add(sb.ToString());
			}

			var rotations = 0;
			foreach (var s2 in strings)
				rotations += Solve2(s2);

			return rotations;
		}

		public static int Solve2(string sParam)
		{
			var sb = new StringBuilder(sParam);
			RemoveDuplicates(sb);
			Normalize(sb);
			int rotations = Prune(sb);
			var s = sb.ToString();
			var b = new BruteForce(s);
			return b.Result + rotations;
		}


		public class StringStats
		{
			public string str;
			public int Length;
			public int[] s;
			public int[] freq;

			public int this[int index] => s[index] - 'a';

			public StringStats(string str)
			{
				this.str = str;
				Length = str.Length;
				s = new int[Length];
				for (int i = 0; i < Length; i++)
					s[i] = str[i] - 'a';

				freq = new int[27];

			}


			public override string ToString()
			{
				return str;
			}
		}

		public Solution(string str)
		{
			mainString = str;
			n = str.Length;
			s = new StringStats(str);
		}

		public static bool Between(int i, int j, int n)
		{
			return i <= j
				? n >= i && n <= j
				: n >= i || n <= j;
		}

		public static void RemoveDuplicates(StringBuilder sb)
		{
			char prev = '\0';
			int write = 0;

			for (int read = 0; read < sb.Length; read++)
			{
				var ch = sb[read];
				if (ch != prev) sb[write++] = ch;
				prev = ch;
			}

			sb.Length = write;
		}

		public static void Normalize(StringBuilder sb)
		{
			var freqs = new int[27];

			for (int read = 0; read < sb.Length; read++)
				freqs[sb[read] - 'a']++;

			int letter = 'a';
			for (int i = 0; i < 26; i++)
			{
				var f = freqs[i];
				freqs[i] = letter;
				if (f > 0) letter++;
			}

			for (int read = 0; read < sb.Length; read++)
			{
				var ch = sb[read];
				sb[read] = (char)freqs[ch - 'a'];
			}
		}


		public static int Prune(StringBuilder sb)
		{
			int rotations = 0;

			if (sb.Length >= 3)
			{
				var found = new bool[26];
				var prev = sb[0];
				int write = 1;
				for (int i = 1; i < sb.Length - 1; i++)
				{
					if (prev > sb[i] && sb[i] + 1 < sb[i + 1])
					{
						if (found[sb[i] - 'a'])
						{
							rotations++;
							continue;
						}
						found[sb[i] - 'a'] = true;
					}
					prev = sb[i];
					sb[write++] = sb[i];
				}
				sb[write++] = sb[sb.Length - 1];
				sb.Length = write;
			}

			RemoveDuplicates(sb);
			Normalize(sb);
			return rotations;
		}

		public class BruteForce : Solution
		{

			Dictionary<string, int> dict = new Dictionary<string, int>();
			public BruteForce(string str) : base(str)
			{
				Result = Solve(str);
			}

			public void Rotate(StringBuilder b, int i)
			{
				if (i == 0) return;
				for (int j = 0; j < i; j++)
					b.Append(b[j]);
				b.Remove(0, i);
			}

			public static string Rotate(string s, int pos)
			{
				var sb = new StringBuilder(s.Length);
				sb.Append(s, pos, s.Length - pos);
				sb.Append(s, 0, pos);
				return sb.ToString();
			}

			public int Solve(string str)
			{
				if (str.Length <= 2)
				{
					if (str.Length == 2) return str[0] <= str[1] ? 0 : 1;
					return 0;
				}

				int value;
				if (dict.TryGetValue(str, out value))
					return value;

				var sb = new StringBuilder(str);
				RemoveDuplicates(sb);
				Normalize(sb);
				int rotations = Prune(sb);

				char minCh = 'z';
				char prev = '\0';
				bool sorted = true;
				for (int i = 0; i < sb.Length; i++)
				{
					var ch = sb[i];
					if (minCh>ch) minCh = ch;
					if (ch < prev) sorted = false;
					prev = ch;
				}
				if (sorted) return rotations;

				// Start

				int f = 0;

				char chPrev = minCh;
				var chNext = 'z';
				str = sb.ToString();
				sb.Clear();
				for (var i = 0; i < str.Length; i++)
				{
					var ch2 = str[i];
					if (ch2 < minCh || ch2 == chPrev) continue;
					if (ch2 == minCh)
						f++;
					else if (chNext > ch2)
						chNext = ch2;
					sb.Append(ch2);
					chPrev = ch2;
				}

				if (sb.Length == 0) return rotations;

				var list = new List<int>(f);
				int write = 0;
				for (int i = 0; i < sb.Length; i++)
				{
					if (sb[i] == minCh)
					{
						if (list.Count == 0 || list[list.Count-1] != write)
							list.Add(write);
						continue;
					}
					sb[write++] = sb[i];
				}
				sb.Length = write;


				if (list.Count == 0) list.Add(0);
				else if (list[list.Count - 1] == sb.Length)
				{
					if (list[0] == 0)
						list.RemoveAt(list.Count - 1);
					else
						list[list.Count - 1] = 0;
					rotations += list.Count;
				}

				if (sb.Length == 0) return rotations;

				str = sb.ToString();
				int minCost = short.MaxValue;
				foreach (int t in list)
				{
					var rotated = Rotate(str, t);
					var cost = Solve(rotated);
					minCost = Min(minCost, cost);
				}

				minCost += rotations;
				dict[str] = minCost;
				return minCost;
			}
		}

	}

	public static class ArrayOperations
	{
		public static int IndexOf(this StringBuilder text, char findChar, int start = 0)
		{
			return text.IndexOf(findChar, start, text.Length - start);
		}

		public static int IndexOf(this StringBuilder text, char findChar, int start, int count)
		{
			while (count-- > 0)
			{
				if (text[start] == findChar)
					return start;
				start++;
			}
			return -1;
		}


	
		/// <summary>
		/// This function swaps d elements starting at index fi
		///  with d elements starting at index si
		/// </summary>
		/// <param name="arr">The arr.</param>
		/// <param name="fi">The fi.</param>
		/// <param name="si">The si.</param>
		/// <param name="size">The d.</param>
		public static void Swap(this int[] arr, int fi, int si, int size)
		{
			for (int i = 0; i < size; i++)
			{
				var temp = arr[fi + i];
				arr[fi + i] = arr[si + i];
				arr[si + i] = temp;
			}
		}

		public static int Gcd(int a, int b)
		{
			if (a > b) Swap(ref a, ref b);

			while (a != 0)
			{
				int tmp = b % a;
				b = a;
				a = tmp;
			}

			return b;
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}



		public static void Fill<T>(this List<T> list, int count)
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(default(T));
			}
		}


		public static void Fill<T>(this List<T> list, int count, T value)
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(value);
			}
		}

		public static void Fill<T>(this List<T> list, int count, Func<T> func)
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(func());
			}
		}


		public static List<T> Repeat<T>(int n, T value)
		{
			var list = new List<T>();
			Fill(list, n, value);
			return list;
		}


	}


}
