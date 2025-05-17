using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution {

    static long buyMaximumProducts(long n, long k, long[] a) {
        
        var index = new long[a.Length];
        for (int i=0; i<a.Length; i++)
            index[i] = i+1;
        
        Array.Sort(a, index);
        
        long max = 0;
        for (int i =0; i<a.Length && k>0; i++)
        {
            if (index[i]*a[i] > k)
            {
                long add = k/a[i];
                max += add;
                break;
            }
            max += index[i];
            k -= index[i] * a[i];
        }
        
        return max;
    }

    static void Main() {
        var n = long.Parse(Console.ReadLine().Trim());
        var arr = Array.ConvertAll(Console.ReadLine().Trim().Split(),long.Parse);
        var k = long.Parse(Console.ReadLine().Trim());
        var result = buyMaximumProducts(n, k, arr);
        Console.WriteLine(result);
    }
}
