using UsingAsync;

Console.WriteLine("Cooking Started");

var turkey = new Turkey();
await turkey.Cook();

var gravy = new Gravy();
await gravy.Cook();

Console.WriteLine("Ready to eat");