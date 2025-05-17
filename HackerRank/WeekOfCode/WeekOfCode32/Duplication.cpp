#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
using namespace std;


int dupe(int n)
{
    return n==0 ? 0 : 1 - dupe(n - (1<<~__builtin_clz(n)));   
}


int main() {
    ios::sync_with_stdio(false);
    
    int q, n;
    
    cin >> q;
    while (q-->0)
    {
        cin >> n;
        cout << dupe(n) << endl;
    }
    
    return 0;
}
