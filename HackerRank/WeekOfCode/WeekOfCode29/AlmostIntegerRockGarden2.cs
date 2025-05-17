namespace HackerRank.WeekOfCode29.AlmostIntegerRockGarden2
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using static System.Math;
	using static System.Console;

	public class Solution
	{

		public static void Main()
		{
			var sw = new Stopwatch();
			sw.Start();
			string[] tokens_x = ReadLine().Split(' ');
			int x = Convert.ToInt32(tokens_x[0]);
			int y = Convert.ToInt32(tokens_x[1]);
			Precompute();
			Search2(x, y);
#if DEBUG
			WriteLine(sw.Elapsed);
#endif

		}

		static Dictionary<long, List<Coord>> coords = new Dictionary<long, List<Coord>>();

		public static void Search2(int x, int y)
		{
			var hashSet = Search(x, y);
			if (hashSet == null) return;

			var c = new Coord(x, y);
			hashSet.Remove(c);
			var score = c.Value;
			foreach (var c2 in hashSet)
			{
				WriteLine($"{c2.X} {c2.Y}");
				score += c2.Value;
			}
#if DEBUG
			WriteLine(score);
#endif
		}

		public static List<Coord> Search(int x, int y)
		{
			var given = new Coord { X = x, Y = y };
			var d = given.Code;

			var output = new List<Coord>() {given};
			int datalen = Data.GetLength(0);
			for (int i = 0; i < datalen; i++)
			{
				int found = -1;
				for (int j = 0; j < 12; j++)
				{
					var c = new Coord(Data[i, j, 0], Data[i, j, 1]);
					if (c.Code == d)
					{
						found = j;
						break;
					}
				}

				if (found == -1)
					continue;

				for (int j = 0; j < 12; j++)
				{
					if (found == j) continue;

					var coord = new Coord(Data[i, j, 0], Data[i, j, 1]);


					foreach (var crd in coords[coord.Code])
					{
						coord.X = crd.X;
						coord.Y = crd.Y;
						bool looking = true;
						for (int k = 0; k < 2 && looking; k++)
						{
							for (int m = 0; m <= 3; m++)
							{
								coord.X = Abs(coord.X) * ((m & 1) == 0 ? 1 : -1);
								coord.Y = Abs(coord.Y) * ((m & 2) == 0 ? 1 : -1);
								if (!output.Contains(coord))
								{
									output.Add(coord);
									looking = false;
									break;
								}
							}

							if (looking)
							{
								coord.X = crd.Y;
								coord.Y = crd.X;
							}
						}
					}
				}
				break;
			}

			return output;
		}


		static void Precompute()
		{
			for (int i = 1; i <= 12; i++)
				for (int j = i; j <= 12; j++)
				{
					var coord = new Coord { X = i, Y = j };
					var d = DoubleToLong(Sqrt(i * i + j * j));
					if (d == 0) continue;
					if (!coords.ContainsKey(d))
						coords[d] = new List<Coord>();
					coords[d].Add(coord);
				}

		}


		public static long DoubleToLong(double d)
		{
			var d2 = d - (long)d;
			d2 *= 1L << 62;
			long value = (long)(d2) << 2;
			return value;
		}

		public class Coord
		{
			public int X;
			public int Y;
			public double Value => Sqrt(X * X + Y * Y);
			public long Code => DoubleToLong(Value);

			public Coord()
			{

			}

			public Coord(int x, int y)
			{
				X = x;
				Y = y;
			}
			public override int GetHashCode()
			{
				return X.GetHashCode() ^ Y.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var coord = obj as Coord;
				return coord != null && coord.X == X && coord.Y == Y;
			}

			public override string ToString()
			{
				return $"{X} {Y}";
			}

		}

		public static double Frac(double f)
		{
			return f - Floor(f);
		}


		public static int[,,] Data =
		{
			{{1, 1}, {2, 3}, {3, 7}, {6, 6}, {3, 11}, {3, 9}, {7, 12}, {-7, 12}, {7, -12}, {5, 11}, {1, 10}, {2, 11}},
			{{1, 2}, {1, 12}, {6, 6}, {9, 11}, {8, 10}, {2, 9}, {11, 12}, {-11, 12}, {3, 9}, {4, 9}, {1, 10}, {1, 9}},
			{{1, 3}, {1, 10}, {2, 3}, {7, 12}, {-7, 12}, {3, 11}, {7, -12}, {7, 7}, {5, 11}, {3, 7}, {2, 11}, {2, 6}},
			{{1, 4}, {1, 7}, {4, 11}, {8, 10}, {4, 12}, {-4, 12}, {4, 9}, {-4, 9}, {2, 8}, {4, -9}, {1, 12}, {1, 3}},
			{{1, 5}, {2, 10}, {4, 9}, {10, 11}, {-4, 9}, {10, 10}, {6, 12}, {2, 5}, {-6, 12}, {7, 8}, {1, 10}, {-1, 5}},
			{{1, 6}, {2, 12}, {3, 6}, {10, 12}, {-10, 12}, {8, 10}, {10, -12}, {5, 11}, {6, 6}, {9, 10}, {4, 12}, {4, 11}},
			{{1, 8}, {-1, 8}, {3, 8}, {4, 10}, {2, 11}, {2, 5}, {8, 12}, {5, 7}, {6, 10}, {4, 4}, {2, 12}, {3, 9}},
			{{1, 11}, {-1, 11}, {2, 12}, {7, 12}, {4, 5}, {3, 10}, {7, 11}, {6, 9}, {6, 6}, {4, 12}, {-4, 12}, {3, 12}},
			{{2, 2}, {2, 11}, {3, 7}, {7, 12}, {5, 11}, {3, 11}, {-7, 12}, {7, -12}, {3, 9}, {2, 3}, {1, 10}, {1, 7}},
			{{2, 4}, {2, 3}, {1, 10}, {7, 12}, {-7, 12}, {5, 11}, {7, -12}, {7, 7}, {3, 11}, {3, 9}, {3, 7}, {3, 6}},
			{{2, 7}, {3, 12}, {9, 10}, {-9, 10}, {5, 7}, {4, 9}, {5, 8}, {-4, 9}, {2, 4}, {1, 9}, {1, 6}, {1, 5}},
			{{3, 3}, {3, 9}, {3, 11}, {7, 12}, {-7, 12}, {5, 11}, {7, -12}, {3, 7}, {4, 4}, {2, 11}, {2, 3}, {1, 10}},
			{{3, 5}, {2, 8}, {5, 11}, {2, 3}, {1, 8}, {1, 3}, {8, 10}, {2, 10}, {8, 8}, {5, 9}, {-2, 10}, {2, -10}},
			{{4, 6}, {-4, 6}, {5, 6}, {2, 10}, {1, 3}, {1, 1}, {6, 12}, {5, 7}, {6, 9}, {4, 4}, {1, 5}, {3, 11}},
			{{4, 8}, {3, 9}, {7, 7}, {7, 12}, {-7, 12}, {7, -12}, {5, 11}, {3, 7}, {3, 11}, {2, 3}, {1, 10}, {1, 2}},
			{{6, 11}, {2, 8}, {2, 7}, {10, 12}, {8, 11}, {9, 9}, {-10, 12}, {10, -12}, {4, 11}, {4, 8}, {1, 12}, {1, 8}},
			{{7, 10}, {6, 9}, {10, 11}, {5, 11}, {5, 7}, {5, 8}, {5, 9}, {-5, 8}, {5, -8}, {5, 6}, {3, 7}, {3, 11}},
			{{11, 11}, {5, 9}, {9, 10}, {5, 8}, {4, 9}, {3, 11}, {7, 8}, {6, 10}, {-6, 10}, {10, 10}, {3, 5}, {1, 6}},
			{{12, 12}, {7, 8}, {9, 10}, {6, 10}, {4, 9}, {5, 8}, {9, 9}, {-6, 10}, {5, 9}, {3, 11}, {3, 5}, {1, 6}},
		};

#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

}