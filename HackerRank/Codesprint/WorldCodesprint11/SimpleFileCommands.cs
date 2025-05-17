namespace HackerRank.WorldCodeSprint11.SimpleFileCommands
{

	// https://www.hackerrank.com/contests/world-codesprint-11/challenges/simple-file-commands

	using System;
	using System.Collections.Generic;
	using System.IO;
	using static FastIO;

	public class Solution
	{

		public static void Main(string[] args)
		{
			Solution solution = new Solution();
			solution.solve();
		}
		public void solve()
		{
			InitInput(Console.OpenStandardInput());
			InitOutput(Console.OpenStandardOutput());
			int q = Ni();
			while (q-- > 0)
			{
				var cmd = Ns();
				switch (cmd)
				{
					case "crt":
						var f = Ns();
						WriteLine($"+ {CreateCore(f)}");
						break;
					case "del":
						f = Ns();
						DeleteCore(f);
						WriteLine($"- {f}");
						break;
					case "rnm":
						f = Ns();
						var f2 = Ns();
						DeleteCore(f);
						var s = CreateCore(f2);
						WriteLine($"r {f} -> {s}");
						break;
				}
			}
			Flush();
		}

		Dictionary<string, Tree> dict = new Dictionary<string, Tree>();
		public string CreateCore(string s)
		{
			Tree tree;
			if (!dict.ContainsKey(s))
			{
				tree = dict[s] = new Tree();
				tree.Insert(0);
				return s;
			}

			tree = dict[s];
			var next = !dict.ContainsKey(s) ? 0 : dict[s].FindLowestAvailable();
			var f = next == 0 ? s : $"{s}({next})";
			tree.Insert(next);
			return f;
		}

		public bool DeleteCore(string s)
		{
			string f;
			int n;
			f = s;
			int i = s.IndexOf('(');
			if (i >= 0)
			{
				f = s.Substring(0, i);
				n = int.Parse(s.Substring(i + 1, s.Length - (i + 1) - 1));
			}
			else
			{
				n = 0;
			}

			if (!dict.ContainsKey(f))
				return false;

			var tree = dict[f];
			tree.Delete(n);
			if (tree.Count == 0)
				dict.Remove(f);
			return true;
		}
	}

	public class Tree
	{
		public Node Root;

		public int Count => Root?.Size ?? 0;

		public void Insert(int index)
		{
			Root = InsertCore(Root, index);
		}

		public void Delete(int index)
		{
			Root = DeleteCore(Root, index);
		}

		public int FindLowestAvailable()
		{
			return FindLowestAvailable(Root, 0);
		}

		int FindLowestAvailable(Node root, int min = 0)
		{
			if (root == null) return min;
			int leftSize = root.Left?.Size ?? 0;
			return leftSize + min >= root.Key ? FindLowestAvailable(root.Right, root.Key + 1) : FindLowestAvailable(root.Left, min);
		}

		Node InsertRoot(Node p, int k)
		{
			if (p == null)
			{
				var result = new Node { Key = k };
				result.Update();
				return result;
			}

			if (k < p.Key)
			{
				p.Left = InsertRoot(p.Left, k);
				return p.RotateRight();
			}
			else
			{
				p.Right = InsertRoot(p.Right, k);
				return p.RotateLeft();
			}
		}

		static Random rand = new Random();

		Node InsertCore(Node p, int k)
		{
			if (p == null)
				p = new Node { Key = k };
			else if (p.Key == k)
				return p;
			else if (rand.Next(p.Size + 1) == 0)
				return InsertRoot(p, k);
			else if (p.Key > k)
				p.Left = InsertCore(p.Left, k);
			else
				p.Right = InsertCore(p.Right, k);
			p.Update();
			return p;
		}

		Node Join(Node p, Node q) // Joining two trees
		{
			if (p == null) return q;
			if (q == null) return p;
			if (rand.Next(p.Size + q.Size) < p.Size)
			{
				p.Right = Join(p.Right, q);
				p.Update();
				return p;
			}
			else
			{
				q.Left = Join(p, q.Left);
				q.Update();
				return q;
			}
		}

		public Node DeleteCore(Node p, int k) // deleting from p tree the first found node with k key 
		{
			if (p == null) return p;

			if (p.Key == k)
			{
				Node q = Join(p.Left, p.Right);
				return q;
			}

			if (k < p.Key)
				p.Left = DeleteCore(p.Left, k);
			else
				p.Right = DeleteCore(p.Right, k);
			p.Update();
			return p;
		}

	}

	public class Node
	{
		#region Core

		public int Key;
		public Node Left;
		public Node Right;
		public Node Parent;
		public int Size = 1;

		#endregion

		#region TreeOperators

		public bool Update()
		{
			var size = 1;

			if (Left != null)
				size += Left.Size;

			if (Right != null)
				size += Right.Size;

			Size = size;
			return true;
		}


		public Node RotateRight()
		{
			var child = Left;
			if (child == null) return this;
			Left = child.Right;
			child.Right = this;
			child.Parent = Parent;
			Parent = child;

			Update();
			child.Update();
			return child;
		}

		public Node RotateLeft()
		{
			var child = Right;
			if (child == null) return this;
			Right = child.Left;
			child.Left = this;
			child.Parent = Parent;
			Parent = child;

			Update();
			child.Update();
			return child;
		}

		#endregion
	}

}