namespace HackerRank.Walmart
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Delivery
	{
		static void Main(String[] args)
		{
			/* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */

			int[] array = Console.ReadLine().Split().Select(int.Parse).ToArray();
			int n = array[0];
			int m = array[1];
			int q = array[2];

			var foodMap = new HashSet<int>[m + 1];
			for (int i = 0; i < m; i++)
			{
				array = Console.ReadLine().Split().Skip(1).Select(int.Parse).ToArray();
				foodMap[i + 1] = new HashSet<int>(array);
			}

			int time = 0;
			int place = 1;
			for (int i = 0; i < q; i++)
			{
				array = Console.ReadLine().Split().Select(int.Parse).ToArray();
				int food = array[0];
				int person = array[1];

				int bestPlace = FindShortestPath(foodMap, place, food, person);
				int t = CalcDistance(bestPlace, place) + CalcDistance(bestPlace, person);

				//Console.WriteLine("place={0}->{0} time+{1}={2}", place,bestPlace,t,time+t);
				time += t;
				place = person;

			}

			Console.WriteLine(time);
		}

		public static int FindShortestPath(HashSet<int>[] foodMap, int loc, int food, int person)
		{
			if (foodMap[food].Contains(loc))
				return loc;

			int bestPlace = 0;
			int bestTime = int.MaxValue;
			foreach (var foodPlace in foodMap[food])
			{
				int time = CalcDistance(foodPlace, loc) + CalcDistance(foodPlace, person);
				if (time < bestTime)
				{
					bestTime = time;
					bestPlace = foodPlace;
				}
			}
			return bestPlace;
		}

		public static int CalcDistance(int place1, int place2)
		{
			if (place1 == place2) return 0;
			if (place1 > place2) return CalcDistance(place2, place1);
			return 1 + CalcDistance(place1, place2 / 2);
		}



	}
}