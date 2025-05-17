import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution 
{
    public static void main(String[] args) throws IOException
    {
        Reader in = new Reader();
        int [][] table;
        int [][] sums;
        int [] mask;
        int [] maskts;
        int time = 1;
        
        int n = in.nextInt(), m = in.nextInt();
        boolean swap = false;
        
        if (n<=m)
        {
            table = new int[n+1][];
            table[0] = new int[m+1];
            for (int i=1; i<=n; i++)
            {
                int[] t = table[i] = new int[m+1];
                for (int j=1; j<=m; j++)
                t[j] = 1 << in.nextInt();
            }
        } 
        else
        {
            table = new int[m+1][];
            for (int j=0; j<=m; j++)
                table[j] = new int[n+1];
            
            for (int i=1; i<=n; i++)
                for (int j=1; j<=m; j++)
                    table[j][i] = 1 << in.nextInt();
            int tmp = n;
            n = m;
            m = tmp;
            swap = true;
        }

        sums = new int[n+1][];
        for (int i=0; i<=n; i++)
        {
            int[] t = table[i];
            int[] s = sums[i] = new int[m+1];
            for (int j=0; j<=m; j++)
                s[j] = t[j]>1 ? 1 : 0;
        }
        
        for (int i=0; i<=n; i++)
        for (int j=1; j<=m; j++)
        {
            table[i][j] ^= table[i][j-1];
            sums[i][j] += sums[i][j-1];
        }
        
        for (int i=1; i<=n; i++)
            for (int j=0; j<=m; j++)
            {
                table[i][j] ^= table[i-1][j];
                sums[i][j] += sums[i-1][j];
            }

        long i0=0,i1=0,j0=0,j1=0;
        long area=1;
        
        for (int ii=n; ii>0; ii--)
        for (int jj=m; jj>0; jj--)
        {
            int xor = table[ii][jj];
            int newarea = ii*jj;
            if (newarea <= area || sums[ii][jj]<=1) break;
            if ((xor & xor-1)!=0) continue;
            area = newarea;
            i1 = ii-1;
            j1 = jj-1;
        }

        mask = new int[1<<10];
        maskts = new int[mask.length];
        
        if (sums[n][m]>1)
        for (int i=0; i<n; i++)
        for (int ii=n; ii>i; ii--)
        {                
            if ( (ii-i) * m <= area ) break;
            int sum = sums[ii][m] - sums[ii][0] - sums[i][m] + sums[i][0];
            if (sum<=1) break;
            
            maskts[0] = ++time;
            mask[0] = 0;
            for (int jj=1; jj<=m; jj++)
            {
                int xs = table[ii][jj] ^ table[ii][0] ^ table[i][jj] ^ table[i][0];
                
                int j = m+1;
                for (int bit=1<<10; bit>0; )
                {
                    bit >>= 1;
                    int mymask = bit ^ xs;
                    if (maskts[mymask] == time && mask[mymask] < j) 
                        j = mask[mymask];
                }                
                
                if (maskts[xs]<time)
                {
                    maskts[xs]=time;
                    mask[xs] = jj;
                }
                
                if (j > jj) continue;
                
                int newarea = (ii-i)*(jj-j);
                if ( newarea <= area ) continue;

                sum = sums[ii][jj] - sums[ii][j] - sums[i][jj] + sums[i][j];
                if (sum>1)
                {
                    area = newarea;
                    i0 = i;
                    j0 = j;
                    i1 = ii-1;
                    j1 = jj-1;
                }
            }
        }
        
        System.out.println(area);
        if (swap) System.out.format("%d %d %d %d", j0, i0, j1, i1);
        else System.out.format("%d %d %d %d", i0, j0, i1, j1);
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

