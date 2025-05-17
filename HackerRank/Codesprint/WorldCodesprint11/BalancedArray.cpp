#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
using namespace std;

// https://www.hackerrank.com/contests/world-codesprint-11/challenges

typedef long long ll;
int main() {
    int n;
    cin >> n;
    
    int a[n];
    int sum = 0;
    for (int i=0; i<n; i++)
    {
        cin >> a[i];
        sum += a[i];
    }

    n/=2;
    ll h = 0;
    for (int i=0; i<n; i++)
        h += a[i];

    cout<< abs(sum-2*h);
    return(0);    
}
