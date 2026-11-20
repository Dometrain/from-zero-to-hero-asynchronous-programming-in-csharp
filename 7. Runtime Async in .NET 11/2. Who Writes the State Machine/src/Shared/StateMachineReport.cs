using System.Reflection;
using System.Runtime.CompilerServices;

namespace WhoWritesTheStateMachine;

static class StateMachineReport
{
	public static void Print(MethodInfo getLibraries)
	{
		var stateMachineType = getLibraries.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;

		if (stateMachineType is null)
		{
			PrintRuntimeAsync(getLibraries);
		}
		else
		{
			PrintCompilerAsync(getLibraries, stateMachineType);
		}
	}

	static void PrintCompilerAsync(MethodInfo getLibraries, Type stateMachineType)
	{
		var stateMachineKind = stateMachineType.IsValueType ? "struct" : "class";
		var fieldNames = stateMachineType
			.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Select(field => field.Name);

		var getLibrariesBytes = CountIlBytes(getLibraries);
		var moveNextBytes = CountIlBytes(GetStateMachineMethod(stateMachineType, "MoveNext"));
		var setStateMachineBytes = CountIlBytes(GetStateMachineMethod(stateMachineType, "SetStateMachine"));
		var ilBytesPerMethod = $"GetLibraries {getLibrariesBytes} + MoveNext {moveNextBytes} + SetStateMachine {setStateMachineBytes}";
		var totalBytes = getLibrariesBytes + moveNextBytes + setStateMachineBytes;

		Console.WriteLine("GetLibraries, compiled by the C# compiler");
		Console.WriteLine($"  MethodImplAttributes:  {getLibraries.MethodImplementationFlags}");
		Console.WriteLine($"  [AsyncStateMachine]:   {stateMachineType.Name} ({stateMachineKind})");
		Console.WriteLine($"  State machine fields:  {string.Join(", ", fieldNames)}");
		Console.WriteLine($"  IL bytes:              {ilBytesPerMethod} = {totalBytes}");
	}

	static void PrintRuntimeAsync(MethodInfo getLibraries)
	{
		Console.WriteLine("GetLibraries, compiled as runtime async");
		Console.WriteLine($"  MethodImplAttributes:  {getLibraries.MethodImplementationFlags}");
		Console.WriteLine("  [AsyncStateMachine]:   none");
		Console.WriteLine("  State machine fields:  none");
		Console.WriteLine($"  IL bytes:              GetLibraries {CountIlBytes(getLibraries)}");
	}

	static MethodInfo GetStateMachineMethod(Type stateMachineType, string name) =>
		stateMachineType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		?? throw new InvalidOperationException($"{stateMachineType.Name}.{name} not found");

	static int CountIlBytes(MethodBase method) =>
		method.GetMethodBody()?.GetILAsByteArray()?.Length
		?? throw new InvalidOperationException($"{method.Name} has no IL");
}