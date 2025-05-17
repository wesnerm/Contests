
namespace HackerRank.Codagon.Positron2
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using static System.Math;

    public class Solution
    {

        public static bool FindMatch(int i, bool[,] w, int[] mr, int[] mc, BitArray seen)
        {
            int m = w.GetLength(1);
            for (var j = 0; j < m; j++)
            {
                if (w[i,j]  && !seen[j])
                {
                    seen[j] = true;
                    if (mc[j] < 0 || FindMatch(mc[j], w, mr, mc, seen))
                    {
                        mr[i] = j;
                        mc[j] = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public static int BipartiteMatching(bool[,] w, out int[] mr, out int[] mc)
        {

            mr = new int[w.GetLength(0)];
            mc = new int[w.GetLength(1)];

            for (int i = 0; i < mr.Length; i++)
                mr[i] = -1;

            for (int i = 0; i < mc.Length; i++)
                mc[i] = -1;

            var ct = 0;
            for (var i = 0; i < w.Length; i++)
            {
                var seen = new BitArray(w.GetLength(1));
                if (FindMatch(i, w, mr, mc, seen)) ct++;
            }
            return ct;
        }

        public static void Driver()
        {
            int t = int.Parse(Console.ReadLine());
            while (t-- > 0)
            {
                var arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                int n = arr[0], m = arr[1];

                var xa = new int[n + 1];
                var ya = new int[n + 1];
                var xb = new int[m + 1];
                var yb = new int[m + 1];

                var w = new bool[n + 1, m + 1];

                for (int i = 0; i < n; ++i)
                {
                    arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                    xa[i] = arr[0];
                    ya[i] = arr[1];
                }
                int bas = 5 * n;
                for (int i = 0; i < m; ++i)
                {
                    arr = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
                    xb[i] = arr[0];
                    yb[i] = arr[1];
                }

                for (int i = 1; i <= n; ++i)
                {
                    for (int j = 1; j <= m; ++j)
                    {
                        if (xa[i] == xb[j]) w[i,j] = true;
                        if (ya[i] == yb[j]) w[i,j] = true;
                        if (Abs(xa[i] - xb[j]) == Abs(ya[i] - yb[j])) w[i,j] = true;
                    }
                }

                int[] xx, yy;
                Console.WriteLine(BipartiteMatching(w, out xx, out yy));
            }
        }


    }

    public static class HackerRankUtils
    {
        public const int MOD = 1000 * 1000 * 1000 + 7;

        static int[] _inverse;

        public static long Inverse(long n)
        {
            long result;

            if (_inverse == null)
                _inverse = new int[3000];

            if (n < _inverse.Length && (result = _inverse[n]) != 0)
                return result - 1;

            result = ModPow(n, MOD - 2);
            if (n < _inverse.Length)
                _inverse[n] = (int)(result + 1);
            return result;
        }

        public static long Mult(long left, long right)
        {
            return (left * right) % MOD;
        }

        public static long Div(long left, long divisor)
        {
            return left % divisor == 0
                ? left / divisor
                : Mult(left, Inverse(divisor));
        }

        public static long Subtract(long left, long right)
        {
            return (left + (MOD - right)) % MOD;
        }

        public static long Fix(long n)
        {
            return ((n % MOD) + MOD) % MOD;
        }


        public static long ModPow(long n, long p, long mod = MOD)
        {
            long b = n;
            long result = 1;
            while (p != 0)
            {
                if ((p & 1) != 0)
                    result = (result * b) % mod;
                p >>= 1;
                b = (b * b) % mod;
            }
            return result;
        }

        public static long Pow(long n, long p)
        {
            long b = n;
            long result = 1;
            while (p != 0)
            {
                if ((p & 1) != 0)
                    result *= b;
                p >>= 1;
                b *= b;
            }
            return result;
        }

        static List<long> _fact;
        static List<long> _ifact;

        public static long Fact(int n)
        {
            if (_fact == null) _fact = new List<long>(100) { 1 };
            for (int i = _fact.Count; i <= n; i++)
                _fact.Add(Mult(_fact[i - 1], i));
            return _fact[n];
        }

        public static long InverseFact(int n)
        {
            if (_ifact == null) _ifact = new List<long>(100) { 1 };
            for (int i = _ifact.Count; i <= n; i++)
                _ifact.Add(Div(_ifact[i - 1], i));
            return _ifact[n];
        }

        public static long Comb(int n, int k)
        {
            if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
            if (k + k > n) return Comb(n, n - k);
            return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
        }

        public static string[] ReadArray()
        {
            int n = int.Parse(Console.ReadLine());
            string[] array = new string[n];
            for (int i = 0; i < n; i++)
                array[i] = Console.ReadLine();
            return array;
        }

        public static int LowerBound<T>(T[] array, T value, int left, int right)
            where T : IComparable<T>
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (value.CompareTo(array[mid]) > 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            return left;
        }

        public static int UpperBound<T>(T[] array, T value, int left, int right)
            where T : IComparable<T>
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (value.CompareTo(array[mid]) >= 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            return left;
        }

        public static void For(int n, Action<int> action)
        {
            for (int i = 0; i < n; i++) action(i);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public static void MemSet<T>(IList<T> list, T value)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                list[i] = value;
        }

        public static void MemSet<T>(T[,] list, T value)
        {
            int count = list.GetLength(0);
            int count2 = list.GetLength(1);
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count2; j++)
                    list[i, j] = value;
        }

        public static void MemSet<T>(IEnumerable<IList<T>> list, T value)
        {
            foreach (var sublist in list)
                MemSet(sublist, value);
        }

        public static void Iota(IList<int> list, int seed)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                list[i] = seed++;
        }

        public class Comparer<T> : IComparer<T>
        {
            readonly Comparison<T> _comparison;

            public Comparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T a, T b)
            {
                return _comparison(a, b);
            }
        }
    }
}