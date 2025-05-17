namespace HackerRank.WorldCodesprint8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	class SamsPuzzle
	{

		static void Main(string[] args)
		{
			LaunchTimer();
			int n = int.Parse(Console.ReadLine());
			var puzzle = Enumerable.Range(0, n).Select(x => Console.ReadLine().Split().Select(int.Parse).ToArray()).ToArray();
			var grid = new Grid(puzzle);
			var moves = Solve(puzzle);
			Report(moves.ToList());
		}

		//static int[][] puzzle;
		private static bool movesReported;
		private static Move bestMoves;
		private static System.Threading.Timer timer;

		public static void LaunchTimer()
		{
			// Use a timer to quit the search
			timer = new System.Threading.Timer(
				delegate
				{
					if (bestMoves != null)
					{
						Report(bestMoves.ToList());
						Environment.Exit(0);
					}
				}, null, 2000, 0);
		}

		public static void Report(List<Move> moves)
		{
			movesReported = true;
			Console.WriteLine(moves.Count);
			foreach (var m in moves)
				Console.WriteLine(m);
		}

		public static Move Solve(int[][] puzzle)
		{
			var grid = new Grid(puzzle);

			var state = grid.lastMove;
			var states = new List<Move>();

			for (int j = 0; j < 500; j++)
			{
				for (int i = 0; i < 50; i++)
				{
					var prevState = grid.lastMove;

					DivideAndConquer(grid, 0, 0, grid.n);

					// If our algorithm no longer makes any more rotations
					// or we exceed the move depth

					if (grid.lastMove != null && grid.lastMove.depth > 500
						|| grid.lastMove == prevState)
					{

						ReportCandidate(grid);
					}


					ReportCandidate(grid);
					states.Add(grid.lastMove);

				}

				grid.Restore(state);
				var random = new Random();
				int size = grid.n;
				if (size < 3) break;
				for (int x = 0; x < 15; x++)
				{
					int k = random.Next(2, size);
					int r = random.Next(0, size - k);
					int c = random.Next(0, size - k);
					grid.Rotate(r, c, k);
					grid.UpdateGoodness();
				}

			}

			return bestMoves;
		}

		public static void DivideAndConquer(Grid g, int r, int c, int k)
		{
			// Divide and conquer smaller areas before tackling larger ones

			if (k == 1) return;

			var state1 = g.lastMove;

			RotateOptimally(g, r, c, k);

			if (k == 2)
				return;

			int halfk = (k + 1) / 2;
			int center = k - halfk;
			DivideAndConquer(g, r, c, halfk);
			DivideAndConquer(g, r + center, c + center, halfk);
			DivideAndConquer(g, r + center, c, halfk);
			DivideAndConquer(g, r, c + center, halfk);


			var oldState = g.lastMove;
			var bestState = g.lastMove;

			int left = r + 1;
			int right = r + k - 2;
			int top = c + 1;
			while (left < right)
			{
				RotateOptimally(g, left, top, right - left + 1);
				//int goodness = g.UpdateGoodness();
				//if (goodness > oldState.goodness)
				//    bestState = g.lastMove;
				//g.Restore(oldState);

				left++;
				right--;
				top++;
			}

			//g.Restore(bestState);
			/*
						var state3 = g.lastMove;
						if (state1 != null && state3.goodness < state1.goodness)
							g.Restore(state1);
			*/
			if (k > 4)
				ReportCandidate(g);
		}

		public static List<Move> EnumerateMoves(int r, int c, int size)
		{
			int count = 0;
			for (int k = size - 1; k >= 2; k--)
				for (int i = r + size - k; i >= r; i--)
					count += size - k + 1;

			var list = new List<Move>(count);
			for (int k = size - 1; k >= 2; k--)
				for (int i = r + size - k; i >= r; i--)
					for (int j = c + size - k; j >= c; j--)
						list.Add(new Move(i, j, k));
			return list;
		}

		public static bool RotateOptimally(Grid g, int r, int c, int k)
		{
			int goodnessv = 0;
			int goodnessh = 0;

			if (k <= 2)
			{
				if (k < 2) return false;
				if (g[r, c] < g[r + 1, c]) goodnessv++;
				if (g[r, c + 1] < g[r + 1, c + 1]) goodnessv++;
				if (g[r, c] < g[r, c + 1]) goodnessh++;
				if (g[r + 1, c] < g[r + 1, c + 1]) goodnessh++;
			}
			else
			{
				g.GetGoodness(r, c, k, out goodnessh, out goodnessv);
			}

			int bestAmount = BestRotation(goodnessh, goodnessv, k);

			g.Rotate(r, c, k, bestAmount);

			return bestAmount > 0;
		}

		public static int BestRotation(int goodnessh, int goodnessv, int size)
		{
			int total = size * size * (size - 1);
			int goodness0 = goodnessh + goodnessv;
			int goodness1 = goodnessh + (total / 2 - goodnessv);
			int goodness2 = total - goodnessh - goodnessv;
			int goodness3 = goodnessv + (total / 2 - goodnessh);

			int goodness = goodness0;
			int rotations = 0;
			if (goodness < goodness1)
			{
				goodness = goodness1;
				rotations = 1;
			}
			if (goodness < goodness2)
			{
				goodness = goodness2;
				rotations = 2;
			}
			if (goodness < goodness3)
			{
				goodness = goodness3;
				rotations = 3;
			}
			return rotations;
		}

		public static void ReportCandidate(Grid grid)
		{
			grid.UpdateGoodness();
		}

		public class Move
		{
			public int r;
			public int c;
			public int k;
			public int depth;
			public Move prev;
			public int goodness = -1;

			public Move(int r, int c, int k, Move prev = null)
			{
				this.r = r;
				this.c = c;
				this.k = k;
				this.prev = prev;
				this.depth = prev == null ? 0 : prev.depth + 1;
			}

			// Used for optimized moves
			public void RotateCoords(int gridSize, int count = 1)
			{
				while (count-- > 0)
				{
					int origR = r + k - 1;
					int origC = c;
					this.c = (gridSize - 1) - origR;
					this.r = origC;
				}
			}

			public List<Move> ToList(Move upTo = null)
			{
				var list = new List<Move>();
				for (var current = this; current != upTo && current != null; current = current.prev)
				{
					if (current.k != 0)
						list.Add(current);
				}
				list.Reverse();
				return list;
			}

			public static Move CommonParent(Move move1, Move move2)
			{
				if (move1 == null || move2 == null)
					return null;

				if (move1.depth > move2.depth)
					return CommonParent(move2, move1);

				while (move1.depth < move2.depth)
					move2 = move2.prev;

				while (move1 != move2)
				{
					move1 = move1.prev;
					move2 = move2.prev;
				}
				return move1;
			}

			public override string ToString()
			{
				return $"{r + 1} {c + 1} {k}";
			}
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}


		public class Grid
		{
			public int[][] puzzle;
			public int goodnessV;
			public int goodnessH;
			public int n;
			public Move lastMove;

			public int Goodness => goodnessV + goodnessH;

			public int this[int r, int c]
			{
				get { return puzzle[r][c]; }
				set { puzzle[r][c] = value; }
			}

			public Grid(int[][] puzzle)
			{
				this.puzzle = puzzle;
				this.n = puzzle.Length;
				this.lastMove = new Move(0, 0, 0);
				lastMove.goodness = UpdateGoodness();
			}

			public Grid Clone()
			{
				var grid = (Grid)MemberwiseClone();
				var p = new int[n][];
				for (int i = 0; i < n; i++)
					puzzle[i] = (int[])puzzle[i].Clone();
				return grid;
			}


			public void Restore(Move state)
			{
				if (state == lastMove)
					return;

				int lastDepth = lastMove == null ? -1 : lastMove.depth;
				int stateDepth = state == null ? -1 : state.depth;

				if (stateDepth < lastDepth)
				{
					// Revert to previous state
					for (var current = lastMove; current != state; current = current.prev)
						Unrotate(current.r, current.c, current.k);
				}
				else if (stateDepth > lastDepth)
				{
					// Advance to forward state
					Restore(state.prev);
					Rotate(state.r, state.c, state.k);
				}

				lastMove = state;
			}

			private void Unrotate(int r, int c, int size)
			{
				Transpose(r, c, size);
				FlipRows(r, c, size);
			}

			public void Rotate(int r, int c, int size, int countParam = 1)
			{
				int count = countParam % 4;
				while (count-- > 0)
				{
					FlipRows(r, c, size);
					Transpose(r, c, size);
					lastMove = new Move(r, c, size, lastMove);
					Validate();
				}

			}

			private void Validate()
			{
				/*
                var hash = new HashSet<int>();
                for (int i=0; i<n; i++)
                    for (int j = 0; j < n; j++)
                        hash.Add(this[i, j]);
                Debug.Assert(hash.Count == n*n);*/
			}

			private void Transpose(int r, int c, int size)
			{
				for (int i = 0; i < size; i++)
					for (int j = 0; j < i; j++)
						Swap(ref puzzle[r + i][c + j], ref puzzle[r + j][c + i]);
				Validate();
			}


			public void FlipRows(int r, int c, int size)
			{
				if (size == n)
				{
					Array.Reverse(puzzle);
				}
				else
				{
					int top = r;
					int bottom = r + size - 1;

					while (top < bottom)
					{
						for (int j = c; j < c + size; j++)
						{
							int tmp = puzzle[top][j];
							puzzle[top][j] = puzzle[bottom][j];
							puzzle[bottom][j] = tmp;
						}
						top++;
						bottom--;
					}
					Validate();
				}
			}

			public void Flip()
			{
				Array.Reverse(puzzle);
				foreach (var row in puzzle)
					Array.Reverse(row);
			}

			public int UpdateGoodness()
			{
				if (lastMove.goodness > -1)
					return lastMove.goodness;

				GetGoodness(0, 0, n, out goodnessH, out goodnessV);
				lastMove.goodness = goodnessV + goodnessH;

				if (lastMove.depth < 499)
				{
					if (bestMoves == null || bestMoves.goodness < lastMove.goodness)
						bestMoves = lastMove;
				}

				return goodnessH + goodnessV;
			}

			public int GetGoodness(int r, int c, int size)
			{
				int goodnessH;
				int goodnessV;
				GetGoodness(r, c, size, out goodnessH, out goodnessV);
				return goodnessH + goodnessV;
			}

			public void GetGoodness(int r, int c, int size, out int goodnessH, out int goodnessV)
			{
				goodnessH = 0;
				goodnessV = 0;

				int rend = r + size;
				int cend = c + size;
				for (int i = r; i < rend; i++)
					for (int j = c; j < cend; j++)
					{
						for (int k = j + 1; k < cend; k++)
						{
							if ((puzzle[i][k] - puzzle[i][j] > 0))
								goodnessH++;
						}

						for (int k = i + 1; k < rend; k++)
						{
							if ((puzzle[k][j] - puzzle[i][j]) > 0)
								goodnessV++;
						}
					}
			}

			public override string ToString()
			{
				var sb = new StringBuilder();
				sb.Append('-', n * 6);
				sb.AppendLine();
				for (int i = 0; i < n; i++)
				{
					sb.AppendFormat("| " + string.Join(" | ", puzzle[i].Select(x => x.ToString().PadLeft(3))) + " |\n");
					sb.Append('-', n * 6);
					sb.AppendLine();
				}

				UpdateGoodness();
				sb.AppendLine("Goodness: " + UpdateGoodness());
				return sb.ToString();
			}
		}


		public static bool Verbose = false;
	}
}