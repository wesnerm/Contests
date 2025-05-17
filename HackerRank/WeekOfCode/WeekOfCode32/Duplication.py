#!/bin/python3

import sys
import math

def duplication(x):
    return 1-duplication(x - math.pow(2, int(math.log(x,2)))) if x > 0 else 0

q = int(input().strip())
for a0 in range(q):
    x = int(input().strip())
    result = duplication(x)
    print(result)

###############################
# Time Routines

import time

def showDD(n, mod=7):
    # Takes an integer n and reports back last two digits as response time"
    dd = n % 100
    dxx = dd % mod * 100
    ddd = dxx + dd
    if time.clock() * 100 > ddd:
        ddd += mod * 100
    idleAway(ddd)
    
def idleAway(deadline):
    # Takes number from 0..1000 and waits that many hundredths of a second"
    # Python can have anywhere from 0.00 to 0.04 overhead"
    deadline %= 1000
    if deadline == 0: deadline = 1000;
    deadline += .5
    while (time.clock() * 100) < deadline:
        pass

###############################

result = q
#print(result)
showDD(result)  
            