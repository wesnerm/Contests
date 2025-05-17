//https://www.hackerrank.com/contests/infinitum18/challenges/count-solutions
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static FastIO;
using static System.Math;
using static System.Array;

public class Solution
{
	public void solve(Stream input, Stream output)
	{
		InitInput(input);
		InitOutput(output);
		solveQ();
		Flush();
	}

	public void solve(int a, int b, int c, int d)
	{
		int count = 0;
		int x = 0;
		long xxma = 0;
		int sign = 1;

		Action<int> func = y =>
		{
			if (y > d) return;
			if (xxma == y * 1L * (b - y))
				count++;
		};

		for (x = 1; x <= c; x++)
		{
			xxma = x * 1L * (x - a);
			sign = xxma < 0 ? -1 : 1;

			if (xxma != 0)
			{
				EnumerateFactors(factors, x, (x-a)*sign, d, func);
			}
			else
			{
				if (b >= 1 && b <= d)
					count++;
			}
		}

		WriteLine(count);
	}

	int[] factors = PrimeFactorsUpTo(100000);

	public void solveQ()
	{
		int q = Ni();
		while (q-- > 0)
		{
			int a = Ni();
			int b = Ni();
			int c = Ni();
			int d = Ni();
			solve(a,b,c,d);
		}
	}

	public static int EnumerateFactors(int[] factors, int n1, int n2, int max, Action<int> action = null, int f = 1)
	{
		if (f > max)
			return 0;

		if (n1 == 1 && n2 == 1)
		{
			action?.Invoke(f);
			return 1;
		}

		int p1 = factors[n1];
		int p2 = factors[n2];
		int p = n1==1 ? p2 : n2 ==1 ? p1 : Min(p1, p2);

		int c = 0;
		int next1 = n1;
		int next2 = n2;

		while (next1 > 1 && factors[next1] == p)
		{
			c++;
			next1 /= p;
		}

		while (next2 > 1 && factors[next2] == p)
		{
			c++;
			next2 /= p;
		}

		int result = EnumerateFactors(factors, next1, next2, max, action, f);
		while (c-- > 0)
		{
			f *= p;
			result += EnumerateFactors(factors, next1, next2, max, action, f);
		}

		return result;
	}

	public static int[] PrimeFactorsUpTo(int n)
	{
		var factors = new int[n + 1];

		for (int i = 2; i <= n; i += 2)
			factors[i] = 2;

		var sqrt = (int)Math.Sqrt(n);
		for (int i = 3; i <= sqrt; i += 2)
		{
			if (factors[i] != 0) continue;
			for (int j = i * i; j <= n; j += i + i)
			{
				if (factors[j] == 0)
					factors[j] = i;
			}
		}

		for (int i = 3; i <= n; i += 2)
		{
			if (factors[i] == 0)
				factors[i] = i;
		}

		return factors;
	}


}

public static class Parameters
{
#if DEBUG
	public const bool Verbose = true;
#else
	public const bool Verbose = false;
#endif
}

class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}
public class Program {
    public static void Main(string[] args)
    {
        Solution solution = new Solution();
        solution.solve(Console.OpenStandardInput(), Console.OpenStandardOutput());

#if DEBUG
		Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
	}
}

