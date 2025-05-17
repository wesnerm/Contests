// https://www.hackerrank.com/contests/w33/challenges/pattern-count

process.stdin.resume();
process.stdin.setEncoding('ascii');

var input_stdin = "";
var input_stdin_array = "";
var input_currentline = 0;

process.stdin.on('data', function (data) {
    input_stdin += data;
});

process.stdin.on('end', function () {
    input_stdin_array = input_stdin.split("\n");
    main();    
});

function readLine() {
    return input_stdin_array[input_currentline++];
}

/////////////// ignore above this line ////////////////////

function patternCount(str){
        
        var s = [];
        for (i=0; i<str.length; i++)
            s[i] = str[i];
            
        for (i=1; i+1<s.length; i++)
            if (s[i-1]=='1' && s[i]==s[i+1])
                s[i] = '1';
        
        var count = 0;
        for (i=1; i+1<s.length; i++)
            if (s[i-1]=='1' && s[i]=='0' && s[i+1]=='1' )
                count++;
            
        return count;
}

function main() {
    var q = parseInt(readLine());
    for(var a0 = 0; a0 < q; a0++){
        var s = readLine();
        var result = patternCount(s);
        process.stdout.write("" + result + "\n");
    }

}
