#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
using namespace std;

int tabledata[1005*1005];
int sumsdata[1005*1005];
int* table[100002];
int* sums[100002];
int mask[1<<10];
int maskts[1<<10];
int counter = 1;

int main() {
    ios::sync_with_stdio(false);
    cin.tie(0);

    int n, m;
    cin >> n >> m;
    bool swap = false;

    if (n<=m)
    {
        table[0] = new int[m+1];
        for (int i=1; i<=n; i++)
        {
            auto t = table[i] = &tabledata[ (m+1) * i ];
            for (int j=1; j<=m; j++)
            {
                int d;
                cin >> d;
                t[j] = 1 << d;
            }
        }
    } 
    else
    {
        for (int j=0; j<=m; j++)
            table[j] = &tabledata[ (n+1) * j ];

        for (int i=1; i<=n; i++)
            for (int j=1; j<=m; j++)
            {
                int d;
                cin >> d;
                table[j][i] = 1 << d;
            }
        int tmp = n;
        n = m;
        m = tmp;
        swap = true;
    }

    for (int i=0; i<=n; i++)
    {
        auto t = table[i];
        auto s = sums[i] = new int[m+1];
        for (int j=0; j<=m; j++)
            s[j] = t[j]>1 ? 1 : 0;
    }

    for (int i=0; i<=n; i++)
        for (int j=1; j<=m; j++)
        {
            table[i][j] ^= table[i][j-1];
            sums[i][j] += sums[i][j-1];
        }

    for (int i=1; i<=n; i++)
        for (int j=0; j<=m; j++)
        {
            table[i][j] ^= table[i-1][j];
            sums[i][j] += sums[i-1][j];
        }

    int i0=0,i1=0,j0=0,j1=0;
    int area=1;

    for (int ii=n; ii>0; ii--)
        for (int jj=m; jj>0; jj--)
        {
            int xr = table[ii][jj];
            int newarea = ii*jj;
            if (newarea <= area || sums[ii][jj]<=1) break;
            if ((xr & xr-1)!=0) continue;
            area = newarea;
            i1 = ii-1;
            j1 = jj-1;
        }

        for (int i=0; i<n; i++)
            for (int ii=n; ii>i; ii--)
            {             

                maskts[0] = ++counter;
                mask[0] = 0;
                for (int jj=1; jj<=m; jj++)
                {
                    int xs = table[ii][jj] ^ table[ii][0] ^ table[i][jj] ^ table[i][0];
                    if (maskts[xs]<counter)
                    {
                        maskts[xs]=counter;
                        mask[xs] = jj;
                    }

                    int j = m+1;
                    for (int bit=1<<10; bit>0; )
                    {
                        bit >>= 1;
                        int mymask = bit ^ xs;
                        if (maskts[mymask] == counter && mask[mymask] < j) 
                            j = mask[mymask];
                    }                

                    if (j >= jj) continue;

                    int newarea = (ii-i)*(jj-j);
                    if ( newarea <= area ) continue;

                    int sum = sums[ii][jj] - sums[ii][j] - sums[i][jj] + sums[i][j];
                    if (sum>1)
                    {
                        area = newarea;
                        i0 = i;
                        j0 = j;
                        i1 = ii-1;
                        j1 = jj-1;
                    }
                }
            }

    cout << area << "\n";
    if (swap) cout << j0 << " " << i0 << " " << j1 << " " << i1 << "\n";
    else cout << i0 << " " << j0 << " " << i1 << " " << j1 << "\n";
}
