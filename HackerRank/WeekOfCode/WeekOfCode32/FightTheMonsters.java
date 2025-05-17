import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    static int getMaxMonsters(int n, int hit, int t, int[] h)
    {
		Arrays.sort(h);

		int i = 0;
		while (t>0 && i<n)
		{
			long div = (h[i] + hit - 1) / hit;
			t -= div;
			if (t < 0) break;
			i++;
		}

		return i;
	}

    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int n = in.nextInt();
        int hit = in.nextInt();
        int t = in.nextInt();
        int[] h = new int[n];
        for(int h_i=0; h_i < n; h_i++){
            h[h_i] = in.nextInt();
        }
        int result = getMaxMonsters(n, hit, t, h);
        System.out.println(result);
    }
}
