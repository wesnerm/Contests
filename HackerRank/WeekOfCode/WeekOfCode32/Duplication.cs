namespace HackerRank.WeekOfCode32.Duplication
{
	// https://www.hackerrank.com/contests/w32/challenges/duplication


	using System;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;

	class Solution
	{


		int duplication(int x)
		{
			if (data == "")
			{
				data = "0";
				while (data.Length < 2000)
					data += data.Replace('0', '2').Replace('1', '0').Replace('2', '1');
			}

			return data[x] - '0';
		}

		int duplication2(int x)
		{
			return x == 0 ? 0 : 1 - duplication2(x - HiBit(x));
		}

		int HiBit(int x)
		{
			while ((x & x - 1) != 0)
				x &= x - 1;
			return x;
		}

		string data = "";

		public void solve(TextReader input, TextWriter output)
		{


			int q = Convert.ToInt32(input.ReadLine());
			for (int a0 = 0; a0 < q; a0++)
			{
				int x = Convert.ToInt32(input.ReadLine());
				string result = duplication(x).ToString();
				output.WriteLine(result);
			}
		}
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
			using (System.IO.TextReader input =
					CaideConstants.InputFile == null ? System.Console.In :
							new System.IO.StreamReader(CaideConstants.InputFile))
			using (System.IO.TextWriter output =
					CaideConstants.OutputFile == null ? System.Console.Out :
							new System.IO.StreamWriter(CaideConstants.OutputFile))

				solution.solve(input, output);
		}
	}


}