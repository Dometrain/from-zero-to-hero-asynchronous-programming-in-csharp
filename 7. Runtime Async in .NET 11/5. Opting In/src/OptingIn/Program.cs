using OptingIn;

CodegenReport.Print();

Console.WriteLine("The framework");

using var httpClient = new HttpClient(new SlowMenuHandler());

// Holding the Task before awaiting it lets us print what the framework returned while it waited.
var menuTask = httpClient.GetStringAsync("https://codetraveler.io/menu");
Console.WriteLine($"  HttpClient.GetStringAsync while waiting: {menuTask.GetType().Name}");

Console.WriteLine($"Menu: {await menuTask}");