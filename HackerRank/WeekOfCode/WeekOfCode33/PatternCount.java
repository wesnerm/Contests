import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

     public static List<String> getAllMatches(String text, String regex) {
        List<String> matches = new ArrayList<String>();
        Matcher m = Pattern.compile(regex).matcher(text);
        while(m.find())
            matches.add(m.group());
        return matches;
    }
    
    public static void main(String[] args) {
        Scanner in = new Scanner(System.in);
        int q = in.nextInt();
        for(int a0 = 0; a0 < q; a0++){
            String s = in.next();
            int result =  getAllMatches(s, "10+(?=1)").size();
            System.out.println(result);
        }
    }
}
