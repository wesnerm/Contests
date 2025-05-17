
namespace HackerRank.WeekOfCode29.DiameterMinimization
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Collections;
	using System.Linq;
	using System.Text;
	using static System.Math;
	using static System.Console;
	using static HackerRankUtils;

	public class Solution
	{
		#region Variables

		int m;
		int n;
		int BestDiameter = int.MaxValue;
		bool toplevel;
		Graph BestGraph = null;

		public static Dictionary<Tuple<int, int>, Solution> memo = new Dictionary<Tuple<int, int>, Solution>();
		public static readonly Random random = new Random(unchecked((int) 0xDEADBEEF));
		public static readonly Dictionary<int, List<Case>> cases = new Dictionary<int, List<Case>>();

		#endregion

		#region Constructor

		static void Main(string[] args)
		{
			var sw = new Stopwatch();
			sw.Start();

			var input = ReadLine().Split(' ');
			var n = int.Parse(input[0]);
			var m = int.Parse(input[1]);

			var sol = new Solution(n, m, true);

			LaunchTimer(() =>
			{
				Report(sol);
				return true;
			}, TimeLimit);

			Setup();
			var g = sol.Solve();
			Report(sol);

#if DEBUG
			WriteLine(sw.Elapsed);
#endif
		}

		static void Report(Solution sol)
		{
			var g = sol.BestGraph;
			WriteLine(sol.BestDiameter);

			var list = new List<int>();
			for (int i = 0; i < sol.n; i++)
			{
				list.Clear();
				list.AddRange(g[i]);
				while (list.Count < sol.m)
					list.Add(list[0]);
				WriteLine(string.Join(" ", list));
			}
		}

		public static void Test()
		{
			for (int i = 2; i < 1000; i++)
			for (int j = 2; j <= 5 && j <= i; j++)
			{
				var sol = new Solution(i, j);
				sol.Solve();
			}
		}

		public Solution(int n, int m, bool toplevel = false)
		{
			this.n = n;
			this.m = m;
			this.toplevel = toplevel;
		}

		#endregion

		public Graph Solve()
		{
			var g = SolveCore();
			if (g != null)
				RecordGraph(g);
			return BestGraph;
		}

		public Graph SolveCore()
		{
			if (m >= n - 1)
				return Graph.Complete(n);

			//if (m == 2)
			//	return Graph.CycleGraph(n, m, true);

			// Case Analysis
			if (cases.ContainsKey(n))
			{
				foreach (var c in cases[n])
				{
					if (m < c.Degree)
						continue;

					var g = c.Action();
					Debug.Assert(g.Diameter() == c.Diameter);

					if (m == c.Degree) return g;

					// Set the graph as an upper bound
					RecordGraph(g);
				}
			}

			// Divide and Conquer
			var sol = Recurse(n / 2, m - 1);
			// Diameter + 1, Degree + 1
			var g2 = Graph.Product(sol.BestGraph, Graph.K2);
			Debug.Assert(g2.Diameter() == sol.BestDiameter + 1);
			if (n % 2 == 0)
			{
				RecordGraph(g2);
				// We need to add one more node for the odd case
			}
			else if (n%3 == 0)
			{
				sol = Recurse(n / 3, m - 2);
				// Diameter + 1, Degree + 1
				g2 = Graph.Product(sol.BestGraph, Graph.Complete(3));
				RecordGraph(g2);
			}


			// Iterate through multiple random graphs
			var limit = toplevel ? RandomIterationsTop : RandomIterations;
			for (int i = 0; i < limit; i++)
				AddRandomGraphs();

			// null means we are unsure;
			return null;
		}

		void AddRandomGraphs()
		{
			Graph g;

			for (int i = 0; i < 2; i++)
			{
				g = Graph.CycleGraph(n, m, i % 2 == 1);
				g.ReduceDiameterToLogN();
				g.AddUniRandomEdges();
				RecordGraph(g);

				g = Graph.CycleGraph(n, m, i % 2 == 0);
				g.AddUniRandomEdges();
				RecordGraph(g);
			}

			g = Graph.RandomGraph2(n, m);
			g.AddBiRandomEdges();
			RecordGraph(g);

			g = Graph.RandomGraph2(n, m);
			g.ReduceDiameterToLogN();
			g.AddUniRandomEdges();
			RecordGraph(g);

			g = Graph.RandomGraph2(n, m);
			g.AddUniRandomEdges();
			RecordGraph(g);

			g = Graph.CrazyRandom(n,m);
			if (g != null) 
				RecordGraph(g);

		}


		public static Solution Recurse(int n, int m)
		{
			var tup = new Tuple<int, int>(n, m);
			if (memo.ContainsKey(tup))
				return memo[tup];
			var sol = new Solution(n / 2, 2);
			sol.Solve();
			memo[tup] = sol;
			return sol;
		}


		public void RecordGraph(Graph g)
		{
			var diameter = g.Diameter(BestDiameter);

			if (BestGraph == null || diameter < BestDiameter)
			{
				BestGraph = g;
				BestDiameter = diameter;
			}
		}

		#region Case Analysis 

		public static void Setup()
		{
			AddCase(18, 3, 3, () => Graph.Graph33(18));
			//AddCase(16, 4, 4, () => Graph.G8(2));
			AddCase(24, 5, 5, () => Graph.G8(3));
			AddCase(4, 2, 2, () => Graph.CubeGraph(2));
			//AddCase(16, 4, 4, () => Graph.CubeGraph(4));
			//AddCase(32, 5, 5, () => Graph.CubeGraph(5)); 4 is optimal

			// K3,3 = C6(1,3)  
			AddGraph(6, 3, 2, new[,] {{0, 1}, {0, 3}, {0, 5}, {1, 2}, {1, 4}, {2, 3}, {2, 5}, {3, 4}, {4, 5}});

			// C8(1,4)  
			AddGraph(8, 3, 2,
				new[,] {{0, 1}, {0, 4}, {0, 7}, {1, 2}, {1, 5}, {2, 3}, {2, 6}, {3, 4}, {3, 7}, {4, 5}, {5, 6}, {6, 7}});

			// P  
			AddGraph(10, 3, 2,
				new[,]
				{
					{0, 1}, {0, 4}, {0, 5}, {1, 2}, {1, 6}, {2, 3}, {2, 7}, {3, 4}, {3, 8}, {4, 9}, {5, 7}, {5, 8}, {6, 8}, {6, 9},
					{7, 9}
				});

			// C = Cml8(2:0,3)  
			//AddGraph(8, 3, 3, new[,] { { 0, 1 }, { 0, 3 }, { 0, 7 }, { 1, 2 }, { 1, 6 }, { 2, 3 }, { 2, 5 }, { 3, 4 }, { 4, 5 }, { 4, 7 }, { 5, 6 }, { 6, 7 } });

			// C10(1,5)  
			AddGraph(10, 3, 3,
				new[,]
				{
					{0, 1}, {0, 5}, {0, 9}, {1, 2}, {1, 6}, {2, 3}, {2, 7}, {3, 4}, {3, 8}, {4, 5}, {4, 9}, {5, 6}, {6, 7}, {7, 8},
					{8, 9}
				});

			// C12(1,6)  
			AddGraph(12, 3, 3,
				new[,]
				{
					{0, 1}, {0, 11}, {0, 6}, {1, 2}, {1, 7}, {2, 3}, {2, 8}, {3, 4}, {3, 9}, {4, 10}, {4, 5}, {5, 11}, {5, 6}, {6, 7},
					{7, 8}, {8, 9}, {9, 10}, {10, 11}
				});

			// PP7(1,3) K2  
			//AddGraph(14, 3, 3, new[,] { { 0, 1 }, { 0, 6 }, { 0, 7 }, { 1, 2 }, { 1, 8 }, { 2, 3 }, { 2, 9 }, { 3, 10 }, { 3, 4 }, { 4, 11 }, { 4, 5 }, { 5, 12 }, { 5, 6 }, { 6, 13 }, { 7, 10 }, { 7, 11 }, { 8, 11 }, { 8, 12 }, { 9, 12 }, { 9, 13 }, { 10, 13 } });

			// PP7(1,2) K2  
			//AddGraph(14, 3, 3, new[,] { { 0, 1 }, { 0, 6 }, { 0, 7 }, { 1, 2 }, { 1, 8 }, { 2, 3 }, { 2, 9 }, { 3, 10 }, { 3, 4 }, { 4, 11 }, { 4, 5 }, { 5, 12 }, { 5, 6 }, { 6, 13 }, { 7, 12 }, { 7, 9 }, { 8, 10 }, { 8, 13 }, { 9, 11 }, { 10, 12 }, { 11, 13 } });

			// Heawood  
			AddGraph(14, 3, 3,
				new[,]
				{
					{0, 1}, {0, 13}, {0, 5}, {1, 10}, {1, 2}, {2, 3}, {2, 7}, {3, 12}, {3, 4}, {4, 5}, {4, 9}, {5, 6}, {6, 11}, {6, 7},
					{7, 8}, {8, 13}, {8, 9}, {9, 10}, {10, 11}, {11, 12}, {12, 13}
				});

			// O = C6(1,2)  
			AddGraph(6, 4, 2,
				new[,] {{0, 1}, {0, 2}, {0, 4}, {0, 5}, {1, 2}, {1, 3}, {1, 5}, {2, 3}, {2, 4}, {3, 4}, {3, 5}, {4, 5}});

			// K4,4 = C8(1,3)  
			AddGraph(8, 4, 2,
				new[,]
				{
					{0, 1}, {0, 3}, {0, 5}, {0, 7}, {1, 2}, {1, 4}, {1, 6}, {2, 3}, {2, 5}, {2, 7}, {3, 4}, {3, 6}, {4, 5}, {4, 7},
					{5, 6}, {6, 7}
				});

			// K3 × K3  
			AddGraph(9, 4, 2,
				new[,]
				{
					{0, 1}, {0, 2}, {0, 3}, {0, 6}, {1, 2}, {1, 4}, {1, 7}, {2, 5}, {2, 8}, {3, 4}, {3, 5}, {3, 6}, {4, 5}, {4, 7},
					{5, 8}, {6, 7}, {6, 8}, {7, 8}
				});

			// C11(1,3)  
			AddGraph(11, 4, 2,
				new[,]
				{
					{0, 1}, {0, 10}, {0, 3}, {0, 8}, {1, 2}, {1, 4}, {1, 9}, {2, 10}, {2, 3}, {2, 5}, {3, 4}, {3, 6}, {4, 5}, {4, 7},
					{5, 6}, {5, 8}, {6, 7}, {6, 9}, {7, 10}, {7, 8}, {8, 9}, {9, 10}
				});

			// Cml15(5:1,4:2,5:3,7:4,4:4,7)  
			AddGraph(15, 4, 2,
				new[,]
				{
					{0, 1}, {0, 11}, {0, 14}, {0, 8}, {1, 2}, {1, 5}, {1, 9}, {2, 12}, {2, 3}, {2, 7}, {3, 10}, {3, 14}, {3, 4},
					{4, 11}, {4, 5}, {4, 8}, {5, 13}, {5, 6}, {6, 10}, {6, 14}, {6, 7}, {7, 12}, {7, 8}, {8, 9}, {9, 10}, {9, 13},
					{10, 11}, {11, 12}, {12, 13}, {13, 14}
				});

			// C14(1,7)  
			AddGraph(14, 3, 4,
				new[,]
				{
					{0, 1}, {0, 13}, {0, 7}, {1, 2}, {1, 8}, {2, 3}, {2, 9}, {3, 10}, {3, 4}, {4, 11}, {4, 5}, {5, 12}, {5, 6},
					{6, 13}, {6, 7}, {7, 8}, {8, 9}, {9, 10}, {10, 11}, {11, 12}, {12, 13}
				});

			// C16(1,8)  
			AddGraph(16, 3, 4,
				new[,]
				{
					{0, 1}, {0, 15}, {0, 8}, {1, 2}, {1, 9}, {2, 10}, {2, 3}, {3, 11}, {3, 4}, {4, 12}, {4, 5}, {5, 13}, {5, 6},
					{6, 14}, {6, 7}, {7, 15}, {7, 8}, {8, 9}, {9, 10}, {10, 11}, {11, 12}, {12, 13}, {13, 14}, {14, 15}
				});

			// PP11(1,3) K2  
			AddGraph(22, 3, 4,
				new[,]
				{
					{0, 1}, {0, 10}, {0, 11}, {1, 12}, {1, 2}, {2, 13}, {2, 3}, {3, 14}, {3, 4}, {4, 15}, {4, 5}, {5, 16}, {5, 6},
					{6, 17}, {6, 7}, {7, 18}, {7, 8}, {8, 19}, {8, 9}, {9, 10}, {9, 20}, {10, 21}, {11, 14}, {11, 19}, {12, 15},
					{12, 20}, {13, 16}, {13, 21}, {14, 17}, {15, 18}, {16, 19}, {17, 20}, {18, 21}
				});

			// McGee = Cml24(3:1,12:2,7:0,17)  
			AddGraph(24, 3, 4,
				new[,]
				{
					{0, 1}, {0, 17}, {0, 23}, {1, 13}, {1, 2}, {2, 3}, {2, 9}, {3, 20}, {3, 4}, {4, 16}, {4, 5}, {5, 12}, {5, 6},
					{6, 23}, {6, 7}, {7, 19}, {7, 8}, {8, 15}, {8, 9}, {9, 10}, {10, 11}, {10, 22}, {11, 12}, {11, 18}, {12, 13},
					{13, 14}, {14, 15}, {14, 21}, {15, 16}, {16, 17}, {17, 18}, {18, 19}, {19, 20}, {20, 21}, {21, 22}, {22, 23}
				});

			// Tutte-Coxeter = Cml30(6:1,17:2,21:3,7)  
			AddGraph(30, 3, 4,
				new[,]
				{
					{0, 1}, {0, 13}, {0, 29}, {1, 18}, {1, 2}, {2, 23}, {2, 3}, {3, 10}, {3, 4}, {4, 27}, {4, 5}, {5, 14}, {5, 6},
					{6, 19}, {6, 7}, {7, 24}, {7, 8}, {8, 29}, {8, 9}, {9, 10}, {9, 16}, {10, 11}, {11, 12}, {11, 20}, {12, 13},
					{12, 25}, {13, 14}, {14, 15}, {15, 16}, {15, 22}, {16, 17}, {17, 18}, {17, 26}, {18, 19}, {19, 20}, {20, 21},
					{21, 22}, {21, 28}, {22, 23}, {23, 24}, {24, 25}, {25, 26}, {26, 27}, {27, 28}, {28, 29}
				});

			// K2 × K3,3  
			AddGraph(12, 4, 3,
				new[,]
				{
					{0, 1}, {0, 10}, {0, 2}, {0, 6}, {1, 11}, {1, 3}, {1, 7}, {2, 3}, {2, 4}, {2, 8}, {3, 5}, {3, 9}, {4, 10}, {4, 5},
					{4, 6}, {5, 11}, {5, 7}, {6, 7}, {6, 8}, {7, 9}, {8, 10}, {8, 9}, {9, 11}, {10, 11}
				});

			// K2 × P  
			AddGraph(20, 4, 3,
				new[,]
				{
					{0, 1}, {0, 10}, {0, 2}, {0, 8}, {1, 11}, {1, 3}, {1, 9}, {2, 12}, {2, 3}, {2, 4}, {3, 13}, {3, 5}, {4, 14},
					{4, 5}, {4, 6}, {5, 15}, {5, 7}, {6, 16}, {6, 7}, {6, 8}, {7, 17}, {7, 9}, {8, 18}, {8, 9}, {9, 19}, {10, 11},
					{10, 14}, {10, 16}, {11, 15}, {11, 17}, {12, 13}, {12, 16}, {12, 18}, {13, 17}, {13, 19}, {14, 15}, {14, 18},
					{15, 19}, {16, 17}, {18, 19}
				});

			// Moebius D  
			//AddGraph(20, 4, 3, new[,] { { 0, 10 }, { 0, 16 }, { 0, 19 }, { 0, 4 }, { 1, 11 }, { 1, 19 }, { 1, 2 }, { 1, 3 }, { 2, 12 }, { 2, 18 }, { 2, 6 }, { 3, 13 }, { 3, 4 }, { 3, 5 }, { 4, 14 }, { 4, 8 }, { 5, 15 }, { 5, 6 }, { 5, 7 }, { 6, 10 }, { 6, 16 }, { 7, 17 }, { 7, 8 }, { 7, 9 }, { 8, 12 }, { 8, 18 }, { 9, 10 }, { 9, 11 }, { 9, 19 }, { 10, 14 }, { 11, 12 }, { 11, 13 }, { 12, 16 }, { 13, 14 }, { 13, 15 }, { 14, 18 }, { 15, 16 }, { 15, 17 }, { 17, 18 }, { 17, 19 } });

			// PP7(1,2,3) K3  
			AddGraph(21, 4, 3,
				new[,]
				{
					{0, 1}, {0, 14}, {0, 6}, {0, 7}, {1, 15}, {1, 2}, {1, 8}, {2, 16}, {2, 3}, {2, 9}, {3, 10}, {3, 17}, {3, 4},
					{4, 11}, {4, 18}, {4, 5}, {5, 12}, {5, 19}, {5, 6}, {6, 13}, {6, 20}, {7, 12}, {7, 14}, {7, 9}, {8, 10}, {8, 13},
					{8, 15}, {9, 11}, {9, 16}, {10, 12}, {10, 17}, {11, 13}, {11, 18}, {12, 19}, {13, 20}, {14, 17}, {14, 18}, {15, 18},
					{15, 19}, {16, 19}, {16, 20}, {17, 20}
				});

			// (4,6)-cage  
			AddGraph(26, 4, 3,
				new[,]
				{
					{0, 1}, {0, 17}, {0, 25}, {0, 5}, {1, 10}, {1, 2}, {1, 22}, {2, 19}, {2, 3}, {2, 7}, {3, 12}, {3, 24}, {3, 4},
					{4, 21}, {4, 5}, {4, 9}, {5, 14}, {5, 6}, {6, 11}, {6, 23}, {6, 7}, {7, 16}, {7, 8}, {8, 13}, {8, 25}, {8, 9},
					{9, 10}, {9, 18}, {10, 11}, {10, 15}, {11, 12}, {11, 20}, {12, 13}, {12, 17}, {13, 14}, {13, 22}, {14, 15},
					{14, 19}, {15, 16}, {15, 24}, {16, 17}, {16, 21}, {17, 18}, {18, 19}, {18, 23}, {19, 20}, {20, 21}, {20, 25},
					{21, 22}, {22, 23}, {23, 24}, {24, 25}
				});

			// PP7(1,2,3,2) C4  
			AddGraph(28, 4, 3,
				new[,]
				{
					{0, 1}, {0, 21}, {0, 6}, {0, 7}, {1, 2}, {1, 22}, {1, 8}, {2, 23}, {2, 3}, {2, 9}, {3, 10}, {3, 24}, {3, 4},
					{4, 11}, {4, 25}, {4, 5}, {5, 12}, {5, 26}, {5, 6}, {6, 13}, {6, 27}, {7, 12}, {7, 14}, {7, 9}, {8, 10}, {8, 13},
					{8, 15}, {9, 11}, {9, 16}, {10, 12}, {10, 17}, {11, 13}, {11, 18}, {12, 19}, {13, 20}, {14, 17}, {14, 18}, {14, 21},
					{15, 18}, {15, 19}, {15, 22}, {16, 19}, {16, 20}, {16, 23}, {17, 20}, {17, 24}, {18, 25}, {19, 26}, {20, 27},
					{21, 23}, {21, 26}, {22, 24}, {22, 27}, {23, 25}, {24, 26}, {25, 27}
				});

			// PP9(1,3) K2 U PP9(2,4) K2  
			AddGraph(18, 5, 2,
				new[,]
				{
					{0, 1}, {0, 2}, {0, 7}, {0, 8}, {0, 9}, {1, 10}, {1, 2}, {1, 3}, {1, 8}, {2, 11}, {2, 3}, {2, 4}, {3, 12}, {3, 4},
					{3, 5}, {4, 13}, {4, 5}, {4, 6}, {5, 14}, {5, 6}, {5, 7}, {6, 15}, {6, 7}, {6, 8}, {7, 16}, {7, 8}, {8, 17},
					{9, 12}, {9, 13}, {9, 14}, {9, 15}, {10, 13}, {10, 14}, {10, 15}, {10, 16}, {11, 14}, {11, 15}, {11, 16}, {11, 17},
					{12, 15}, {12, 16}, {12, 17}, {13, 16}, {13, 17}, {14, 17}
				});

			// D  
			AddGraph(20, 3, 5,
				new[,]
				{
					{0, 16}, {0, 19}, {0, 4}, {1, 19}, {1, 2}, {1, 3}, {2, 18}, {2, 6}, {3, 4}, {3, 5}, {4, 8}, {5, 6}, {5, 7},
					{6, 10}, {7, 8}, {7, 9}, {8, 12}, {9, 10}, {9, 11}, {10, 14}, {11, 12}, {11, 13}, {12, 16}, {13, 14}, {13, 15},
					{14, 18}, {15, 16}, {15, 17}, {17, 18}, {17, 19}
				});

			// C × K2  
			AddGraph(16, 4, 4,
				new[,]
				{
					{0, 1}, {0, 3}, {0, 7}, {0, 8}, {1, 2}, {1, 6}, {1, 9}, {2, 10}, {2, 3}, {2, 5}, {3, 11}, {3, 4}, {4, 12}, {4, 5},
					{4, 7}, {5, 13}, {5, 6}, {6, 14}, {6, 7}, {7, 15}, {8, 11}, {8, 15}, {8, 9}, {9, 10}, {9, 14}, {10, 11}, {10, 13},
					{11, 12}, {12, 13}, {12, 15}, {13, 14}, {14, 15}
				});

			// C5 × C5  
			AddGraph(25, 4, 4,
				new[,]
				{
					{0, 1}, {0, 20}, {0, 4}, {0, 5}, {1, 2}, {1, 21}, {1, 6}, {2, 22}, {2, 3}, {2, 7}, {3, 23}, {3, 4}, {3, 8},
					{4, 24}, {4, 9}, {5, 10}, {5, 6}, {5, 9}, {6, 11}, {6, 7}, {7, 12}, {7, 8}, {8, 13}, {8, 9}, {9, 14}, {10, 11},
					{10, 14}, {10, 15}, {11, 12}, {11, 16}, {12, 13}, {12, 17}, {13, 14}, {13, 18}, {14, 19}, {15, 16}, {15, 19},
					{15, 20}, {16, 17}, {16, 21}, {17, 18}, {17, 22}, {18, 19}, {18, 23}, {19, 24}, {20, 21}, {20, 24}, {21, 22},
					{22, 23}, {23, 24}
				});

			// PP7(1,3) K2 × K2  
			//AddGraph(28, 4, 4, new[,] { { 0, 1 }, { 0, 14 }, { 0, 6 }, { 0, 7 }, { 1, 15 }, { 1, 2 }, { 1, 8 }, { 2, 16 }, { 2, 3 }, { 2, 9 }, { 3, 10 }, { 3, 17 }, { 3, 4 }, { 4, 11 }, { 4, 18 }, { 4, 5 }, { 5, 12 }, { 5, 19 }, { 5, 6 }, { 6, 13 }, { 6, 20 }, { 7, 10 }, { 7, 11 }, { 7, 21 }, { 8, 11 }, { 8, 12 }, { 8, 22 }, { 9, 12 }, { 9, 13 }, { 9, 23 }, { 10, 13 }, { 10, 24 }, { 11, 25 }, { 12, 26 }, { 13, 27 }, { 14, 15 }, { 14, 20 }, { 14, 21 }, { 15, 16 }, { 15, 22 }, { 16, 17 }, { 16, 23 }, { 17, 18 }, { 17, 24 }, { 18, 19 }, { 18, 25 }, { 19, 20 }, { 19, 26 }, { 20, 27 }, { 21, 24 }, { 21, 25 }, { 22, 25 }, { 22, 26 }, { 23, 26 }, { 23, 27 }, { 24, 27 } });

			// Heawood × K2  
			AddGraph(28, 4, 4,
				new[,]
				{
					{0, 1}, {0, 13}, {0, 14}, {0, 5}, {1, 10}, {1, 15}, {1, 2}, {2, 16}, {2, 3}, {2, 7}, {3, 12}, {3, 17}, {3, 4},
					{4, 18}, {4, 5}, {4, 9}, {5, 19}, {5, 6}, {6, 11}, {6, 20}, {6, 7}, {7, 21}, {7, 8}, {8, 13}, {8, 22}, {8, 9},
					{9, 10}, {9, 23}, {10, 11}, {10, 24}, {11, 12}, {11, 25}, {12, 13}, {12, 26}, {13, 27}, {14, 15}, {14, 19},
					{14, 27}, {15, 16}, {15, 24}, {16, 17}, {16, 21}, {17, 18}, {17, 26}, {18, 19}, {18, 23}, {19, 20}, {20, 21},
					{20, 25}, {21, 22}, {22, 23}, {22, 27}, {23, 24}, {24, 25}, {25, 26}, {26, 27}
				});

			// PP9(1,2,3,4) C4  
			AddGraph(36, 4, 4,
				new[,]
				{
					{0, 1}, {0, 27}, {0, 8}, {0, 9}, {1, 10}, {1, 2}, {1, 28}, {2, 11}, {2, 29}, {2, 3}, {3, 12}, {3, 30}, {3, 4},
					{4, 13}, {4, 31}, {4, 5}, {5, 14}, {5, 32}, {5, 6}, {6, 15}, {6, 33}, {6, 7}, {7, 16}, {7, 34}, {7, 8}, {8, 17},
					{8, 35}, {9, 11}, {9, 16}, {9, 18}, {10, 12}, {10, 17}, {10, 19}, {11, 13}, {11, 20}, {12, 14}, {12, 21}, {13, 15},
					{13, 22}, {14, 16}, {14, 23}, {15, 17}, {15, 24}, {16, 25}, {17, 26}, {18, 21}, {18, 24}, {18, 27}, {19, 22},
					{19, 25}, {19, 28}, {20, 23}, {20, 26}, {20, 29}, {21, 24}, {21, 30}, {22, 25}, {22, 31}, {23, 26}, {23, 32},
					{24, 33}, {25, 34}, {26, 35}, {27, 31}, {27, 32}, {28, 32}, {28, 33}, {29, 33}, {29, 34}, {30, 34}, {30, 35},
					{31, 35}
				});

			// PP9(1,2,3,4,2) C5  
			AddGraph(45, 4, 4,
				new[,]
				{
					{0, 1}, {0, 36}, {0, 8}, {0, 9}, {1, 10}, {1, 2}, {1, 37}, {2, 11}, {2, 3}, {2, 38}, {3, 12}, {3, 39}, {3, 4},
					{4, 13}, {4, 40}, {4, 5}, {5, 14}, {5, 41}, {5, 6}, {6, 15}, {6, 42}, {6, 7}, {7, 16}, {7, 43}, {7, 8}, {8, 17},
					{8, 44}, {9, 11}, {9, 16}, {9, 18}, {10, 12}, {10, 17}, {10, 19}, {11, 13}, {11, 20}, {12, 14}, {12, 21}, {13, 15},
					{13, 22}, {14, 16}, {14, 23}, {15, 17}, {15, 24}, {16, 25}, {17, 26}, {18, 21}, {18, 24}, {18, 27}, {19, 22},
					{19, 25}, {19, 28}, {20, 23}, {20, 26}, {20, 29}, {21, 24}, {21, 30}, {22, 25}, {22, 31}, {23, 26}, {23, 32},
					{24, 33}, {25, 34}, {26, 35}, {27, 31}, {27, 32}, {27, 36}, {28, 32}, {28, 33}, {28, 37}, {29, 33}, {29, 34},
					{29, 38}, {30, 34}, {30, 35}, {30, 39}, {31, 35}, {31, 40}, {32, 41}, {33, 42}, {34, 43}, {35, 44}, {36, 38},
					{36, 43}, {37, 39}, {37, 44}, {38, 40}, {39, 41}, {40, 42}, {41, 43}, {42, 44}
				});

			// PP11(1,2,4,3,5) C5  
			AddGraph(55, 4, 4,
				new[,]
				{
					{0, 1}, {0, 10}, {0, 11}, {0, 44}, {1, 12}, {1, 2}, {1, 45}, {2, 13}, {2, 3}, {2, 46}, {3, 14}, {3, 4}, {3, 47},
					{4, 15}, {4, 48}, {4, 5}, {5, 16}, {5, 49}, {5, 6}, {6, 17}, {6, 50}, {6, 7}, {7, 18}, {7, 51}, {7, 8}, {8, 19},
					{8, 52}, {8, 9}, {9, 10}, {9, 20}, {9, 53}, {10, 21}, {10, 54}, {11, 13}, {11, 20}, {11, 22}, {12, 14}, {12, 21},
					{12, 23}, {13, 15}, {13, 24}, {14, 16}, {14, 25}, {15, 17}, {15, 26}, {16, 18}, {16, 27}, {17, 19}, {17, 28},
					{18, 20}, {18, 29}, {19, 21}, {19, 30}, {20, 31}, {21, 32}, {22, 26}, {22, 29}, {22, 33}, {23, 27}, {23, 30},
					{23, 34}, {24, 28}, {24, 31}, {24, 35}, {25, 29}, {25, 32}, {25, 36}, {26, 30}, {26, 37}, {27, 31}, {27, 38},
					{28, 32}, {28, 39}, {29, 40}, {30, 41}, {31, 42}, {32, 43}, {33, 36}, {33, 41}, {33, 44}, {34, 37}, {34, 42},
					{34, 45}, {35, 38}, {35, 43}, {35, 46}, {36, 39}, {36, 47}, {37, 40}, {37, 48}, {38, 41}, {38, 49}, {39, 42},
					{39, 50}, {40, 43}, {40, 51}, {41, 52}, {42, 53}, {43, 54}, {44, 49}, {44, 50}, {45, 50}, {45, 51}, {46, 51},
					{46, 52}, {47, 52}, {47, 53}, {48, 53}, {48, 54}, {49, 54}
				});

			// PP13(1,5,4,2,3,6) C6  
			AddGraph(78, 4, 4,
				new[,]
				{
					{0, 1}, {0, 12}, {0, 13}, {0, 65}, {1, 14}, {1, 2}, {1, 66}, {2, 15}, {2, 3}, {2, 67}, {3, 16}, {3, 4}, {3, 68},
					{4, 17}, {4, 5}, {4, 69}, {5, 18}, {5, 6}, {5, 70}, {6, 19}, {6, 7}, {6, 71}, {7, 20}, {7, 72}, {7, 8}, {8, 21},
					{8, 73}, {8, 9}, {9, 10}, {9, 22}, {9, 74}, {10, 11}, {10, 23}, {10, 75}, {11, 12}, {11, 24}, {11, 76}, {12, 25},
					{12, 77}, {13, 18}, {13, 21}, {13, 26}, {14, 19}, {14, 22}, {14, 27}, {15, 20}, {15, 23}, {15, 28}, {16, 21},
					{16, 24}, {16, 29}, {17, 22}, {17, 25}, {17, 30}, {18, 23}, {18, 31}, {19, 24}, {19, 32}, {20, 25}, {20, 33},
					{21, 34}, {22, 35}, {23, 36}, {24, 37}, {25, 38}, {26, 30}, {26, 35}, {26, 39}, {27, 31}, {27, 36}, {27, 40},
					{28, 32}, {28, 37}, {28, 41}, {29, 33}, {29, 38}, {29, 42}, {30, 34}, {30, 43}, {31, 35}, {31, 44}, {32, 36},
					{32, 45}, {33, 37}, {33, 46}, {34, 38}, {34, 47}, {35, 48}, {36, 49}, {37, 50}, {38, 51}, {39, 41}, {39, 50},
					{39, 52}, {40, 42}, {40, 51}, {40, 53}, {41, 43}, {41, 54}, {42, 44}, {42, 55}, {43, 45}, {43, 56}, {44, 46},
					{44, 57}, {45, 47}, {45, 58}, {46, 48}, {46, 59}, {47, 49}, {47, 60}, {48, 50}, {48, 61}, {49, 51}, {49, 62},
					{50, 63}, {51, 64}, {52, 55}, {52, 62}, {52, 65}, {53, 56}, {53, 63}, {53, 66}, {54, 57}, {54, 64}, {54, 67},
					{55, 58}, {55, 68}, {56, 59}, {56, 69}, {57, 60}, {57, 70}, {58, 61}, {58, 71}, {59, 62}, {59, 72}, {60, 63},
					{60, 73}, {61, 64}, {61, 74}, {62, 75}, {63, 76}, {64, 77}, {65, 71}, {65, 72}, {66, 72}, {66, 73}, {67, 73},
					{67, 74}, {68, 74}, {68, 75}, {69, 75}, {69, 76}, {70, 76}, {70, 77}, {71, 77}
				});

			// Robertson-Wegner  
			AddGraph(30, 5, 3,
				new[,]
				{
					{0, 10}, {0, 15}, {0, 19}, {0, 2}, {0, 29}, {1, 12}, {1, 2}, {1, 21}, {1, 28}, {1, 4}, {2, 26}, {2, 3}, {2, 8},
					{3, 13}, {3, 18}, {3, 22}, {3, 5}, {4, 15}, {4, 24}, {4, 5}, {4, 7}, {5, 11}, {5, 29}, {5, 6}, {6, 16}, {6, 21},
					{6, 25}, {6, 8}, {7, 10}, {7, 18}, {7, 27}, {7, 8}, {8, 14}, {8, 9}, {9, 11}, {9, 19}, {9, 24}, {9, 28}, {10, 11},
					{10, 13}, {10, 21}, {11, 12}, {11, 17}, {12, 14}, {12, 22}, {12, 27}, {13, 14}, {13, 16}, {13, 24}, {14, 15},
					{14, 20}, {15, 17}, {15, 25}, {16, 17}, {16, 19}, {16, 27}, {17, 18}, {17, 23}, {18, 20}, {18, 28}, {19, 20},
					{19, 22}, {20, 21}, {20, 26}, {21, 23}, {22, 23}, {22, 25}, {23, 24}, {23, 29}, {24, 26}, {25, 26}, {25, 28},
					{26, 27}, {27, 29}, {28, 29}
				});

			// PP9(1,2,3,4) K4  
			AddGraph(36, 5, 3,
				new[,]
				{
					{0, 1}, {0, 18}, {0, 27}, {0, 8}, {0, 9}, {1, 10}, {1, 19}, {1, 2}, {1, 28}, {2, 11}, {2, 20}, {2, 29}, {2, 3},
					{3, 12}, {3, 21}, {3, 30}, {3, 4}, {4, 13}, {4, 22}, {4, 31}, {4, 5}, {5, 14}, {5, 23}, {5, 32}, {5, 6}, {6, 15},
					{6, 24}, {6, 33}, {6, 7}, {7, 16}, {7, 25}, {7, 34}, {7, 8}, {8, 17}, {8, 26}, {8, 35}, {9, 11}, {9, 16}, {9, 18},
					{9, 27}, {10, 12}, {10, 17}, {10, 19}, {10, 28}, {11, 13}, {11, 20}, {11, 29}, {12, 14}, {12, 21}, {12, 30},
					{13, 15}, {13, 22}, {13, 31}, {14, 16}, {14, 23}, {14, 32}, {15, 17}, {15, 24}, {15, 33}, {16, 25}, {16, 34},
					{17, 26}, {17, 35}, {18, 21}, {18, 24}, {18, 27}, {19, 22}, {19, 25}, {19, 28}, {20, 23}, {20, 26}, {20, 29},
					{21, 24}, {21, 30}, {22, 25}, {22, 31}, {23, 26}, {23, 32}, {24, 33}, {25, 34}, {26, 35}, {27, 31}, {27, 32},
					{28, 32}, {28, 33}, {29, 33}, {29, 34}, {30, 34}, {30, 35}, {31, 35}
				});

			// PP7(1,2,3,2) C4 × K2  
			AddGraph(56, 5, 4,
				new[,]
				{
					{0, 1}, {0, 21}, {0, 28}, {0, 6}, {0, 7}, {1, 2}, {1, 22}, {1, 29}, {1, 8}, {2, 23}, {2, 3}, {2, 30}, {2, 9},
					{3, 10}, {3, 24}, {3, 31}, {3, 4}, {4, 11}, {4, 25}, {4, 32}, {4, 5}, {5, 12}, {5, 26}, {5, 33}, {5, 6}, {6, 13},
					{6, 27}, {6, 34}, {7, 12}, {7, 14}, {7, 35}, {7, 9}, {8, 10}, {8, 13}, {8, 15}, {8, 36}, {9, 11}, {9, 16}, {9, 37},
					{10, 12}, {10, 17}, {10, 38}, {11, 13}, {11, 18}, {11, 39}, {12, 19}, {12, 40}, {13, 20}, {13, 41}, {14, 17},
					{14, 18}, {14, 21}, {14, 42}, {15, 18}, {15, 19}, {15, 22}, {15, 43}, {16, 19}, {16, 20}, {16, 23}, {16, 44},
					{17, 20}, {17, 24}, {17, 45}, {18, 25}, {18, 46}, {19, 26}, {19, 47}, {20, 27}, {20, 48}, {21, 23}, {21, 26},
					{21, 49}, {22, 24}, {22, 27}, {22, 50}, {23, 25}, {23, 51}, {24, 26}, {24, 52}, {25, 27}, {25, 53}, {26, 54},
					{27, 55}, {28, 29}, {28, 34}, {28, 35}, {28, 49}, {29, 30}, {29, 36}, {29, 50}, {30, 31}, {30, 37}, {30, 51},
					{31, 32}, {31, 38}, {31, 52}, {32, 33}, {32, 39}, {32, 53}, {33, 34}, {33, 40}, {33, 54}, {34, 41}, {34, 55},
					{35, 37}, {35, 40}, {35, 42}, {36, 38}, {36, 41}, {36, 43}, {37, 39}, {37, 44}, {38, 40}, {38, 45}, {39, 41},
					{39, 46}, {40, 47}, {41, 48}, {42, 45}, {42, 46}, {42, 49}, {43, 46}, {43, 47}, {43, 50}, {44, 47}, {44, 48},
					{44, 51}, {45, 48}, {45, 52}, {46, 53}, {47, 54}, {48, 55}, {49, 51}, {49, 54}, {50, 52}, {50, 55}, {51, 53},
					{52, 54}, {53, 55}
				});

		}

		public static void AddGraph(int n, int deg, int diam, int[,] array)
		{
			AddCase(new Case
			{
				N = n,
				Degree = deg,
				Diameter = diam,
				Action = () =>
				{
					var g = new Graph(n, deg);
					int len = array.GetLength(0);
					for (int i = 0; i < len; i++)
						g.AddBidirectionalEdge(array[i, 0], array[i, 1]);
					return g;
				},
			});
		}

		public static void AddCase(int n, int m, int diam, Func<Graph> action)
		{
			AddCase(new Case
			{
				N = n,
				Degree = m,
				Diameter = diam,
				Action = action,
			});
		}

		public static void AddCase(Case c)
		{
			if (!cases.ContainsKey(c.N))
				cases[c.N] = new List<Case>();
			cases[c.N].Add(c);
		}

		public class Case
		{
			public int N;
			public int Degree;
			public int Diameter;
			public Func<Graph> Action;
		}

		#endregion


		public class Graph
		{
			int m;
			int n;
			public List<int>[] Nodes;

			public Graph(int n, int m)
			{
				this.n = n;
				this.m = m;
				Nodes = new List<int>[n];
				for (int i = 0; i < n; i++)
					Nodes[i] = new List<int>(m);
			}


			public List<int> this[int index] => Nodes[index];

			public static Graph CubeGraph(int m)
			{
				var n = 1 << m;
				var g = new Graph(n, m);

				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
						g[i].Add(i ^ (1 << j));
				}
				return g;
			}


			public static Graph RandomGraph2(int n, int degree)
			{
				var g = new Graph(n, degree);

				for (int i = 1; i < n; i++)
				{
					var j = random.Next(0, i);
					if (g.Nodes[j].Count >= degree)
					{
						i--;
						continue;
					}
					g.AddBidirectionalEdge(i, j);
				}
				return g;
			}

			public static Graph CrazyRandom(int n, int degree)
			{
				var list = new List<int>(n*degree*2);

				for (int i=0; i<n; i++)
				for (int j = 0; j < degree*2; j++)
					list.Add(i);

				var g = new Graph(n, degree);
				int timer = list.Count * 2;
				while (list.Count>0 && timer-->0)
				{
					int i = random.Next(0, list.Count);
					int j = (i+random.Next(0, list.Count-1)+1) % list.Count;

					if (list[i] == list[j])
						continue;

					g[list[i]].Add(list[j]);
					var v1 = list[list.Count-1];
					var v2 = list[list.Count - 2];
					list[i] = v1;
					list[j] = v2;
					list.RemoveRange(list.Count-2, 2);
				}

				if (timer < 0) return null;
				var tarjan = new Tarjan(g.Nodes);
				if (!tarjan.RunScc()) return null;

				return g;
			}

			public void AddUniRandomEdges()
			{
				// Add random edges 
				for (int i = 0; i < n; i++)
				{
					var list = Nodes[i];
					while (list.Count < m)
					{
						// Excess edges
						if (list.Count >= n - 1)
						{

							list.Add((i + random.Next(1, n)) % n);
							continue;
						}

						int attach = random.Next(0, n - 1 - list.Count);
						int attach2 = attach;
						if (attach >= i) attach2++;
						foreach (var v in list)
							if (attach >= v) attach2++;

						if (attach2 > Nodes.Length)
							attach2 = Nodes.Length - 1;
						list.Add(attach2);
					}
				}
			}

			public void AddBiRandomEdges()
			{
				// Add random edges 
				for (int i = 0; i + 1 < n; i++)
				{
					var list = Nodes[i];
					int timer = 2 * m;
					while (list.Count < m && timer-- > 0)
					{
						int attach = random.Next(i + 1, n);
						if (Nodes[attach].Count >= m || list.Contains(attach))
							continue;
						AddBidirectionalEdge(i, attach);
					}
				}

				AddUniRandomEdges();
			}

			public void ReduceDiameterToLogN()
			{
				int head = 0;
				int[] next = new int[n];

				next[n - 1] = -1;
				for (int i = 0; i + 1 < n; i++)
					next[i] = i + 1;

				while (head != -1)
				{
					var cur = head;
					var prev = -1;
					int headorig = head;
					while (cur != -1)
					{
						int tmp = cur;
						cur = next[cur] != -1 ? next[next[cur]] : -1;

						if (prev == -1)
							head = next[tmp];
						else
							next[prev] = next[tmp];

						next[tmp] = cur;
						Nodes[tmp].Add(cur != -1 ? cur : head != -1 && Nodes[tmp].Contains(headorig) ? head : headorig);
					}
				}
			}

			public int GreedyDiameter(int abortBound = int.MaxValue)
			{
				var visited = new BitArray(Nodes.Length);
				var queue = new Queue<int>();
				int maxDiameter = 0;
				var searched = new List<int>();
				int cur = 0;

				while (cur != -1 && !searched.Contains(cur))
				{
					int dist = 0;
					int last = -1;
					queue.Enqueue(cur);
					visited[cur] = true;
					searched.Add(cur);

					while (queue.Count > 0)
					{
						int count = queue.Count;
						for (int j = 0; j < count; j++)
						{
							last = queue.Dequeue();
							foreach (var child in Nodes[last])
							{
								if (visited[child]) continue;
								visited[child] = true;
								queue.Enqueue(child);
							}
						}

						dist++;
						if (dist > maxDiameter)
						{
							maxDiameter = dist;
							if (maxDiameter >= abortBound)
								return maxDiameter;
						}
					}

					cur = last;
					visited.SetAll(false);
				}

				return maxDiameter;
			}



			public int Diameter(int abortBound = int.MaxValue)
			{
				// Could use bitarray
				var visited = new int[Nodes.Length];
				var queue = new Queue<int>();
				int maxDiameter = 0;

				for (int i = 0; i < n; i++)
				{
					Array.Clear(visited, 0, visited.Length);
					queue.Enqueue(i);
					visited[i] = 1;
					while (queue.Count > 0)
					{
						var pop = queue.Dequeue();
						var popdist = visited[pop];
						foreach (var child in Nodes[pop])
						{
							if (visited[child] != 0) continue;
							visited[child] = popdist + 1;
							queue.Enqueue(child);
							if (popdist > maxDiameter)
							{
								maxDiameter = popdist;
								if (maxDiameter >= abortBound)
									return maxDiameter;
							}
						}
					}
				}

				return maxDiameter;
			}

			//http://research.nii.ac.jp/graphgolf/2015/candar15/graphgolf2015-mizuno.pdf


			public void AddBidirectionalEdge(int i, int j)
			{
				if (i == j) return;
				if (!Nodes[i].Contains(j)) Nodes[i].Add(j);
				if (!Nodes[j].Contains(i)) Nodes[j].Add(i);
			}

			public static readonly Graph K2 = Complete(2);

			public static Graph Complete(int n)
			{
				// Degree n-1
				// Diameter 1
				var g = new Graph(n, n - 1);
				for (int i = 0; i < n; i++)
				for (int j = i + 1; j < n; j++)
					g.AddBidirectionalEdge(i, j);
				return g;
			}

			public static Graph Graph33(int n)
			{
				// Degree and diameter 3
				int[] array = null;
				switch (n)
				{
					case 8:
						array = new[] {6, 3, 4, 1, 2, 7, 0, 5};
						break;
					case 10:
						array = new[] {7, 6, 5, 9, 8, 2, 1, 0, 3, 4};
						break;
					case 12:
						array = new[] {10, 3, 8, 1, 6, 11, 4, 9, 2, 7, 0, 5};
						break;
					case 14:
						array = new[]
						{
							5, 10, 7, 12, 9, 0, 11,
							2, 13, 4, 1, 6, 3, 8
						};
						break;
					case 18:
						array = new[]
						{
							14, 5, 9, 16, 12, 1,
							11, 15, 13, 2, 17, 6,
							4, 8, 0, 7, 3, 10,
						};
						break;
				}

				if (array == null) return null;
				var g = CycleGraph(n, 3, true);
				for (int i = 0; i < array.Length; i++)
					g.AddBidirectionalEdge(i, array[i]);
				return g;
			}

			public static Graph CycleGraph(int n, int m = 1, bool bidrectional = false)
			{
				var g = new Graph(n, m);
				g[n - 1].Add(0);
				for (int i = 1; i < n; i++)
					g[i - 1].Add(i);

				if (bidrectional)
				{
					g[0].Add(n - 1);
					for (int i = 1; i < n; i++)
						g[i].Add(i - 1);
				}
				return g;
			}

			public static Graph G8(int k)
			{
				// New order = 8 * k
				// New degree = k + 2
				// Diameter = 2

				var g8 = new Graph(8 * k, k + 2);

				for (int i = 0; i < k; i++)
				{
					g8.AddBidirectionalEdge(i * 8 + 0, i * 8 + 2);
					g8.AddBidirectionalEdge(i * 8 + 0, i * 8 + 3);
					g8.AddBidirectionalEdge(i * 8 + 0, i * 8 + 4);
					g8.AddBidirectionalEdge(i * 8 + 1, i * 8 + 2);
					g8.AddBidirectionalEdge(i * 8 + 1, i * 8 + 3);
					g8.AddBidirectionalEdge(i * 8 + 1, i * 8 + 4);
					g8.AddBidirectionalEdge(i * 8 + 2, i * 8 + 5);
					g8.AddBidirectionalEdge(i * 8 + 3, i * 8 + 6);
					g8.AddBidirectionalEdge(i * 8 + 4, i * 8 + 7);
					g8.AddBidirectionalEdge(i * 8 + 5, i * 8 + 6);
					g8.AddBidirectionalEdge(i * 8 + 5, i * 8 + 7);
					g8.AddBidirectionalEdge(i * 8 + 6, i * 8 + 7);
				}

				for (int i = 0; i < k; i++)
				for (int j = 0; j < k; j++)
				{
					if (i == j) continue;
					g8.AddBidirectionalEdge(i * 8 + 0, j * 8 + 1);
					g8.AddBidirectionalEdge(i * 8 + 1, j * 8 + 0);
					g8.AddBidirectionalEdge(i * 8 + 2, j * 8 + 6);
					g8.AddBidirectionalEdge(i * 8 + 3, j * 8 + 7);
					g8.AddBidirectionalEdge(i * 8 + 4, j * 8 + 5);
					g8.AddBidirectionalEdge(i * 8 + 5, j * 8 + 4);
					g8.AddBidirectionalEdge(i * 8 + 6, j * 8 + 2);
					g8.AddBidirectionalEdge(i * 8 + 7, j * 8 + 3);
				}

				return g8;
			}

			public static Graph Product(Graph g1, Graph g2, bool strong = false)
			{
				var g = new Graph(g1.n * g2.n, g1.m + g2.m + (strong ? g1.m * g2.m : 0));
				var f = g2.n;

				for (int i = 0; i < g1.n; i++)
				{
					var iedges = g1[i];
					for (int j = 0; j < g2.n; j++)
					{
						var jedges = g2[j];

						foreach (var e1 in iedges)
							g[i * f + j].Add(e1 * f + j);

						foreach (var e2 in jedges)
							g[i * f + j].Add(i * f + e2);

						if (strong)
						{
							foreach (var e1 in iedges)
							foreach (var e2 in jedges)
								g[i * f + j].Add(e1 * f + e2);
						}
					}
				}

				// Preserves diameter
				// Order(g) = Order(g1) * Order(g2)

				// Weak
				// Degree(g) = Degree(g1)+Degree(g2)
				// Diameter(g) = Diameter(g1) + Diameter(g2)

				// Strong
				// if (strong) Degree(g) = Degree(g1)*Degree(g2) + Degree(g1) + Degree(g2)
				// Diameter(g) = Max(Diameter(g1), Diameter(g2))


				// G(n/2,2) * C(2) -> G(n,5)   // C(2) diameter=1
				// G(n/3,2) * C(3) -> G(n/3,5) // C(2) diameter=2 
				// Question: Is C(3) degree 1
				return g;
			}
		}




#if DEBUG
		public static bool Verbose = true;
#else
		public static bool Verbose = false;
#endif
	}

	public class TimestampedArray<T>
	{
		public T[] Array;
		public int[] TimeStamp;
		public int Time;
		public T DefaultValue;

		public TimestampedArray(int size, T defaultValue = default(T))
		{
			Array = new T[size];
			TimeStamp = new int[size];
			DefaultValue = defaultValue;
		}

		public T this[int x]
		{
			get { return TimeStamp[x] >= Time ? Array[x] : DefaultValue; }
			set
			{
				Array[x] = value;
				TimeStamp[x] = Time;
			}
		}

		public bool ContainsKey(int x)
		{
			return TimeStamp[x] >= Time;
		}

		public void InitializeAll()
		{
			for (int i = 0; i < Array.Length; i++)
			{
				if (TimeStamp[i] > Time) continue;
				Array[i] = DefaultValue;
				TimeStamp[i] = Time;
			}
		}

		public void Clear()
		{
			Time++;
		}
	}

	public static class HackerRankUtils
	{

		#region Reporting Answer

		static volatile bool _reported;
		static System.Threading.Timer _timer;
		static Func<bool> _timerAction;

		public static void LaunchTimer(Func<bool> action, long ms = 2800)
		{
			_timerAction = action;
			_timer = new System.Threading.Timer(
				delegate
				{
#if !DEBUG
					Report();
				if (_reported)
					Environment.Exit(0);
#endif
				}, null, ms, 0);
		}

		public static void Report()
		{
			if (_reported) return;
			_reported = true;
			_reported = _timerAction();
		}

		[Conditional("DEBUG")]
		public static void Trace(string s)
		{
			Console.WriteLine(s);
		}

		#endregion


		public static int TimeLimit = 2000;
		public static int RandomIterations = 20;
		public static int RandomIterationsTop = RandomIterations + 20;
	}
}
