using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using static System.Array;
using static System.Math;
using static Library;
using static FastIO;

namespace HackerRank.WeekOfCode34.RecurrentOnATree
{

	class Solution
	{
		private int n;
		private List<int>[] graph;
		private int[] w;
		private TreeGraph tree;

		// http://fedelebron.com/fast-modular-fibonacci
		private const int PisanoPeriod = 2000000016;

		public void solve()
		{
			n = Ni();

			graph = new List<int>[n + 1];
			for (int i = 0; i < graph.Length; i++)
				graph[i] = new List<int>();

			for (int i = 1; i < n; i++)
			{
				int u = Ni();
				int v = Ni();
				graph[u].Add(v);
				graph[v].Add(u);
			}

			w = new int[n + 1];
			for (int i = 1; i <= n; i++)
				w[i] = Ni();

			tree = new TreeGraph(graph, 1);

			InitFib();
			long sum = 0;
			long[] f = new long[n + 1];
			long[] fm1 = new long[n + 1];

			//http://www.maths.surrey.ac.uk/hosted-sites/R.Knott/Fibonacci/fibmaths.html#section1
			// F(n)^2 + F(n+1)^2 = F( 2*n + 1 )
			// F(n+m) = F(m-1)*F(n) + F(m)*F(n+1)
			// gcd(F(m),F(n)) = F( gcd(m,n) )

			for (int i = tree.TreeSize - 1; i >= 0; i--)
			{
				int u = tree.Queue[i];

				var fu = Fibonacci(w[u]);
				var fum1 = Fibonacci(w[u] - 1);

				long s = 0;
				long sm1 = 0;

				long ss = 0;
				long ssm1 = 0;

				// T(n) = F(n+1)
				// T(n+m) = T(m-1)*F(n-1) + F(m)*F(n)

				var p = tree.Parents[u];
				foreach (var v in graph[u])
				{
					if (v == p) continue;
					var fv = f[v];
					var fvm1 = fm1[v];
					var fvm2 = fv - fvm1;
					if (fvm2 < 0) fvm2 += MOD;

					s += fv;
					sm1 += fvm1;

					ss += fvm1 * fvm1 % MOD + fv * fv % MOD;
					ssm1 += fvm2 * fvm1 % MOD + fvm1 * fv % MOD;
				}

				s = Fix(s);
				sm1 = Fix(sm1);

				// Add u's weight to each path
				long fadd = fum1 * sm1 % MOD + fu * s % MOD;
				long fum2 = (fu - fum1) % MOD;
				long faddm1 = fum2 * sm1 % MOD + fum1 * s % MOD;

				f[u] = (fu + fadd) % MOD;
				fm1[u] = (fum1 + faddm1) % MOD;

				// Combines two children
				long sm2 = s - sm1;
				long term = (sm1 * sm1 % MOD + s * s % MOD - ss) % MOD;
				long termm1 = (sm2 * sm1 % MOD + sm1 * s % MOD - ssm1) % MOD;
				long add = fum1 * termm1 % MOD + fu * term % MOD;

				sum += add + 2 * fadd;
			}

			for (int i = 1; i <= n; i++)
				sum += Fibonacci(w[i]);

			sum = Fix(sum);
			WriteLine(sum);
		}

		private static int[] fib = new int[0];

		public static void InitFib()
		{
			fib = new int[2500000];
			fib[0] = 0;
			fib[1] = 1;
			for (int i = 2; i < fib.Length; i++)
			{
				var r = fib[i - 1] + fib[i - 2];
				if (r >= MOD) r -= MOD;
				fib[i] = r;
			}
		}

		public static long Fix(long m)
		{
			var result = m % MOD;
			if (m < 0) result += MOD;
			return result;
		}

		public static long Fibonacci(long n)
		{
			long x;
			return Fibonacci(n + 1, out x);
		}

		public static long Fibonacci(long n, out long fnp1)
		{
			if (n + 1 < fib.Length)
			{
				fnp1 = fib[n + 1];
				return fib[n];
			}

			long b;
			var a = Fibonacci(n / 2, out b);
			var c = 2 * b - a;
			if (c < 0) c += MOD;
			c = a * c % MOD;
			var d = (a * a + b * b) % MOD;
			if ((n & 1) == 0)
			{
				fnp1 = d;
				return c;
			}
			fnp1 = c + d;
			return d;
		}

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
				_inverse[n] = (int) (result + 1);
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

/*	public static long Fix(long n)
	{
		return ((n % MOD) + MOD) % MOD;
	}*/

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

		public TreeGraph(IList<int>[] g, int root = -1, int avoid = -1)
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

			for (int i = 0; i < n; i++)
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

		#endregion


	}


}