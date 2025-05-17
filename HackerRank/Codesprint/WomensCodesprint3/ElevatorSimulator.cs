namespace HackerRank.WomensCodesprint
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static System.Math;
	class ElevatorSimulator
	{
		static bool verbose = false;

		static void Main(String[] args)
		{
			int n = Read();
			int w = Read();
			int c = Read();
			int rory = Read();

			var list = new List<Person>();
			for (int a0 = 1; a0 <= n; a0++)
			{
				int id = Read();
				int t = Read();
				int f = Read();
				list.Add(new Person { Index = a0, Teacher = id, Time = t, Floor = f });
			}

			var students = new Queue<Person>();
			var teachers = new Queue<Person>();
			foreach (var v in list)
			{
				if (v.Teacher != 2) students.Enqueue(v);
				else teachers.Enqueue(v);
			}
			list.Sort((a, b) => a.Time.CompareTo(b.Time));

			int time = 1;
			int trips = 0;
			int tripRory = 0;
			int last = 0;
			var elev = new List<Person>();
			while (students.Count + teachers.Count > 0)
			{
				trips++;

				Person lastPerson = null;
				int minTime = teachers.Count > 0 ? teachers.Peek().Time : int.MaxValue;
				minTime = Min(students.Count > 0 ? students.Peek().Time : int.MaxValue, minTime);
				time = Max(time, minTime);

				if (verbose) Console.WriteLine($"Trip #{trips}");
				if (verbose) Console.WriteLine($"Elevator opens at {time} and closes at {time + w}");

				elev.Clear();
				while (elev.Count < c && teachers.Count > 0 && teachers.Peek().Time <= time + w)
				{
					var person = teachers.Dequeue();
					elev.Add(person);
					lastPerson = person;
					if (verbose) Console.WriteLine($"  {person} enters at {person.Time}");
				}

				while (elev.Count < c && students.Count > 0 && students.Peek().Time <= time + w)
				{
					var person = students.Dequeue();
					elev.Add(person);
					//if (lastPerson == null || lastPerson.Time < person.Time
					//    || lastPerson.Time >= person.Time && person.Teacher==2)
					lastPerson = person;
					if (verbose) Console.WriteLine($"  {person} enters at {person.Time}");
				}

				elev.Sort((a, b) => a.Floor.CompareTo(b.Floor));

				time += w;

				int maxFloor = elev.Max(x => x.Floor);
				int p = 0;
				for (int i = 1; i <= maxFloor; i++)
				{
					time++;
					if (verbose) Console.WriteLine($"Elevator arrives at floor #{i} at time {time}");
					int oldP = p;
					while (p < elev.Count && elev[p].Floor <= i)
					{
						if (verbose) Console.WriteLine($"  {elev[p]} leaves elevator");
						if (elev[p].Index == rory)
							tripRory = trips;
						if (elev[p] == lastPerson)
							last = time;
						p++;
					}
				}

				time += maxFloor;
			}

			Console.WriteLine($"{tripRory} {last}");
		}


		public class Person
		{
			public int Index;
			public int Time;
			public int Floor;
			public int Teacher;
			public override string ToString() => (Teacher == 2 ? "Teacher":"Student") + Index;
    }

		public static int[] ReadNumbers(int n)
		{
			var list = new int[n];
			for (int i = 0; i < n; i++)
				list[i] = Read();
			return list;
		}

		public static int Read()
		{
			int number = 0;
			bool hasNum = false;
			while (true)
			{
				int c = Console.Read();
				if (c < 0) break;
				int d = c - '0';
				if (d >= 0 && d <= 9)
				{
					number = number * 10 + d;
					hasNum = true;
					continue;
				}

				if (hasNum == true)
					break;
			}
			return number;
		}
	}

}