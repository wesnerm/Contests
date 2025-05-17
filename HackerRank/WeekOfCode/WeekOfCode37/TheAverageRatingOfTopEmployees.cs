using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Solution {

    // Complete the averageOfTopEmployees function below.
    static void averageOfTopEmployees(int[] rating) {

        decimal result = 0;
        int count = 0;
        foreach(var r in rating)
        {
            if (r >= 90)
            {
                result += r;
                count ++;
            }
        }
        
        decimal avgRating = result/count;
        Console.WriteLine(avgRating.ToString("F2"));
    }

    static void Main(string[] args) {
        int n = Convert.ToInt32(Console.ReadLine().Trim());

        int[] rating = new int [n];

        for (int ratingItr = 0; ratingItr < n; ratingItr++) {
            int ratingItem = Convert.ToInt32(Console.ReadLine().Trim());
            rating[ratingItr] = ratingItem;
        }

        averageOfTopEmployees(rating);
    }
}
