#!/bin/python3

import sys

n, s, t = input().strip().split(' ')
n, s, t = [int(n), int(s), int(t)]
r_0, g, seed, p = input().strip().split(' ')
r_0, g, seed, p = [int(r_0), int(g), int(seed), int(p)]

def fix(x):
    x %= n
    return x if x >= 0 else x + n

def betw(left, right, x):
    c = right - left
    left = fix(left)
    if x < left: x += n
    return x - left <= c

def circularWalk(n, s, t, r0, g, seed, p):
    r = [0 for i in range(0,n)]
    r[0] = r0
    for i in range(1,n):
        r[i] = (r[i-1] * g + seed) % p
    
    left = s
    right = s
    minl = left - r[s]
    maxr = right + r[s]
    
    k=0
    while True:
        if betw(left, right, t): return k
        if left==minl and right==maxr: return -1
        k += 1   
    
        oldl,oldr,left,right = left,right,minl,maxr
        
        for i in range(left, oldl):
            v = r[fix(i)]
            minl, maxr = min(i-v, minl), max(i+v, maxr)
            
        for i in range(oldr+1, right+1):
            v = r[fix(i)]
            minl, maxr = min(i-v, minl), max(i+v, maxr)

result = circularWalk(n, s, t, r_0, g, seed, p)
print(result)
