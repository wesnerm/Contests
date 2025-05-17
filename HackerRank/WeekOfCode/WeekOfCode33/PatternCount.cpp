#include <cmath>
#include <cstdio>
#include <vector>
#include <iostream>
#include <algorithm>
#include <string>
#include <regex>
using namespace std;


int main() {
    /* Enter your code here. Read input from STDIN. Print output to STDOUT */   
    int q;
    string s;
    cin >> q;
    
    auto e = regex("10+(?=1)");
    while (q -->0)
    {
        cin >> s;
        smatch sm;
        int count = 0;
        while (regex_search(s, sm, e))
        {
            count++;
            s = sm.suffix();
        }
        cout << count << "\n";
    }
    
    return 0;
}
