// http://www.learncs.org/

// I guess you need one of these in every script?
using System;

// Just make everything public fuck it
public class Hello
{
	// "void" means it doesn't return anything
	// "Main" is a keyword
	// Might make it run every time the script is called
	// Since it doesn't seem like it's called anywhere
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
    }
}

// Declaring variables with a type
int myInt = 1;
float myFloat = 1f; // Floating point needs the f
bool myBoolean = true;
string myName = "John";
char myChar = 'a';
double myDouble = 1.75;

// But you don't always need to specify
// If you don't, the interpreter will guess
// If it guesses wrong then the type will stick
var x = 1;
var y = 2;
var sum = x + y;    // sum will also be defined as an integer

// Enums are enumerated lists
using System;
public enum CarType
{
    Toyota = 1,
    Honda = 2,
    Ford = 3,
}
public class Tutorial
{
    public static void Main()
    {
        CarType myCarType = CarType.Toyota;
        Console.WriteLine(myCarType); // Toyota
        Console.WriteLine((int)myCarType); // 1
    }
}

// Arrays are basically lists, but have a fixed length and typing
int[] nums = { 1, 2, 3, 4, 5 };
int firstNumber = nums[0];
int secondNumber = nums[1];
nums[2] = 10;

// You can also create "empty" arrays
// Though it really seems like it's just an array of 0s
int[] nums = new int[10];

// You can also create matrices
int[,] matrix = new int[2,2];
matrix[0,0] = 1;
matrix[0,1] = 2;
matrix[1,0] = 3;
matrix[1,1] = 4;
int[,] predefinedMatrix = new int[2,2] { { 1, 2 }, { 3, 4 } };

// Lists are basically lists, but have a fixed typing
List<string> fruits = new List<string>();
fruits.Add("apple");
fruits.Add("banana");
fruits.Add("orange");
fruits.Remove("banana");
Console.WriteLine(fruits.Count);
fruits.RemoveAt(1);
Console.WriteLine(fruits.Count);

// To concatenate an array or list to a list
List<string> food = new List<string>();
food.AddRange(fruits);

// Dictionaries are also a thing
Dictionary<string, long> phonebook = new Dictionary<string, long>();
phonebook.Add("Alex", 4154346543);
phonebook["Jessica"] = 4159484588;
if (phonebook.ContainsKey("Alex"))
{
    Console.WriteLine("Alex's number is " + phonebook["Alex"]);
}
phonebook.Remove("Jessica");

// Printing numbers
int integer = 1;
string ourString = "Something to be replaced by an int";
ourString = integer.ToString();
System.Console.WriteLine(ourString);

// You can also use string formatting
int x = 1, y = 2;
int sum = x + y;
string sumCalculation = String.Format("{0} + {1} = {2}", x, y, sum);
Console.WriteLine(sumCalculation);

// String methods
string fullString = "full string";
fullString.Substring(5); // "string"
fullString.Substring(5, 3); // "str"
fullString.Replace("string", "toot!"); // "full toot!"
fullString.IndexOf("full"); // 0
fullString.IndexOf("string"); // 5
fullString.IndexOf("fart"); // -1

// Loop stuff
for(int i = 0; i < 16; i++) // Start; Stop; On each iteration
{
    if(i % 2 == 1)
    {
        continue; // Skip to next iteration
    }
	if(i == 12)
    {
        break; // Exit out of the loop
    }
    Console.WriteLine(i); // Will cast to string for you
}

// While loop is a while loop
int n = 0;
while( n == 0)
{
    Console.WriteLine("N is 0");
    n++;
}

// Methods are basically functions, but each script is within a class
// "public" means anything is able to use it
// "static" means it's a method on the class itself, not the instance of the class
// Multiply would be static because it's just a function you want to plug ints into
// If you had a shape.area method you would want that to be tied to the instance, so you wouldn't make it static
// Since you need to specify a data type, you can use "void" rather than int (or whatever) to return nothing
public static int Multiply(int a, int b)
{
    return a * b;
}

// Classes
using System;
class Shape{
  public string Type;
  public int Sides;
  public int Sidelength;
  public double Area;
  // This is the constructor, telling the class what the above variables mean
  public Shape(string type, int sides, int sidelength, double area){
    Type = type;
    Sides = sides;
    Sidelength = sidelength;
    Area = area;
  }
}
class MainClass{ // Still need a class for the program to run
  public static void Main(){
	// Use "new" to create instance
    Shape square = new Shape("square", 4, 1, 1);
    Console.WriteLine(
		"A {0} with {1} sides of length {2} has an area of {3}", 
		square.Type, square.Sides, square.Sidelength, square.Area);
	// Can use them as named parameters
    Shape square = new Shape(Type: "triangle", Sides: 3, SideLength: 1, Area: .5);
    Console.WriteLine(
		"A {0} with {1} sides of length {2} has an area of {3}", 
		square.Type, square.Sides, square.Sidelength, square.Area);
  }
}

// Optional parameters are a thing
public void ExampleMethod(
	int required, 
	string optionalstr = "default string",
    int optionalint = 10){}
anExample.ExampleMethod(5);
anExample.ExampleMethod(3, optionalint: 4);
