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
// Powered by caide (code generator, tester, and library code inliner)


typedef long long ll;


const int MAXN = 1000 * 1000 * 10 + 1;

int r[MAXN];

class Solution {
public:

	ll n, s, t;
	ll r0, g, seed, p;

	void solve(istream& in, ostream& out) {

		in >> n >> s >> t;
		in >> r[0] >> g >> seed >> p;

		for (int i= 1; i<n; i++)
		{
			ll ri = 1LL * r[i - 1] * g + seed; 
			r[i] = ri % p;
		}

		ll left = s;
		ll right = s;
		ll maxLeft = left - r[s];
		ll maxRight = right + r[s];

		ll answer = -1;
		for (int k = 0; k <= n; k++)
		{
			if (between(left, right, t))
			{
				answer = k;
				break;
			}

			if (left == maxLeft && right == maxRight)
				break;

			auto oldLeft = left;
			auto oldRight = right;
			left = maxLeft;
			right = maxRight;

			for (ll i = left; i < oldLeft; i++)
			{
				auto v = r[fix(i)];
				maxLeft = min(i - v, maxLeft);
				maxRight = max(i + v, maxRight);
			}

			for (ll i = oldRight + 1; i <= right; i++)
			{
				auto v = r[fix(i)];
				maxLeft = min(i - v, maxLeft);
				maxRight = max(i + v, maxRight);
			}
		}

		out << answer;
	}

	bool between(ll left, ll right, ll x)
	{
		auto count = right - left;
		left = fix(left);
		if (x < left) x += n;
		return x - left <= count;
	}

	ll fix(ll x) { 	return (x % n + n) % n; }
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

