using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

// Disable 'unreachable code' warning
#pragma warning disable 162

static partial class CaideTester
{
    public static bool ENABLE_CUSTOM_CHECKER = true;
    public static double Area = 0;
    public static double TotalArea = 0;

    public static bool CustomCheck(TextReader input, TextReader output)
    {
        int n = int.Parse(input.ReadLine());
        var pts = new int[n+1][];

        for (int i = 1; i <= n; i++)
            pts[i] = Array.ConvertAll(input.ReadLine().Split(), int.Parse);

        int k = int.Parse(output.ReadLine());
        var good = true;
        var counts = new int[n+1];
        Area = 0;
        for (int i = 1; i <= k; i++)
        {
            var array = Array.ConvertAll(output.ReadLine().Split(), int.Parse);
            if (array[0] + 1 != array.Length)
            {
                System.Console.Error.WriteLine($"{i} partition length does not match");
                good = false;
            }

            var elements = array.Skip(1).ToArray();

            int len = elements.Length;
            if (len <= 0 || len > n)
            {
                System.Console.Error.WriteLine($"Length of partition {i} is out of range {len}");
                good = false;
                continue;
            }

            foreach (var v in elements)
            {
                if (v < 1 || v > n)
                {
                    System.Console.Error.WriteLine($"Point {v} is out of range");
                    good = false;
                    continue;
                }

                counts[v]++;
            }


            if (elements.Length > 2)
            {
                var plist = elements.Select(x =>
                        new Point2D {X = pts[x][0], Y = pts[x][1]})
                    .ToList();
                var cv = new ConvexHull(plist);
                if (cv.Points.Count < elements.Length)
                    System.Console.WriteLine($"Some points are inside hull in partition {i}");
                var area = ConvexHull.Area(cv.Points);
                Area += area;
            }
            else
            {
                System.Console.WriteLine("Unused points: " + string.Join(" ",
                                             elements.Select(x => x + 1)));
            }
        }

        var count = counts.Sum();
        if (count != n)
        {
            System.Console.Error.WriteLine($"{count} out of {n} points written");
            good = false;
        }

        TotalArea += Area;
        System.Console.Error.WriteLine($"Area is {Area} / {TotalArea}");
        return good;
    }

    public class ConvexHull
    {
        public readonly List<Point2D> Points;

        public ConvexHull(List<Point2D> points)
        {
            points.Sort(Compare);

            var uniqueEnd = Unique(points);
            points.RemoveRange(uniqueEnd, points.Count - uniqueEnd);

            var up = new List<Point2D>();
            var down = new List<Point2D>();
            for (var i = 0; i < points.Count; i++)
            {
                while (up.Count > 1 && Area2(up[up.Count - 2], up[up.Count - 1], points[i]) > 0) up.RemoveAt(up.Count - 1);
                while (down.Count > 1 && Area2(down[down.Count - 2], down[down.Count - 1], points[i]) < 0) down.RemoveAt(down.Count - 1);
                up.Add(points[i]);
                down.Add(points[i]);
            }

            var hashSet = new HashSet<Point2D>(points);
            hashSet.Clear();
            hashSet.UnionWith(down);
            for (var i = up.Count - 2; i >= 1; i--)
            {
                if (hashSet.Contains(up[i])) continue;
                down.Add(up[i]);
            }

            Points = down;
        }

        public static int Compare(Point2D a, Point2D b)
        {
            int cmp = a.X.CompareTo(b.X);
            if (cmp != 0) return cmp;
            return a.Y.CompareTo(b.Y);
        }

        public static int Unique(List<Point2D> list)
        {
            var write = Math.Min(list.Count, 1);
            for (int read = 1; read < list.Count; read++)
            {
                if (Compare(list[write - 1], list[read]) != 0)
                    list[write++] = list[read];
            }

            list.RemoveRange(write, list.Count - write);
            return write;
        }

        #region Helpers

        const double Eps = 1e-9;

        static bool Between(Point2D a, Point2D b, Point2D c) =>
            Math.Abs(Area2(a, b, c)) < Eps
            && (a.X - b.X) * (c.X - b.X) <= 0
            && (a.Y - b.Y) * (c.Y - b.Y) <= 0;

        static double Cross(Point2D p, Point2D q) => p.X * q.Y - p.Y * q.X;

        static double Area2(Point2D a, Point2D b, Point2D c)
            => Cross(a, b) + Cross(b, c) + Cross(c, a);

        // Probably the same as Area2
        static double Cross(Point2D a, Point2D b, Point2D c)
            => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

        public static double Area(IList<Point2D> p)
        {
            double area = 0;
            if (p.Count < 3) return 0;

            for (var i = 0; i < p.Count; i++)
            {
                var j = (i + 1) % p.Count;
                //area += p[i].X * p[j].Y - p[j].X * p[i].Y;
                area += Cross(p[i], p[j]);
            }
            return Math.Abs(area / 2.0);
        }

        #endregion
    }

    public struct Point2D : IEquatable<Point2D>
    {
        public double X, Y;

        public bool Equals(Point2D other) => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object obj)
            => obj is Point2D && Equals((Point2D)obj);

        public override int GetHashCode()
            => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
    }


}

// Redefine Console
class Console
{
	public static Stream StandardInput;
	public static Stream StandardOutput;

	public static TextReader In;
	public static TextWriter Out;
	public static TextWriter Error => System.Console.Error;
	
	public static Stream OpenStandardInput() => StandardInput;
	public static Stream OpenStandardOutput() => StandardOutput;
}

public class Test
{
    private static string RunTest(string inputFileName, string outputFileName)
    {
		var process = System.Diagnostics.Process.GetCurrentProcess();
        using (var inputStream = File.Open(inputFileName, FileMode.Open))
        using (var outputStream = File.Open(outputFileName, FileMode.Create))
        {
			Console.StandardInput = inputStream;
			Console.StandardOutput = outputStream;
			Console.In = new StreamReader(inputStream);
			Console.Out = new StreamWriter(outputStream);

            var bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var type = Assembly.GetEntryAssembly()
                .GetTypes().First(t => t.Name != "Test" && t.GetMethod("Main", bf) != null);

            var startTime = process.TotalProcessorTime;
            type.GetMethod("Main", bf)?.Invoke(null, null);

#if DEBUG
            Console.Error.WriteLine($"Elapsed Time: {process.TotalProcessorTime - startTime}");
#endif

			Console.Out.Flush();
			Console.In = null;
			Console.Out = null;
			Console.StandardInput = null;
			Console.StandardOutput = null;
        }

        var result = File.ReadAllText(outputFileName);
        return result;
    }


    //-----------------------------------------------------------------------------//
    private static Process Run(string exe, string args)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
            WorkingDirectory = Directory.GetCurrentDirectory(),
        };
        return Process.Start(psi);
    }


    public static void Main(string[] args)
    {
        // Read path to caide executable from a file in current directory
        string testDir = ".";
        string caideExeFile = Path.Combine(testDir, "caideExe.txt");
        if (!File.Exists(caideExeFile))
        {
            testDir = Path.Combine(".caideproblem", "test");
            caideExeFile = Path.Combine(testDir, "caideExe.txt");
        }
        if (!File.Exists(caideExeFile))
        {
            throw new InvalidOperationException("Test musts be run from problem directory");
        }
        string caideExe = File.ReadAllText(caideExeFile).Trim();

        // Prepare the list of test cases in correct order; add recently created test cases too.
        Process updateTestsProcess = Run(caideExe, "update_tests");
        updateTestsProcess.WaitForExit();
        if (updateTestsProcess.ExitCode != 0)
        {
            Console.Error.WriteLine("caide update_tests returned non-zero error code " + updateTestsProcess.ExitCode);
        }

        StringWriter report = new StringWriter();

        // Process each test case described in a file in current directory
        foreach (string line in File.ReadAllLines(Path.Combine(testDir, "testList.txt")))
        {
            string[] words = line.Split(' ');
            string testName = words[0], testState = words[1];
            if (testState == "Skip")
            {
                Console.Error.WriteLine("Skipping test " + testName);
                report.WriteLine(testName + " skipped");
            }
            else if (testState == "Run")
            {
                Console.Error.WriteLine("Running test " + testName);
                string inputFile = Path.Combine(testDir, testName + ".in");

                string result = null;
                try
                {
                    result = RunTest(inputFile, Path.Combine(testDir, testName + ".out"));
                }
                catch
                {
                    Console.Error.WriteLine("Test " + testName + " threw an exception");
                    report.WriteLine(testName + " failed");
                    continue;
                }

                if (CaideTester.ENABLE_CUSTOM_CHECKER)
                {
                    try
                    {
                        using (StringReader output = new StringReader(result))
                        using (StreamReader input = new StreamReader(inputFile))
                        {
                            bool ok = CaideTester.CustomCheck(input, output);
                            if (ok)
                            {
                                report.WriteLine(testName + " OK");
                            }
                            else
                            {
                                Console.Error.WriteLine("Test " + testName + " failed!");
                                report.WriteLine(testName + " failed");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Checker for test " + testName + " threw an exception: " + e.Message);
                        report.WriteLine(testName + " error");
                    }
                }
                else
                {
                    report.WriteLine(testName + " ran");
                }

                if (result.Length > 200)
                    result = result.Substring(0, 200) + " [...] (output truncated)\n";
                Console.Error.WriteLine(result);
            }
            else
            {
                report.WriteLine(testName + " error unknown test status");
            }
        }

        File.WriteAllText(Path.Combine(testDir, "report.txt"), report.ToString());

        // optional: evaluate tests automatically
        Process evalTestsProcess = Run(caideExe, "eval_tests");
        evalTestsProcess.WaitForExit();
        if (evalTestsProcess.ExitCode != 0)
        {
            Console.Error.WriteLine("Tests failed!");
            Environment.Exit(evalTestsProcess.ExitCode);
        }
    }
}

partial class CaideTester
{
   public const bool IS_TOPCODER_PROBLEM = false;

   public static void TopcoderSolve(TextReader input, TextWriter output) {
   }

   private static void ReadIfTopcoderProblem(TextReader input, TextReader output) {
   }
}
