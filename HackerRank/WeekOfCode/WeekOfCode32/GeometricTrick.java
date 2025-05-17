import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    static long geometricTrick(String str){
        s = str;
        n = str.length();
        
        factors = new int[n+1];
        for (int i=0; i<=n; i++)
            factors[i] = i%2==0 ? 2 : i;

        int sq = (int) Math.sqrt(n);
        for (int i=3; i<=sq; i++)
            if (factors[i] == i)
            {
                for (int j=i*i; j<=n; j+=2*i)
                    if (factors[j] == j) 
                        factors[j] = i;
            }

        long count = 0;
        for (int j=0; j<n; j++)
            if (s.charAt(j)=='b')
                count += dfs(j+1, j+1, 1);
        return count;
    }
    
    static int n;
    static String s;
    static int factors[];

    static long dfs(int d, int j, long f)
    {
        if (f > j) return 0;
        if (d==1)
        {
            int ff = (int)f;
            if (s.charAt(ff-1)=='b') return 0;
            long f2 = ((long)j*j/f);
            if (f2<=n && (int)s.charAt(ff-1)==((int)s.charAt((int)f2-1)^2)) return 1;
            return 0;
        }

        int p = factors[d];
        int c = 1;
        int next = d/p;
        while (next > 1 && factors[next]==p) { c++; next/=p; }

        c*=2;
        long res = dfs(next, j, f);
        while (c-- >0)
        {
            f*=p;
            res += dfs(next, j, f);
        }
        return res;        
    }


    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int n = in.nextInt();
        String s = in.next();
        long result = geometricTrick(s);
        System.out.println(result);
    }
}
