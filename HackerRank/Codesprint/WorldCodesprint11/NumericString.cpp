#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
using namespace std;

// https://www.hackerrank.com/contests/world-codesprint-11/challenges/numeric-string

typedef long long ll;

int main() {
    
    int k,b,m;
    string s;
    cin >> s >> k >> b >> m;
    
    ll bk = 1;
    for (int i = 1; i <= k; i++)
        bk = bk * b % m;

    ll sum1 = 0;
    ll result = 0;
    for (int i = 0; i< s.length(); i++)
    {
        int d = s[i] - '0';

        sum1 = (b * sum1 + d) % m;

        int x = i - k;
        if (x >= 0)
        {
            int d2 = s[(int)x] - '0';
            sum1 = ((sum1 - d2 * bk) % m + m) % m;
        }


        if (x >= -1)
            result += sum1;
    }

    
    cout << result;
    return 0;
}
