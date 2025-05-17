#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
#include <functional>
using namespace std;

int n;
string s;
const int MAXN = 500000;
int factors[MAXN+1];
typedef long long ll;

ll dfs(ll d, ll j, ll f, int(*action)(ll,ll))
{
    if (f > j) return 0;
    if (d==1)
    {
        return action(f,j);
    }
    
    auto p = factors[d];
    int c = 1;
    ll next = d/p;
    while (next > 1 && factors[next]==p) { c++; next/=p; }

    c*=2;
    long res = dfs(next, j, f, action);
    while (c-- >0)
    {
        f*=p;
        res += dfs(next, j, f, action);
    }
    return res;        
}

int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);
    cin >> n >> s;
    
    for (int i=0; i<=MAXN; i++)
        factors[i] = i%2==0 ? 2 : i;
   
    int sq = (int) sqrt(n);
    for (int i=3; i<=sq; i++)
        if (factors[i] == i)
        {
            for (int j=i*i; j<=n; j+=2*i)
                if (factors[j] == j) 
                    factors[j] = i;
        }
    
    
    int (*func)(ll,ll) = [](ll f, ll j1){
                if (s[f-1]=='b') return 0;
                ll f2 = j1*j1/f;
                if (f2<=n && s[f-1]==(s[f2-1]^2)) return 1;
                return 0;
            };
    
    ll count = 0;
    for (ll j=0; j<n; j++)
        if (s[j]=='b')
            count += dfs(j+1, j+1, 1, func);
    cout << count;
    
    return 0;
}
