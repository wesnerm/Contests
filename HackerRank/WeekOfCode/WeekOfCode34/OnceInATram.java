import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    static int onceInATram(int x) {
        do x++; while (Sum(x%1000)!=Sum(x/1000));
        return x; 
    }
    
    static int Sum(int x)
    {
        int sum = 0;
        while (x>0)
        {
            sum += x % 10;
            x/=10;
        }
        return sum;
    }

    public static void main(String[] args) {
    Scanner in = new Scanner(System.in);
    int x = in.nextInt();
    int result = onceInATram(x);
    System.out.println(result);
    }
}