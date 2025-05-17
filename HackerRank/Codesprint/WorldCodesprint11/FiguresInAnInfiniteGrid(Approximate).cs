namespace HackerRank.AdInfinitum18.FiguresInAnInfiniteGrid
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using static FastIO;
	using static System.Math;
	using static System.Array;

	// https://www.hackerrank.com/contests/world-codesprint-11/challenges/puzzle-9

	public class Solution
	{
		StreamWriter writer;

		public void solve(Stream input, Stream output)
		{
			InitInput(input);
			writer = new StreamWriter(output);
			solve();
			writer.Flush();
		}

		int hfactor = 1;
		int wfactor = 1;

		List<Figure> figures = new List<Figure>();
		List<int> factors = new List<int>(160);
		Figure canvas = new Figure(new Point2D[0], 0);

		public void solve()
		{

			int cells = 0;
			int hMax = 0;
			int wMax = 0;

			int n = Ni();
			for (int i = 1; i <= n; i++)
			{
				int s = Ni();
				cells += s;

				var p = new Point2D[s];
				for (int j = 0; j < s; j++)
					p[j] = new Point2D(Ni(), Ni());

				var f = new Figure(p, i);
				figures.Add(f);

				int h = Max(f.xmax, f.ymax);
				int w = Min(f.xmax, f.ymax);
				hMax = Max(hMax, h);
				wMax = Max(wMax, w);
			}

			factors = Factor(cells);
			factors.RemoveAll(x => x > wMax || cells / x > hMax);

			hfactor = hMax;
			wfactor = wMax;
			if (hfactor * wfactor < cells && factors.Count > 0)
			{
				hfactor = factors[factors.Count - 1];
				wfactor = factors[factors.Count - 1];
			}

			Loop();

			writer.WriteLine($"{canvas.XHeight} {canvas.YWidth}");
			writer.WriteLine(canvas);
		}

		void Loop2()
		{
			foreach (var fig in figures)
			{
				var cells = new HashSet<Point2D>(fig.Cells.Keys);
				var segmentFromCells = SegmentsFromCells(cells);
				fig.shape = ShapeFromSegments(segmentFromCells, cells, out fig.outline);
			}

			figures.Sort((a, b) => a.Cells.Count.CompareTo(b.Cells.Count));

			// Place largest figure at the center of the grid
			Figure largestFigure = figures[figures.Count - 1];
			figures.RemoveAt(figures.Count - 1);
			canvas.PlaceFigure(largestFigure, new Transformation());

			while (figures.Count > 0)
			{
				// Compare suffix strings against each other
				foreach (var figure in figures)
				{
					//shapeBuilder.FindLongestPattern(figure.shapeBuilder);
				}

				// Choose the best one 
				var f = figures[figures.Count - 1];
				figures.RemoveAt(figures.Count - 1);

				// Check collision

				var tm = new Transformation();
				//tm.Transform(0, 0, 0, 0, 0, 0);

				//PlaceFigure(f.points);
			}
		}

		void Loop()
		{
			var random = new Random();
			figures.Sort((a, b) => a.Cells.Count.CompareTo(b.Cells.Count));

			var transform = new Transformation();
			while (figures.Count > 0)
			{
				Figure f = figures[figures.Count - 1];
				figures.RemoveAt(figures.Count - 1);

				canvas.ComputeBoardDimensions();

				int f1 = 1;
				int f2 = 1;
				int iter = 0;
				while (true)
				{
					iter++;
					if (iter % 100 == 48) f.Rotate(1);
					if (iter % 200 == 100) f1++;
					if (iter % 200 == 199) f2++;

					transform.xOffset = random.Next(0, Math.Max(1, hfactor * f1 / 8 - f.YWidth));
					transform.yOffset = random.Next(0, Math.Max(1, wfactor * f2 / 8 - f.XHeight));

					if (!canvas.CheckCollision(f, transform))
					{
						canvas.PlaceFigure(f, transform);
						break;
					}
				}
			}
		}


		public static List<Point2D[]> SegmentsFromCells(HashSet<Point2D> cells)
		{
			int[] dir = { 0, 1, 0, -1, 0 };

			var segs = new List<Point2D[]>();
			var seg = new List<Point2D>();

			foreach (var p in cells)
			{
				for (int i = 0; i < 4; i++)
				{
					int dx = dir[i];
					int dy = dir[i + 1];
					var p2 = new Point2D(p.X + dx, p.Y + dy);
					if (cells.Contains(p2)) continue;

					seg.Clear();
					if (dx == +1 || dy == +1) seg.Add(new Point2D(p.X + 1, p.Y + 1));
					if (dx == -1 || dy == -1) seg.Add(new Point2D(p.X + 0, p.Y + 0));
					if (dx == -1 || dy == +1) seg.Add(new Point2D(p.X + 0, p.Y + 1));
					if (dx == +1 || dy == -1) seg.Add(new Point2D(p.X + 1, p.Y + 0));
					segs.Add(seg.ToArray());
				}
			}

			return segs;
		}

		public static string ShapeFromSegments(List<Point2D[]> segs,
			HashSet<Point2D> cells,
			out List<Point2D> outlinePoints)
		{
			var sb = new StringBuilder();

			var points = new HashSet<Point2D>(segs.SelectMany(x => x)).ToList();

			var indexMap = new Dictionary<Point2D, int>();
			var graph = new List<int>[points.Count];
			for (int i = 0; i < points.Count; i++)
			{
				indexMap[points[i]] = i;
				graph[i] = new List<int>();
			}

			foreach (var seg in segs)
			{
				var p1 = indexMap[seg[0]];
				var p2 = indexMap[seg[1]];
				graph[p1].Add(p2);
				graph[p2].Add(p1);
			}

			outlinePoints = new List<Point2D>();
			var visited = new BitArray(points.Count);
			for (int i = 0; i < visited.Length; i++)
			{
				if (visited[i]) continue;

				var v = i;

				int outputStart = outlinePoints.Count;

				while (!visited[v])
				{
					visited[v] = true;
					outlinePoints.Add(points[v]);

					int u = -1;
					for (int j = 0; j < graph[v].Count; j++)
						if (!visited[graph[v][j]])
						{
							u = graph[v][j];
							break;
						}

					if (u == -1) break;
					v = u;
				}

				var size = outlinePoints.Count - outputStart;
				Debug.Assert(size >= 3);

				var prev = outlinePoints[outlinePoints.Count - 1];
				for (int j = 0; j < size; j++)
				{
					var cur = outlinePoints[outputStart + j];
					var next = outlinePoints[outputStart + (j + 1) % size];

					var xmin = Min(prev.X, Min(cur.X, next.X));
					var ymin = Min(prev.Y, Min(cur.Y, next.Y));
					if (prev.X == next.X || prev.Y == next.Y)
						sb.Append('U');
					else
						sb.Append(cells.Contains(new Point2D(xmin, ymin)) ? 'R' : 'L');
				}

				sb.Append('/');
				outlinePoints.Add(points[outputStart]);
			}

			return sb.ToString();
		}


		public void Match(SuffixAutomaton automaton, string pattern,
			out int start, out int length)
		{
			var sb = new StringBuilder();

			sb.Append(pattern);
			sb.Append(pattern);

			int start1, length1;
			MatchCore(automaton, sb, out start1, out length1);
			length1 = Math.Min(length1, pattern.Length);

			// Reverse:
			int left = 0;
			int right = sb.Length - 1;
			while (left < right)
			{
				var tmp = sb[left];
				sb[left++] = sb[right];
				sb[right--] = tmp;
			}

			int start2, length2;
			MatchCore(automaton, sb, out start2, out length2);
			length2 = Math.Min(length2, pattern.Length);

			if (length1 > length2)
			{
				start = start1;
				length = length1;
			}
			else
			{
				start = start2;
				length = length2;
			}
		}

		void MatchCore(SuffixAutomaton shapeAutomaton, StringBuilder pattern,
			out int start,
			out int length)
		{
			start = 0;
			length = 0;
			if (pattern.Length == 0) return;
			var v = shapeAutomaton.Start;
			int len = 0;
			for (int i = 0; i < pattern.Length; i++)
			{
				while (v != shapeAutomaton.Start && v[pattern[i]] == null)
				{
					v = v.Link;
					len = v.Len;
				}

				if (v[pattern[i]] != null)
				{
					v = v[pattern[i]];
					len++;
				}

				if (len > length)
				{
					length = len;
					start = i;
				}
			}

			start += 1 - length;
		}


		public class Transformation
		{
			public double Xa, Xb, xOffset;
			public double Ya, Yb, yOffset;

			public Transformation()
			{
				Xa = Yb = 1;
			}

			public List<Point2D> Transform(List<Point2D> points)
			{
				var list = new List<Point2D>();
				foreach (var pt in points)
					list.Add(Transform(pt));
				return list;
			}

			public Point2D Transform(Point2D pt) => new Point2D(Xa * pt.X + Xb * pt.Y + xOffset, Ya * pt.X + Yb * pt.Y + yOffset);

			public static Transformation Translate(double x, double y) => new Transformation { xOffset = x, yOffset = y };

			public static Transformation Transpose() => new Transformation { Xa = 0, Xb = 1, Ya = 0, Yb = 1 };

			public static Transformation MapPoints(Point2D self1, Point2D self2, Point2D self3, Point2D new1, Point2D new2, Point2D new3)
			{
				double[,] matrix =
				{
				{ (int)self1.X, (int)self1.Y, 1 },
				{ (int)self2.X, (int)self2.Y, 1 },
				{ (int)self3.X, (int)self3.Y, 1 }
			};

				var inverse = Invert(matrix);

				var t = new Transformation();
				t.Xa = inverse[0, 0] * new1.X + inverse[0, 1] * new2.X + inverse[0, 2] * new3.X;
				t.Xb = inverse[1, 0] * new1.X + inverse[1, 1] * new2.X + inverse[1, 2] * new3.X;
				t.xOffset = inverse[2, 0] * new1.X + inverse[2, 1] * new2.X + inverse[2, 2] * new3.X;

				t.Ya = inverse[0, 0] * new1.Y + inverse[0, 1] * new2.Y + inverse[0, 2] * new3.Y;
				t.Yb = inverse[1, 0] * new1.Y + inverse[1, 1] * new2.Y + inverse[1, 2] * new3.Y;
				t.yOffset = inverse[2, 0] * new1.Y + inverse[2, 1] * new2.Y + inverse[2, 2] * new3.Y;

				Debug.Assert(t.Xa * self1.X + t.Xb * self1.Y + t.xOffset == new1.X);
				Debug.Assert(t.Ya * self1.X + t.Yb * self1.Y + t.yOffset == new1.Y);
				Debug.Assert(t.Xa * self2.X + t.Xb * self2.Y + t.xOffset == new2.X);
				Debug.Assert(t.Ya * self2.X + t.Yb * self2.Y + t.yOffset == new2.Y);
				Debug.Assert(t.Xa * self3.X + t.Xb * self3.Y + t.xOffset == new3.X);
				Debug.Assert(t.Ya * self3.X + t.Yb * self3.Y + t.yOffset == new3.Y);
				return t;
			}

			public static double[,] Invert(double[,] a)
			{
				// determinant is 1 or -1
				double det = 0;
				for (int i = 0; i < 3; i++)
					det += (a[0, i] * (a[1, (i + 1) % 3] * a[2, (i + 2) % 3] - a[1, (i + 2) % 3] * a[2, (i + 1) % 3]));

				Debug.Assert(det == 1 || det == -1);
				var inverse = new double[3, 3];
				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++)
						inverse[i, j] = (a[(i + 1) % 3, (j + 1) % 3] * a[(i + 2) % 3, (j + 2) % 3]) - (a[(i + 1) % 3, (j + 2) % 3] * a[(i + 2) % 3, (j + 1) % 3]) / det;

				return inverse;
			}
		}

		public class Figure
		{
			public Dictionary<Point2D, int> Cells;
			public int xmin;
			public int ymin;
			public int ymax;
			public int xmax;
			bool dirty = true;

			public string shape;
			public List<Point2D> outline;

			public Figure(IList<Point2D> points, int value)
			{
				Reset(points.Count);
				foreach (var p in points)
					this[p] = value;
			}

			public Figure(IList<Point2D> points, int value, Transformation t)
			{
				Reset(points.Count);
				foreach (var p in points)
					this[t.Transform(p)] = value;
			}

			public Figure(Figure f, Transformation t)
			{
				Reset(f.Cells.Count);
				foreach (var p in f.Cells)
					this[t.Transform(p.Key)] = p.Value;
			}

			public void Normalize()
			{
				ComputeBoardDimensions();
				if (xmin == 1 && ymin == 1) return;
				TransformSelf(Transformation.Translate(1 - xmin, 1 - ymin));
			}


			public Transformation RotationTransform(int rot)
			{
				var transform = new Transformation();

				rot &= 3;
				if (rot == 0)
					return transform;

				ComputeBoardDimensions();
				switch (rot)
				{
					case 1:
						transform.Xa = transform.Yb = 0;
						transform.Xb = 1;
						transform.Ya = -1;
						transform.yOffset = XHeight + 1;
						break;
					case 2:
						transform.Xa = -1;
						transform.Yb = -1;
						transform.xOffset = XHeight + 1;
						transform.yOffset = YWidth + 1;
						break;
					case 3:
						transform.Xa = transform.Yb = 0;
						transform.Xb = -1;
						transform.Ya = 1;
						transform.xOffset = YWidth + 1;
						break;
				}
				return transform;
			}

			public void Rotate(int rot)
			{
				rot &= 3;
				if (rot == 0)
					return;

				TransformSelf(RotationTransform(rot));
			}

			public bool CheckCollision(Figure figure, Transformation transform)
			{
				foreach (var pt in figure.Cells.Keys)
				{
					var t = transform.Transform(pt);
					if (Cells.ContainsKey(t))
						return true;
				}
				return false;
			}

			public void PlaceFigure(Figure figure, Transformation transform)
			{
				foreach (var pair in figure.Cells)
				{
					var pt = transform.Transform(pair.Key);
					this[pt] = pair.Value;
				}
			}

			public void TransformSelf(Transformation t)
			{
				var oldCells = Cells;
				Reset(Cells.Count);
				foreach (var pair in oldCells)
					this[t.Transform(pair.Key)] = pair.Value;
			}

			public int this[Point2D p]
			{
				get
				{
					int v;
					return Cells.TryGetValue(p, out v) ? v : 0;
				}
				set
				{
					Cells[p] = value;

					if (value <= 0) dirty = true;
					if (p.X < xmin) xmin = (int)p.X;
					if (p.X > xmax) xmax = (int)p.X;
					if (p.Y < ymin) ymin = (int)p.Y;
					if (p.Y > ymax) ymax = (int)p.Y;
				}
			}

			public int this[double x, double y]
			{
				get { return this[new Point2D(x, y)]; }
				set { this[new Point2D(x, y)] = value; }
			}

			public override string ToString()
			{
				ComputeBoardDimensions();

				var sb = new StringBuilder();
				var list = new List<int>();
				for (long i = xmin; i <= xmax; i++)
				{
					list.Clear();
					for (long j = ymin; j <= ymax; j++)
						list.Add(this[i, j]);
					sb.AppendLine(string.Join(" ", list));
				}

				return sb.ToString();
			}

			public void ComputeBoardDimensions()
			{
				if (!dirty) return;
				dirty = false;

				xmax = int.MinValue;
				ymax = int.MinValue;
				xmin = int.MaxValue;
				ymin = int.MaxValue;

				foreach (var p in Cells.Keys)
				{
					if (p.X < xmin) xmin = (int)p.X;
					if (p.X > xmax) xmax = (int)p.X;
					if (p.Y < ymin) ymin = (int)p.Y;
					if (p.Y > ymax) ymax = (int)p.Y;
				}
			}

			public void Reset(int count = 0)
			{
				dirty = false;
				xmax = int.MinValue;
				ymax = int.MinValue;
				xmin = int.MaxValue;
				ymin = int.MaxValue;
				Cells = new Dictionary<Point2D, int>(count);
			}

			public int YWidth => Max(0, ymax - ymin + 1);

			public int XHeight => Max(0, xmax - xmin + 1);

			public int Area => XHeight * YWidth;
		}


		#region Helpers

		public List<int> Factor(int n)
		{
			List<int> factors = new List<int>();
			for (int i = 1; i * i <= n; i++)
			{
				if (n % i == 0)
					factors.Add(i);
			}
			return factors;
		}


		#endregion
	}

	public struct Point2D
	{
		public double X;
		public double Y;

		public Point2D(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		public static Point2D operator +(Point2D p, Point2D p2)
		{
			return new Point2D(p.X + p2.X, p.Y + p2.Y);
		}
		public static Point2D operator -(Point2D p0, Point2D p)
		{
			return new Point2D(p0.X - p.X, p0.Y - p.Y);
		}

		public static Point2D operator *(Point2D p, double c)
		{
			return new Point2D(p.X * c, p.Y * c);
		}

		public static Point2D operator /(Point2D p, double c)
		{
			return new Point2D(p.X / c, p.Y / c);
		}

		public override string ToString()
		{
			return "(" + X + "," + Y + ")";
		}

		public static bool operator <(Point2D lhs, Point2D rhs)
		{
			return lhs.Y < rhs.Y || lhs.Y == rhs.Y && lhs.X < rhs.X;
		}

		public static bool operator >(Point2D lhs, Point2D rhs)
		{
			return lhs.Y > rhs.Y || lhs.Y == rhs.Y && lhs.X > rhs.X;
		}

		public static bool operator ==(Point2D lhs, Point2D rhs)
		{
			return lhs.Y == rhs.Y && lhs.X == rhs.X;
		}

		public static bool operator !=(Point2D lhs, Point2D rhs)
		{
			return lhs.Y != rhs.Y || lhs.X != rhs.X;
		}

		public bool Equals(Point2D other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Point2D && Equals((Point2D)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

	}

	public class SuffixAutomaton
	{
		public Node Start;
		public Node End;
		public int NodeCount;
		public IEnumerable<char> Text;

		Node[] _nodes;

		private SuffixAutomaton()
		{
			Start = new Node();
			End = Start;
			NodeCount = 1;
		}

		public SuffixAutomaton(IEnumerable<char> s) : this()
		{
			Text = s;

			foreach (var c in s)
				Extend(c);

			for (var p = End; p != Start; p = p.Link)
				p.IsTerminal = true;
		}

		/// <summary>
		/// Extends an automaton by one character
		/// </summary>
		/// <param name="c"></param>
		public void Extend(char c)
		{
			var node = new Node
			{
				Key = c,
				Len = End.Len + 1,
				Link = Start,
				Index = NodeCount,
			};
			NodeCount++;

			Node p;
			for (p = End; p != null && p[c] == null; p = p.Link)
				p[c] = node;
			End = node;

			if (p == null) return;

			var q = p[c];
			if (p.Len + 1 == q.Len)
				node.Link = q;
			else
			{
				var clone = q.Clone();
				clone.Len = p.Len + 1;
				clone.Index = NodeCount;
				NodeCount++;

				for (; p != null && p[c] == q; p = p.Link)
					p[c] = clone;

				q.Link = node.Link = clone;
			}
		}

		public Node[] GetNodes()
		{
			if (_nodes != null && NodeCount == _nodes.Length)
				return _nodes;

			var nodes = _nodes = new Node[NodeCount];
			int stack = 0;
			int idx = NodeCount;

			nodes[stack++] = Start;
			while (stack > 0)
			{
				var current = nodes[--stack];

				if (current.Index > 0)
					current.Index = 0;

				current.Index--;
				var index = current.NextCount + current.Index;
				if (index >= 0)
				{
					stack++;

					var child = current.Next[index];
					if (child.Index >= -child.NextCount)
						nodes[stack++] = current.Next[index];
				}
				else if (index == -1)
				{
					nodes[--idx] = current;
				}
				Debug.Assert(idx >= stack);
			}

			if (idx != 0)
			{
				Debug.Assert(idx == 0, "NodeCount smaller than number of nodes");
				NodeCount -= idx;
				_nodes = new Node[NodeCount];
				Array.Copy(nodes, idx, _nodes, 0, NodeCount);
			}

			UpdateNodeIndices();
			return _nodes;
		}

		public IEnumerable<Node> NodesBottomUp()
		{
			var nodes = GetNodes();
			for (int i = NodeCount - 1; i >= 0; i--)
				yield return nodes[i];
		}

		void UpdateNodeIndices()
		{
			var nodes = _nodes;
			for (int i = 0; i < NodeCount; i++)
				nodes[i].Index = i;
		}

		public Node FindNode(string pattern, int index, int count)
		{
			var node = Start;
			for (int i = 0; i < count; i++)
			{
				node = node[pattern[index + i]];
				if (node == null) return null;
			}
			return node;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var n in GetNodes())
			{
				sb.Append($"{{id:{0}, len:{n.Len}, link:{n.Link?.Index ?? -1}, cloned:{n.IsCloned}, Next:{{");
				sb.Append(string.Join(",", n.Children.Select(c => c.Key + ":" + c.Index)));
				sb.AppendLine("}}");
			}
			return sb.ToString();
		}

		public class Node : TrieNode<Node>
		{
			public int Len;
			public int Index;
			public Node Link;
			public Node Original;

			public int FirstOccurrence => Original?.Len ?? this.Len;

			public bool IsCloned => Original != null;

			public new Node Clone()
			{
				var node = base.Clone();
				node.Original = Original ?? this;
				return node;
			}
		}

		public class TrieNode<T> where T : TrieNode<T>
		{
			public char Key;
			public bool IsTerminal;
			public byte NextCount;
			int KeyMask;
			public T[] Next;
			public static readonly T[] Empty = new T[0];

			public TrieNode()
			{
				Next = Empty;
			}

			public T this[char ch]
			{
				get
				{
					if ((KeyMask << ~ch) < 0)
					{
						int left = 0;
						int right = NextCount - 1;
						while (left <= right)
						{
							int mid = (left + right) >> 1;
							var val = Next[mid];
							int cmp = val.Key - ch;
							if (cmp < 0)
								left = mid + 1;
							else if (cmp > 0)
								right = mid - 1;
							else
								return val;
						}
					}
					return null;
				}
				set
				{
					int left = 0;
					int right = NextCount - 1;
					while (left <= right)
					{
						int mid = (left + right) >> 1;
						var val = Next[mid];
						int cmp = val.Key - ch;
						if (cmp < 0)
							left = mid + 1;
						else if (cmp > 0)
							right = mid - 1;
						else
						{
							Next[mid] = value;
							return;
						}
					}

					if (NextCount >= Next.Length)
						Resize(ref Next, Math.Max(2, NextCount * 2));
					if (NextCount > left)
						Copy(Next, left, Next, left + 1, NextCount - left);
					NextCount++;
					Next[left] = value;
					KeyMask |= 1 << ch;
				}
			}

			/// <summary>
			/// Return child nodes
			/// </summary>
			public IEnumerable<T> Children
			{
				get
				{
					for (int i = 0; i < NextCount; i++)
						yield return Next[i];
				}
			}

			/// <summary>
			/// Clones an node
			/// </summary>
			/// <returns></returns>
			public T Clone()
			{
				var node = (T)MemberwiseClone();
				node.Next = (T[])node.Next.Clone();
				return node;
			}

		}

	}

	public static class FastIO
	{
		#region  Input
		static Stream _inputStream;
		static int _inputIndex, _bytesRead;
		static byte[] _inputBuffer;
		const int MonoBufferSize = 4096;

		public static void InitInput(Stream input = null)
		{
			_inputStream = input ?? Console.OpenStandardInput();
			_inputIndex = _bytesRead = 0;
			_inputBuffer = new byte[MonoBufferSize];
		}

		static void ReadMore()
		{
			_inputIndex = 0;
			_bytesRead = _inputStream.Read(_inputBuffer, 0, _inputBuffer.Length);
			if (_bytesRead <= 0) _inputBuffer[0] = 32;
		}

		public static int Read()
		{
			if (_inputIndex >= _bytesRead) ReadMore();
			return _inputBuffer[_inputIndex++];
		}

		public static int Ni()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg) { c = Read(); }

			int number = c - '0';
			while (true)
			{
				var d = Read() - '0';
				if ((uint)d > 9) break;
				number = number * 10 + d;
			}
			return neg ? -number : number;
		}

		public static int SkipSpaces()
		{
			int c;
			do c = Read(); while ((uint)c - 33 >= (127 - 33));
			return c;
		}
		#endregion

	}

	class CaideConstants
	{
		public const string InputFile = null;
		public const string OutputFile = null;
	}
	public class Program
	{
		public static void Main(string[] args)
		{
			Solution solution = new Solution();
			solution.solve(Console.OpenStandardInput(), Console.OpenStandardOutput());

#if DEBUG
			Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
		}
	}

}