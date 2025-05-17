using System;
using System.Diagnostics;

namespace HackerRank.SplitPlane
{
	public class Tree
	{
		public Node Root;

		public int Count => Root?.Size ?? 0;

		public void Insert(int index, int value)
		{
			Root = RandomizedInsert(Root, index, value);
			Validate();
		}

		public void Delete(int index)
		{
			Root = RandomizedDelete(Root, index);
			Validate();
		}

		Node InsertRoot(Node p, int k, int v) // Inserting a new Node with k key in p tree
		{
			Node result;
			if (p == null)
			{
				result = new Node {Key = k, Value = v};
				result.Update();
				return result;
			}
			if (k < p.Key)
			{
				p.Left = InsertRoot(p.Left, k, v);
				return p.RotateRight();
			}
			else
			{
				p.Right = InsertRoot(p.Right, k, v);
				return p.RotateLeft();
			}
		}

		static Random rand = new Random();

		Node RandomizedInsert(Node p, int k, int v) // a randomized Insertion of a new Node with k key in p tree 
		{
			if (p==null)
				p = new Node { Key = k, Value = v };
			else if (p.Key == k)
				p.Value = v;
			else if (rand.Next(p.Size + 1) == 0)
				return InsertRoot(p, k, v);
			else if (p.Key > k)
				p.Left = RandomizedInsert(p.Left, k, v);
			else 
				p.Right = RandomizedInsert(p.Right, k, v);
			p.Update();
			return p;
		}

		Node RandomizedJoin(Node p, Node q) // Joining two trees
		{
			if (p==null) return q;
			if (q==null) return p;
			if (rand.Next(p.Size + q.Size) < p.Size)
			{
				p.Right = RandomizedJoin(p.Right, q);
				p.Update();
				return p;
			}
			else
			{
				q.Left = RandomizedJoin(p, q.Left);
				q.Update();
				return q;
			}
		}

		public Node RandomizedDelete(Node p, int k) // deleting from p tree the first found node with k key 
		{
			if (p == null) return p;

			if (p.Key == k)
			{
				Node q = RandomizedJoin(p.Left, p.Right);
				return q;
			}

			if (k < p.Key)
				p.Left = RandomizedDelete(p.Left, k);
			else
				p.Right = RandomizedDelete(p.Right, k);
			p.Update();
			return p;
		}


		public void AddToDisjointSet(DisjointSet set, int left, int right, int comp)
		{
			AddToDisjointSetCore(Root, set, left, right, comp);
		}

		static void AddToDisjointSetCore(Node node, DisjointSet set, int left, int right, int comp)
		{
			if (node == null) return;

			if (node.Key >= left && node.Key <= right)
			{
				node.Value = set.Union(comp, node.Value);
				if (node.SameValue)
					return;
			}

			if (node.Left != null && left <= node.Key)
				AddToDisjointSetCore(node.Left, set, left, right, comp);

			if (node.Right != null && right >= node.Key)
				AddToDisjointSetCore(node.Right, set, left, right, comp);

			node.Update();
		}

		public Node FindNode(Node root, int key)
		{
			while (root != null)
			{
				if (root.Key < key)
					root = root.Right;
				else if (root.Key > key)
					root = root.Left;
				else
					break;
			}
			return root;
		}

		public int Bound(Node node, int key, bool lower)
		{
			int rank = 0;
			while (node != null)
			{
				if (key < node.Key || lower && key == node.Key)
					node = node.Left;
				else
				{
					if (node.Left != null)
						rank += node.Left.Size;
					rank += 1;
					node = node.Right;
				}
			}
			return rank;
		}

		public int CountInclusive(int leftKey, int rightKey)
		{
			var node = Root;
			while (node != null)
			{
				if (node.Key > rightKey)
					node = node.Left;
				else if (node.Key < leftKey)
					node = node.Right;
				else
					break;
			}
			return UpperBound(node, rightKey) - LowerBound(node, leftKey);
		}

		public int LowerBound(Node root, int key)
		{
			return Bound(root, key, true);
		}

		public int UpperBound(Node root, int key)
		{
			return Bound(root, key, false);
		}

		[Conditional("DEBUG")]
		public void Validate()
		{
			Root?.Validate(int.MinValue, int.MaxValue);
		}
	}


	// First Key
	// Last Key
	// FloorKey - 
	// ceiling -- same as 

	public partial class Node
	{
		#region Core

		public int Key;
		public bool SameValue;
		public Node Left;
		public Node Right;
		public int Size = 1;
		public int Value;

		#endregion

		#region TreeOperators

		public bool Update()
		{
			var size = 1;
			int comp = Value;

			if (Left != null)
			{
				size += Left.Size;
				if (comp != Left.Value || !Left.SameValue)
					comp = -1;
			}

			if (Right != null)
			{
				size += Right.Size;
				if (comp != Right.Value || !Right.SameValue)
					comp = -1;
			}

			SameValue = comp != -1;
			Size = size;
			return true;
		}

		#endregion

		#region Rotation
		public Node RotateRight()
		{
			var child = Left;
			if (child == null) return this;
			Left = child.Right;
			child.Right = this;
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
			Update();
			child.Update();
			return child;
		}

		#endregion

		#region Validation
		[Conditional("DEBUG")]
		public void Validate(int min, int max)
		{
			if (this == null) return;
			Debug.Assert(Key >= min);
			Debug.Assert(Key <= max);
			Debug.Assert(Size == 1 + (Left?.Size ?? 0) + (Right?.Size ?? 0));
			Left?.Validate(min, Key);
			Right?.Validate(Key, max);
		}
		#endregion
	}
}
