using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Softperson.ComputationalGeometry;

namespace HackerRank.AdInfinitum17
{

    public static class PolygonClipping
    {

        private class Edge
        {
            public Edge(Point2D from, Point2D to)
            {
                From = from;
                To = to;
            }

            public Point2D From;
            public Point2D To;
        }

        public static Point2D[] GetIntersectedPolygon(Point2D[] subjectPoly, Point2D[] clipPoly)
        {
            if (subjectPoly.Length < 3 || clipPoly.Length < 3)
                return subjectPoly;

            var outputList = subjectPoly.ToList();

            //	Make sure it's clockwise
            if (!IsClockwise(subjectPoly))
                outputList.Reverse();

            //	Walk around the clip polygon clockwise
            foreach (var clipEdge in IterateEdgesClockwise(clipPoly))
            {
                var inputList = outputList;
                if (inputList.Count == 0)
                    break;

                outputList = new List<Point2D>(inputList.Count);

                var s = inputList[inputList.Count - 1];
                foreach (Point2D e in inputList)
                {
                    if (IsInside(clipEdge, e))
                    {
                        if (!IsInside(clipEdge, s))
                        {
                            Point2D? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);
                            if (point == null)
                            {
                                // throw new ApplicationException("Line segments don't intersect");
                                outputList.Add(e);
                            }
                            //	may be colinear, or may be a bug
                            else
                                outputList.Add(point.Value);
                        }

                        outputList.Add(e);
                    }
                    else if (IsInside(clipEdge, s))
                    {
                        Point2D? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);
                        if (point == null)
                            throw new ApplicationException("Line segments don't intersect");
                        //	may be colinear, or may be a bug
                        outputList.Add(point.Value);
                    }

                    s = e;
                }
            }

            return outputList.ToArray();
        }

        private static IEnumerable<Edge> IterateEdgesClockwise(Point2D[] polygon)
        {
            if (IsClockwise(polygon))
            {
                for (int cntr = 0; cntr < polygon.Length - 1; cntr++)
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                yield return new Edge(polygon[polygon.Length - 1], polygon[0]);

            }
            else
            {
                for (int cntr = polygon.Length - 1; cntr > 0; cntr--)
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                yield return new Edge(polygon[0], polygon[polygon.Length - 1]);
            }
        }

        private static Point2D? GetIntersect(Point2D line1From, Point2D line1To, Point2D line2From, Point2D line2To)
        {
            var direction1 = line1To - line1From;
            var direction2 = line2To - line2From;
            double dotPerp = (direction1.X * direction2.Y) - (direction1.Y * direction2.X);

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if (IsNearZero(dotPerp))
                return null;

            var c = line2From - line1From;
            double t = (c.X * direction2.Y - c.Y * direction2.X) / dotPerp;
            //if (t < 0 || t > 1)
            //    return null;		//	lies outside the line segment

            //double u = (c.X * direction1.Y - c.Y * direction1.X) / dotPerp;
            //if (u < 0 || u > 1)
            //    return null;		//	lies outside the line segment

            //	Return the intersection point
            return line1From + direction1* t;
        }

        private static bool IsInside(Edge edge, Point2D test)
        {
            return IsLeftOf(edge, test) != true;
        }

        private static bool IsClockwise(Point2D[] polygon)
        {
            for (int cntr = 2; cntr < polygon.Length; cntr++)
            {
                bool? isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                if (isLeft != null)
                    //	some of the points may be colinear.  That's ok as long as the overall is a polygon
                    return !isLeft.Value;
            }
            return true;
        }

        private static bool? IsLeftOf(Edge edge, Point2D test)
        {
            var tmp1 = edge.To - edge.From;
            var tmp2 = test - edge.To;

            double x = (tmp1.X * tmp2.Y) - (tmp1.Y * tmp2.X); //	dot product of perpendicular?

	        return x < 0 ? false : (x > 0 ? (bool?) true : null);
        }

        private static bool IsNearZero(double testValue)
        {
            return Math.Abs(testValue) <= 1e-9;
        }


    }


}
