import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {
    static int x[];
    static int w[];
    static long cache[][];
    
    public static void main(String[] args) throws IOException{
        Reader in = new Reader();
        int n = in.nextInt();
        int k = in.nextInt();
        x = new int[n];
        w = new int[n];
        cache = new long[n+1][k+1];
        for(int i = 0; i < n; i++){
            x[i] = in.nextInt();
            w[i] = in.nextInt();
        }
        long ans = MinCost(k, n);
        System.out.println(ans);
    }

    static final long MaxCost = -1L >>> 16;
    
    static long MinCost(int k, int n)
    {
        long[] cache = new long[n+1];
        long[] cache2 = new long[n+1];
        long[] buffer = new long[n+1];
        
        for (int nn = 1; nn <= n; nn++)
            cache[nn] = MaxCost;

        for (int kk = 1; kk <= k; kk++)
        {
            cache2[kk] = 0;

            long min = -1 >>> 1;
            int limit = n-(k-kk);
           
            for (int nn = kk + 1; nn <= limit; nn++)
            {
                long rollCost = 0;
                long rollsCost = 0;
                long minCost = MaxCost;
                long xprev = x[nn - 1];
                for (int i = nn - 1; i >= kk - 1; i--)
                {
                    rollsCost += rollCost * (xprev - x[i]);
                    long cost = cache[i] + rollsCost;
                    if (cost < minCost) minCost = cost;
                    rollCost += w[i];
                    if (rollsCost >= minCost) break;
                    xprev = x[i];
                }
                cache2[nn] = minCost;
            }

            long[] tmp = cache;
            cache = cache2;
            cache2 = tmp;
        }
        
        return cache[n];
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

    public String readLine() throws IOException
    {
        byte[] buf = new byte[64]; // line length
        int cnt = 0, c;
        while ((c = read()) != -1)
        {
            if (c == '\n')
                break;
            buf[cnt++] = (byte) c;
        }
        return new String(buf, 0, cnt);
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

