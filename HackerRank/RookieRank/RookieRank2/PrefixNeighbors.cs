namespace HackerRank.Contests.RookieRank2.PrefixNeighbors
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Solution
	{

		static void Main()
		{
			Convert.ToInt32(Console.ReadLine());
			string[] s = Console.ReadLine().Split();

			var trie = new Trie();
			trie.Insert(s);

			MinLength(trie);
			long max = MaxScore(trie).MaxScore;
			Console.WriteLine(max);
		}

		static ScorePair MaxScore(Trie trie, int neighborDepth = int.MaxValue, long score = 0, int depth = 0)
		{
			score += trie.Ch;

			var list = new List<ScorePair>();
			int neighborDepth2 = trie.Word != null ? trie.minLength : neighborDepth;
			if (trie.Next != null)
			{
				foreach (var child in trie.Next)
					if (child != null)
					{
						var maxScore = MaxScore(child, neighborDepth2, score, depth + 1);
						list.Add(maxScore);
					}
			}

			var result = new ScorePair();
			result.Depth = int.MaxValue;

			
			if (trie.Word != null)
			{
				foreach (var p in list)
				{
					bool isNeighbor = neighborDepth2 == p.Depth;
					result.ScoreParent += isNeighbor ? p.ScoreKids : p.MaxScore;
					result.ScoreKids += p.MaxScore;
				}
			}
			else
			{
				foreach (var p in list)
				{
					if (p.Depth < result.Depth)
					{
						result.Depth = p.Depth;
						result.ScoreParent = 0;
					}

					if (p.Depth == result.Depth)
					{
						result.ScoreParent += p.ScoreParent;
						result.ScoreKids += p.ScoreKids;
					}
					else
					{
						result.ScoreKids += p.MaxScore;
					}
				}
			}

			if (trie.Word != null)
			{
				result.ScoreParent += score;
				result.Depth = depth;
			}
			return result;
		}

		static int MinLength(Trie trie, int depth = 0)
		{
			trie.minLength = int.MaxValue;
			if (trie.Next != null)
			{
				foreach (var child in trie.Next)
					if (child != null)
					{
						var ml = MinLength(child, depth + 1);
						trie.minLength = Math.Min(trie.minLength, ml);
					}
			}

			if (trie.Word == null)
				return trie.minLength;
			return depth;
		}


		struct ScorePair
		{
			public int Depth;
			public long ScoreParent;
			public long ScoreKids;
			public long MaxScore => Math.Max(ScoreParent, ScoreKids);
		}

		public class Trie
		{
			const char FirstChar = 'a';
			const int CharRange = 26;

			// alternative, I could use Trie nextSibling and Trie firstChild to save memory
			public Trie[] Next;
			// normally, I would use bool isWord
			public char Ch;
			public int Size;
			public string Word;
			public int WordCount;
			public int minLength;

			public Trie Insert(string word)
			{
				var p = Find(word, true);
				p.Word = word;

				var trie = this;

				foreach (var ch in word)
				{
					trie.Size++;
					trie = trie.MoveNext(ch, true);
				}

				trie.Word = word;
				trie.WordCount++;
				trie.Size++;
				return trie;
			}

			public void Insert(IEnumerable<string> words)
			{
				foreach (var w in words)
					Insert(w);
			}

			public void Delete(string word)
			{
				if (!Contains(word))
					return;

				var trie = this;
				foreach (var ch in word)
				{
					trie.Size--;
					var child = trie.MoveNext(ch, false);
					if (child.Size == 1)
					{
						trie.Next[ch - FirstChar] = null;
						return;
					}
					trie = child;
				}

				if (--trie.WordCount == 0)
					trie.Word = null;
				trie.Size--;
			}


			public bool StartsWith(string word)
			{
				return Find(word) != null;
			}

			public bool Contains(string word)
			{
				var trie = Find(word);
				return trie != null && trie.Word != null;
			}

			public Trie Find(string word, bool create = false)
			{
				var p = this;
				foreach (var c in word)
				{
					p = p.MoveNext(c, create);
					if (p == null)
						break;
				}
				return p;
			}

			public Trie MoveNext(char ch, bool create = false)
			{
				int i = Char.ToLower(ch) - FirstChar;
				if (i < 0 || i > CharRange) return null;

				if (Next == null)
				{
					if (!create) return null;
					Next = new Trie[CharRange];
				}

				if (Next[i] == null)
				{
					if (!create) return null;
					Next[i] = new Trie { Ch = ch };
				}
				return Next[i];
			}

		}
	}

}
