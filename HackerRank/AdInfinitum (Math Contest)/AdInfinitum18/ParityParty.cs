namespace HackerRank.AdInfinitum18.ParityParty
{
	// https://www.hackerrank.com/contests/infinitum18/challenges/parity-party

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static Library;
	using static FastIO;
	using static System.Math;
	using static System.Array;

	public class Solution
	{
		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			InitOutput(output);
			nOrig = Ni();
			a = Ni();
			b = Ni();
			c = Ni();

			long result = solve();
			WriteLine(result);
			Flush();
		}

		int nOrig, a, b, c;

		long[,] oddParts = new long[2000, 2000];
		public long NumberOfOddParts(int n, int k)
		{
			if (n == 0) return k == 0 ? 1 : 0;
			if (k <= 1)
				return k == 1 && (n & 1) == 1 ? 1 : 0;

			long result = oddParts[n, k];
			if (result != 0) return result - 1;

			for (int i = 1; i <= n; i += 2)
				result += Comb(n, i) * NumberOfOddParts(n - i, k - 1) % MOD;

			result %= MOD;
			oddParts[n, k] = (int)result + 1;
			return result;
		}


		int[,] evenParts = new int[2000, 2000];
		public long NumberOfEvenParts(int n, int k)
		{
			if (n == 0) return 1;
			if (k <= 1)
				return k == 1 && (n & 1) == 0 ? 1 : 0;

			long result = evenParts[n, k];
			if (result != 0) return result - 1;

			for (int i = 0; i <= n; i += 2)
				result += Comb(n, i) * NumberOfEvenParts(n - i, k - 1) % MOD;

			result %= MOD;
			evenParts[n, k] = (int)result + 1;
			return result;
		}

		public long solve()
		{
			long result = 0;
			int limit = a == 0 ? 0 : nOrig;
			for (int ia = a; ia <= limit; ia += 2)
			{
				long composa = NumberOfOddParts(ia, a);
				//long composa = CompositionOfOddParts(ia, a);
				if (composa == 0) continue;

				int n = nOrig - ia;
				long facta = Comb(nOrig, ia);
				long comba = facta * composa % MOD;

				long combb = 0;
				int blimit = b > 0 ? n : 0;
				for (int ib = c > 0 ? 0 : n; ib <= blimit; ib += 2)
				{
					long composb = NumberOfEvenParts(ib, b);
					// long composb = CompositionIntoEvenParts(ib);
					if (composb == 0)
						continue;

					int ic = n - ib;
					long factb = Comb(n, ib);
					long composc = ModPow(c, ic);
					long combbTmp = factb * composb % MOD;

					long addend = combbTmp * composc % MOD;
					combb += addend;
				}
				combb %= MOD;

				var comb = comba * combb % MOD;
				result += comb;
			}
			return result % MOD;
		}


		static long CompositionOfOddParts(long n)
		{
			Debug.Assert(Fib(0) == 0);
			Debug.Assert(Fib(1) == 1);
			Debug.Assert(Fib(2) == 1);
			Debug.Assert(Fib(3) == 2);
			return Fib((int)n);
		}

		static long CompositionOfOddParts(int n, int k)
		{
			// n and k need to be same parity
			if (((n ^ k) & 1) != 0) return 0;
			return Comb((n - k) / 2 + k - 1, k - 1);
		}

		static long CompositionIntoEvenParts(int n)
		{
			if ((n & 1) == 1) return 0;
			if (n == 0) return 1;
			return ModPow(2, n / 2 - 1);
		}

		static long WeakComposition(int n, int k)
		{
			return Comb(n + k - 1, k - 1);
		}

		static long Compositions(int n)
		{
			return ModPow(2, n - 1);
		}

		static long Compositions(int n, int k)
		{
			if (n < 1 || k < 1) return 0;
			return Comb(n - 1, k - 1);
		}


		static double Stirling(int n, int k)
		{
			if (k == 0) return n == 0 ? 1 : 0;
			if (k < 0) return 0;

			double sum = 0;
			for (int j = 0; j <= k; ++j)
			{
				var a = (k - j) % 2 == 1 ? -1 : 1; ;
				sum += a * Comb(k, j) * Pow(j, n);
			}
			return sum / Fact(k);
		}


		public static long Fib(int n)
		{
			int f1 = 0;
			int f0 = 1;
			var m = FibMatrix(n);
			m.Apply(ref f1, ref f0);
			return f1;
		}

		public static Matrix FibMatrix(int n)
		{
			return n < 0 ? new Matrix() : new Matrix(1, 1, 1, 0).Pow(n);
		}

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
			Matrix m = new Matrix
			{
				e11 = (int)Add(Mult(m1.e11, m2.e11), Mult(m1.e12, m2.e21)),
				e12 = (int)Add(Mult(m1.e11, m2.e12), Mult(m1.e12, m2.e22)),
				e21 = (int)Add(Mult(m1.e21, m2.e11), Mult(m1.e22, m2.e21)),
				e22 = (int)Add(Mult(m1.e21, m2.e12), Mult(m1.e22, m2.e22))
			};
			return m;
		}

		public static Matrix operator +(Matrix m1, Matrix m2)
		{
			Matrix m = new Matrix
			{
				e11 = (int)Add(m1.e11, m2.e11),
				e12 = (int)Add(m1.e12, m2.e12),
				e21 = (int)Add(m1.e21, m2.e21),
				e22 = (int)Add(m1.e22, m2.e22)
			};
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

	public class Ntt
	{
		const int Root = 3;
		const int Root1 = 2446678;
		const int RootPw = (1 << 20);
		const int Mod = 7340033;

		public void Fft(long[] a, bool invert)
		{
			int n = a.Length;
			for (int i = 1, j = 0; i < n; ++i)
			{
				int bit = n >> 1;
				for (; j >= bit; bit >>= 1)
					j -= bit;
				j += bit;
				if (i < j)
				{
					var tmp = a[i];
					a[i] = a[j];
					a[j] = tmp;
				}
			}

			for (int len = 2; len <= n; len <<= 1)
			{
				long wlen = invert ? Root1 : Root;
				for (int i = len; i < RootPw; i <<= 1)
					wlen = (wlen * wlen) % Mod;
				for (int i = 0; i < n; i += len)
				{
					long w = 1;
					for (int j = 0; j < len / 2; ++j)
					{
						long u = a[i + j];
						long v = (a[i + j + len / 2] * w) % Mod;
						a[i + j] = u + v < Mod ? u + v : u + v - Mod;
						a[i + j + len / 2] = u - v >= 0 ? u - v : u - v + Mod;
						w = (w * wlen) % Mod;
					}
				}
			}
			if (invert)
			{
				long nrev = ModPow(n, Mod - 2);
				for (int i = 0; i < n; ++i)
					a[i] = (a[i] * nrev) % Mod;
			}
		}

		void Multiply(IList<long> a, IList<long> b, out long[] c)
		{
			int sz = 2 * a.Count;
			var ta = new long[sz];
			var tb = new long[sz];
			a.CopyTo(ta, 0);
			b.CopyTo(tb, 0);
			Fft(ta, false);
			Fft(tb, false);
			for (int i = 0; i < sz; ++i)
				ta[i] = (1L * ta[i] * tb[i]) % Mod;

			Fft(ta, true);
			c = ta;
		}
	}

	public class NTT2
	{
		const int Mod = 7340033;
		const int Root = 3;

		void Ntt(int[] a, int n, bool rev = false)
		{
			int h = 0;
			for (int i = 0; 1 << i < n; i++)
				h++;

			for (int i = 0; i < n; i++)
			{
				int j = 0;
				for (int k = 0; k < h; k++)
					j |= (i >> k & 1) << (h - 1 - k);

				if (i < j)
				{
					var tmp = a[i];
					a[i] = a[j];
					a[j] = tmp;
				}
			}
			for (int i = 1; i < n; i *= 2)
			{
				int w = (int)ModPow(Root, (Mod - 1) / (i * 2));
				if (rev)
					w = (int)Inverse(w);
				for (int j = 0; j < n; j += i * 2)
				{
					int wn = 1;
					for (int k = 0; k < i; k++)
					{
						int s = a[j + k + 0];
						int t = (int)Mult(a[j + k + i], wn);
						a[j + k + 0] = (int)Add(s, t);
						a[j + k + i] = (int)Add(s, Mod - t);
						wn = (int)Mult(wn, w);
					}
				}
			}
			if (rev)
			{
				int v = (int)Inverse(n);
				for (int i = 0; i < n; i++)
					a[i] = (int)Mult(a[i], v);
			}
		}
	}

	public static class Library
	{
		#region Mod Math
		public const int MOD = 7340033;

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

		public static long Add(long x, long y)
		{
			return (x += y) >= MOD ? x - MOD : x;
		}

		public static long Subtract(long left, long right)
		{
			long result = left - right;
			if (result < 0) result += MOD;
			return result;
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

		/*
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
		}*/

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

		public static long Fact(int n, int m)
		{
			var fact = Fact(n);
			if (m < n)
				fact = Mult(fact, InverseFact(n - m));
			return fact;
		}


		public static long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
		}

		#endregion

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