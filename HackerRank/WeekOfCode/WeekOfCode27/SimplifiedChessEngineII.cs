namespace HackerRank.WeekOfCode27
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	unsafe class SimplifiedChessEngineII
	{
		static void Main()
		{
			int g = int.Parse(Console.ReadLine());
			while (g-- > 0)
			{
				var a = Console.ReadLine().Split().Select(int.Parse).ToArray();
				int w = a[0], b = a[1], m = a[2];
				var white = new char[w][];
				for (int i = 0; i < w; i++)
					white[i] = Array.ConvertAll(Console.ReadLine().Split(), x => x[0]);

				var black = new char[b][];
				for (int i = 0; i < b; i++)
					black[i] = Array.ConvertAll(Console.ReadLine().Split(), x => x[0]);

				Solve(white, black, m);
			}
		}

		public static readonly int[,] EmptyMoves = { };
		public static readonly int[,] RookMoves = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
		public static readonly int[,] BishopMoves = { { 1, 1 }, { -1, 1 }, { -1, -1 }, { 1, -1 } };

		public static readonly int[,] KnightMoves =
		{
			{2, 1}, {1, 2}, {2, -1}, {-1, 2},
			{-2, 1}, {1, -2}, {-2, -1}, {-1, -2}
		};

		public static readonly int[,] QueenMoves =
		{
			{1, 0}, {0, 1}, {-1, 0}, {0, -1},
			{1, 1}, {-1, 1}, {-1, -1}, {1, -1}
		};

		public static readonly int[][,] Moves =
		{
			EmptyMoves,
			QueenMoves,
			RookMoves,
			BishopMoves,
			KnightMoves,
			new[,] {{0, 1}, {-1, 1}, {1, 1}},
			null,
			null,
			EmptyMoves,
			QueenMoves,
			RookMoves,
			BishopMoves,
			KnightMoves,
			new[,] {{0, -1}, {-1, -1}, {1, -1}},
		};



		public static char[] Text = { ' ', 'Q', 'R', 'B', 'N', 'P' };

		[Flags]
		public enum Pieces
		{
			Queen = 1,
			Rook = 2,
			Bishop = 3,
			Knight = 4,
			Pawn = 5,
			Black = 8,
		}


		public static void Solve(char[][] white, char[][] black, int m)
		{
			var solver = new Solver { MaxMoves = m };
			var grid = new Grid(white, black);
			var canWin = solver.CanWin(grid, m);
			Console.WriteLine(canWin ? "YES" : "NO");
		}

		public static Pieces ParsePiece(char ch)
		{
			switch (ch)
			{
				case 'Q':
					return Pieces.Queen;
				case 'R':
					return Pieces.Rook;
				case 'B':
					return Pieces.Bishop;
				case 'N':
					return Pieces.Knight;
				case 'P':
					return Pieces.Pawn;
			}
			return 0;
		}

		public class Solver
		{
			Dictionary<ulong, int> dict = new Dictionary<ulong, int>();

			public int MaxMoves;

			public bool CheckCache(Grid g, int moves, out bool win)
			{
				int value;
				if (dict.TryGetValue(g.grid, out value)
					&& (value & 1 << (16 + moves)) != 0)
				{
					win = (value & (1 << moves)) != 0;
					return true;
				}

				win = false;
				return false;
			}

			public bool Cache(Grid g, int moves, bool win)
			{
				var key = g.grid;
				int value;
				dict.TryGetValue(key, out value);

				bool white = (MaxMoves - moves & 1) == 0;
				var mask = (1 << moves) & 0xffff; ;

				if (white && win)
					mask = (mask * 0x5555 & 0xffff);

				var newValue = mask << 16;
				if (win)
					newValue |= mask;

				try
				{
					if (dict.Count > 1000000) dict.Clear();
					dict[key] = value | newValue;
				}
				catch
				{
					dict.Clear();
				}

				return win;
			}

			const int MaxCands = 100;

			public bool CanWin(Grid g, int moves, int turn = 0)
			{
				bool win = turn != 0;
				if (moves == 0)
					return win;

				bool cachedWin;
				if (CheckCache(g, moves, out cachedWin))
					return cachedWin;

				int captures = 0;
				int size = 0;
				var list = stackalloc Grid[MaxCands];

				for (int i = 0; i < 4; i++)
					for (int j = 0; j < 4; j++)
					{
						var p = g[i, j];
						if (Color(p) != turn) continue;

						var cand = Moves[(int)p];
						var len = cand.GetLength(0);
						var piece = Piece(p);

						for (int k = 0; k < len; k++)
						{
							int dx = cand[k, 0], dy = cand[k, 1];
							int x = i + dx, y = j + dy;

							bool singleMove = piece >= Pieces.Knight;
							while (x >= 0 && x < 4 && y >= 0 && y < 4)
							{
								var p2 = g[x, y];
								if (Color(p2) == turn) break;

								// Pawns must capture diagonal and move straight
								if (piece == Pieces.Pawn && (p2 != 0) != (dx != 0))
									break;

								bool captured = false;
								if (p2 != 0)
								{
									if (Piece(p2) == Pieces.Queen)
										return Cache(g, moves, true);
									captured = true;
								}

								var g2 = g;
								g2[i, j] = 0;

								var pieceStart = p;
								var pieceEnd = p;

								if (piece == Pieces.Pawn && y == (turn == 0 ? 3 : 0))
								{
									pieceStart = Pieces.Rook | p & Pieces.Black;
									pieceEnd = Pieces.Knight | p & Pieces.Black;
								}

								for (var promote = pieceStart; promote <= pieceEnd; promote++)
								{
									g2[x, y] = promote;
									if (captured)
									{
										list[size++] = list[captures];
										list[captures++] = g2;
									}
									else
									{
										list[size++] = g2;
									}
								}

								if (captured || singleMove)
									break;
								x += dx;
								y += dy;
							}
						}
					}

				if (size > 0)
					win = false;

				for (int i = 0; i < size; i++)
				{
					var g2 = list[i];
					if (!CanWin(g2, moves - 1, 1 - turn))
					{
						win = true;
						break;
					}
				}

				return Cache(g, moves, win);
			}
		}

		public struct Grid
		{
			public ulong grid;

			public Grid(char[][] white, char[][] black)
			{
				grid = 0;
				Parse(white, 0);
				Parse(black, Pieces.Black);
			}

			private void Parse(char[][] pieces, Pieces mask)
			{
				foreach (var ar in pieces)
				{
					var piece = ParsePiece(ar[0]);
					int x = ar[1] - 'A';
					int y = ar[2] - '1';
					this[x, y] = piece | mask;
				}
			}


			public Pieces this[int x, int y]
			{
				get
				{
					int pos = (x << 2) + y;
					pos <<= 2;
					return (Pieces)((grid >> pos) & 0xf);
				}
				set
				{
					int pos = (x << 2) + y;
					pos <<= 2;
					grid = (grid & ~(0xfUL << pos)) | ((ulong)value << pos);
				}
			}

			public override string ToString()
			{
				return string.Join("\r\n", Rows) + "\r\n";
			}

			public string[] Rows
			{
				get
				{
					var list = new List<string>();
					var sb = new StringBuilder();
					for (int y = 0; y < 4; y++)
					{
						sb.Clear();
						for (int x = 0; x < 4; x++)
						{
							var piece = this[x, y];
							var ch = piece == 0 ? '.' : Text[(int)Piece(piece)];
							if (Color(piece) > 0) ch = char.ToLower(ch);
							sb.Append(ch);
						}
						list.Add(sb.ToString());
					}
					return list.ToArray();
				}
			}
		}

		public static int Color(Pieces piece)
		{
			if (piece >= Pieces.Black) return 1;
			if (piece == 0) return -1;
			return 0;
		}

		public static Pieces Piece(Pieces piece)
		{
			return piece & ~Pieces.Black;
		}
	}
}
