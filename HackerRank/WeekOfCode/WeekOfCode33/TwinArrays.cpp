#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
using namespace std;

#define long long long

void getmins(int n, long &a1, long &a2, long &v1, long &v2)
{
    a1 = 0;
    a2 = 1;
    cin >> v1 >> v2;
    if (v1 < v2) { swap(a1, a2); swap(v1, v2); }
    
    for (int i=2; i<n; i++)
    {
        long v;
        cin >> v;
       
        if (v > v1) continue;
        a1 = i;
        v1 = v;
        if (v1 < v2) { swap(a1, a2); swap(v1, v2); }
    }
}

int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);

    long n;
    cin >> n;
    
    long a1, a2, b1, b2, av1, av2, bv1, bv2;
    getmins(n, a1, a2, av1, av2);
    getmins(n, b1, b2, bv1, bv2);
    
    // cout << a1 << a2 << b1 << b2 << "\n";
    // cout << av1 << av2 << bv1 << bv2 << "\n";
    
    if (a2 != b2) cout << ( av2 + bv2 );
    else cout << min( av1 + bv2, av2 + bv1 );
    
    return 0;
}


