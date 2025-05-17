
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace HackerRank.WeekOfCode29.BigSorting
{
	using static Console;

	public class Solution
	{
		#region Variables


		#endregion


		static void Main()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var random = new Random();
			using (var writer = File.CreateText(@"d:\test\hr\bs.txt"))
			{
				int sizes = 5;
				int n = 1000000 / sizes;
				var s = new String('1', sizes);
				writer.WriteLine(n);
				for (int i=0; i<n; i++)
					writer.WriteLine(s /*random.Next(10000000, 99999999)*/);

			}

			SetOut(new StringWriter());
			//CodeBigInteger();

			SetIn(File.OpenText(@"d:\test\hr\bs.txt"));
			stopwatch.Restart();
			SetOut(new StringWriter());
			CodeRadix();
			var elapsedRadix = stopwatch.Elapsed;

			SetIn(File.OpenText(@"d:\test\hr\bs.txt"));
			stopwatch.Restart();
			SetOut(new StringWriter());
			CodeSort();
			var elapsedSort = stopwatch.Elapsed;



		}


		static void CodeBigInteger()
		{
			int n = Convert.ToInt32(ReadLine());
			var unsorted = new BigInteger[n];
			for (int unsorted_i = 0; unsorted_i < n; unsorted_i++)
			{
				unsorted[unsorted_i] = BigInteger.Parse(ReadLine());
			}

			Array.Sort(unsorted);
			foreach (var v in unsorted)
				WriteLine(v);
		}


		static void CodeRadix()
		{
			int n = int.Parse(ReadLine());
			var array = new string[n];

			for (int i = 0; i < n; i++)
			{
				array[i] = ReadLine();
			}

			buffer = new string[n];
			Array.Sort(array, (a, b) => a.Length.CompareTo(b.Length));
			//Console.WriteLine(string.Join(",", array));

			for (int i = 0; i < array.Length;)
			{
				int j = i;
				while (j + 1 < array.Length && array[j + 1].Length == array[j].Length)
					j++;
				RadixSort(array, i, j, 0);
				i = j + 1;
			}

			foreach (var v in array)
				WriteLine(v);
		}

		static void CodeSort()
		{
			int n = Convert.ToInt32(ReadLine());
			var unsorted = new string[n];
			for (int unsorted_i = 0; unsorted_i < n; unsorted_i++)
			{
				unsorted[unsorted_i] = ReadLine();
			}

			Array.Sort(unsorted, (a, b) =>
			{
				int cmp = a.Length.CompareTo(b.Length);
				if (cmp == 0)
					cmp = a.CompareTo(b);
				return cmp;
			});
			foreach (var v in unsorted)
				WriteLine(v);
		}


		static string[] buffer;


		static unsafe void RadixSort(string[] array, int left, int right, int index)
		{
			if (left >= right || index >= array[left].Length)
				return;

			var offsets = stackalloc int[10];
			var pos = stackalloc int[10];

			while (left < right && index < array[left].Length)
			{
				for (int i = 0; i < 10; i++)
					offsets[i] = 0;

				for (int i = left; i <= right; i++)
					offsets[array[i][index] - '0']++;

				int sum = left;
				for (int i = 0; i < 10; i++)
				{
					int tmp = offsets[i];
					offsets[i] = sum;
					pos[i] = sum;
					sum += tmp;
				}

				for (int i = left; i <= right; i++)
				{
					int radix = array[i][index] - '0';
					buffer[pos[radix]++] = array[i];
				}

				for (int i = left; i <= right; i++)
					array[i] = buffer[i];

				int maxrange = 0;
				int maxradix = 0;

				for (int i = 0; i < 10; i++)
				{
					int range = pos[i] - offsets[i];
					if (range >= maxrange)
					{
						maxrange = range;
						maxradix = i;
					}
				}

				index++;
				for (int i = 0; i < 10; i++)
				{
					if (i != maxradix)
						RadixSort(array, offsets[i], pos[i] - 1, index);
				}

				// This reduces recursion since index can go up to 10^6
				left = offsets[maxradix];
				right = pos[maxradix] - 1;
			}
		}



	}


	public static class HackerRankUtils
	{
		#region Mod Math
		public const int MOD = 1000 * 1000 * 1000 + 7;

		static int[] _inverse;
		public static long Inverse(long n)
		{
			long result;

			if (_inverse == null)
				_inverse = new int[3000];

			if (n < _inverse.Length && (result = _inverse[n]) != 0)
				return result - 1;

			result = ModPow(n, MOD - 2);
			if (n < _inverse.Length)
				_inverse[n] = (int)(result + 1);
			return result;
		}

		public static long Mult(long left, long right)
		{
			return (left * right) % MOD;
		}

		public static long Div(long left, long divisor)
		{
			return left % divisor == 0
				? left / divisor
				: Mult(left, Inverse(divisor));
		}

		public static long Subtract(long left, long right)
		{
			return (left + (MOD - right)) % MOD;
		}

		public static long Fix(long n)
		{
			return ((n % MOD) + MOD) % MOD;
		}


		public static long ModPow(long n, long p, long mod = MOD)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result = (result * b) % mod;
				p >>= 1;
				b = (b * b) % mod;
			}
			return result;
		}

		public static long Pow(long n, long p)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result *= b;
				p >>= 1;
				b *= b;
			}
			return result;
		}

		#endregion

		#region Combinatorics
		static List<long> _fact;
		static List<long> _ifact;

		public static long Fact(int n)
		{
			if (_fact == null) _fact = new List<long>(100) { 1 };
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public static long InverseFact(int n)
		{
			if (_ifact == null) _ifact = new List<long>(100) { 1 };
			for (int i = _ifact.Count; i <= n; i++)
				_ifact.Add(Div(_ifact[i - 1], i));
			return _ifact[n];
		}

		public static long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
		}

		#endregion

		#region Input
		public static string[] ReadArray()
		{
			int n = int.Parse(ReadLine());
			string[] array = new string[n];
			for (int i = 0; i < n; i++)
				array[i] = ReadLine();
			return array;
		}

		public static int[] ReadInts()
		{
			return ReadLine().Split().Select(int.Parse).ToArray();
		}
		#endregion

		#region Arrays
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

		public static void For(int n, Action<int> action)
		{
			for (int i = 0; i < n; i++) action(i);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		public static void MemSet<T>(IList<T> list, T value)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
				list[i] = value;
		}

		public static void MemSet<T>(T[,] list, T value)
		{
			int count = list.GetLength(0);
			int count2 = list.GetLength(1);
			for (int i = 0; i < count; i++)
				for (int j = 0; j < count2; j++)
					list[i, j] = value;
		}

		public static void MemSet<T>(IEnumerable<IList<T>> list, T value)
		{
			foreach (var sublist in list)
				MemSet(sublist, value);
		}

		public static void Iota(IList<int> list, int seed)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
				list[i] = seed++;
		}
		#endregion

		#region Comparer

		public class Comparer<T> : IComparer<T>
		{
			readonly Comparison<T> _comparison;

			public Comparer(Comparison<T> comparison)
			{
				_comparison = comparison;
			}

			public int Compare(T a, T b)
			{
				return _comparison(a, b);
			}
		}

		#endregion

		#region Reporting Answer

		static volatile bool _reported;
		static Timer _timer;
		static Func<bool> _timerAction;

		public static void LaunchTimer(Func<bool> action, long ms = 2800)
		{
			_timerAction = action;
			_timer = new Timer(
				delegate
				{
					Report();
#if !DEBUG
				if (_reported)
					Environment.Exit(0);
#endif
			}, null, ms, 0);
		}

		public static void Report()
		{
			if (_reported) return;
			_reported = true;
			_reported = _timerAction();
		}

#if DEBUG
		static Stopwatch _stopWatch = new Stopwatch();
#endif

		public static void Run(string name, Action action)
		{
#if DEBUG
			Write(name + ": ");
			_stopWatch.Restart();
			action();
			_stopWatch.Stop();
			WriteLine($"Elapsed Time: {_stopWatch.Elapsed}\n");
#else
		action();
#endif
		}

		[Conditional("DEBUG")]
		public static void Run2(string name, Action action)
		{
			// Ignore
		}

		#endregion
	}

}