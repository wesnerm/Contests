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
using NUnit.Framework;
using static System.Array;
using static System.Math;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0675
#endregion


[TestFixture]
partial class Solution
{
 

    [Test]
    public void TestDerivative()
    {
        Assert.AreEqual(new long[] { 1, 2, 3 }, Derivative(new long[] { 1, 1, 1, 1 }));
    }

    [Test]
    public void MultipointTest()
    {
        var poly = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        var xs = new long[] { 0, 1, 2, 300, 500, 1000, 100000, 1000000, 99999999 };

        var results = Multipoint(poly, xs);
        var results2 = Evaluate(poly, xs);
        Assert.AreEqual(results2, results);

    }

    [Test]
    public void AddTest()
    {
        var a = new long[] {1, 2, 3, 4, 5};
        var b = new long[] {7, 6, 8, 9, 10, 11, 12};

        Assert.AreEqual(new long[] {8, 8, 11, 13, 15, 11, 12}, Add(a, b));
    }

    [Test]
    public void SubTest()
    {
        var a = new long[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var b = new long[] { 7, 6, 8, 9, 10, 11, 12 };

        Assert.AreEqual(new long[] { 6, 4, 5, 5, 5, 5, 5, MOD-8 }, Sub(b, a));
    }


    public long[] Evaluate(long[] poly, long[] xs )
    {
        return xs.Select(x => Evaluate(poly, x)).ToArray();
    }


}
