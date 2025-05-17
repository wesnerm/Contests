using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

class Solution {

    // Complete the whichSection function below.
    static int whichSection(int n, int k, int[] a) {
        k--;
        
        int section = 0;
        foreach(var c in a)
        {
            section++;
            k-=c;
            if (k<0) break;
        }
        
        return section;
    }

    static void Main(string[] args) {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        int t = Convert.ToInt32(Console.ReadLine());

        for (int tItr = 0; tItr < t; tItr++) {
            string[] nkm = Console.ReadLine().Split(' ');

            int n = Convert.ToInt32(nkm[0]);

            int k = Convert.ToInt32(nkm[1]);

            int m = Convert.ToInt32(nkm[2]);

            int[] a = Array.ConvertAll(Console.ReadLine().Split(' '), aTemp => Convert.ToInt32(aTemp))
            ;
            int result = whichSection(n, k, a);

            textWriter.WriteLine(result);
        }

        textWriter.Flush();
        textWriter.Close();
    }
}
