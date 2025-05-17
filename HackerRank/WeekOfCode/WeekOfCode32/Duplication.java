import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    static int duplication(int x){
        return x==0 ? 0 : 1 ^ duplication(x - Integer.highestOneBit(x));
    }

    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int q = in.nextInt();
        for(int a0 = 0; a0 < q; a0++){
            int x = in.nextInt();
            int result = duplication(x);
            System.out.println(result);
        }
    }
}
