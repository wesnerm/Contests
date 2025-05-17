using System;

class Reporting
{
    #region Reporting Answer

    static System.Threading.Timer _timer;
    public static System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
    public static double Elapsed => process.TotalProcessorTime.TotalMilliseconds;

    public static void LaunchTimer(Func<bool> action, long ms = 2900)
    {
        ms -= (long)Elapsed + 1;
        _timer = new System.Threading.Timer(delegate {
#if !DEBUG
                if (action()) Environment.Exit(0);
#endif
        }, null, ms, 0);
    }

    public static void Run(string name, Action action)
    {
#if DEBUG
        System.Console.Error.Write(name + ": ");
        var start = Elapsed;
#endif
        action();
#if DEBUG
        System.Console.Error.WriteLine($"Elapsed Time: {Elapsed - start}\n");
#endif
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Run2(string name, Action action) => GC.KeepAlive(action);

    #endregion
}
