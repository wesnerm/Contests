#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static System.Array;
using static System.Math;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0675
#endregion

partial class Solution
{
    #region Variables
    const int MOD = 998244353;
    const int FactCache = 1000;
    const long BIG = long.MaxValue >> 15;

    #endregion

    public void Solve()
    {
        int N = Ni();
        long[] A = Nl(N);
        long[] C = Nl(N);

        System.Console.Error.WriteLine("This is the experimental one, not the main one!!");

        if (false)
        {
            // Working -- Gaussian  n^3
            int[] solve = InterpolatePolynomials(
                            A.Select(x => (int)x).ToArray(),
                            C.Select(y => (int)y).ToArray());
            WriteLine(string.Join(" ", solve));
        }
        else if (false)
        {
            // Working  n^2
            long[] solve = InterpolateNaive(A, C);
            //foreach (var v in A)
            //    Console.Error.WriteLine(Evaluate(solve, v));
            WriteLine(string.Join(" ", solve));
        }
        else if (false)
        {
            // Working n lg n
            long[] solve = Interpolate(A, C);
            //foreach (var v in A)
            //    Console.Error.WriteLine(Evaluate(solve, v));
            WriteLine(string.Join(" ", solve));
        }
        else
        {
            // Failing n lg n
            var solve = new PolynomialInterpolation(A, C).Result;
            //foreach (var v in A)
            //    Console.Error.WriteLine(Evaluate(solve, v));
            WriteLine(string.Join(" ", solve));
        }
    }


    public int[] InterpolatePolynomials(int[] xs, int[] ys)
    {
        int n = xs.Length;
        int[][] mat = new int[n][];

        mat[0] = new int[n];
        for (int j = 0; j < n; j++)
            mat[0][j] = 1;

        for (int i = 1; i < n; i++)
        {
            int[] pre = mat[i - 1];
            int[] row = mat[i] = new int[n];
            for (int j = 0; j < n; j++)
                row[j] = (int)((long)pre[j] * xs[j] % MOD);
        }
        return GaussianElimination(mat, ys);
    }

    public static unsafe int[] GaussianElimination(int[][] A, int[] b)
    {
        int n = b.Length;
        for (int p = 0; p < n; p++)
        {
            int max = p;
            for (int i = p; i < n; i++)
            {
                if (A[i][p] != 0)
                {
                    max = i;
                    break;
                }
            }

            int[] prow = A[max];
            A[max] = A[p];
            A[p] = prow;

            int t = b[p]; b[p] = b[max]; b[max] = t;

            long ipivot = InverseDirect(A[p][p]);
            for (int i = p + 1; i < n; i++)
            {
                fixed (int* row = A[i])
                {
                    long alpha = MOD - (int)(row[p] * ipivot % MOD);
                    b[i] = (int)((b[i] + alpha * b[p]) % MOD);
                    for (int j = p; j < n; j++)
                    {
                        row[j] = (int)((row[j] + alpha * prow[j]) % MOD);
                    }
                }
            }
        }

        int[] x = new int[n];
        for (int i = n - 1; i >= 0; i--)
        {
            fixed (int* row = A[i])
            {
                long sum = MOD - b[i];
                for (int j = i + 1; j < n; j++)
                {
                    sum = (sum + (long)row[j] * x[j]) % MOD;
                }
                x[i] = (int)Div(MOD - sum, row[i]);
            }
        }
        return x;
    }

    private static long[] Derivative(long[] a)
    {
        if (a.Length == 0)
            return a;

        long[] result = new long[a.Length - 1];
        for (int i = 1; i < a.Length; ++i)
            result[i - 1] = a[i] * i % MOD;

        return result;
    }

    private static List<long[]> GetSegmentProducts(long[] pts)
    {
        var segmentPolys = new List<long[]>();
        Func<int, int, int> fillPolys = null;
        fillPolys = (left, right) =>
        {
            if (left + 1 == right)
            {
                segmentPolys.Add(new[] { (MOD - pts[left]) % MOD, 1 });
                return segmentPolys.Count - 1;
            }
            int m = (left + right) >> 1;
            int i = fillPolys(left, m);
            int j = fillPolys(m, right);
            long[] newPoly = MultiplyPolynomialsMod(segmentPolys[i], segmentPolys[j]);
            segmentPolys.Add(newPoly);
            return segmentPolys.Count - 1;
        };
        fillPolys(0, pts.Length);
        return segmentPolys;
    }

    public static List<long> Multipoint(long[] poly, long[] pts)
    {
        if (pts.Length == 0)
            return new List<long>();

        var segmentPolys = GetSegmentProducts(pts);
        var ans = new List<long>();
        Action<long[]> FillAns = null;
        FillAns = p =>
        {
            if (segmentPolys[segmentPolys.Count - 1].Length <= 2)
            {
                ans.Add(p.Length == 0 ? 0 : p[0]);
                segmentPolys.RemoveAt(segmentPolys.Count - 1);
                return;
            }
            segmentPolys.RemoveAt(segmentPolys.Count - 1);

            long[] r = ModPolynomial(p, segmentPolys[segmentPolys.Count - 1]);
            FillAns(r);
            r = ModPolynomial(p, segmentPolys[segmentPolys.Count - 1]);
            FillAns(r);
        };
        FillAns(poly);

        ans.Reverse();
        return ans;
    }

    public static List<long> Multipoint2(long[] poly, long[] xs)
    {
        return xs.Select(x => Evaluate(poly, x)).ToList();
    }

    private static long[] Interpolate(long[] xs, long[] ys)
    {
        Debug.Assert(xs.Length == ys.Length);
        if (xs.Length == 0)
            return xs;

        var segmentPolys = GetSegmentProducts(xs);
        long[] der = Derivative(segmentPolys[segmentPolys.Count - 1]);
        var coeffs = Multipoint2(der, xs);
        for (int i = 0; i < coeffs.Count; i++)
            coeffs[i] = Inverse(coeffs[i]);

        for (int i = 0; i < ys.Length; ++i)
            coeffs[i] = coeffs[i] * ys[i] % MOD;

        Func<long[]> getAns = null;
        getAns = () =>
        {
            if (segmentPolys[segmentPolys.Count - 1].Length <= 2)
            {
                segmentPolys.RemoveAt(segmentPolys.Count - 1);
                long[] res = { coeffs[coeffs.Count - 1] };
                coeffs.RemoveAt(coeffs.Count - 1);
                return res;
            }
            else
            {
                segmentPolys.RemoveAt(segmentPolys.Count - 1);
                long[] p1 = segmentPolys[segmentPolys.Count - 1];
                long[] q1 = getAns();
                long[] p2 = segmentPolys[segmentPolys.Count - 1];
                long[] q2 = getAns();
                return Add(MultiplyPolynomialsMod(p1, q2), MultiplyPolynomialsMod(p2, q1));
            }
        };

        return Normalize(getAns());
    }


    public static long Evaluate(long[] poly, long x)
    {
        //if (poly.Length == 0) return 0;

        //long result = poly[poly.Length - 1];
        //for (int i = poly.Length - 2; i >= 0; i--)
        //    result = (x * result + poly[i]) % MOD;
        //return result;
        long result = 0;
        for (int i = poly.Length - 1; i >= 0; i--)
            result = (x * result + poly[i]) % MOD;
        return result;
    }

    public class MultipointEvaluation
    {
        readonly long[] y;
        readonly long[][] prod;
        readonly long[] x;

        public long[] Result;
        public MultipointEvaluation(long[] poly, long[] x)
        {
            this.x = x;
            prod = new long[8 * x.Length][];
            y = new long[x.Length];
            BuildTree(0, x.Length, 0);
            ChineseRemainderTheorem(0, x.Length, 0, poly);
            Result = y;
        }

        long[] BuildTree(int i, int j, int k)
        {
            if (i == j) return prod[k] = new[] { 1L };
            if (i + 1 == j) return prod[k] = new[] { MOD - x[i], 1 };
            int mid = i + j >> 1;
            return prod[k] =
                MultiplyPolynomialsMod(
                    BuildTree(i, mid, 2 * k + 1),
                    BuildTree(mid, j, 2 * k + 2));
        }

        void ChineseRemainderTheorem(int i, int j, int k, long[] p)
        {
            if (j - i <= 8)
            {
                for (; i < j; ++i) y[i] = Evaluate(p, x[i]);
                return;
            }
            int mid = i + j >> 1;
            ChineseRemainderTheorem(i, mid, 2 * k + 1, ModPolynomial(p, prod[2 * k + 1]));
            ChineseRemainderTheorem(mid, j, 2 * k + 2, ModPolynomial(p, prod[2 * k + 2]));
        }
    }


    public class PolynomialInterpolation
    {
        long[] u;
        long[][] prod;
        long[] x, y;

        public long[] Result;
        public PolynomialInterpolation(long[] x, long[] y)
        {
            this.x = x;

            prod = new long[8 * x.Length][]; 
            Run(0, x.Length, 0);

            var H = prod[0].ToArray();
            for (int i = 1; i < H.Length; ++i) H[i - 1] = Mult(H[i], i);
            H = Trim(GetRange(H, 0, H.Length-1));

            u = new long[x.Length];
            MPE(0, x.Length, 0, H); 

            for (int i = 0; i < x.Length; ++i) u[i] = Div(y[i], u[i]);

            Result = F(0, x.Length, 0);
        }

        long[] Run(int i, int j, int k)
        {
            if (i == j) return prod[k] = new [] { 1L };
            if (i + 1 == j) return prod[k] = new [] { MOD - x[i], 1 };
            return prod[k] = 
                MultiplyPolynomialsMod(
                    Run(i, (i + j) / 2, 2 * k + 1), 
                    Run((i + j) / 2, j, 2 * k + 2));
        }
        
        void MPE(int i, int j, int k, long[] p)
        {
            if (j - i <= 8)
            {
                for (; i < j; ++i) u[i] = Evaluate(p, x[i]);
            }
            else
            {
                int mid = i + j >> 1;
                MPE(i, mid, 2 * k + 1, ModPolynomial(p, prod[2 * k + 1]));
                MPE(mid, j, 2 * k + 2, ModPolynomial(p, prod[2 * k + 2]));
            }
        }

        long[] F(int i, int j, int k)
        {
            if (i >= j) return new long[0];
            if (i + 1 == j) return new [] { u[i] };
            int mid = i + j >> 1;
            return Add(
                MultiplyPolynomialsMod(F(i, mid, 2 * k + 1), prod[2 * k + 2]),
                MultiplyPolynomialsMod(F(mid, j, 2 * k + 2), prod[2 * k + 1]));
        }
    }


    long[] InterpolateNaive(long[] x, long[] y)
    {
        int n = x.Length;
        var dp = new long[n+1];
        dp[0] = 1;
        for (int i = 0; i < n; ++i)
        {
            for (int j = i; j >= 0; --j)
            {
                dp[j + 1] = (dp[j + 1] + dp[j]) % MOD;
                dp[j] = dp[j] * (MOD - x[i]) % MOD;
            }
        }

        var r = new long[n];
        for (int i = 0; i < n; ++i)
        {
            long den = 1, res = 0;
            for (int j = 0; j < n; ++j)
                if (i != j)
                    den = den * ((x[i] - x[j]) % MOD) % MOD;
            den = Inverse(den);

            for (int j = n - 1; j >= 0; --j)
            {
                res = (dp[j + 1] + res * x[i] % MOD) % MOD;
                r[j] = (r[j] + res *( den * y[i] % MOD) % MOD) %MOD;
            }
        }
        return Trim(r);
    }


    public static long[] Invert(long[] a, int prec)
    {
        Debug.Assert(a[0] != 0);

        long[] res = new long[] { Inverse((int)a[0]) };
        int k = 1;
        while (k < prec)
        {
            k *= 2;
            long[] tmp = MultiplyPolynomialsMod(res, GetRange(a, 0, Math.Min(k, a.Length)));
            for (int i = 0; i < tmp.Length; i++)
                tmp[i] = -tmp[i];

            tmp[0] = (tmp[0] + 2) % MOD;
            res = MultiplyPolynomialsMod(tmp, res, k);
        }

        return Normalize(res, prec);
    }

    public static void DivMod(long[] a, long[] b, out long[] quotient, out long[] remainder)
    {
        int n = a.Length;
        int m = b.Length;
        if (n < m)
        {
            quotient = new long[0];
            remainder = a;
            return;
        }

        Reverse(a);
        Reverse(b);
        quotient = MultiplyPolynomialsMod(a, Invert(b, n - m + 1), n - m + 1);
        Reverse(a);
        Reverse(b);
        Reverse(quotient);
        remainder = Normalize(Sub(a, MultiplyPolynomialsMod(b, quotient)));
    }

    static long[] ModPolynomial(long[] a, long[] b)
    {
        long[] q, r;
        DivMod(a, b, out q, out r);
        return r;
    }


    #region Inversion
#if false
    public static long[] Invert(long[] poly, int n)
    {
        Debug.Assert(poly != null && poly.Length > 0 && poly[0] != 0);

        long[] ret = new long[2 * n];
        ret[0] = Inverse(poly[0]);

        for (int i = 1; i < n; i <<= 1)
        {
            long[] left = GetSubrange(poly, 0, Min(i, poly.Length), true);
            long[] right = GetSubrange(poly, Min(i, poly.Length), Min(2 * i, poly.Length) - Min(i, poly.Length), true);

            left = MultiplyPolynomialsMod(left, ret, 2 * i);
            right = MultiplyPolynomialsMod(right, ret, i);

            for (int j = 0; j < i - 1; ++j)
            {
                if (j + i >= left.Length) break;
                right[j] += left[j + i];
                if (right[j] >= MOD)
                    right[j] -= MOD;
            }

            right = MultiplyPolynomialsMod(right, ret, i);
            for (int j = 0; j < i; ++j)
            {
                long t = ret[i + j] + MOD - right[j];
                if (t >= MOD) t -= MOD;
                ret[i + j] = t;
            }
        }

        return GetSubrange(ret, 0, n);
    }

    static long[] divInvrev;
    static long[] divDivisor;
    public static long[] InvertRev(long[] right, int n)
    {
        long[] invrev;
        if (right == divDivisor && divDivisor.Length >= n)
        {
            invrev = divInvrev;
        }
        else
        {
            long[] invrevOld = (long[])right.Clone();
            Array.Reverse(invrevOld);
            invrev = Invert(invrevOld, n);
            if (invrev.Length < n)
                invrev = GetSubrange(invrev, 0, n, true);
        }
        divDivisor = right;
        divInvrev = invrev;
        if (invrev.Length > 2 * n)
            invrev = GetSubrange(invrev, 0, n);
        return invrev;
    }

    public static long[] DivPolynomial(long[] left, long[] right)
    {
        if (right.Length > left.Length)
            return new long[1];

        int rsize = left.Length - right.Length + 1;
        long[] invrev = InvertRev(right, rsize);

        long[] q = (long[])left.Clone();
        Array.Reverse(q);
        q = MultiplyPolynomialsMod(q, invrev, rsize);
        Array.Reverse(q);
        return q;
    }


    public static long[] ModPolynomial(long[] left, long[] right, long[] quotient = null)
    {
        if (right.Length > left.Length)
            return left;

        if (quotient == null)
            quotient = DivPolynomial(left, right);

        long[] r = (long[])left.Clone();
        long[] qright = MultiplyPolynomialsMod(quotient, right, r.Length);
        for (int i = 0; i < qright.Length; i++)
            r[i] = (r[i] - qright[i] + MOD) % MOD;
        return Trim(r);
    }
#endif

    public static T[] GetSubrange<T>(T[] x, int start, int count, bool extend = false)
    {
        if (start + count > x.Length && !extend) throw new ArgumentOutOfRangeException();
        var result = new T[count];
        Array.Copy(x, start, result, 0, Math.Min(count, x.Length - start));
        return result;
    }

    public static long[] Trim(long[] poly)
    {
        int length = poly.Length;
        while (length > 1 && poly[length - 1] == 0)
            length--;
        return GetSubrange(poly, 0, length);
    }


    static long[] Add(long[] a, long[] b)
    {
        long[] result = GetRange(a, 0, Math.Max(a.Length, b.Length), true);
        for (int i = 0; i < b.Length; ++i)
            result[i] = (result[i] + b[i]) % MOD;
        return result;
    }

    static long[] Sub(long[] a, long[] b)
    {
        long[] result = GetRange(a, 0, Math.Max(a.Length, b.Length), true);
        for (int i = 0; i < b.Length; ++i)
            result[i] = (result[i] + MOD - b[i]) % MOD;
        return result;
    }

    public static long[] MultiplyPolynomialsMod(long[] a, long[] b, int size = 0)
    {
        if (size == 0) size = a.Length + b.Length - 1;
        size = Math.Min(a.Length + b.Length - 1, size);
        long[] result = new long[size];
        for (int i = 0; i < a.Length; i++)
        for (int j = Math.Min(size - i, b.Length) - 1; j >= 0; j--)
        {
            long r = (result[i + j] + a[i] * b[j] % MOD) % MOD;
            if (r >= MOD) r -= MOD;
            result[i + j] = r;
        }

        return result;
    }

    public static long[] Normalize(long[] a, int size = -1)
    {
        int len = (size == -1) ? a.Length : Math.Min(size, a.Length);
        while (len > 0 && a[len - 1] == 0)
            len--;

        if (a.Length == len) return a;
        return GetRange(a, 0, len);
    }

    public static T[] GetRange<T>(T[] x, int start, int count, bool extend = false)
    {
        if (!extend && (start < 0 || start + count > x.Length)) throw new ArgumentOutOfRangeException();
        var result = new T[count];
        Array.Copy(x, Max(0, start), result, Max(0, -start), Math.Min(count, x.Length - start));
        return result;
    }

#endregion


#region Library
#region Mod Math

    static int[] _inverse;
    static long Inverse(long n)
    {
        long result;

        if (_inverse == null)
            _inverse = new int[1000];

        if (n >= 0 && n < _inverse.Length && (result = _inverse[n]) != 0)
            return result - 1;

        result = InverseDirect((int)n);
        if (n >= 0 && n < _inverse.Length)
            _inverse[n] = (int)(result + 1);
        return result;
    }

    public static int InverseDirect(int a)
    {
        if (a < 0) return -InverseDirect(-a);
        int t = 0, r = MOD, t2 = 1, r2 = a;
        while (r2 != 0)
        {
            int q = r / r2;
            t -= q * t2;
            r -= q * r2;

            if (r != 0)
            {
                q = r2 / r;
                t2 -= q * t;
                r2 -= q * r;
            }
            else
            {
                r = r2;
                t = t2;
                break;
            }
        }
        return r <= 1 ? (t >= 0 ? t : t + MOD) : -1;
    }

    static long Mult(long left, long right) =>
        (left * right) % MOD;

    static long Div(long left, long divisor) =>
        left * Inverse(divisor) % MOD;

    static long Add(long x, long y) =>
        (x += y) >= MOD ? x - MOD : x;

    static long Subtract(long x, long y) => (x -= y) < 0 ? x + MOD : x;

    static long Fix(long n) => (n %= MOD) >= 0 ? n : n + MOD;

    static long ModPow(long n, long p, long MOD = MOD)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = (result * b) % MOD;
            p >>= 1;
            b = (b * b) % MOD;
        }
        return result;
    }

    static List<long> _fact;

    static long Fact(int n)
    {
        if (_fact == null) _fact = new List<long>(FactCache) { 1 };
        for (int i = _fact.Count; i <= n; i++)
            _fact.Add(Mult(_fact[i - 1], i));
        return _fact[n];
    }

    static long[] _ifact = new long[0];
    static long InverseFact(int n)
    {
        long result;
        if (n < _ifact.Length && (result = _ifact[n]) != 0)
            return result;

        long inv = Inverse(Fact(n));
        if (n >= _ifact.Length) Resize(ref _ifact, _fact.Capacity);
        _ifact[n] = inv;
        return inv;
    }

    static long Fact(int n, int m)
    {
        long fact = Fact(n);
        if (m < n) fact = fact * InverseFact(n - m) % MOD;
        return fact;
    }

    static long Comb(int n, int k)
    {
        if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
        return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
    }

    public static long Combinations(long n, int k)
    {
        if (k <= 0) return k == 0 ? 1 : 0;  // Note: n<0 -> 0 unless k=0
        if (k + k > n) return Combinations(n, (int)(n - k));

        long result = InverseFact(k);
        for (long i = n - k + 1; i <= n; i++) result = result * i % MOD;
        return result;
    }
#endregion

#region Common
    partial void TestData();

    static void Swap<T>(ref T a, ref T b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    static void Clear<T>(T[] t, T value = default(T))
    {
        for (int i = 0; i < t.Length; i++)
            t[i] = value;
    }

    static V Get<K, V>(Dictionary<K, V> dict, K key) where V : new()
    {
        V result;
        if (dict.TryGetValue(key, out result) == false)
            result = dict[key] = new V();
        return result;
    }

    static int Bound<T>(T[] array, T value, bool upper = false)
        where T : IComparable<T>
    {
        int left = 0;
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left >> 1);
            int cmp = value.CompareTo(array[mid]);
            if (cmp > 0 || cmp == 0 && upper)
                left = mid + 1;
            else
                right = mid - 1;
        }
        return left;
    }

    static long IntPow(long n, long p)
    {
        long b = n;
        long result = 1;
        while (p != 0)
        {
            if ((p & 1) != 0)
                result = (result * b);
            p >>= 1;
            b = (b * b);
        }
        return result;
    }

    public static int Gcd(int n, int m)
    {
        while (true)
        {
            if (m == 0) return n >= 0 ? n : -n;
            n %= m;
            if (n == 0) return m >= 0 ? m : -m;
            m %= n;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe int Log2(long value)
    {
        double f = unchecked((ulong)value); // +.5 -> -1 for zero
        return (((int*)&f)[1] >> 20) - 1023;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int BitCount(long y)
    {
        ulong x = unchecked((ulong)y);
        x -= (x >> 1) & 0x5555555555555555;
        x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
        x = (x + (x >> 4)) & 0x0f0f0f0f0f0f0f0f;
        return unchecked((int)((x * 0x0101010101010101) >> 56));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // static int HighestOneBit(int n) => n != 0 ? 1 << Log2(n) : 0;
    static unsafe int HighestOneBit(int x)
    {
        double f = unchecked((uint)x);
        return unchecked(1 << (((int*)&f)[1] >> 20) - 1023 & ~((x - 1 & -x - 1) >> 31));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe long HighestOneBit(long x)
    {
        double f = unchecked((ulong)x);
        return unchecked(1L << (((int*)&f)[1] >> 20) - 1023 & ~(x - 1 >> 63 & -x - 1 >> 63));
    }
#endregion

#region Fast IO
#region  Input
    static Stream inputStream;
    static int inputIndex, bytesRead;
    static byte[] inputBuffer;
    static StringBuilder builder;
    const int MonoBufferSize = 4096;
    const char EOL = (char)10, DASH = (char)45, ZERO = (char)48;

    static void InitInput(Stream input = null, int stringCapacity = 16)
    {
        builder = new StringBuilder(stringCapacity);
        inputStream = input ?? Console.OpenStandardInput();
        inputIndex = bytesRead = 0;
        inputBuffer = new byte[MonoBufferSize];
    }

    static void ReadMore()
    {
        if (bytesRead < 0) throw new FormatException();
        inputIndex = 0;
        bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
        if (bytesRead > 0) return;
        bytesRead = -1;
        inputBuffer[0] = (byte)EOL;
    }

    static int Read()
    {
        if (inputIndex >= bytesRead) ReadMore();
        return inputBuffer[inputIndex++];
    }

    static T[] Na<T>(int n, Func<T> func, int z = 0)
    {
        n += z;
        var list = new T[n];
        for (int i = z; i < n; i++) list[i] = func();
        return list;
    }

    static int[] Ni(int n, int z = 0) => Na(n, Ni, z);

    static long[] Nl(int n, int z = 0) => Na(n, Nl, z);

    static string[] Ns(int n, int z = 0) => Na(n, Ns, z);

    static int Ni() => checked((int)Nl());

    static long Nl()
    {
        int c = SkipSpaces();
        bool neg = c == DASH;
        if (neg) { c = Read(); }

        long number = c - ZERO;
        while (true)
        {
            int d = Read() - ZERO;
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
            if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    static char[] Nc(int n)
    {
        char[] list = new char[n];
        for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
        return list;
    }

    static string Ns()
    {
        int c = SkipSpaces();
        builder.Clear();
        while (true)
        {
            if (unchecked((uint)c - 33 >= (127 - 33))) break;
            builder.Append((char)c);
            c = Read();
        }
        return builder.ToString();
    }

    static int SkipSpaces()
    {
        int c;
        do c = Read(); while (unchecked((uint)c - 33 >= (127 - 33)));
        return c;
    }

    static string ReadLine()
    {
        builder.Clear();
        while (true)
        {
            int c = Read();
            if (c < 32) { if (c == 10 || c <= 0) break; continue; }
            builder.Append((char)c);
        }
        return builder.ToString();
    }
#endregion

#region Output
    static Stream outputStream;
    static byte[] outputBuffer;
    static int outputIndex;

    static void InitOutput(Stream output = null)
    {
        outputStream = output ?? Console.OpenStandardOutput();
        outputIndex = 0;
        outputBuffer = new byte[65535];
    }

    static void WriteLine(object obj = null)
    {
        Write(obj);
        Write(EOL);
    }

    static void WriteLine(long number)
    {
        Write(number);
        Write(EOL);
    }

    static void Write(long signedNumber)
    {
        ulong number = unchecked((ulong)signedNumber);
        if (signedNumber < 0)
        {
            Write(DASH);
            number = unchecked((ulong)(-signedNumber));
        }

        Reserve(20 + 1); // 20 digits + 1 extra for sign
        int left = outputIndex;
        do
        {
            outputBuffer[outputIndex++] = (byte)(ZERO + number % 10);
            number /= 10;
        }
        while (number > 0);

        int right = outputIndex - 1;
        while (left < right)
        {
            byte tmp = outputBuffer[left];
            outputBuffer[left++] = outputBuffer[right];
            outputBuffer[right--] = tmp;
        }
    }

    static void Write(object obj)
    {
        if (obj == null) return;

        string s = obj.ToString();
        Reserve(s.Length);
        for (int i = 0; i < s.Length; i++)
            outputBuffer[outputIndex++] = (byte)s[i];
    }

    static void Write(char c)
    {
        Reserve(1);
        outputBuffer[outputIndex++] = (byte)c;
    }

    static void Write(byte[] array, int count)
    {
        Reserve(count);
        Copy(array, 0, outputBuffer, outputIndex, count);
        outputIndex += count;
    }

    static void Reserve(int n)
    {
        if (outputIndex + n <= outputBuffer.Length)
            return;

        Dump();
        if (n > outputBuffer.Length)
            Resize(ref outputBuffer, Max(outputBuffer.Length * 2, n));
    }

    static void Dump()
    {
        outputStream.Write(outputBuffer, 0, outputIndex);
        outputIndex = 0;
    }

    static void Flush()
    {
        Dump();
        outputStream.Flush();
    }

#endregion
#endregion

#region Main

    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
        {
            Flush();
            var e = (Exception)arg.ExceptionObject;
            Console.Error.WriteLine(e);
            int line = new StackTrace(e, true).GetFrames()
                .Select(x => x.GetFileLineNumber()).FirstOrDefault(x => x != 0);
            int wait = line % 300 * 10 + 5;
            var process = Process.GetCurrentProcess();
            while (process.TotalProcessorTime.TotalMilliseconds > wait && wait < 3000) wait += 1000;
            while (process.TotalProcessorTime.TotalMilliseconds < Min(wait, 3000)) ;
            Environment.Exit(1);
        };

        InitInput(Console.OpenStandardInput());
        InitOutput(Console.OpenStandardOutput());
#if __MonoCS__ && !C7
        var thread = new System.Threading.Thread(()=>new Solution().Solve());
        var f = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = thread.GetType().GetField("internal_thread", f).GetValue(thread);
        t.GetType().GetField("stack_size", f).SetValue(t, 32 * 1024 * 1024);
        thread.Start();
        thread.Join();
#else
        new Solution().Solve();
#endif
        Flush();
        Console.Error.WriteLine(Process.GetCurrentProcess().TotalProcessorTime);
    }
#endregion
#endregion
}
class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}
