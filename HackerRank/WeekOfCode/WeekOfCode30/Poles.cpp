#include <bits/stdc++.h>
#include <stdio.h>

#define ll long long

using namespace std;

int x[5001];
int w[5001];
ll rmq[5001];
ll acache1[5001];
ll acache2[5001];


#define MAXCOST (1L << 48)

ll MinCost(int k, int n)
{
	ll * cache = acache1;
	ll * cache2 = acache2;

	for (int nn = 1; nn <= n; nn++)
		cache[nn] = MAXCOST;

	for (int kk = 1; kk <= k; kk++)
	{
		cache2[kk] = 0;

		ll minc = 1L << 45;
		int limit = n - (k - kk);
		for (int i = kk - 1; i<limit; i++)
			rmq[i] = minc = min(cache[i], minc);

		for (int nn = kk + 1; nn <= limit; nn++)
		{
			long rollCost = 0;
			long rollsCost = 0;
			long minCost = MAXCOST;
			long xprev = x[nn - 1];
			for (int i = nn - 1; i >= kk - 1; i--)
			{
				rollsCost += rollCost * (xprev - x[i]);
				long cost = cache[i] + rollsCost;
				if (cost < minCost) minCost = cost;
				else  if (rollsCost + rmq[i] >= minCost) break;
				rollCost += w[i];
				xprev = x[i];
			}
			cache2[nn] = minCost;
		}

		auto tmp = cache;
		cache = cache2;
		cache2 = tmp;
	}

	return cache[n];
}

int main() {
	int n;
	int k;
	scanf("%d %d", &n, &k);
	for (int i = 0; i < n; i++)
		scanf("%d %d", &x[i], &w[i]);

	printf("%lld", MinCost(k, n));
	return 0;
}

