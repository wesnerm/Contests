using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using static System.Math;
using static System.Console;

namespace HackerRank.SplitPlane
{
	using STType = Int64;
	using static FastIO;

	public partial class Solution
	{
		#region Variables

		int maxX;
		int maxY;
		Segment[] segments;

		#endregion

		public static void Main()
		{
			InitIO();
			int q = Ni();
			for (int i = 0; i < q; i++)
			{
				var sol = new Solution();
#if DEBUG
				sol.Test();
#else
				sol.Run();
#endif
			}
		}

		void Run()
		{
			ReadSegments();

			Array.Sort(segments);
			CompressCoordinates();

			if (maxY > maxX)
				ReverseSegments();

			var faces = (long)maxY * maxX > Parameters.BruteForceThreshold
				? ProcessSegments()
				: BruteForce();

			WriteLine(faces);
		}


		long ProcessSegments()
		{
			long edges;
			long vertices;
			long components;
			SweepLine(out vertices, out edges, out components);

			var faces = edges - vertices + 1 + components;
#if DEBUG
			Error.WriteLine($"F = E - V + 1 + C = {edges} - {vertices} + 1 + {components} = {faces}");
#endif
			return faces;
		}

		public void SweepLine(out long vertices, out long edges, out long components)
		{
			vertices = 0;
			edges = 0;

			var tree = new Tree();
			var ds = new DisjointSet(segments.Length);
			var pts = new DataPoint[2 * segments.Length + 1];
			int index = 0;
			foreach (var s in segments)
			{
				pts[index++] = new DataPoint
				{
					Horiz = s.Horiz,
					X = s.X1,
					Y = s.Y1,
					Start = true,
					Id = s.Id,
					Segment = s,
				};

				pts[index++] = new DataPoint
				{
					Horiz = s.Horiz,
					X = s.X2,
					Y = s.Y2,
					Start = false,
					Id = s.Id,
					Segment = s,
				};
			}

			pts[index++] = new DataPoint
			{
				X = int.MaxValue
			};

			Array.Sort(pts, (a, b) =>
			{
				int cmp = a.X.CompareTo(b.X);
				if (cmp != 0) return cmp;
				cmp = a.Y.CompareTo(b.Y);
				if (cmp != 0) return cmp;
				cmp = -a.Start.CompareTo(b.Start);
				if (cmp != 0) return cmp;
				cmp = -a.Horiz.CompareTo(b.Horiz);
				return cmp;
			});


			var xlines = new int[maxY];
			var xids = new int[maxY];
			var xstart = new int[maxY];

			int previousX = int.MinValue;
			int previousY = int.MinValue;
			bool dropVertex = false;
			bool dropVEdge = false;
			bool dropHEdge = false;
			int ylines = 0;
			int yid = -1;
			int compid = -1;
			int yExtent = 0;

			foreach (var pt in pts)
			{
				// New point

				bool newPoint = false;
				if (pt.X > previousX)
				{
					newPoint = true;
					ylines = 0;
					yid = -1;
					yExtent = 0;
				}
				else if (pt.Y > previousY)
				{
					newPoint = true;
					if (ylines > 0)
					{
						if (dropVertex)
							dropVEdge = true;

						var yStart = previousY + 1;
						var yEnd = pt.Y - 1;
						if (yEnd >= yStart)
						{
							var intersections = tree.CountInclusive(yStart, yEnd);
							vertices += intersections;
							edges += 2 * intersections;

							// Union segments here
							tree.AddToDisjointSet(ds, yStart, yEnd, yid);
#if DEBUG
							//if (intersections == 0)
							Error.WriteLine($"{intersections} vertices, HEdge, and VEdges at ({pt.X},{yStart}) to ({pt.X},{yEnd})");
#endif
						}
					}
				}

				if (newPoint)
				{
					compid = -1;
					if (dropVertex)
					{
						vertices++;
#if DEBUG
						Error.WriteLine($"Vertex at ({previousX},{previousY})");
#endif
					}
					if (dropVEdge)
					{
						edges++;
#if DEBUG
						Error.WriteLine($"VEdge at ({previousX},{previousY})");
#endif
					}
					if (dropHEdge)
					{
						edges++;
#if DEBUG
						Error.WriteLine($"HEdge at ({previousX},{previousY})");
#endif
					}
					dropVertex = false;
					dropVEdge = false;
					dropHEdge = false;
				}

				if (pt.X == int.MaxValue) break;

				if (pt.Horiz)
				{
					if (ylines > 0)
					{
						ds.Union(pt.Id, yid);
						dropVertex = true;
						if (xlines[pt.Y] != 0 && xstart[pt.Y] < pt.X)
							dropHEdge = true;
					}

					if (pt.Start)
					{
						if (xlines[pt.Y]++ == 0)
						{
							dropVertex = true;
							xstart[pt.Y] = pt.X;
							tree.Insert(pt.Y, pt.Id);
						}
						else
						{
							ds.Union(xids[pt.Y], pt.Id);
						}
						xids[pt.Y] = pt.Id;
					}
					else
					{
						if (--xlines[pt.Y] == 0)
						{
							dropVertex = true;
							dropHEdge = true;
							tree.Delete(pt.Y);
						}
					}

				}
				else
				{
					if (pt.Start)
					{
						if (ylines++ == 0)
						{
							dropVertex = true;
						}
						else
							ds.Union(yid, pt.Id);

						yExtent = Max(yExtent, pt.Segment.Y2);
					}
					else if (--ylines == 0)
					{
						dropVertex = true;
					}

					yid = pt.Id;
					if (xlines[pt.Y] > 0)
					{
						if (xstart[pt.Y] < pt.X) dropHEdge = true;
						ds.Union(pt.Id, xids[pt.Y]);
					}
				}

				if (compid >= 0)
					ds.Union(compid, pt.Id);
				else
					compid = pt.Id;

				previousX = pt.X;
				previousY = pt.Y;
			}

			components = ds.Count;
		}

		public partial class Segment : IComparable<Segment>
		{
#region Core
			public int Id;
			public int X1;
			public int Y1;
			public int X2;
			public int Y2;
			public bool Horiz;

			public int CompareTo(Segment other)
			{
				int cmp = X1.CompareTo(other.X1);
				if (cmp != 0) return cmp;
				cmp = X2.CompareTo(other.X2);
				if (cmp != 0) return cmp;
				cmp = Y1.CompareTo(other.Y1);
				if (cmp != 0) return cmp;
				cmp = Y2.CompareTo(other.Y2);
				if (cmp != 0) return cmp;
				cmp = Horiz.CompareTo(other.Horiz);
				return cmp;
			}

			public override string ToString()
			{
				return $"{X1} {Y1} {X2} {Y2} {Horiz}";
			}

#endregion

		}


		void CompressCoordinates()
		{
			var coords = new HashSet<int>();
			var remap = new Dictionary<int, int>();

			foreach (var s in segments)
			{
				coords.Add(s.Y1);
				coords.Add(s.Y2);
			}

			var list = coords.ToArray();
			Array.Sort(list);
			for (var i = 0; i < list.Length; i++)
				remap[list[i]] = i;

			foreach (var s in segments)
			{
				s.Y1 = remap[s.Y1];
				s.Y2 = remap[s.Y2];
			}

			maxY = list.Length;

			coords.Clear();
			remap.Clear();

			foreach (var s in segments)
			{
				coords.Add(s.X1);
				coords.Add(s.X2);
			}

			list = coords.ToArray();
			Array.Sort(list);
			for (var i = 0; i < list.Length; i++)
				remap[list[i]] = i;

			foreach (var s in segments)
			{
				s.X1 = remap[s.X1];
				s.X2 = remap[s.X2];
			}

			maxX = list.Length;
		}


		long BruteForce()
		{
			var left = new bool[maxX + 1, maxY + 1];
			var top = new bool[maxX + 1, maxY + 1];

			Func<int, int, int> ids = (x, y) => x >= 0 && y >= 0 && x < maxX && y < maxY ? x * maxY + y : maxX * maxY;

			foreach (var s in segments)
			{
				if (s.Horiz)
				{
					for (int x = s.X1; x < s.X2; x++)
						top[x, s.Y1] = true;
				}
				else
				{
					for (int y = s.Y1; y < s.Y2; y++)
						left[s.X1, y] = true;
				}
			}

			var ds = new DisjointSet(maxX * maxY + 1);
			for (int i = 0; i < maxX; i++)
				for (int j = 0; j < maxY; j++)
				{
					var id = ids(i, j);
					if (!top[i, j]) ds.Union(id, ids(i, j - 1));
					if (!left[i, j]) ds.Union(id, ids(i - 1, j));
				}

#if DEBUG
			var line1 = new StringBuilder();
			var line2 = new StringBuilder();
			var lines = new List<string>();
			for (int y = 0; y < maxY; y++)
			{
				line1.Clear();
				line2.Clear();
				for (int x = 0; x < maxX; x++)
				{
					var area = $" {x} ";
					line1.Append(top[x, y] ? " ---" : left[x, y] ? $"    " : $"    ");
					line2.Append(left[x, y] ? $"|{area}" : $" {area}");
				}
				lines.Add(line1.ToString());
				lines.Add(line2.ToString());
			}
			lines.Reverse();
			foreach (var line in lines)
				Error.WriteLine(line);
#endif

			return ds.Count;
		}

		void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		void ReadSegments()
		{
			int n = Ni();

			segments = new Segment[n];

			for (int i = 0; i < n; i++)
			{
				int x1 = Ni();
				int y1 = Ni();
				int x2 = Ni();
				int y2 = Ni();

				var seg = new Segment
				{
					Id = i,
					X1 = Min(x1, x2),
					Y1 = Min(y1, y2),
					X2 = Max(x1, x2),
					Y2 = Max(y1, y2),
					Horiz = y1 == y2,
				};

				segments[i] = seg;
			}
		}

		void ReverseSegments()
		{
			foreach (var s in segments)
			{
				var x1 = s.X1;
				var x2 = s.X2;
				var y1 = s.Y1;
				var y2 = s.Y2;
				s.X1 = y1;
				s.X2 = y2;
				s.Y1 = x1;
				s.Y2 = x2;
				s.Horiz ^= true;
			}

			var tmp = maxY;
			maxY = maxX;
			maxX = tmp;
		}



		public class DataPoint
		{
			public bool Horiz;
			public bool Start;
			public int X;
			public int Y;
			public int Id;
			public Segment Segment;

			public override string ToString()
			{
				return $"({X}, {Y})" + (Horiz ? " H" : "V") + (Start ? " Start" : "End");
			}
		}

	}

	public static class FastIO
	{
		static System.IO.Stream stream;
		static int idx, bytesRead;
		static byte[] buffer;


		public static void InitIO(
			int bufferSize = 1 << 20,
			System.IO.Stream input = null)
		{
			stream = input ?? Console.OpenStandardInput();
			idx = bytesRead = 0;
			buffer = new byte[bufferSize];
		}


		static void ReadMore()
		{
			idx = 0;
			bytesRead = stream.Read(buffer, 0, buffer.Length);
			if (bytesRead <= 0) buffer[0] = 32;
		}

		public static int Read()
		{
			if (idx >= bytesRead) ReadMore();
			return buffer[idx++];
		}


		public static int Ni()
		{
			var c = SkipSpaces();
			bool neg = c == '-';
			if (neg)
			{
				c = Read();
			}

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

	}

	public class DisjointSet
	{
		private readonly int[] _ds;
		private readonly int[] _counts;
		private int _components;
		private bool _oneBased;

		public int Count => _components;

		public DisjointSet(int size, bool onesBased = false)
		{
			_ds = new int[size + 1];
			_counts = new int[size + 1];
			_oneBased = onesBased;
			Clear();
		}

		public void Clear()
		{
			int size = _ds.Length - 1;
			_components = size;

			for (int i = 0; i <= size; i++)
			{
				_ds[i] = i;
				_counts[i] = 1;
			}

			if (_oneBased)
				_ds[0] = size;
			else
				_ds[size] = 0;
		}

		public int Union(int x, int y)
		{
			var rx = Find(x);
			var ry = Find(y);
			if (rx == ry) return rx;

			_components--;
			if (_counts[ry] > _counts[rx])
			{
				_ds[rx] = ry;
				_counts[ry] += _counts[rx];
				return ry;
			}
			else
			{
				_ds[ry] = rx;
				_counts[rx] += _counts[ry];
				return rx;
			}
		}

		public int Find(int x)
		{
			var root = _ds[x];
			return root == x
				? x
				: (_ds[x] = Find(root));
		}

		public int GetCount(int x)
		{
			var root = Find(x);
			return _counts[root];
		}

		public IEnumerable<int> Roots()
		{
			int start = _oneBased ? 1 : 0;
			int limit = _oneBased ? _ds.Length : _ds.Length - 1;
			for (int i = start; i < limit; i++)
			{
				if (_ds[i] == i)
					yield return i;
			}
		}
		public IEnumerable<List<int>> Components()
		{
			var comp = new Dictionary<int, List<int>>();
			foreach (var c in Roots())
				comp[c] = new List<int>(GetCount(c));

			int start = _oneBased ? 1 : 0;
			int limit = _oneBased ? _ds.Length : _ds.Length - 1;

			for (int i = start; i < limit; i++)
				comp[Find(i)].Add(i);
			return comp.Values;
		}
	}

	public class Parameters
	{
		public const int BruteForceThreshold = 0;  // 4000000;
	}

}
