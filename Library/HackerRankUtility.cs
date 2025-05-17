using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csharplib
{
    static class HackerRankUtility
    {
        public static int TestCase()
        {
            return Directory.EnumerateFiles(".", "input*.in").
                Max(f => int.Parse(f.Substring(f.Length - 8, 5)));
        }

        #region Input
        private static string[] Input;
        private static int InputIndex;

        public static string[] NextList()
            => Console.ReadLine()?.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

        public static int[] NextInts() => Array.ConvertAll(NextList(), int.Parse);

        public static string Next()
        {
            for (; Input != null; Input = NextList(), InputIndex = 0)
                if (InputIndex < Input.Length)
                    return Input[InputIndex++];
            return null;
        }

        #endregion

        #region Stack
        public static int GetStackSize(Thread thread)
        {
            FieldInfo threadField = typeof(Thread).GetField("internal_thread",
                BindingFlags.NonPublic | BindingFlags.Instance);
            object internalThread = threadField.GetValue(thread);
            Type itType = internalThread.GetType();
            FieldInfo stackField = itType.GetField("stack_size", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)stackField.GetValue(internalThread);
        }

        // Standard size -- 1 in .NET32, 2 in MONO, 4 in .NET64
        // 32Mb is very safe; allows 39 variables per frame
        public static void GrowStack(ThreadStart action, int stackSize = 32 * 1024 * 1024)
        {
            var thread = new Thread(action, stackSize);
#if __MonoCS__
        const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
        var it = typeof(Thread).GetField("internal_thread", bf).GetValue(thread);
        it.GetType().GetField("stack_size", bf).SetValue(it, stackSize);
#endif
            thread.Start();
            thread.Join();
        }
        #endregion

        #region Check For Mono
        public static bool IsRunningOnMono()
        {
#if __MonoCS__
            return true;
#else
            return false;
#endif
        }

        public static bool IsRunningOnMono2()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
        #endregion

        #region C7

        #endregion


        #region Debugging

        static void CatchErrors()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
            {
                var e = (Exception)arg.ExceptionObject;
                Console.Error.WriteLine(e);
                int line = new StackTrace(e, true).GetFrames()
                    .Select(x => x.GetFileLineNumber()).FirstOrDefault(x => x != 0);
                int wait = Math.Min(3000, line % 300 * 10 + 5);
                var process = Process.GetCurrentProcess();
                while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 3000) wait += 1000;
                while (process.TotalProcessorTime.TotalMilliseconds < wait) continue;
                Environment.Exit(1);
            };
        }

        static void CatchExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
            {
                var e = (Exception)arg.ExceptionObject;
                int wait = e is FormatException ? 1
                : e is InvalidCastException ? 2
                : e is NullReferenceException || e is ArgumentNullException ? 3
                : e is OverflowException ? 4
                : e is IndexOutOfRangeException || e is ArgumentOutOfRangeException ? 5
                : e is InvalidOperationException ? 6
                : e is OutOfMemoryException ? 7
                : e is StackOverflowException ? 15 : 19;
                var process = Process.GetCurrentProcess();
                while (process.TotalProcessorTime.TotalMilliseconds < wait * 100 + 1006) continue;
                Environment.Exit(1);
            };
        }

        static void Sleep(long amount)
        {
            const int MaxTime = 10000;
            const int bump = 1000;

            long wait = Math.Min(MaxTime, amount * 10 + 5);
            var process = Process.GetCurrentProcess();
            while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < MaxTime - bump) wait += bump;
            while (process.TotalProcessorTime.TotalMilliseconds < wait) continue;
            Environment.Exit(1);
        }


        public static long ShowDDD(long x)
        {
            x %= 1000;
            Sleep(x);
            return x;
        }

        public static long ShowXX(long x)
        {
            return ShowDD((x % 300) * 3 + 100);
        }

        public static long ShowDD(long x)
        {
            return ShowDDD((x % 100) * 10);
        }

        public static long Show7DD(long x, long mod = 7)
        {
            long dd = x % 100;
            long ddd = dd % mod * 100 + dd;
            return ShowDDD(ddd);
        }

        public static long ShowLogEDD(long n)
        {
            n = Math.Abs(n);

            var e = n != 0 ? (long)Math.Log10(n) : 0;

            long m = Math.Abs(n); // # mantissa
            if (m != 0)
            {
                while (m >= 100) m /= 10;
                while (m * 10 < 100) m *= 10;
            }

            // Helps distinguish round numbers
            if (e > 0 && m == 10 && n == (long)Math.Pow(10, e))
                m = 5;

            // For integers n<10, unit digit of mantissa is always zero, shift by 5
            if (e == 0 && m % 10 == 0)
                m += 5;

            return ShowDDD(e < 9 ? e % 10 * 100 + m : 900 + (e % 10) * 10 + (m / 10));
        }


        #endregion
    }
}
