using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csharplib.Algorithms.Graphs;
using csharplib.Collections;
using static Library;


namespace csharplib
{

    public class Solution
    {

        #region Plain
        public void solve()
        {

        }
        #endregion

        #region Tests
        public void solveTests()
        {
            int tests = Ni();


            for (int t = 0; t <= tests; t++)
            {

            }
        }
        #endregion

        #region Queries

        int queryCount;
        long[] answers;
        private Query[] queries;

        public void solveQueries()
        {
            queryCount = Ni();

            queries = new Query[queryCount];
            answers = new long[queryCount];

            for (int q = 0; q < queryCount; q++)
            {
                var qu = queries[q] = new Query
                {
                    Index = q, A = Ni(), B = Ni(),
                };
            }

            foreach (var ans in answers)
                WriteLine(ans);
        }

        public class Query : IComparable<Query>
        {
            public int Index;
            public int A;
            public int B;

            public int CompareTo(Query other)
            {
                int cmp;
                cmp = A.CompareTo(other.A);
                if (cmp != 0) return cmp;
                cmp = B.CompareTo(other.B);
                return cmp;
            }

            public override string ToString()
            {
                return $"#{Index} ({A},{B})";
            }
        }
        #endregion





    }

    public class Program
    {

        #region Debugging
        public static void CatchErrors()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
            {
                var e = (Exception)arg.ExceptionObject;
                Sleep(GetLine(e) % 300);
                // Sleep(GetExceptionCode(e) * 10);
                Environment.Exit(1);
            };
        }

        public static int GetLine(Exception e)
        {
            var trace = new System.Diagnostics.StackTrace(e, true);
            int lineNumber = 0;
            for (int f = 0; lineNumber == 0 && f < trace.FrameCount; f++)
                lineNumber = trace.GetFrame(f).GetFileLineNumber();
            return lineNumber;
        }

        public static int GetExceptionCode(Exception e)
        {
            if (e is FormatException) return 1;
            if (e is InvalidCastException) return 2;
            if (e is NullReferenceException || e is ArgumentNullException) return 3;
            if (e is OverflowException) return 4;
            if (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException) return 5;
            if (e is InvalidOperationException) return 6;
            if (e is OutOfMemoryException) return 7;
            return 9;
        }

        public static void Sleep(int hundredths)
        {
            const int limit = 3000;
            var deadline = hundredths * 10 + 5; // shift of 5 or lower for best precision
            var process = System.Diagnostics.Process.GetCurrentProcess();

            // If we are over time, try holding out to the next second until 3 seconds
            while (process.TotalProcessorTime.TotalMilliseconds > deadline && deadline + 1000 < limit)
                deadline += 1000;

            while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(deadline, limit)) ;
        }
        #endregion

        public static void Main(string[] args)
        {
            InitInput(Console.OpenStandardInput());
            InitOutput(Console.OpenStandardOutput());
            var solution = new Solution();
            solution.solve();
            Flush();
#if DEBUG
            Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
        }
    }
}
