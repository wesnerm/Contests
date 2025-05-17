#!/bin/python3

import sys
import math

def geometricTrick(s):
    return sum( dfs(j+1,j+1,1) for j in range(n) if s[j] == "b" )
    
def dfs(d, j, f):
    if f>j: return 0
    if d==1:
        if s[f-1] == "b": return 0
        f2 = j*j // f
        return (1 if f2-1 < len(s) and ord(s[f-1]) == (ord(s[f2-1]) ^ 2) else 0) 
    
    p = factors[d]
    c = 1
    next = d // p
    while next>1 and factors[next] == p:
        c += 1
        next //= p
    
    res = dfs(next, j, f)
    for i in range(c*2):
        f *= p
        res += dfs(next, j, f)
    return res
    
n = int(input().strip())
s = input().strip()

factors = [ (2 if i%2==0 else i) for i in range(n+1)]
sqrt = int(math.sqrt(n))
for i in range(3, sqrt+1, 2):
    if factors[i] != i: continue
    for j in range(i*i, n+1, i+i):
        if factors[j] == j:
            factors[j] = i

result = geometricTrick(s)
print(result)
