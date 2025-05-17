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
using namespace std;
// Powered by caide (code generator, tester, and library code inliner)

const int N = 1000 + 5;

vector <int> v[N];
int n, s[N], ans1[N], ans2[N];

static int Gx, Gy;


struct point {
	int x, y, id;
};

vector <point> p;



static bool mycomp1(point a, point b) {
	return (a.x - Gx)*(b.y - Gy) > (b.x - Gx)*(a.y - Gy);
}

void dfs1(int g, int fr) {
	s[g] = 1;
	for (int t : v[g]) {
		if (t != fr) {
			dfs1(t, g);
			s[g] += s[t];
		}
	}
}


void dfs2(int g, int fr, int l, int r) {

	for (int i = l + 1; i < r; i++) {
		if (p[l].x > p[i].x || (p[l].x == p[i].x && p[l].y > p[i].y)) {
			swap(p[l], p[i]);
		}
	}

	ans1[g] = p[l].x;
	ans2[g] = p[l].y;

	Gx = p[l].x;
	Gy = p[l].y;

	l++;

	sort(p.begin() + l, p.begin() + r, mycomp1);

	for (int t : v[g]) {
		if (t != fr) {
			dfs2(t, g, l, l + s[t]);
			l += s[t];
		}
	}
}
	


void solve(std::istream& in, std::ostream& out) {
	in >> n;
	int maxlen = 0;
	for (int i = 1; i < n; i++) {
		int x, y, len;
		in >> x >> y >> len;;
		v[x].push_back(y);
		v[y].push_back(x);
		if (len > maxlen) len = maxlen;
	}

	for (int i = 0; i<32; i++)
		for (int j = 0; j<32; j++)
		{
			point po;
			po.x = i * maxlen;
			po.y = i * maxlen;
			po.id = i;
			p.push_back(po);
		}

	dfs1(1, 1);
	dfs2(1, 1, 0, int(p.size()));
	for (int i = 1; i <= n; i++)
		out << ans1[i] << ans2[i];
}

class Solution
{
public:
	void solve(std::istream& in, std::ostream& out)
	{
		solve(in, out);
	}
};