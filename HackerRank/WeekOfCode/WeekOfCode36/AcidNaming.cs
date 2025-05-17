using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution {

    static string acidNaming(string acid_name) {
        if (!acid_name.EndsWith("ic"))
            return "not an acid";
        if (acid_name.StartsWith("hydro"))
            return "non-metal acid";
         return "polyatomic acid";
    }

    static void Main(String[] args) {
        int n = Convert.ToInt32(Console.ReadLine().Trim());
        for(int a0 = 0; a0 < n; a0++){
            string acid_name = Console.ReadLine().Trim();
            string result = acidNaming(acid_name);
            Console.WriteLine(result);
        }
    }
}
