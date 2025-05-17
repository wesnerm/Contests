#include <bits/stdc++.h>
using namespace std;

int A[1000001];
int B[1000001];
char C[1000001];
int n;

int maximumGcdAndSum() {

    int max = 1000001;
    
    for (int i=0; i<n; i++)
    {
        C[A[i]] |= 1;
        C[B[i]] |= 2;
    }

    int maxsum = 0;
    for (int i=max-1; i>=1; i--)
    {
        int maxa = 0;
        int maxb = 0;
        for (int j=i; j<max ;j+=i)
        {
            int v = C[j];
            if ((v & 1)!=0) { maxa=j; }
            if ((v & 2)!=0) { maxb=j; }
        }
        if (maxa!=0 && maxb!=0)
        {
            maxsum = maxa + maxb;
            break;
        }
    }
    
    return maxsum;
}

int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);
    cin >> n;
    for (int i = 0; i < n; i++) {
       cin >> A[i];
    }
    for (int i = 0; i < n; i++) {
       cin >> B[i];
    }
    int res = maximumGcdAndSum();
    cout << res;
    return 0;
}
