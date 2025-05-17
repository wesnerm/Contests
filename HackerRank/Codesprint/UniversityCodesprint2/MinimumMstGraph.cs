namespace HackerRank.UniversityCodesprint2
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using static System.Math;
	using static System.Console;

	public class MinimumMstGraph
	{
		#region Variables


		#endregion

		public static void Main()
		{
			int q = int.Parse(ReadLine());
			var sol = new MinimumMstGraph();
			for (int i = 0; i < q; i++)
			{
				sol.Run();
			}
		}

		void Run()
		{
			var input = Console.ReadLine().Split();
			long n = long.Parse(input[0]);
			long m = long.Parse(input[1]);
			long s = long.Parse(input[2]);

			var ans = Solve(n, m, s);
			Console.WriteLine(ans);
		}

		long Solve(long n, long m, long s)
		{
			if (m == n - 1) return s;

			long min;
			if (TryClique(n, m, s, out min))
				return min;

			long newMin = TryAveraged(n, m, s);
			long result = Min(min, newMin);
			return result;
		}


		long TryAveraged(long n, long m, long s)
		{
			// All have approximately equal weights

			long mstEdges = n - 1;
			long extraEdges = m - mstEdges;

			long weightDiv = s / mstEdges;
			long weightMod = s % mstEdges;


			// Attempt0 - No excess weight

			if (weightMod == 0)
			{
				long min0 = s + extraEdges * weightDiv;
				return min0;
			}

			// Attempt1 - Evenly distribute excess among all
			long loEdges = (mstEdges - weightMod);
			long loNodes = loEdges + 1;
			long loCompleteEdges = loNodes * (loNodes - 1) / 2;
			long extraLoCompleteEdges = loCompleteEdges - loEdges;

			long min1, s1 = s;
			var extraEdges1 = extraEdges - extraLoCompleteEdges;
			if (extraEdges1 <= 0)
			{
				s1 = s;
				min1 = s1 + extraEdges * weightDiv;
			}
			else
			{
				s1 = s + extraLoCompleteEdges * weightDiv;
				min1 = s1 + extraEdges1 * (weightDiv + 1);
			}

			// Attempt2 - Place excess on lone node
			long cliqueNodes = n - 1;
			long cliqueEdges = cliqueNodes - 1;
			long completeCliqueEdges = cliqueNodes * (cliqueNodes - 1) / 2;
			long extraCliqueEdges = completeCliqueEdges - cliqueEdges;

			long min2, s2;
			var extraEdges2 = extraEdges - extraCliqueEdges;
			if (extraEdges2 <= 0)
			{
				s2 = s;
				min2 = s2 + extraEdges * weightDiv;
			}
			else
			{
				s2 = s + extraCliqueEdges * weightDiv;
				min2 = s2 + extraEdges2 * (weightDiv + weightMod);
			}

			long newMin = Min(min1, min2);
			return newMin;
		}


		// All but one vertices form a clique with edges set to 1
		// All but one edges have has 1

		bool TryClique(long n, long m, long s, out long min)
		{
			if (m == n - 1)
			{
				min = s;
				return true;
			}

			long mstEdges = n - 1;
			long extraEdges = m - mstEdges;

			long cliqueNodes = n - 1;
			long cliqueEdges = cliqueNodes - 1;

			long completeCliqueEdges = cliqueNodes * (cliqueNodes - 1) / 2;
			long extraCliqueEdges = completeCliqueEdges - cliqueEdges;

			if (extraEdges <= extraCliqueEdges)
			{
				min = s + extraEdges;
				return true;
			}

			// Extra Edges

			var loneWeight = s - cliqueEdges;
			long s2 = s + extraCliqueEdges;
			long extraEdges2 = extraEdges - extraCliqueEdges;

			min = s2 + extraEdges2 * loneWeight;
			return false;
		}


		public static long ExponentialSearch(long start)
		{
			long left = start;
			long right = start;
			while (!CheckLength(right))
			{
				left = right;
				right *= 2;
			}

			while (left <= right)
			{
				long mid = left + (right - left) / 2;
				if (!CheckLength(mid))
					left = mid + 1;
				else
					right = mid - 1;
			}
			return left;
		}

		public static bool CheckLength(long start)
		{
			return false;
		}


#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
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
		static System.Threading.Timer _timer;
		static Func<bool> _timerAction;

		public static void LaunchTimer(Func<bool> action, long ms = 2800)
		{
			_timerAction = action;
			_timer = new System.Threading.Timer(
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
			Console.Write(name + ": ");
			_stopWatch.Restart();
			action();
			_stopWatch.Stop();
			Console.WriteLine($"Elapsed Time: {_stopWatch.Elapsed}\n");
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