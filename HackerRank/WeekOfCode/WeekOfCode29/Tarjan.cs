using System;
using System.Collections.Generic;

namespace HackerRank.WeekOfCode29.DiameterMinimization
{
	public class Tarjan
	{

		int _index;
		readonly Stack<int> _set = new Stack<int>();

		public struct VData
		{
			public int Index;
			public int LowLink;
			public int Count;
			public bool Pushed;
			public bool Visited;
		}

		public VData[] Vertices;
		List<int>[] g;
		bool dirty = false;

		public Tarjan(List<int>[] g)
		{
			this.g = g;
			Vertices = new VData[g.Length];
		}

		public bool RunScc()
		{
			if (dirty)
			{
				Array.Clear(Vertices, 0, Vertices.Length);
				_set.Clear();
				_index = 0;
			}

			for (int v = 0; v < g.Length; v++)
				if (Vertices[v].Visited == false)
					StrongConnect(g, v);

			dirty = true;
			return IsConnected();
		}

		public bool IsConnected()
		{
			int count = 0;
			for (int i = 0; i < Vertices.Length; i++)
			{
				if (Vertices[i].LowLink == Vertices[i].Index)
				{
					count++;
					if (count > 1)
						return false;
				}
			}
			return true;
		}



		void StrongConnect(List<int>[] g, int v)
		{
			Vertices[v].Index = _index;
			Vertices[v].LowLink = _index;
			Vertices[v].Pushed = true;
			Vertices[v].Visited = true;
			_index++;
			_set.Push(v);

			foreach (var w in g[v])
			{
				if (Vertices[w].Visited == false)
				{
					StrongConnect(g, w);
					Vertices[v].LowLink = Math.Min(Vertices[v].LowLink, Vertices[w].LowLink);
				}
				else if (Vertices[w].Pushed)
				{
					Vertices[v].LowLink = Math.Min(Vertices[v].LowLink, Vertices[w].Index);
				}
			}

			if (Vertices[v].LowLink == Vertices[v].Index)
			{
				int w;
				do
				{
					w = _set.Pop();
					Vertices[w].Pushed = false;
					Vertices[v].Count++;
				} while (w != v);
			}
		}


	}
}