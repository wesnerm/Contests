using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace csharplib.Library
{
    class Timers
    {
        public void Report(int wait)
        {
            wait = wait * 10 + 5;
            // WriteLine(wait);
            // Flush();
            var process = Process.GetCurrentProcess();
            while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 8000) wait += 1000;
            while (process.TotalProcessorTime.TotalMilliseconds < Math.Min(wait, 8000)) ;
            Environment.Exit(0);
        }

        public void EDD(int n)
        {
            n = Abs(n);

            int e = n == 0 ? 0 : (int)Log10(n);
            int m = n;
            if (m != 0)
            {
                while (m >= 100) m /= 10;
                while (m * 10 < 100) m *= 10;
            }

            if (e == 0 && m == 10 && n == (int)Pow(10, e)) m = 5;
            if (e == 0 && m % 10 == 0) m += 5;
            var x = e < 7 ? e % 10 * 100 + m : 700 + ((e - 7) % 10) * 10 + m / 10;

            Report(x);
        }

    }
}
