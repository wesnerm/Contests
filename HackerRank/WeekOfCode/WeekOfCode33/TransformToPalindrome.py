#!/bin/python3
# https://www.hackerrank.com/contests/w33/challenges/transform-to-palindrome

import sys

def find(x):
    if ds[x]==x: return x
    ds[x] = find(ds[x])
    return ds[x]

def union(x,y):
    ds[find(y)] = find(x)

n,k,m = input().strip().split(' ')
n,k,m = [int(n),int(k),int(m)]
ds = [i for i in range(0,n+1)]
for a0 in range(k):
    x,y = input().strip().split(' ')
    x,y = [int(x),int(y)]
    union(x,y)
    
a = [find(int(x)) for x in input().strip().split(' ')]
#print(a)
dp = [[0] * m for i in range(m)]
    
for i in range(0,m):
    dp[i][i] = 1

pl = 1
for length in range(1,m):
    for i in range(0,m-length):
        j = i+length
        tmp = (dp[i+1][j-1] + 2) if a[i]==a[j] else 0
        #print(dp);
        #print("dp[{0},{1}] = max({2},{3},{4})".format(i,j,dp[i][j-1], dp[i+1][j], tmp))
        dp[i][j] = max(dp[i][j-1], dp[i+1][j], tmp)
        pl = max(dp[i][j], pl)    

print(pl)

