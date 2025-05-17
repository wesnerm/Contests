
namespace HackerRank.NcrCodesprint.FlexibleTreeVisitorPattern
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	enum Color
	{
		RED,
		GREEN
	}

	abstract class TreeVis
	{
		public abstract int getResult();
		public abstract void visitNode(TreeNode node);
		public abstract void visitLeaf(TreeLeaf leaf);
	}

	abstract class Tree
	{
		private int value;
		private Color color;
		private int depth;

		public Tree(int value, Color color, int depth)
		{
			this.value = value;
			this.color = color;
			this.depth = depth;
		}

		public int getValue()
		{
			return value;
		}

		public Color getColor()
		{
			return color;
		}

		public int getDepth()
		{
			return depth;
		}

		public abstract void accept(TreeVis visitor);
	}

	class TreeNode : Tree
	{
		private List<Tree> children = new List<Tree>();

		public TreeNode(int value, Color color, int depth)
			: base(value, color, depth)
		{
		}

		public override void accept(TreeVis visitor)
		{
			visitor.visitNode(this);
			foreach (var child in children)
				child.accept(visitor);
		}

		public void addChild(Tree child)
		{
			children.Add(child);
		}
	}

	class TreeLeaf : Tree
	{
		public TreeLeaf(int value, Color color, int depth)
			: base(value, color, depth)
		{
		}

		public override void accept(TreeVis visitor)
		{
			visitor.visitLeaf(this);
		}
	}


	class SumInLeavesVisitor : TreeVis
	{
		public override int getResult()
		{
			//implement this
			return 0;
		}

		public override void visitNode(TreeNode node)
		{
			//implement this
		}

		public override void visitLeaf(TreeLeaf leaf)
		{
			//implement this
		}
	}


	class ProductOfRedNodesVisitor : TreeVis
	{
		public override int getResult()
		{
			//implement this
			return 1;
		}

		public override void visitNode(TreeNode node)
		{
			//implement this
		}

		public override void visitLeaf(TreeLeaf leaf)
		{
			//implement this
		}
	}

	class FancyVisitor : TreeVis
	{
		public override int getResult()
		{
			//implement this
			return 0;
		}

		public override void visitNode(TreeNode node)
		{
			//implement this
		}

		public override void visitLeaf(TreeLeaf leaf)
		{
			//implement this
		}
	}


	class Solution
	{
		static Tree solve()
		{
			//read the tree from STDIN and return its root as a return value of this function
			return null;
		}


		static void Main()
		{
			Tree root = solve();

			var vis1 = new SumInLeavesVisitor();
			var vis2 = new ProductOfRedNodesVisitor();
			var vis3 = new FancyVisitor();

			root.accept(vis1);
			root.accept(vis2);
			root.accept(vis3);

			int res1 = vis1.getResult();
			int res2 = vis2.getResult();
			int res3 = vis3.getResult();

			Console.WriteLine(res1);
			Console.WriteLine(res2);
			Console.WriteLine(res3);
		}
	}
}