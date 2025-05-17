using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution {

    static string feeOrUpfront(int n, int k, int x, int d, int[] p) {

        double cost = 0;
        double cost2 = d;
        foreach(var pr in p)
            cost += Math.Max(k, x*pr/100.0);
        
        Console.Error.WriteLine($"fee = {cost}  once = {cost2}" );
        if (cost2 >= cost)
            return "fee";
        
        return "upfront";
    }

    static void Main(String[] args) {
        int q = Convert.ToInt32(Console.ReadLine());
        for(int a0 = 0; a0 < q; a0++){
            string[] tokens_n = Console.ReadLine().Split(' ');
            int n = Convert.ToInt32(tokens_n[0]);
            int k = Convert.ToInt32(tokens_n[1]);
            int x = Convert.ToInt32(tokens_n[2]);
            int d = Convert.ToInt32(tokens_n[3]);
            string[] p_temp = Console.ReadLine().Split(' ');
            int[] p = Array.ConvertAll(p_temp,Int32.Parse);
            string result = feeOrUpfront(n, k, x, d, p);
            Console.WriteLine(result);
        }
    }
}
