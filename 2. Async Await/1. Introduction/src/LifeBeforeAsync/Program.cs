using LifeBeforeAsync;

Console.WriteLine("Cooking Started");

var turkey = new Turkey();
turkey.Cook()
	.ContinueWith(_ =>
	{
		var gravy = new Gravy();
		gravy.Cook();
	});

//Code continues running on this line
Console.ReadLine();