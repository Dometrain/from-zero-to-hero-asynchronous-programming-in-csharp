using UsingAsync;

namespace CallStack;

static class Kitchen
{
	public static async Task ServeDinner()
	{
		var gravy = await MakeGravy();
		StackPeek.Print("ServeDinner resumed");
		Console.WriteLine($"Serving turkey with {gravy}");
	}

	static async Task<string> MakeGravy()
	{
		StackPeek.Print("MakeGravy, before the oven");
		await new Gravy().Cook();
		StackPeek.Print("MakeGravy, after the oven");
		return "gravy";
	}

	public static async Task ServeBurntDinner() => await MakeBurntGravy();

	static async Task<string> MakeBurntGravy()
	{
		await new Gravy().Cook();
		throw new InvalidOperationException("The gravy burned");
	}
}