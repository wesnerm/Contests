
namespace HackerRank.WorldCodeSprint8
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class TreeCoordinates
    {
        private static DateTime date;

        static void Main(string[] args)
        {
            Console.SetIn(File.OpenText("d:\\test\\input02.txt"));


            var input = Console.ReadLine().Split();
            int n = int.Parse(input[0]);
            int m = int.Parse(input[1]);
            var edges = Enumerable.Range(0, n - 1).Select(x => Console.ReadLine().Split().Select(int.Parse).ToArray()).ToArray();
            var points = Enumerable.Range(0, m).Select(x => Console.ReadLine().Split().Select(int.Parse).ToArray()).ToList();

/*            int[][] edges;
            List<int[]> points;
            int n;
            CreateSampleEdges(out edges, out points, out n);
            */

            date = DateTime.Now;

            var result = new TreeCoordinates(edges, points, n).Solve();
            Console.WriteLine(result);

        }

        private static int maxDist;
        private static System.Threading.Timer timer;

        public static void LaunchTimer()
        {
            // Use a timer to quit the search
            timer = new System.Threading.Timer(
                delegate
                {
                    if (maxDist != 0)
                    {
                        Console.WriteLine(maxDist);
                        Environment.Exit(0);
                    }
                }, null, 2000, 0);
        }


        private Tree[] nodes;
        private List<int[]> points;
        private Dictionary<int, Cluster> clusters;

        TreeCoordinates(int[][] edges, List<int[]> points, int n)
        {
            this.nodes = new Tree[n + 1];
            this.points = points;
            Build(edges, n);
        }


        public static void CreateSampleEdges(out int[][] edges, out List<int[]> points, out int n)
        {
            var elist = new List<int[]>();
            var plist = new List<int[]>();


            int vertex = 2;
            for (int i = 0; i < 4; i++)
            {
                int prev = 1;
                for (int j = 0; j < 1000; j++)
                {
                    elist.Add(new int[] {prev, vertex});
                    prev = vertex++;
                }
            }

            var random = new Random();
            for (int i = 0; i < 3000; i++)
            {
                elist.Add(new int[] {random.Next(1, vertex - 1), vertex++});
            }

            for (int i = 0; i < 7500; i++)
                plist.Add(new int[] {random.Next(1, vertex), random.Next(1, vertex)});

            edges = elist.ToArray();
            points = plist;
            n = vertex - 1;
        }

        private int[] ds;

        public void BuildClusters(Dictionary<int, List<int>> graph, int n)
        {
            int targetSize = (int) (2*Math.Sqrt(n));

            // Record leaves
            var queue = new Queue<int>();
            var degree = new int[n + 1];

            foreach (var pair in graph)
            {
                var v = pair.Key;
                var count = degree[v] = pair.Value.Count;
                if (count != 1) continue;
                degree[v] = 0;
                queue.Enqueue(v);
            }

            // Create Clusters of Sqrt(n)
            int[] counts;
            ds = CreateDisjointSet(n + 1, out counts);

            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var v2 in graph[v])
                {
                    var r1 = Find(ds, v);
                    var r2 = Find(ds, v2);
                    if (counts[r1] + counts[r2] < targetSize)
                        Union(ds, counts, v, v2);
                    if (--degree[v2] == 1)
                    {
                        degree[v2] = 0;
                        queue.Enqueue(v2);
                    }
                }
            }

            int id = 0;
            clusters = new Dictionary<int, Cluster>();
            for (int i = 1; i <= n; i++)
            {
                var r = Find(ds, i);
                var node = nodes[r];
                if (!clusters.ContainsKey(r))
                    clusters[r] = new Cluster() {Id = id++, Vertex = r, Depth = node.Depth};
                clusters[r].List.Add(i);
            }
        }

        private List<PointCluster> pointClusters;

        void BuildPointClusters()
        {
            var dict = new Dictionary<Tuple<int, int>, PointCluster>();
            foreach (var p in points)
            {
                var x = p[0];
                var y = p[1];
                var cx = VertexToCluster(x);
                var cy = VertexToCluster(y);
                var tuple = new Tuple<int,int>(cx,cy);
                if (!dict.ContainsKey(tuple))
                    dict[tuple]= new PointCluster()
                    {
                        X = cx,
                        Y = cy,
                        Depth = nodes[x].Depth + nodes[y].Depth,
                    };
                dict[tuple].Points.Add(p);
            }

            pointClusters = new List<PointCluster>(dict.Values);
        }

        public int VertexToCluster(int v)
        {
            int r = Find(ds, v);
            return clusters[r].Id;
        }


        public class PointCluster
        {
            public int X;
            public int Y;
            public int Depth;
            public List<int[]> Points = new List<int[]>();
        }

        public class Cluster
        {
            public int Id;
            public int Vertex;
            public int Depth;
            public List<int> List = new List<int>();
        }

        public int FindBalancedRoot(Dictionary<int, List<int>> graph, int n)
        {
            // Find Balanced Root O(V) -- it gives us good distance metrics
            // -- start from leaves
            // -- last queued item is the root
            var queue = new Queue<int>();
            var degree = new int[n + 1];

            foreach (var pair in graph)
            {
                var v = pair.Key;
                var count = degree[v] = pair.Value.Count;
                if (count != 1) continue;
                degree[v] = 0;
                queue.Enqueue(v);
            }

            int root = -1;
            while (queue.Count > 0)
            {
                root = queue.Dequeue();
                foreach (var v2 in graph[root])
                    if (--degree[v2] == 1) // Might still be buggy
                    {
                        degree[v2] = 0;
                        queue.Enqueue(v2);
                    }
            }

            return root;
        }


        public static Dictionary<int, List<int>> Clone(Dictionary<int, List<int>> graph)
        {
            var dict = new Dictionary<int, List<int>>(graph.Count);
            foreach (var pair in graph)
                dict[pair.Key] = new List<int>(pair.Value);
            return dict;
        }


        public static int[] CreateDisjointSet(int size, out int[] counts)
        {
            int[] set = new int[size];
            counts = new int[size];
            for (int i = 0; i < size; i++)
            {
                set[i] = i;
                counts[i] = 1;
            }
            return set;
        }


        public static bool Union(int[] ds, int[] counts, int a, int b)
        {
            int ra = Find(ds, a);
            int rb = Find(ds, b);
            if (ra == rb)
                return false;

            if (ra < rb)
            {
                ds[rb] = ra;
                counts[ra] += counts[rb];
            }
            else
            {
                ds[ra] = rb;
                counts[rb] += counts[ra];
            }
            return true;
        }

        public static int Find(int[] ds, int a)
        {
            int r = ds[a];
            if (r != a)
                ds[a] = r = Find(ds, r);
            return r;
        }

        public bool tryOptimal = true;

        public void Build(int[][] edges, int n)
        {
            var graph = BuildEdges(edges, n);
            var root = FindBalancedRoot(graph, n);
            var tree = BuildTree(graph, root);

            BuildClusters(graph, n);


        }

        public Tree BuildTree(Dictionary<int, List<int>> edges, int parent)
        {
            var queue = new Queue<Tree>();

            var tree = nodes[parent] = new Tree(parent, null);
            queue.Enqueue(tree);

            var children = new List<Tree>();
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                children.Clear();
                foreach (var c in edges[p.Id])
                {
                    if (nodes[c] != null) continue;

                    var child = nodes[c] = new Tree(c, p);
                    queue.Enqueue(child);
                }
                p.children = children.ToArray();
            }
            return tree;
        }

        public Dictionary<int, List<int>> BuildEdges(int[][] edges, int n)
        {
            var dict = Enumerable.Range(1, n).ToDictionary(x => x, x => new List<int>());

            foreach (var e in edges)
            {
                dict[e[0]].Add(e[1]);
                dict[e[1]].Add(e[0]);
            }
            return dict;
        }


        public int Distance(int a, int b)
        {
            var p = Tree.Lca(nodes[a], nodes[b]);
            return nodes[a].Depth + nodes[b].Depth - 2*p.Depth;
        }

        public class Tree
        {
            public static int AncestorCount = 17;
            public int Id;
            public int Depth;
            public Tree[] ancestors;
            public Tree[] children;

            public Tree(int id, Tree parent)
            {
                ancestors = new Tree[AncestorCount];
                if (parent == null)
                {
                    for (int i = 0; i < ancestors.Length; i++)
                        ancestors[i] = this;
                }
                else
                {
                    ancestors[0] = parent;
                    for (int i = 1; i < ancestors.Length; i++)
                        ancestors[i] = ancestors[i - 1].ancestors[i - 1];
                }

                Id = id;
                Depth = parent != null ? parent.Depth + 1 : 0;
            }

            public static Tree Lca(Tree node1, Tree node2)
            {
                if (node1.Depth < node2.Depth)
                    Swap(ref node1, ref node2);


                for (int i = node1.ancestors.Length - 1; i >= 0; i--)
                {
                    if (node1.Depth - (1 << i) >= node2.Depth)
                        node1 = node1.ancestors[i];
                }

                if (node1 == node2)
                    return node1;

                for (int i = node1.ancestors.Length - 1; i >= 0; i--)
                {
                    if (node1.ancestors[i] != null && node1.ancestors[i] != node2.ancestors[i])
                    {
                        node1 = node1.ancestors[i];
                        node2 = node2.ancestors[i];
                    }
                }

                return node1.ancestors[0];
            }
        }

        public class PointPairing
        {
            public int index1;
            public int index2;
            public int distance;
        }

        public int PointDistance(int[] point1, int[] point2)
        {
            int xdist = Distance(point1[0], point2[0]);
            int ydist = Distance(point1[1], point2[1]);
            var dist = xdist + ydist;
            return dist;
        }

        long Solve()
        {
            if (tryOptimal)
            {
                    return Solve1();
            }

            return Solve2();
        }


        long Solve1()
        {
            BuildPointClusters();

            var pairings = new List<PointPairing>();

            // Sorted in DESCENDING ORDER by depth
            // pointClusters.Sort((a, b) => b.Depth.CompareTo(a.Depth));

            for (int i =0; i<pointClusters.Count; i++)
                for (int j = 0; j < pointClusters.Count; j++)
                {
                    var pi = pointClusters[i].Points[0];
                    var pj = pointClusters[j].Points[0];
                    var dist = PointDistance(pi, pj);
                    maxDist = Math.Max(maxDist, dist);

                    pairings.Add(new PointPairing
                    {
                        index1 = i,
                        index2 = j,
                        distance = dist,
                    });
                }

            pairings.Sort((a,b)=> -a.distance.CompareTo(b.distance));


            foreach (var pairing in pairings)
            {
                var points1 = pointClusters[pairing.index1].Points;
                var points2 = pointClusters[pairing.index2].Points;
                for (int i = 0; i < points1.Count; i++)
                    for (int j = 0; j < points2.Count; j++)
                    {
                        var dist = PointDistance(points1[i], points2[j]);
                        maxDist = Math.Max(maxDist, dist);
                    }
            }

            return maxDist;
        }

        long Solve2()
        {
            // Approach 1: Balance the tree by longest path

            // Approach 2: Sort by result

            points.Sort((a, b) =>
            {
                int depth1 = nodes[a[0]].Depth + nodes[a[1]].Depth;
                int depth2 = nodes[b[0]].Depth + nodes[b[1]].Depth;
                return -depth1.CompareTo(depth2);
            });

            int minSize = Math.Min(50, points.Count);

            for (int i = 0; i < minSize; i++)
                for (int j = i + 1; j < minSize; j++)
                {
                    int xdist = Distance(points[i][0], points[j][0]);
                    int ydist = Distance(points[i][1], points[j][1]);
                    var dist = xdist + ydist;
                    maxDist = Math.Max(maxDist, dist);
                }

            for (int i = 0; i < points.Count; i++)
                for (int j = Math.Max(minSize, i + 1); j < points.Count; j++)
                {
                    int xdist = Distance(points[i][0], points[j][0]);
                    int ydist = Distance(points[i][1], points[j][1]);
                    var dist = xdist + ydist;
                    maxDist = Math.Max(maxDist, dist);
                }

            return maxDist;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
    }
}
