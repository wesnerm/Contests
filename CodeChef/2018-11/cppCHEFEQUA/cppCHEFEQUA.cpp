#include <bits/stdc++.h>


#define all(x) (x).begin(), (x).end()

using namespace std;

inline int nxt() {
	int x;
	scanf("%d", &x);
	return x;
}

#ifdef LOCAL
ostream& operator <<(ostream& ostr, const vector<auto>& v) {
	ostr << "{";
	for (int i = 0; i < (int)v.size(); ++i) {
		if (i) {
			ostr << ", ";
		}
		ostr << v[i];
	}
	ostr << "}";
	return ostr;
}
#endif

const int mod = 1000000007;

const int L = 21;
const int N = 1 << L;
bool fft_initialized = false;

using ld = long double;
using base = complex<ld>;
using Poly = vector<long long>;

const ld pi = acosl(-1);
base angles[N + 1];
int bitrev[N];

inline int getInv(int a, int b) {
	return a == 1 ? 1 : b - 1ll * getInv(b % a, a) * b / a % b;
}

// don't know why such eps, may be changed
const ld eps = 1e-7;

inline bool eq(ld x, ld y) {
	return abs(x - y) < eps;
}

void fft_init() {
	for (int i = 0; i <= N; ++i) {
		angles[i] = { cosl(2 * pi * i / N), sinl(2 * pi * i / N) };
	}

	for (int i = 0; i < N; ++i) {
		int x = i;
		for (int j = 0; j < L; ++j) {
			bitrev[i] = (bitrev[i] << 1) | (x & 1);
			x >>= 1;
		}
	}

	fft_initialized = true;
}

inline int revBit(int x, int len) {
	return bitrev[x] >> (L - len);
}

void fft(vector<base>& a, bool inverse = false) {
	assert(fft_initialized && "you fucking cunt just write fft_init()");
	int n = a.size();
	assert(!(n & (n - 1)));	// work only with powers of two
	int l = __builtin_ctz(n);

	for (int i = 0; i < n; ++i) {
		int j = revBit(i, l);
		if (i < j) {
			swap(a[i], a[j]);
		}
	}

	for (int len = 1; len < n; len *= 2) {
		for (int start = 0; start < n; start += 2 * len) {
			for (int i = 0; i < len; ++i) {
				base x = a[start + i], y = a[start + len + i];
				int idx = N / 2 / len * i;
				base w = y * angles[inverse ? N - idx : idx];
				a[start + i] = x + w;
				a[start + len + i] = x - w;
			}
		}
	}

	if (inverse) {
		for (auto& x : a) {
			x /= n;
		}
	}
}

Poly multiply(Poly a, Poly b) {
	int n = 1;
	while (n < (int)a.size() || n < (int)b.size()) {
		n *= 2;
	}
	vector<base> ar(n + n), br(n + n);
	for (int i = 0; i < (int)a.size(); ++i) {
		ar[i] = a[i];
	}
	for (int i = 0; i < (int)b.size(); ++i) {
		br[i] = b[i];
	}
	fft(ar);
	fft(br);
	for (int i = 0; i < n + n; ++i) {
		ar[i] = ar[i] * br[i];
	}
	fft(ar, true);
	a.resize(ar.size());
	for (int i = 0; i < n + n; ++i) {
		a[i] = (long long)(ar[i].real() + 0.5);
		a[i] %= mod;
	}
	return a;
}

const int shift = 15;
const int first_mod = 1 << shift;

Poly add(Poly a, const Poly& b) {
	a.resize(max(a.size(), b.size()));
	for (int i = 0; i < (int)b.size(); ++i) {
		a[i] = (a[i] + b[i]) % mod;
	}
	return a;
}

bool isGood(const Poly& a) {
	for (auto x : a) {
		if (x < 0 || x >= mod) {
			return false;
		}
	}
	return true;
}

Poly multiply_large(Poly a, Poly b, int k = 0) {
	// cerr << a << " * " << b << " = ";
	if (k == 0) {
		k = max(0, (int)a.size() + (int)b.size() - 2);
	}
	int n = 1;
	while (n < (int)a.size() || n < (int)b.size()) {
		n *= 2;
	}
	a.resize(n + n);
	b.resize(n + n);
	vector<base> tempA(n + n), tempB(n + n);
	for (int i = 0; i < n + n; ++i) {
		tempA[i] = base(((int)a[i]) & (first_mod - 1), a[i] >> shift);
		tempB[i] = base(((int)b[i]) & (first_mod - 1), b[i] >> shift);
	}
	fft(tempA);
	fft(tempB);
	vector<base> tempF(n + n), tempS(n + n);
	for (int i = 0; i < n + n; ++i) {
		int j = (n + n - i) & (n + n - 1);
		base a1 = (tempA[i] + conj(tempA[j])) / base(2, 0);
		base a2 = (tempA[i] - conj(tempA[j])) / base(0, 2);
		base b1 = (tempB[i] + conj(tempB[j])) / base(4 * n, 0);
		base b2 = (tempB[i] - conj(tempB[j])) / base(0, 4 * n);
		tempF[j] = a1 * b1 + a2 * b2 * base(0, 1);
		tempS[j] = a1 * b2 + a2 * b1;
	}
	fft(tempF);
	fft(tempS);

	Poly result(n + n);
	for (int i = 0; i < (int)result.size(); ++i) {
		long long x = round(real(tempF[i])), y = round(real(tempS[i])), z = round(imag(tempF[i]));
		x %= mod;
		y %= mod;
		z %= mod;
		result[i] = (x + (y << shift) + (z << (2 * shift))) % mod;
	}
	if ((int)result.size() > k + 1) {
		result.resize(k + 1);
	}
	// cerr << result << "\n";
	return result;
}

Poly derivative(Poly a) {
	if (a.empty()) {
		return a;
	}
	for (int i = 0; i < (int)a.size(); ++i) {
		a[i] = a[i] * i % mod;
	}
	a.erase(a.begin());
	return a;
}

// returns $b(x) = \int_0^x{a(t)\,dt}$
Poly primitive(Poly a) {
	if (a.empty()) {
		return a;
	}
	for (int i = 0; i < (int)a.size(); ++i) {
		a[i] = a[i] * getInv(i + 1, mod) % mod;
	}
	a.insert(a.begin(), 0);
	return a;
}

Poly sub(Poly a, const Poly& b) {
	a.resize(max(a.size(), b.size()));
	for (int i = 0; i < (int)b.size(); ++i) {
		a[i] = (a[i] + mod - b[i]) % mod;
	}
	return a;
}

Poly normalize(Poly a) {
	while (!a.empty() && a.back() == 0) {
		a.pop_back();
	}
	return a;
}

#define multiply multiply_large

// get such $b$ that $a\cdot b = 1 \pmod{x^{prec}}$
Poly getInversed(Poly a, int prec) {
	assert(a[0]);

	Poly res = { getInv(a[0], mod) };
	int k = 1;
	while (k < prec) {
		k *= 2;
		Poly tmp = multiply(res, Poly({ a.begin(), a.begin() + min(k, (int)a.size()) }));
		for (auto& x : tmp) {
			x = x ? mod - x : 0;
		}
		tmp[0] = (tmp[0] + 2) % mod;

		res = multiply(tmp, res);
		res.resize(k);
	}
	res.resize(prec);
	return res;
}

// get such q and r that a = b * q + r, deg(r) < deg(b)
pair<Poly, Poly> divMod(Poly a, Poly b) {
	int n = a.size();
	int m = b.size();
	if (n < m) {
		return { {0}, a };
	}
	reverse(all(a));
	reverse(all(b));
	auto quotient = multiply(a, getInversed(b, n - m + 1));
	quotient.resize(n - m + 1);
	reverse(all(a));
	reverse(all(b));
	reverse(all(quotient));
	auto remainder = sub(a, multiply(b, quotient));
	while (!remainder.empty() && remainder.back() == 0) {
		remainder.pop_back();
	}
	return { quotient, remainder };
}

// this is for multipoint and interpolate functions
vector<Poly> getSegmentProducts(const vector<long long>& pts) {
	vector<Poly> segment_polys;
	function<int(int, int)> fill_polys = [&](int l, int r) {
		if (l + 1 == r) {
			segment_polys.push_back({ (mod - pts[l]) % mod, 1 });
			return (int)segment_polys.size() - 1;
		}
		int m = (l + r) / 2;
		int i = fill_polys(l, m);
		int j = fill_polys(m, r);
		auto new_poly = multiply(segment_polys[i], segment_polys[j]);
		segment_polys.push_back(new_poly);
		return (int)segment_polys.size() - 1;
	};
	fill_polys(0, pts.size());

	return segment_polys;
}

// get p and {x1, x2, ..., xn}, return {p(x1), p(x2), ..., p(xn)}
vector<long long> multipoint(const Poly& poly, const vector<long long>& pts) {
	if (pts.empty()) {
		return {};
	}

	vector<Poly> segment_polys = getSegmentProducts(pts);
	vector<long long> ans;
	function<void(const Poly&)> fill_ans = [&](const Poly& p) {
		if ((int)segment_polys.back().size() <= 2) {
			ans.push_back(p.empty() ? 0 : p[0]);
			segment_polys.pop_back();
			return;
		}
		segment_polys.pop_back();
		fill_ans(divMod(p, segment_polys.back()).second);
		fill_ans(divMod(p, segment_polys.back()).second);
	};
	fill_ans(poly);
	reverse(all(ans));

	return ans;
}

// get {x1, ..., xn} and {y1, ..., yn}, return such p that p(xi) = yi
Poly interpolate(const vector<long long>& xs, const vector<long long>& ys) {
	assert(xs.size() == ys.size());
	if (xs.empty()) {
		return { 0 };
	}

	vector<Poly> segment_polys = getSegmentProducts(xs);
	auto der = derivative(segment_polys.back());
	auto coeffs = multipoint(der, xs);
	for (auto& c : coeffs) {
		c = getInv(c, mod);
	}
	for (int i = 0; i < (int)ys.size(); ++i) {
		coeffs[i] = coeffs[i] * ys[i] % mod;
	}

	function<Poly()> get_ans = [&]() {
		Poly res;
		if (segment_polys.back().size() <= 2) {
			segment_polys.pop_back();
			res = { coeffs.back() };
			coeffs.pop_back();
		}
		else {
			segment_polys.pop_back();

			auto p1 = segment_polys.back();
			auto q1 = get_ans();

			auto p2 = segment_polys.back();
			auto q2 = get_ans();

			res = add(multiply(p1, q2), multiply(p2, q1));
		}
		return res;
	};
	return normalize(get_ans());
}

// takes 1 + b, returns b - b^2/2 + b^3/3 - ... mod x^{prec}
// ofc b must be divisible by x
Poly logarithm(Poly a, int prec) {
	assert(a[0] == 1);
	auto res = primitive(multiply(derivative(a), getInversed(a, prec)));
	res.resize(prec);
	return res;
}

// returns 1 + a + a^2/2 + a^3/6 + ... mod x^{prec}
// ofc a must be divisible by x
Poly exponent(Poly a, int prec) {
	assert(a[0] == 0);

	Poly res = { 0 };
	int k = 1;
	while (k < prec) {
		k *= 2;
		Poly tmp = { a.begin(), a.begin() + min(k, (int)a.size()) };
		tmp[0] += 1;
		tmp = sub(tmp, logarithm(res, k));

		res = multiply(tmp, res);
		res.resize(k);
	}
	res.resize(prec);
	return res;
}

Poly shiftPoly(Poly a, int k) {
	while (k--) {
		a.insert(a.begin(), 0);
	}
	return a;
}

const int NN = 2111111;
long long fact[NN];
long long inv[NN];
long long invfact[NN];

void solve() {

	int n;
	long t;
	cin >> n;
	vector<long long> A(n);
	vector<long long> C(n);
	for (int i = 0; i < n; i++)
		cin >> A[i];

	for (int i = 0; i < n; i++)
		cin >> C[i];

	auto result = interpolate(A, C);

	for (int i = 0; i < n; i++)
		cout << result[i] << " ";
	cout << endl;

}

int main() {
	fft_init();
	int t = 1;
	while (t--) {
		solve();
	}

	return 0;
}