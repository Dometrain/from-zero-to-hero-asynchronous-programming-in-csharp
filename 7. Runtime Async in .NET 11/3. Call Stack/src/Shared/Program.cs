using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CallStack;
using UsingAsync;

CodegenBanner.Print();

if (args.Contains("--codegen"))
{
	var cookMethod = typeof(Food).GetMethod(nameof(Food.Cook))
		?? throw new InvalidOperationException($"{nameof(Food)} has no {nameof(Food.Cook)} method");
	var stateMachine = cookMethod.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType.Name ?? "none";

	Console.WriteLine("Food.Cook (chapter 02)");
	Console.WriteLine($"  MethodImplAttributes: {cookMethod.MethodImplementationFlags}");
	Console.WriteLine($"  [AsyncStateMachine]:  {stateMachine}");

	// Holding the Task before awaiting it lets us print what a paused call really returns.
	var cooking = new Gravy().Cook();
	Console.WriteLine($"  Task while cooking:   {cooking.GetType().Name}");
	await cooking;
	return;
}

if (args.Contains("--burn"))
{
	try
	{
		await Kitchen.ServeBurntDinner();
	}
	catch (InvalidOperationException exception)
	{
		// Paths differ per machine; the frames and line numbers are what we compare.
		Console.WriteLine(Regex.Replace(exception.ToString(), " in (?<path>.+):line ", ReplacePathWithFileName));
	}

	return;
}

// The default run is also what --check compares, so --check needs no branch of its own.
StackPeek.ShowAllFrames = args.Contains("--all-frames");

await Kitchen.ServeDinner();
StackPeek.Print("Main resumed");

static string ReplacePathWithFileName(Match pathMatch) =>
	$" in {Path.GetFileName(pathMatch.Groups["path"].Value)}:line ";