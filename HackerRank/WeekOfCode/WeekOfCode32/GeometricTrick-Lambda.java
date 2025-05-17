import java.io.*;
import java.util.*;

import java.text.*;
import java.math.*;
import java.util.regex.*;
import java.util.function.*;

public class Solution {

    static long geometricTrick(byte[] str){
        s = str;
        n = str.length;
        
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
        
        BiFunction<Integer,Integer,Integer> func = (f,j)->
        {
            if (s[f-1]==(int)'b') return 0;
            long f2 = ((long)j*j/f);
            if (f2<=n && s[f-1]==(s[(int)f2-1]^2)) return 1;
            return 0;            
        };

        long count = 0;
        for (int j=0; j<n; j++)
            if (s[j]=='b')
                count += dfs(j+1, j+1, 1, func);
        return count;
    }
    
    static int n;
    static byte[] s;
    static int factors[];

    static long dfs(int d, int j, long f, BiFunction<Integer, Integer, Integer> func)
    {
        if (f > j) return 0;
        if (d==1)
        {
            return func.apply((int)f, j);

        }

        int p = factors[d];
        int c = 1;
        int next = d/p;
        while (next > 1 && factors[next]==p) { c++; next/=p; }

        c*=2;
        long res = dfs(next, j, f, func);
        while (c-- >0)
        {
            f*=p;
            res += dfs(next, j, f, func);
        }
        return res;        
    }


    public static void main(String[] args) throws IOException {
        Reader in = new Reader();
        int n = in.nextInt();
        byte[] s = in.readLine(n);
        long result = geometricTrick(s);
        System.out.println(result);
    }
}

class Reader
{
    final private int BUFFER_SIZE = 1 << 16;
    private DataInputStream din;
    private byte[] buffer;
    private int bufferPointer, bytesRead;

    public Reader()
    {
        din = new DataInputStream(System.in);
        buffer = new byte[BUFFER_SIZE];
        bufferPointer = bytesRead = 0;
    }

    public Reader(String file_name) throws IOException
    {
        din = new DataInputStream(new FileInputStream(file_name));
        buffer = new byte[BUFFER_SIZE];
        bufferPointer = bytesRead = 0;
    }

    public byte[] readLine(int n) throws IOException
    {
        int c = read();
        while (c <= ' ')
            c = read();

        byte[] buf = new byte[n]; // line length
        int cnt = 0;
        for (int i=0; i<n; i++)
        {
          buf[i] = (byte) c;
          c = read();
        }
        return buf;
    }

    public int nextInt() throws IOException
    {
        int ret = 0;
        byte c = read();
        while (c <= ' ')
            c = read();
        boolean neg = (c == '-');
        if (neg)
            c = read();
        do
        {
            ret = ret * 10 + c - '0';
        }  while ((c = read()) >= '0' && c <= '9');

        if (neg)
            return -ret;
        return ret;
    }

    public long nextLong() throws IOException
    {
        long ret = 0;
        byte c = read();
        while (c <= ' ')
            c = read();
        boolean neg = (c == '-');
        if (neg)
            c = read();
        do {
            ret = ret * 10 + c - '0';
        }
        while ((c = read()) >= '0' && c <= '9');
        if (neg)
            return -ret;
        return ret;
    }

    public double nextDouble() throws IOException
    {
        double ret = 0, div = 1;
        byte c = read();
        while (c <= ' ')
            c = read();
        boolean neg = (c == '-');
        if (neg)
            c = read();

        do {
            ret = ret * 10 + c - '0';
        }
        while ((c = read()) >= '0' && c <= '9');

        if (c == '.')
        {
            while ((c = read()) >= '0' && c <= '9')
            {
                ret += (c - '0') / (div *= 10);
            }
        }

        if (neg)
            return -ret;
        return ret;
    }

    private void fillBuffer() throws IOException
    {
        bytesRead = din.read(buffer, bufferPointer = 0, BUFFER_SIZE);
        if (bytesRead == -1)
            buffer[0] = -1;
    }

    private byte read() throws IOException
    {
        if (bufferPointer == bytesRead)
            fillBuffer();
        return buffer[bufferPointer++];
    }

    public void close() throws IOException
    {
        if (din == null)
            return;
        din.close();
    }
}
