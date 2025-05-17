using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Softperson.Algorithms.Strings;

class WordGameTest
{
    static void Driver(string[] args)
    {
        /* Enter your code here. Read input from STDIN. Print output to STDOUT */

        var input = new StringReader(
            @"9
+ race
+ space
+ rock
+ rocketeer
+ rocket
- rock
+ rock
- space
+ rockstar
");

        var trie = new SimpleTrie();
        int count = int.Parse(input.ReadLine());
        for (int i = 0; i < count; i++)
        {
            var line = input.ReadLine().Split();
            var word = line[1];
            var op = line[0];

            if (op == "-")
                trie.Delete(word);
            else
            {
                // op == '+'
                int matches = Score(trie, word);
                Console.WriteLine(matches);
                trie.Insert(word);
            }
        }
    }

    public static int Score(SimpleTrie trie, string word)
    {
        var t = trie;
        int count = 0;
        int index = 1;
        foreach (var ch in word)
        {
            t = t.MoveNext(ch, false);
            if (t == null)
                break;
            count += t.Size*index;
            index++;
        }
        return count;

    }
}