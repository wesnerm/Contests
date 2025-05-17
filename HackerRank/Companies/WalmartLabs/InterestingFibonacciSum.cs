namespace HackerRank.WalmartLabs
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	class InterestingFibonacciSum
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			int q = int.Parse(Console.ReadLine());


			//for (int i=0; i<8; i++)
			//    Console.WriteLine(i + " -> " + Fib(i-1));


			for (int i = 0; i < q; i++)
			{
				Console.ReadLine();
				var array = Console.ReadLine().Split().Select(int.Parse).ToArray();
				Console.WriteLine(Fibo(array));
			}
		}

		public static int Fibo(int[] array)
		{
			Matrix m = new Matrix();
			int n = array.Length;

			var id = new Matrix(1, 0, 0, 1);
			var rightmat = new Matrix();
			for (int i = array.Length - 1; i >= 0; i--)
			{
				var mat = FibMatrix(array[i]);
				rightmat = mat * (id + rightmat);
				m += rightmat;
			}

			int f1 = 0;
			int f0 = 1;
			m.Apply(ref f1, ref f0);
			return f1;
		}

		public const int MOD = 1000 * 1000 * 1000 + 7;

		public static int Inverse(long n)
		{
			return ModPow(n, MOD - 2);
		}

		public static int Mult(long left, long right)
		{
			return (int)((left * right) % MOD);
		}

		public static int Add(int left, int right)
		{
			return ((left + right) % MOD);
		}

		public static int ModPow(long n, long p)
		{
			long b = n;
			long result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result = (result * b) % MOD;
				p >>= 1;
				b = (b * b) % MOD;
			}
			return (int)result;
		}

		public static Matrix FibMatrix(int n)
		{
			if (n < 0)
				return new Matrix();

			return new Matrix(1, 1, 1, 0).Pow(n);
		}

		public struct Matrix
		{
			public int e11;
			public int e12;
			public int e21;
			public int e22;

			public Matrix(int m11, int m12, int m21, int m22)
			{
				e11 = m11;
				e12 = m12;
				e21 = m21;
				e22 = m22;
			}

			public static Matrix operator *(Matrix m1, Matrix m2)
			{
				Matrix m = new Matrix();
				m.e11 = Add(Mult(m1.e11, m2.e11), Mult(m1.e12, m2.e21));
				m.e12 = Add(Mult(m1.e11, m2.e12), Mult(m1.e12, m2.e22));
				m.e21 = Add(Mult(m1.e21, m2.e11), Mult(m1.e22, m2.e21));
				m.e22 = Add(Mult(m1.e21, m2.e12), Mult(m1.e22, m2.e22));
				return m;
			}

			public static Matrix operator +(Matrix m1, Matrix m2)
			{
				Matrix m = new Matrix();
				m.e11 = Add(m1.e11, m2.e11);
				m.e12 = Add(m1.e12, m2.e12);
				m.e21 = Add(m1.e21, m2.e21);
				m.e22 = Add(m1.e22, m2.e22);
				return m;
			}

			public void Apply(ref int x, ref int y)
			{
				int x2 = e11 * x + e12 * y;
				int y2 = e21 * x + e22 * y;
				x = x2;
				y = y2;
			}

			public Matrix Pow(int p)
			{
				Matrix b = this;
				Matrix result = new Matrix(1, 0, 0, 1);
				while (p != 0)
				{
					if ((p & 1) != 0)
						result *= b;
					p >>= 1;
					b *= b;
				}
				return result;
			}

		}

	}
}