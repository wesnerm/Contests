
namespace HackerRank.WorldCodeSprint8
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using static System.Math;
	using STType = System.Int64;

	public class BoxOperations
	{
		public static void Main()
		{
			var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
			int n = arr[0];
			int q = arr[1];

			var box = Console.ReadLine().Split().Select(long.Parse).ToArray();

			var queries = new List<Query>();
			while (q-- > 0)
			{
				arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
				queries.Add(new Query
				{
					Type = arr[0],
					Left = arr[1],
					Right = arr[2],
					Value = arr.Length > 3 ? arr[3] : 0,
				});
			}

			segTree = new DynamicSegmentTree(box);

			foreach (var query in queries)
			{
				long answer;

				int count = query.Right - query.Left + 1;
				switch (query.Type)
				{
					case AddQuery:

						segTree.Add(query.Left, count, query.Value);
						break;
					case DivQuery:
						segTree.Div(query.Left, count, query.Value);
						break;
					case MinQuery:
						answer = segTree.GetMin(query.Left, count);
						Console.WriteLine(answer);
						break;
					case SumQuery:
						answer = segTree.GetSum(query.Left, count);
						Console.WriteLine(answer);
						break;
				}
			}
		}

		public static DynamicSegmentTree segTree;


		public const int AddQuery = 1;
		public const int DivQuery = 2;
		public const int MinQuery = 3;
		public const int SumQuery = 4;

		public class Query
		{
			public int Type;
			public int Left;
			public int Right;
			public int Value;
		}

		public class DynamicSegmentTree
		{
			public STType Min;
			public STType Max;
			public STType Sum;
			public STType LazyAdd;
			public int Count;
			public DynamicSegmentTree Left;
			public DynamicSegmentTree Right;
			public bool Covering;

			public DynamicSegmentTree(STType[] array)
				: this(array, 0, array.Length)
			{
			}

			DynamicSegmentTree(STType[] array, int start, int count)
			{
				Count = count;

				if (count >= 2)
				{
					int mid = count / 2;
					Left = new DynamicSegmentTree(array, start, mid);
					Right = new DynamicSegmentTree(array, start + mid, count - mid);
					UpdateNode();
				}
				else
				{
					var v = array[start];
					Min = v;
					Max = v;
					Sum = v;
				}
			}


			public long GetMin(int start, int count)
			{
				int end = start + count;
				if (start <= 0 && Count <= end)
					return Min;
				if (start >= Count || end <= 0)
					return long.MaxValue;

				LazyPropagate();
				return Min(Left.GetMin(start, count),
					Right.GetMin(start - Left.Count, count));
			}


			public long GetSum(int start, int count)
			{
				int end = start + count;
				if (start <= 0 && Count <= end)
					return Sum;
				if (start >= Count || end <= 0)
					return 0;

				LazyPropagate();
				return Left.GetSum(start, count)
					+ Right.GetSum(start - Left.Count, count);
			}

			public void Add(int start, int count, STType value)
			{
				int end = start + count;
				if (start >= Count || end <= 0)
					return;

				if (start <= 0 && Count <= end)
				{
					Add(value);
					return;
				}

				LazyPropagate();
				Left.Add(start, count, value);
				Right.Add(start - Left.Count, count, value);
				UpdateNode();
			}

			void Add(STType value)
			{
				Sum += value * Count;
				Min += value;
				Max += value;
				LazyAdd += value;
			}


			public void Cover(int start, int count, STType value)
			{
				int end = start + count;
				if (start >= Count || end <= 0)
					return;

				if (start <= 0 && Count <= end)
				{
					Cover(value);
					return;
				}

				LazyPropagate();
				Left.Cover(start, count, value);
				Right.Cover(start - Left.Count, count, value);
				UpdateNode();
			}

			void Cover(STType value)
			{
				Min = value;
				Max = value;
				LazyAdd = 0;
				Sum = value * Count;
				Covering = true;
			}

			void LazyPropagate()
			{
				if (Count <= 1)
					return;

				if (Covering)
				{
					Left.Cover(Min);
					Right.Cover(Min);
					LazyAdd = 0;
					Covering = false;
					return;
				}

				if (LazyAdd != 0)
				{
					var value = LazyAdd;
					LazyAdd = 0;
					Left.Add(value);
					Right.Add(value);
				}
			}

			void UpdateNode()
			{
				var left = Left;
				var right = Right;
				Sum = left.Sum + right.Sum;
				Min = Min(left.Min, right.Min);
				Max = Max(left.Max, right.Max);
			}

			public void Div(int start, int count, int value)
			{
				int end = start + count;
				if (value == 1 || start >= Count || end <= 0)
					return;

				if (start <= 0 && Count <= end)
				{
					/*
					// Before
					Min = Div(Min, value);
					Max = Div(Max, value);
					if (Min == Max)
					{
						Cover(Min);
						return;
					}
					*/

					// After
					var d1 = Div(Min, value) - Min;
					var d2 = Div(Max, value) - Max;
					if (d1 == d2)
					{
						Add(d1);
						return;
					}
				}

				LazyPropagate();
				Left.Div(start, count, value);
				Right.Div(start - Left.Count, count, value);
				UpdateNode();
			}

			static long Div(long v, long d)
			{
				return (long)Floor(v / (double)d);
			}
		}
	}
}
