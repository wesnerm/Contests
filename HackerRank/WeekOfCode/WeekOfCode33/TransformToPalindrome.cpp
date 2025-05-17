#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
#include <cstring>

using namespace std;

int ds[1000001];
int a[1001];
int dp[1001][1001];


int find(int x)
{
    if (ds[x] == 0) ds[x] = x;  
    if (ds[x] == x) return x;
    return ds[x] = find(ds[x]);
}

int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);
    
    int n, k, m;
    cin >> n >> k >> m;
        
    for(int a0 = 0; a0 < k; a0++){
        int x, y;
        cin >> x >> y;
        ds[find(y)] = find(x);
    }

    for(int a_i=0; a_i < m; a_i++){
        int x;
        cin >> x;
        a[a_i] = find(x);
    }

    for (int i=0; i<m; i++)
        dp[i][i] = 1;

    int mx = 1;
    for (int len=1; len<m; len++)
        for (int i=0; i+len<m; i++)
    {
        int j=i+len;
        dp[i][j] = max(max(dp[i+1][j], dp[i][j-1]), dp[i+1][j-1] + (a[i]==a[j]?2:0));
        mx = max(dp[i][j], mx);
    }

    cout << mx;
    return 0;
}
