import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    static long n;
    
    static long circularWalk(int n, int s, int t, int r0, int g, int seed, int p){
        Solution.n = n;
		int[] r = new int[n];
		r[0] = (int) r0;
		for (int i=1; i<n; i++)
		{
			long ri = r[i - 1] * g + seed; // BUGGY
			r[i] = (int) (ri % p);
		}

		long left = s;
		long right = s;
		long maxleft = left - r[s];
		long maxright = right + r[s];

		long ans = -1;
		for (int k=0; k<=n; k++)
		{
			if (between(left, right, t))
			{
				ans = k;
				break;
			}

			long newLeft = left;
			long newRight = right;

			for (long i=maxleft; i<left; i++)
			{
			     long  v = r[fix(i)];
				newLeft = Math.min(i - v, newLeft);
				newRight = Math.max(i + v, newRight);
			}

			for (long i = right+1; i<= maxright; i++)
			{
				long v = r[fix(i)];
				newLeft = Math.min(i - v, newLeft);
				newRight = Math.max(i + v, newRight);
			}

            if (left == maxleft && right == maxright)
                break;
            
			left = maxleft;
			right = maxright;
			maxleft = newLeft;
			maxright = newRight;
		}
        
		return(ans);
	}

	static boolean between(long left, long right, long pos)
	{
		long count = right - left;

		long fixleft = fix(left);
		if (pos < fixleft)
			pos += n;

		return pos - fixleft <= count;
	}


	static int fix(long x)
	{
		return (int)(((x % n) + n) % n);
	}

    
    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int n = in.nextInt();
        int s = in.nextInt();
        int t = in.nextInt();
        int r_0 = in.nextInt();
        int g = in.nextInt();
        int seed = in.nextInt();
        int p = in.nextInt();
        long result = circularWalk(n, s, t, r_0, g, seed, p);
        System.out.println(result);
    }
}
