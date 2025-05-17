import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.util.regex.*;

public class Solution {

    public static void main(String[] args) throws IOException {
        new Solution().solve();
    }
    
    private int n, q;
	private int[] arr;
	private Bucket[] buckets;
	private Bucket empty = new Bucket();
	private Query[] queries;

	private void remap()
	{
		HashMap<Integer, Integer> map = new HashMap<Integer, Integer>();
		int id = 1;

		for (int i = 0; i < n; i++)
		{
			int v = arr[i];
			if (!map.containsKey(v)) map.put(v, id++);
			arr[i] = map.get(v);
		}
        
		buckets = new Bucket[id];
		for (int i = 0; i < id; i++)
			buckets[i] = new Bucket();

        for (int i = 0; i < n; i++)
            buckets[arr[i]].Indices.add(i);

		for (Solution.Query qq : queries)
		{
			qq.X = map.containsKey(qq.X) ? map.get(qq.X) : 0;
			qq.Y = map.containsKey(qq.Y) ? map.get(qq.Y) : 0;
			if (qq.X > qq.Y)
			{
                int tmp = qq.X;
                qq.X = qq.Y;
                qq.Y = tmp;
			}
		}

	}

	public final void solve() throws IOException
	{
        Reader in = new Reader();
        n = in.nextInt();
        q = in.nextInt();
        arr = new int[n];
        for(int arr_i = 0; arr_i < n; arr_i++){
            arr[arr_i] = in.nextInt();
        }

        queries = new Query[q];
		for (int i = 0; i < q; i++)
        {
            int x = in.nextInt();
            int y = in.nextInt();
			Query tempVar = new Query();
			tempVar.X = x;
			tempVar.Y = y;
			queries[i] = tempVar;
        }
        in.close();

		remap();

		long allRanges = AllRanges(n);
		int time = 0;
		int freqStart = n;
		int[] freq = new int[2 * n + 1];
		int[] timestamp = new int[freq.length];

		Query[] answers = Arrays.copyOf(queries, q);
		Arrays.sort(queries, (a,b)->
		{
            int cmp = a.X - b.X;
            if (cmp != 0) return cmp;
            return a.Y - b.Y;
		});

		Query prevQuery = null;
		for (Solution.Query qq : queries)
		{
			int x = qq.X;
			int y = qq.Y;

			if (prevQuery != null && prevQuery.X == x && prevQuery.Y == y)
			{
				qq.Answer = prevQuery.Answer;
				continue;
			}
			prevQuery = qq;

			if (x == y)
			{
				qq.Answer = allRanges;
				continue;
			}

			Bucket bx = buckets[x], by = buckets[y];

			if (x == 0)
			{
				long emptyCount = AllRanges(by.getFirst() - 0) + AllRanges(n - (by.getLast() + 1));
				int length = by.getLast() - by.getFirst() + 1;
				if (by.Indices.size() == 2)
				{
					qq.Answer = emptyCount + AllRanges(by.getLast() - by.getFirst() - 1);
					continue;
				}
				else if (length - by.Indices.size() < 2)
				{
					qq.Answer = emptyCount + (length - by.Indices.size());
					continue;
				}
			}

			int i = 0;
			int j = 0;
			int prev = -1;
			int bal = freqStart;
			time++;

			timestamp[bal] = time;
			freq[bal] = 0;

			while (i < bx.Indices.size() || j < by.Indices.size())
			{
				int v;
				int newBal;
				if (i >= bx.Indices.size() || j < by.Indices.size() && by.Indices.get(j).compareTo(bx.Indices.get(i)) < 0)
				{
					v = by.Indices.get(j++);
					newBal = bal - 1;
				}
				else
				{
					v = bx.Indices.get(i++);
					newBal = bal + 1;
				}

				if (timestamp[newBal] < time)
				{
					timestamp[newBal] = time;
					freq[newBal] = 0;
				}

				freq[bal] += v - prev;
				prev = v;
				bal = newBal;
			}

			freq[bal] += n - prev;

			int start = freqStart;
			while (start > 0 && timestamp[start - 1] == time) start--;

			long count = 0;
			while (start < freq.length && timestamp[start] == time)
			{
				long f = freq[start];
				count += f * (f-1) >> 1;
				start++;
			}
            
            qq.Answer = count;
		}

        StringBuilder builder = new StringBuilder();
        for (Query qq : answers)
		{
		  builder.append(qq.Answer);
		  builder.append('\n');
        }
        System.out.println(builder);
	}

	private long AllRanges(long length)
	{
		return length * (length + 1) / 2;
	}

	public static class Query
	{
		public long Answer;
		public int X;
		public int Y;
	}

	public static class Bucket
	{
		public int X;
		public ArrayList<Integer> Indices = new ArrayList<Integer>();
		public final int getFirst()
		{
			return Indices.get(0);
		}
		public final int getLast()
		{
			return Indices.get(Indices.size() - 1);
		}
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
