namespace HackerRank.UniversityCodesprint
{
	using System;
	using System.Linq;

	class ArrayReconstruction
	{
		static void Main(String[] args)
		{
			new ArrayReconstruction().Run();
		}

		int n, s, k;
		int[] buffer;
		int[] table;
		int[] tablesum;

		public void Run()
		{

			int q = int.Parse(Console.ReadLine());
			for (int qq = 0; qq < q; qq++)
			{
				var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
				n = a[0];
				s = a[1];
				k = a[2];
				buffer = new int[n];

				table = new int[n];
				tablesum = new int[n + 1];
				for (int i = 0; i < n; i++)
				{
					table[i] = 2 * i - n + 1;
					tablesum[i + 1] = table[i] + tablesum[i];
				}


				if (Sum(0, s, k, 0, 0))
					Console.WriteLine(string.Join(" ", buffer));
				else
					Console.WriteLine(-1);
			}
		}

		public int SearchMin(int bottom, int top, Func<int, int> f)
		{
			int left = bottom;
			int right = top;
			while (left < right)
			{
				int div = (right - left) / 3;
				int m1 = left + div;
				int m2 = right - div;
				if (m1 >= m2) m2 = m1 + 1;
				int f1 = f(m1);
				int f2 = f(m2);
				if (f1 > f2) left = m1 + 1;
				else right = m2 - 1;
			}
			return left;
		}

		public bool Sum(int index, int s, int k, int bottom, int ksum)
		{
			if (s < 0 || k < 0) return false;
			if (index >= buffer.Length) return s == 0 && k == 0;
			if (index == buffer.Length - 1) bottom = s;

			int prevsum = this.s - s;
			int top = s / (n - index);

			if (k < index * s - (n - index) * prevsum) return false;
			// k = sum( (0<=i<n) 2*i*a[i] - (n-1)*a[i])
			// k = sum((0<=i<index)(2*i-n+1)*a[i]) + bound*sum( (index<=i<n) (2*i-n+1) )
			// bound = (k - sum((0<=i<index)(2*i-n+1)*a[i]))/ sum( (index<=i<n) (2*i-n+1))

			if (index < buffer.Length - 1)
			{
				int b2 = SearchMin(bottom, top, x =>
				{
					int sumnm1 = s - bottom * (buffer.Length - 1 - index);
					int knm1 = ksum + bottom * (tablesum[buffer.Length - 1] - tablesum[index]) + sumnm1 * table[buffer.Length - 1];
					return Math.Abs(this.k - knm1);
				});
				bottom = b2;
			}

			if (tablesum[n] - tablesum[index] > 0)
			{
				int bound = (this.k - ksum) / (tablesum[n] - tablesum[index]);
				top = Math.Min(bound, top);
			}

			for (int i = bottom; i <= top; i++)
			{
				int diff = index * i - prevsum;
				buffer[index] = i;
				if (Sum(index + 1, s - i, k - diff, i, ksum + (2 * index - n + 1) * i))
					return true;
			}

			return false;
		}
	}
}
