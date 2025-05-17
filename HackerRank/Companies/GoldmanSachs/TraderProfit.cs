using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution {

    static int traderProfit(int k, int n, int[] prices) {
            int len = prices.Length;
            if (len < 2) return 0;
            if (k>len/2) k = len/2;  

            /*
            if (k>len/2)
            { // simple case
                int ans = 0;
                for (int i=1; i<len; ++i){
                    ans += max(prices[i] - prices[i-1],0);
                }
                return ans;
            }
            */

            int[] buy = new int[k+1];
            int[] sell = new int[k+1];

            for (int i=0; i<=k; i++)
                buy[i] = int.MinValue;

            for (int i=0; i<len; i++)
            {
                int p = prices[i];
                for (int j=k; j>0; j--)
                {
                    sell[j] = Math.Max(sell[j], buy[j] + p);
                    buy[j] = Math.Max(buy[j], sell[j-1] - p);
                }
            }
            return sell[k];            
        }

    static void Main(String[] args) {
        int q = Convert.ToInt32(Console.ReadLine());
        for(int a0 = 0; a0 < q; a0++){
            int k = Convert.ToInt32(Console.ReadLine());
            int n = Convert.ToInt32(Console.ReadLine());
            string[] arr_temp = Console.ReadLine().Split(' ');
            int[] arr = Array.ConvertAll(arr_temp,Int32.Parse);
            int result = traderProfit(k, n, arr);
            Console.WriteLine(result);
        }
    }
}
