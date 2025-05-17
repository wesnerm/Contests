namespace HackerRank.WorldCodeSprint8.QueensAttack
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using static System.Math;
    class Solution
    {

        static void Driver()
        {
            var tokens_n = Console.ReadLine().Split();
            int n = int.Parse(tokens_n[0]);
            int k = int.Parse(tokens_n[1]);

            var tokens_rQueen = Console.ReadLine().Split();
            int rQueen = int.Parse(tokens_rQueen[0]);
            int cQueen = int.Parse(tokens_rQueen[1]);

            int a0 = Min(rQueen, cQueen) - 1;
            int r0 = rQueen - a0;
            int c0 = cQueen - a0;
            int len0 = r0 < c0 ? n - c0 + 1 : n - r0 + 1;

            int a1 = Min(cQueen - 1, n - rQueen);
            int r1 = rQueen + a1;
            int c1 = cQueen - a1;
            int len1 = r1 == 1 ? c1 : Abs(r1-c1) + 1;

            long attackSquares = 2 * (n - 1); //horizontal and vertical
            attackSquares += len0 - 1; // left diagonal        
            attackSquares += len1 - 1; // right diagonal

            // horizontal, vertical, leftdiag, rightdiag
            var min = new int[] { 1, 1, c0, c1 };
            var max = new int[] { n, n, c0 + len0-1, c1+len1-1 };

            while (k-- > 0)
            {
                var arr = Console.ReadLine().Split();
                int r = int.Parse(arr[0]);
                int c = int.Parse(arr[1]);
                int dr = r - rQueen;
                int dc = c - cQueen;

                int type = -1;
                int q = cQueen;
                int pos = c;
                if (r == rQueen) { type = 0;}
                else if (c == cQueen) { type = 1; q = rQueen; pos=r; }
                else if (dr == dc) { type = 2; }
                else if (dr == -dc) { type = 3; }
                else continue;

                if (pos < min[type] || pos > max[type])
                    continue;

                if (pos < q)
                {
                    attackSquares -= (pos - min[type])+1;
                    min[type] = pos+1;
                }
                else
                {
                    attackSquares -= (max[type] - pos)+1;
                    max[type] = pos-1;
                }
            }

            Console.WriteLine(attackSquares);
        }


    }
}
