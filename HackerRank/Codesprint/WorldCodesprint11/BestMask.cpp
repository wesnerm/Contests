#include <algorithm>
#include <iomanip>
#include <istream>
#include <map>
#include <numeric>
#include <ostream>
#include <set>
#include <sstream>
#include <string>
#include <utility>
#include <vector>
#include <iostream>
using namespace std;
// https://www.hackerrank.com/contests/world-codesprint-11/challenges

#include <limits>

int list[100001];
int lists[27][100001];
int n;

const int intmax = 1 << 30;

class Solution {

public:
    void solve(istream& in, ostream& out) {
		
		in >> n;

		for (int i = 0; i < n; i++)
			in >> list[i];

		sort(list, list+n);
		int max = list[n - 1];

		int result = Dfs(list, n);
		out << result;

    }

	int Dfs(int * listParam, int length, int mask = 0, int exclude = 0, int max = intmax, int bitcount = 31, int depth = 0)
	{
		int * lst = &lists[depth][0];

		int listCount = 0;
		auto prev = -1;
		auto an = -1;
		int minBitCount = intmax;
		int minBitValue = intmax;
		for (int i = 0; i < length; i++)
		{
			auto v = listParam[i] & ~exclude;
			if ((listParam[i] & mask) != 0 || v == prev) continue;
			lst[listCount++] = v;
			an &= v;
			prev = v;

			auto bc = BitCount(v & ~exclude);
			if (bc <= minBitCount)
			{
				if (bc < minBitCount)
				{
					if (bc == 0)
						return max;

					minBitCount = bc;
					minBitValue = v;
				}
				else
					minBitValue = min(minBitValue, v);
			}
		}

		if (listCount == 0)
			return 0;

		if (an != 0)
			return an & -an;

		int result = max;
		int mask2 = minBitValue;
		while (mask2 != 0)
		{
			int bit = mask2 & -mask2;
			mask2 -= bit;

			auto check = bit | mask;
			int cmp = compare(BitCount(check), bitcount);
			if (cmp > 0 || cmp == 0 && check >= result) continue;

			auto tmp = bit | mask | Dfs(lst, listCount, bit | mask, exclude, result, bitcount, depth + 1);
			cmp = compare(BitCount(tmp), bitcount);
			if (cmp < 0 || cmp == 0 && tmp < result)
			{
				result = tmp;
				bitcount = BitCount(tmp);
			}

			exclude |= bit;
		}

		return result;
	}

	int compare(int x, int y)
	{
		if (x < y) return -1;
		if (x > y) return 1;
		return 0;
	}

	int BitCount(int x)
	{
		int count;
		auto y = ((unsigned)x);
		for (count = 0; y != 0; count++)
			y &= y - 1;
		return count;
	}
};

void solve(istream& in, ostream& out)
{
    out << setprecision(12);
    Solution solution;
    solution.solve(in, out);
}


#include <fstream>


int main() {
    
    ios_base::sync_with_stdio(0);
    cin.tie(0);


    istream& in = cin;


    ostream& out = cout;

    solve(in, out);
    return 0;
}


