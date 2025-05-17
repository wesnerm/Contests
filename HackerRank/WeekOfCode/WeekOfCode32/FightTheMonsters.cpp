#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
#include <bits/stdc++.h>
using namespace std;



int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);
    
    long long n, hit, t;
    cin >> n >> hit >> t;
    
    auto h = new long long[n];
    for (int i=0; i<n; i++)
        cin >> h[i];
    
    sort(h, h+n);

    int i = 0;
    while (t>0 && i<n)
    {
        auto div = (h[i] + hit - 1) / hit;
        t -= div;
        if (t < 0) break;
        i++;
    }

    cout << i;

    /* Enter your code here. Read input from STDIN. Print output to STDOUT */   
    return 0;
}
