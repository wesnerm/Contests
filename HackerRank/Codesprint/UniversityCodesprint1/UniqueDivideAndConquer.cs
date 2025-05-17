namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;
	using bigint = System.Int64;

	class UniqueDivideAndConquer
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */
			new UniqueDivideAndConquer().Run();

		}

		bigint mod;
		int n;
		bigint[] fact;
		bigint[][] buffer;
		bigint[] udc;

		void Run()
		{
			var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
			n = a[0];
			mod = a[1];
			buffer = new bigint[n + 1][];
			udc = new bigint[n + 1];

			for (int i = 0; i <= n; i++)
				buffer[i] = new bigint[i + 1];

			fact = new bigint[n + 1];
			fact[0] = 1;
			for (int i = 1; i <= n; i++)
				fact[i] = (i * fact[i - 1]) % mod;

			Console.WriteLine(Udc(n));
		}

		bigint Udc(int n)
		{
			if (n < 1) return 0;
			if (n < udc.Length && udc[n] != 0) return udc[n] - 1;

			//cases
			// c=2: n != even
			// c>2, n even: n/2 not good, n/2-1 fined because increasing it causes others to connect; if one component has n/2, we reduce it to n/2-1 or snaller and the rest will have n/2 
			// c>2, n odd: n/2 ok, n/2-1 ok 

			int mid = n / 2;
			var result = n * Dfs(n - 1, mid + n % 2 - 1) % mod;
			if (n < udc.Length) udc[n] = result + 1;

			//Console.WriteLine($"Udc({n}) = {result}");
			return result;
		}

		bigint Dfs(int n, int max)
		{
			if (max > n) max = n;
			if (max <= 1) return max == 1 || n == 0 ? 1 : 0;
			if (buffer[n][max] != 0) return buffer[n][max] - 1;

			//Console.WriteLine($"Entered Dfs({n}, {max})");
			bigint count = Dfs(n, max - 1) % mod;
			bigint u = Udc(max);
			if (u != 0)
			{
				bigint factor = 1;
				int q = n / max;
				int nn = n;
				//Console.WriteLine($"{i}) u={u} q={n}");
				for (int j = 1; j <= q; j++)
				{
					factor = Mult(factor, u);
					factor = Div(factor, j);
					bigint comb = Mult(max, Comb(nn, max));
					factor = Mult(factor, comb);
					nn -= max;
					bigint dfs = Dfs(nn, max - 1);
					count = (count + Mult(factor, dfs)) % mod;
					//Console.WriteLine($" {i}.{j}> factor={factor} comb={comb} dfs={dfs} tmp={tmp} addend={tmp}");
				}
			}

			buffer[n][max] = count + 1;
			//Console.WriteLine($"Exited Dfs({n}, {max}) = {count}");
			return count;
		}

		/*
			Test cases
			1 1
			2 0
			3-6 same
			7 6937
			8 47888
			9 662265
		*/

		/*public long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);

			if (_comb == null)
				_comb = new List<long[]>(Math.Max(n+1, 100));

			while (n >= _comb.Count)
				_comb.Add(new long[n/2+1]);

			var result = _comb[n][k];
			if (result != 0)
				return result - 1;
			result = Div(Mult(n,Comb(n - 1, k - 1)),k);
			_comb[n][k] = result + 1;
			return result;
		} */

		public bigint CombQuick(int n, int k)
		{
			bigint result = 1;
			for (int i = 1; i <= k; i++)
				result = Div(Mult(result, n - i + 1), i);
			return result;
		}

		static List<long> _fact = new List<long> { 1, 1 };
		static List<long> _ifact = new List<long> { 1, 1 };

		public bigint Fact(int n)
		{
			for (int i = _fact.Count; i <= n; i++)
				_fact.Add(Mult(_fact[i - 1], i));
			return _fact[n];
		}

		public bigint InverseFact(int n)
		{
			for (int i = _ifact.Count; i <= n; i++)
				_ifact.Add(Div(_ifact[i - 1], i));
			return _ifact[n];
		}

		public long Comb(int n, int k)
		{
			if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
			if (k + k > n) return Comb(n, n - k);
			return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
		}

		static bigint[] _inverse = new bigint[3000];
		public bigint Inverse(bigint n)
		{
			bigint result;
			if (n < _inverse.Length && (result = _inverse[n]) != 0)
				return result - 1;

			result = ModPow(n, mod - 2);
			if (n < _inverse.Length)
				_inverse[n] = result + 1;
			return result;

		}

		public bigint Mult(bigint left, bigint right)
		{
			return ((left * right) % mod);
		}

		public bigint Div(bigint left, bigint divisor)
		{
			if (left % divisor == 0)
				return left / divisor;

			return Mult(left, Inverse(divisor));
		}

		public bigint ModPow(bigint n, bigint p)
		{
			var b = n;
			bigint result = 1;
			while (p != 0)
			{
				if ((p & 1) != 0)
					result = (result * b) % mod;
				p >>= 1;
				b = (b * b) % mod;
			}
			return result;
		}

	}
}
