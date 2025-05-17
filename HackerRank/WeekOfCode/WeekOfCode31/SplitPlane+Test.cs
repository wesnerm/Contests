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

		void Test()
		{
			while (true)
			{
				RandomSegments(10, 5);
				Array.Sort(segments);
				CompressCoordinates();

				var faces = ProcessSegments();
				WriteLine(faces);

#if DEBUG
				long faces2 = BruteForce();
				WriteLine(faces);
				Debug.Assert(faces == faces2);
#endif
			}
		}


		void RandomSegments(int width, int edges)
		{
			segments = new Segment[edges];
			var random = new Random();

			for (int i=0; i<edges; i++)
			{

				var x1 = random.Next(0, width);
				var x2 = random.Next(0, width);
				var y = random.Next(0, width);
				var h = random.Next(0, 2) == 0;
				if (x1 == x2)
				{
					i--; continue;
				}

				if (x1 > x2)
				{
					var tmp = x1;
					x1 = x2;
					x2 = tmp;
				}


				segments[i] = new Segment()
				{
					Id = i,
					X1 = h ? x1 : y,
					X2 = h ? x2 : y,
					Y1 = h ? y : x1,
					Y2 = h ? y : x2,
					Horiz = h,
				};
			}

		}


		public int ConnectedSegments(DisjointSet ds)
		{
			int hcount = 0;
			foreach (var s in segments)
			{
				if (s.Horiz)
					hcount++;
			}

			int h = 0;
			int v = 0;
			int vcount = segments.Length - hcount;
			var horiz = new Segment[hcount];
			var vert = new Segment[vcount];
			foreach (var s in segments)
			{
				if (s.Horiz)
					horiz[h++] = s;
				else
					vert[v++] = s;
			}

			for (int i = 0; i < hcount; i++)
			{
				var si = horiz[i];

				int left = 0;
				int right = vcount - 1;
				while (left <= right)
				{
					int mid = left + (right - left >> 1);
					if (si.X1 > vert[mid].X1)
						left = mid + 1;
					else
						right = mid - 1;
				}

				for (int j = left; j < vcount; j++)
				{
					var sj = vert[j];
					if (sj.X1 > si.X2) break;
					if (si.Y1 >= sj.Y1 && si.Y1 <= sj.Y2)
						ds.Union(si.Id, sj.Id);
				}
			}

			return ds.Count;
		}
	}


}