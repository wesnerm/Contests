#!/bin/python3

import sys
import re

def patternCount(s):
    return len(re.findall('10+(?=1)', s))
    
q = int(input().strip())
for a0 in range(q):
    s = input().strip()
    result = patternCount(s)
    print(result)
