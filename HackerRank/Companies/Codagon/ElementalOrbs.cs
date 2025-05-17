namespace HackerRank.Codagon.ElementalOrbs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using static System.Math;
    using static System.Console;

    public class Solution
    {
        static void Main(String[] args)
        {
            int s = Convert.ToInt32(Console.ReadLine());
            for (int a0 = 0; a0 < s; a0++)
            {
                string[] tokens_n = Console.ReadLine().Split(' ');
                int n = Convert.ToInt32(tokens_n[0]);
                e = Convert.ToInt32(tokens_n[1]);
                string[] b_temp = Console.ReadLine().Split(' ');
                b = Array.ConvertAll(b_temp, Int32.Parse);
                Array.Sort(b);
                maxb = Min(b[b.Length - 1], n);
                table = new int[n + 1, maxb + 1];
                var answer = Orb(n);
                Console.WriteLine(answer);
            }
        }

        private static int[,] table;
        static int e;
        static int[] b;
        static int maxb;

        static long Orb(int n)
        {
            return Dfs(n, -1);
        }

        static long DacTry(int n, int elem)
        {
            if (n == 0) return 1;

            var bb = Math.Min(b[elem], n);
            int mid = n / 2;
            var n2 = Math.Max(mid, n - mid - 1);
            var start1 = Math.Max(0, mid-(bb-1));

            var work = new int[n2+1];
            for (int i = 0; i < n2; i++)
            {
                work[i] = (int)Dfs(i, elem);
            }

            var worksum = new int[n2 + 1];
            int sum = 0;
            for (int i = 0; i < n2; i++)
            {
                sum = (sum + work[i] % MOD);
                worksum[i] = sum;

            }

            long count = 0;
            for (int i = Math.Max(0, mid-bb-1) ; i <= mid; i++)
            {
                int end = Math.Min(start1 + bb, n);
                int subtract = n - end;
                long w = i>0 ? work[i-1] : 1;
                long w2 = worksum[n-mid-1];
                if (subtract > 0)
                    subtract = MOD - worksum[subtract];
                w2 += subtract;
                long tot = w * w2 % MOD;
                count += tot;
            }

            return count;
        }

        private const int MOD = 1000 * 1000 + 7;

        static long Dfs(int n, int skip)
        {
            if (n == 0) return 1;

            var bb = -1;
            if (skip >= 0)
            {
                bb = Math.Min(n, b[skip]);
                if (table[n, bb] != 0)
                    return table[n, bb] - 1;
            }


            long count = 0;
            for (int i = 0; i < e; i++)
            {
                if (i == skip) continue;
                for (int j = Math.Min(b[i], n); j > 0; j--)
                    count += Dfs(n - j, i);
            }

            count %= MOD;
            if (bb >= 0)
                table[n, bb] = (int)(count + 1);
            return count;
        }

    }

}
