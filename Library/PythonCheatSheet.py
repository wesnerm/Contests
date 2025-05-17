#!/bin/python3

import sys

#Python Performance
# use sys.stdin.read() instead of input()
# use readlines() instead of readline()
# use functions instead of main
# use list comprehensions instead of loops
# read all at once
# stdout.write(data + "\n") is faster too


def function(arg):
	if test:
		# do stuff if true
		pass
	elif test2:
		# do stuff if test2 true
		pass
	else:
		# otherwise
		pass

	while test:
		# loop
		pass

	for x in range(0, n):
		pass

	the_string = "Hello world"
	the_string = 'Hello world'

	the_string[4]
	the_string.split(' ')

	words = [ "this", "is", "a", "list", "of", "strings" ]
	' '.join(words)

	print( "Hello {}!".format(this_string ) )
	print( "{} {}".format( 'Hello', 'world ') )
	print( "{0} {1}".format( 'Hello', 'world ') )

	emptyTuple = ()
	single = ("span",)
	thistuple = 12, 89, 'a'
	thistuple = (12, 89, 'a')

	thistuple[0]		# returns 12

	emptDict = {}
	thisdict = { 'a':1, 'b':23 }
	del thisdict['b']
	thisdict.has_key9'a')
	thisdiict.keys()
	thisdict.items()
	'c' in thisdict

	thelist = [5,3,‘p’,9,‘e’]
	thelist[0]			# returns 5
	thelist[1:3]		# returns [3, ‘p’]
	thelist[2:]			# returns [‘p’, 9, ‘e’]
	thelist[:2]			# returns [5, 3]
	thelist[2:-1]		# returns [‘p’, 9]
	len(thelist)		# returns 5
	thelist.sort()		# no return value
	thelist.append(37)
	thelist.pop()		# returns 37
	thelist.pop(1)		# returns 5
	thelist.insert(2, ‘z’)
	thelist.remove(‘e’)
	del thelist[0]
	thelist + [0]		#returns [‘z’,9,’p’,0]
	9 in thelist		#returns True

	# [expression for expr in sequence if condition] 

def myFunc(param1, param2):
	“By putting this initial sentence in triple quotes, you can
	access it by calling myFunc.__doc___”””
 
	#indented code block goes here
	spam = param1 + param2
	return spam

class Eggs(ClassWeAreOptionallyInheriting):
	def __init__(self):
		ClassWeAreOptionallyInheriting.__init__(self)
		#initialization (constructor) code goes here
		self.cookingStyle = ‘scrambled’
 
	def anotherFunction(self, argument):
		if argument == “just contradiction”:
			return False
		else:
			return True

theseEggsInMyProgram = Eggs()

thisfile = open(“datadirectory/file.txt”) 
thisfile.read() # reads entire file into one string
thisfile.readline() # reads one line of a file
thisfile.readlines() # reads entire file into a list of strings, one per line

for eachline in thisfile: 