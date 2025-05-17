#!/bin/python3

import sys

def getMaxMonsters(n, hit, t, h):
    h = sorted(h)
    i = 0
    while t>0 and i<n:
        div = (h[i]+hit-1) // hit
        t -= div
        if t<0: break
        i += 1
    return i

n, hit, t = input().strip().split(' ')
n, hit, t = [int(n), int(hit), int(t)]
h = list(map(int, input().strip().split(' ')))
result = getMaxMonsters(n, hit, t, h)
print(result)


            