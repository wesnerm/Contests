import java.util.*;
import java.math.*;
import java.io.*;

public class Solution
{
	private int n;
	private int[] v;
	private int g;
	private Group[] groups;
	private int[][] inverted;
	private int sqrtFake;
	private int[] updateLog;
    static Reader in;
    static StringBuilder builder;
    
	public static void main(String[] args) throws IOException
	{
        builder = new StringBuilder();
        in = new Reader();
		int t = in.nextInt();
		for (int a0 = 0; a0 < t; a0++)
		{
			Solution sol = new Solution();
			sol.Run();
		}
	}

	private void Run() throws IOException
	{
		n = in.nextInt();
       
        v = new int[n];
        for(int i = 0; i < n; i++){
            v[i] = in.nextInt();
        }
        
		g = in.nextInt();

		sqrtFake = Math.min(Parameters.MinSqrt, (int)(Math.sqrt(n) + 1));

		groups = new Group[g];
		for (int i = 0; i < g; i++)
		{
			Group tempVar = new Group();
			tempVar.L = in.nextInt();
			tempVar.R = in.nextInt();
			tempVar.X = in.nextInt();
			groups[i] = tempVar;
		}

		SolveLargeCases();

		Solve();

		for (Solution.Group group : groups)
		{
            builder.append((group.Answer != null) ? group.Answer : -1);
            builder.append('\n');
		}
        
        System.out.print(builder);
        builder.setLength(0);
	}

	private void SolveLargeCases()
	{
		int[] counts = new int[n];
		int[] indices = new int[n];
		for (int e : v)
		{
			counts[e]++;
		}

		inverted = new int[n][];
		ArrayList<Integer> largeElements = new ArrayList<Integer>(sqrtFake);
		for (int i = 0; i < inverted.length; i++)
		{
			if (counts[i] >= sqrtFake)
			{
				inverted[i] = new int[counts[i]];
				largeElements.add(i);
			}
		}

		for (int i = 0; i < v.length; i++)
		{
			int e = v[i];
			int[] ie = inverted[e];
			if (ie != null)
			{
				ie[indices[e]++] = i;
			}
		}

		for (Solution.Group group : groups)
		{
			if (group.X >= sqrtFake)
			{
				group.Answer = -1;
				for (int e : largeElements)
				{
					int[] array = inverted[e];
					if (array.length < group.X)
					{
						continue;
					}
					int lower = BinarySearch(array, group.L, 0, array.length - 1);
					int upper = BinarySearch(array, group.R, 0, array.length - 1, true);
					if (upper - lower == group.X)
					{
						group.Answer = e;
						break;
					}
				}
			}
		}
	}

	private int[] counts;
	private MinBag[] bags;

	private void Solve()
	{
		counts = new int[n];
		updateLog = new int[n];

		int sqrtN = (int)Math.sqrt(n);
		ArrayList<Group> queriesSorted = new ArrayList<Group>();
        for (Group g : groups)
            if (g.Answer == null)
                queriesSorted.add(g);
        
		Collections.sort(queriesSorted, (x, y) ->
		{
				int cmp = (x.L / sqrtN) - (y.L / sqrtN);
				if (cmp != 0)
				{
					return cmp;
				}
				cmp = x.R - (y.R);
				if (cmp != 0)
				{
					return cmp;
				}
				return x.L - y.L;
		});

		   bags = new MinBag[sqrtFake];
		for (Group q : queriesSorted)
		{
			int x = q.X;
			if (bags[x] == null)
			{
				bags[x] = new MinBag(updateLog);
			}
		}


		int mosLeft = 0;
		int mosRight = -1;

		for (Group q : queriesSorted)
		{
			while (mosRight < q.R)
			{
				AddDelta(v[++mosRight], +1);
			}
			while (mosRight > q.R)
			{
				AddDelta(v[mosRight--], -1);
			}
			while (mosLeft > q.L)
			{
				AddDelta(v[--mosLeft], +1);
			}
			while (mosLeft < q.L)
			{
				AddDelta(v[mosLeft++], -1);
			}
			int x = q.X;
			q.Answer = bags[x].Count != 0 ? bags[x].Min() : -1;
		}
	}

	private void AddDelta(int e, int delta)
	{
		int eCount = counts[e];

		if (eCount < bags.length)
		{
			if (bags[eCount] != null) bags[eCount].Remove(e);
		}

		eCount += delta;

		if (eCount < bags.length)
		{
			if (bags[eCount] != null) bags[eCount].Insert(e);
		}

		counts[e] = eCount;
	}


	public static int BinarySearch(int[] array, int value, int left, int right)
	{
		return BinarySearch(array, value, left, right, false);
	}

	public static int BinarySearch(int[] array, int value, int left, int right, boolean upper)
	{
		while (left <= right)
		{
			int mid = left + (right - left) / 2;
			int cmp = (new Integer(value)).compareTo(array[mid]);
			if (cmp > 0 || cmp == 0 && upper)
			{
				left = mid + 1;
			}
			else
			{
				right = mid - 1;
			}
		}
		return left;
	}

	public static class Group
	{
		public int Index;
		public int L;
		public int R;
		public int X;
		public Integer Answer = null;
		@Override
		public String toString()
		{
			return String.format("%1$s %2$s %3$s %4$s", Index, L, R, X);
		}
	}



}


class MinBag
{
	private SegmentTree tree;

	public int Count;
	private int[] updateLog;

	public MinBag(int[] updateLog)
	{
		tree = new SegmentTree(0);
		this.updateLog = updateLog;
	}

	public final void Insert(int v)
	{
		int check = updateLog[v];
		if (check >= 0 && check < Count && tree.getItem(check) == v)
		{
			return;
		}

		tree.Ensure(v);
		check = Count++;
		tree.setItem(check, v);
		updateLog[v] = check;
	}

	public final void Remove(int v)
	{
		int check = updateLog[v];
		if (check >= 0 && check < Count && tree.getItem(check) == v)
		{
			updateLog[v] = -1;

			Count--;
			if (check < Count)
			{
				int rep = tree.getItem(Count);
				tree.setItem(check, rep);
				updateLog[rep] = check;
			}
		}
	}

	public final int Min()
	{
		return tree.QueryInclusive(0, Count - 1);
	}

}

class SegmentTree
{
	private int[] _tree;
	private static int[] empty = new int[0];

	public SegmentTree(int size)
	{
		_tree = CreateTree(size);
	}

	public final int getLength()
	{
		return _tree.length >>> 1;
	}

	public final void Ensure(int index)
	{
		if (index << 1 < _tree.length)
		{
			return;
		}
		int newSize = Math.max(Parameters.InitialBagSize, Math.max(index + 1, _tree.length));
		int[] oldTree = _tree;
		int oldTreeSize = oldTree.length >>> 1;
		_tree = CreateTree(newSize);
		Transfer(oldTree, oldTreeSize, oldTreeSize);
	}

	private int[] CreateTree(int size)
	{
		if (size == 0)
		{
			return empty;
		}
		int[] tree = new int[size * 2];
		for (int i = 0; i < tree.length; i++)
		{
			tree[i] = Integer.MAX_VALUE;
		}
		return tree;
	}

	private void Transfer(int[] array, int start, int count)
	{
		int size = _tree.length >> 1;
		System.arraycopy(array, start, _tree, size, count);
		for (int i = size - 1; i > 0; i--)
		{
			_tree[i] = Math.min(_tree[i << 1], _tree[i << 1 | 1]);
		}
	}

	public final int getItem(int index)
	{
		return _tree[index + (_tree.length >> 1)];
	}
	public final void setItem(int index, int value)
	{
		int i = index + (_tree.length >> 1);
		_tree[i] = value;
		int min = value;

        for (; i > 1; i >>= 1)
		{
			int newMin = Math.min(_tree[i], _tree[i ^ 1]);

            if (_tree[i >> 1] == newMin)
			{
				break;
			}
			_tree[i >> 1] = newMin;
		}
	}

	public final int QueryInclusive(int left, int right)
	{
		int size = _tree.length / 2;
		left += size;
		right += size;

		int result = Integer.MAX_VALUE;
		for (; left <= right; left >>= 1, right >>= 1)
		{
			if (left % 2 == 1)
			{
				result = Math.min(result, _tree[left++]); // if parent is the left child, then parents have the sum
			}
			if (right % 2 == 0)
			{
				result = Math.min(result, _tree[right--]); // if parent is the right child, then parents have the sum
			}
		}
		return result;
	}

	@Override
	public String toString()
	{
		StringBuilder sb = new StringBuilder();
		for (int t : _tree)
		{
			sb.append(t);
			sb.append(',');
		}
		return sb.toString();
	}

}


 final class Parameters
{
	public static final int InitialBagSize = 16;
	public static final int MinSqrt = 100;
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
