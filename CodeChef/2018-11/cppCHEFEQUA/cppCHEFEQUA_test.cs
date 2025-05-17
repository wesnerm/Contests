using System;
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
    public static bool ENABLE_CUSTOM_CHECKER = false;
    public static bool CustomCheck(TextReader input, TextReader output)
    {
        // Implement the checker here.
        // Use static variables of this class
        // use input and output streams for a classical problem.
        // Return true if the result is correct.

        return true;
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

                if (result.Length > 500)
                    result = result.Substring(0, 500) + " [...] (output truncated)\n";
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
