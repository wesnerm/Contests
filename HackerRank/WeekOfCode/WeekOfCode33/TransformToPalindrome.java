import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

     static int[] ds;
    
    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int n = in.nextInt();
        int k = in.nextInt();
        int m = in.nextInt();
        
        ds = new int[n+1];
        
        for(int a0 = 0; a0 < k; a0++){
            int x = in.nextInt();
            int y = in.nextInt();
            ds[find(y)] = find(x);
        }
        
        int[] a = new int[m];
        for(int a_i=0; a_i < m; a_i++){
            a[a_i] = find(in.nextInt());
        }
        
        int[][] dp = new int[m][m];
        for (int i=0; i<m; i++)
            dp[i][i] = 1;
        
        int max = 1;
        for (int len=1; len<m; len++)
            for (int i=0; i+len<m; i++)
            {
                int j=i+len;
                dp[i][j] = Math.max(Math.max(dp[i+1][j], dp[i][j-1]), dp[i+1][j-1] + (a[i]==a[j]?2:0));
                max = Math.max(dp[i][j], max);
            }
        
        System.out.println(max);
    }
    
    
    public static int find(int x)
    {
        if (ds[x] == 0) ds[x] = x;  
        if (ds[x] == x) return x;
        return ds[x] = find(ds[x]);
    }
    
}
