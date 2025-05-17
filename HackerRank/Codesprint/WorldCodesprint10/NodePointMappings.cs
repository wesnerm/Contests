
namespace HackerRank.WorldCodeSprint10.NodePointMapping
{
	using System;
	using System.Linq;
	using System.IO;
	using System.Collections.Generic;

// Powered by caide (code generator, tester, and library code inliner)

	class Solution
	{
		int[] asst;
		List<int>[] graph;
		List<Point> points = new List<Point>();

		public void solve(TextReader input, TextWriter output)
		{
			int n = Convert.ToInt32(input.ReadLine());

			graph = new List<int>[n];
			asst = new int[n];

			for (int i = 0; i < n; i++)
				graph[i] = new List<int>();

			for (int a0 = 0; a0 < n - 1; a0++)
			{
				// An edge connects nodes 'u' and 'v':
				string[] tokens_u = input.ReadLine().Split(' ');
				int u = Convert.ToInt32(tokens_u[0]) - 1;
				int v = Convert.ToInt32(tokens_u[1]) - 1;
				graph[u].Add(v);
				graph[v].Add(u);
			}

			var xmin = int.MaxValue;
			var ymin = int.MaxValue;
			var argmin = -1;
			for (int a0 = 0; a0 < n; a0++)
			{
				// Cartesian coordinate:
				string[] tokens_x = input.ReadLine().Split(' ');
				int x = Convert.ToInt32(tokens_x[0]);
				int y = Convert.ToInt32(tokens_x[1]);
				points.Add(new Point {X = x, Y = y, Index = a0});

				if (x < xmin || x == xmin && y < ymin)
				{
					xmin = x;
					ymin = y;
					argmin = a0;
				}
			}

			foreach (var p in points)
			{
				p.Radius = Math.Pow(p.X - xmin, 2) + Math.Pow(p.Y - ymin, 2);
				p.Angle = Math.Atan2(p.Y - ymin, p.X - xmin);
			}

			var pt = points[argmin];

			points.RemoveAt(argmin);

			points.Sort((a, b) =>
			{
				int cmp = a.Angle.CompareTo(b.Angle);
				if (cmp != 0) return cmp;
				return a.Radius.CompareTo(b.Radius);
			});

			points.Insert(0, pt);

			Approach1();

			output.WriteLine(string.Join(" ", asst.Select(x => x + 1)));
		}

		public void Approach1()
		{
			int index = 0;
			Dfs(0, -1, ref index);
		}

		public void Approach2()
		{
			var inputPoints = points.ToList();
			var outputPoints = new List<Point>();

			while (inputPoints.Count > 0)
			{
				var pts = ConvexHull(inputPoints);
				outputPoints.AddRange(pts);

				foreach (var p in pts)
					p.Delete = true;

				inputPoints.RemoveAll(p => p.Delete);
			}

			points = outputPoints;
			Approach1();
		}


		public void Dfs(int node, int parent, ref int index)
		{
			asst[node] = points[index++].Index;

			foreach (var child in graph[node])
			{
				if (child == parent) continue;
				Dfs(child, node, ref index);
			}
		}

		public class Point
		{
			public double X;
			public double Y;
			public double Angle;
			public double Radius;
			public int Index;
			public bool Delete;

			public override string ToString()
			{
				return $"({X}, {Y}) A={Angle} R={Radius}";
			}
		}


		private const double Eps = 1e-7;

		private static double Cross(Point p, Point q)
		{
			return p.X * q.Y - p.Y * q.X;
		}

		private static double Area2(Point a, Point b, Point c)
		{
			return Cross(a, b) + Cross(b, c) + Cross(c, a);
		}

		/// <summary>
		/// Compute the 2D convex hull of a set of points using the monotone chain
		/// algorithm.  
		/// Points property contains a List of points in the convex hull, counterclockwise, starting 
		/// with bottommost/leftmost point
		/// Running time: O(n log n)
		/// </summary>
		/// <param name="points">a List of input points, unordered.</param>
		/// <param name="removeRedundant">Eliminate redundant points from the hull</param>

		public List<Point> ConvexHull(IList<Point> points)
		{
			var pts = new List<Point>(points);
			pts.Sort();

			var up = new List<Point>();
			var dn = new List<Point>();
			for (var i = 0; i < pts.Count; i++)
			{
				while (up.Count > 1 && Area2(up[up.Count - 2], up[up.Count - 1], pts[i]) >= 0)
					up.RemoveAt(up.Count - 1);
				while (dn.Count > 1 && Area2(dn[dn.Count - 2], dn[dn.Count - 1], pts[i]) <= 0)
					dn.RemoveAt(dn.Count - 1);
				up.Add(pts[i]);
				dn.Add(pts[i]);
			}

			for (var i = up.Count - 2; i >= 1; i--)
				dn.Add(up[i]);

			return dn;
		}

	}

}