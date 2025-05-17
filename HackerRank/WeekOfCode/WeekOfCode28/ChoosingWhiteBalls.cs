namespace HackerRank.WeekOfCode28.ChoosingWhiteBalls
{
    using System;
    using System.Collections.Generic;

    class Solution
    {
        static void Driver()
        {
            var time = DateTime.Now;
            var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
            int n = arr[0];
            int k = arr[1];
            var s = Console.ReadLine();
            int balls = 0;
            foreach (var c in s)
                balls = balls * 2 + (c == 'W' ? 1 : 0);

            nMinimum = n - k;
            dp = new Dictionary<int, double>(1000000);
            var answer = Solve(balls, n);
            Console.WriteLine(answer);
#if VERBOSE
            Console.WriteLine(DateTime.Now - time);
#endif
        }

        private static int nMinimum;
        private static Dictionary<int, double> dp;

        static double Solve(int balls, int n)
        {
            if (balls == 0 || n == nMinimum)
                return 0;

            var n1 = (long) balls >> 16 | (long) balls << 16;
            n1 = n1 >> 0x8 & 0x00ff00ff | n1 << 0x8 & 0xff00ff00;
            n1 = n1 >> 0x4 & 0x0f0f0f0f | n1 << 0x4 & 0xf0f0f0f0;
            n1 = n1 >> 0x2 & 0x33333333 | n1 << 0x2 & 0xcccccccc;
            n1 = n1 >> 0x1 & 0x55555555 | n1 << 0x1 & 0xaaaaaaaa;
            balls = Math.Min(balls, (int)(n1 >> (32 - n)));

            var id = balls + (1 << n);
            double result;
            if (dp.TryGetValue(id, out result))
                return result;

            int y = balls;
            int w;
            for (w = 0; y != 0; w++)
                y &= y - 1;
            w = Math.Min(n - nMinimum, w);
            int left, right;
            for (left=0, right=n-1; left < right; left++, right--)
            {
                bool swap = (balls & (1 << right)) != 0;
                int first = swap ? right : left;
                int second = swap ? left : right;

                double s = Solve(RemoveBall(balls, first), n - 1) + ((balls & 1 << first) != 0 ? 1.0 : 0.0);
                if (s < w)
                    s = Math.Max(s, Solve(RemoveBall(balls, second), n - 1) + ((balls & 1 << second) != 0 ? 1.0 : 0.0));

                result += 2 * s;
            }
            if (left == right)
                result += Solve(RemoveBall(balls, left), n - 1) + ((balls & 1 << left) != 0 ? 1.0 : 0.0);
            return dp[id] = result/n;
        }

        static int RemoveBall(int balls, int i)
        {
            var mask = (1 << i) - 1;
            return (balls & mask) | (balls >> 1 & ~mask);
        }
    }
}
