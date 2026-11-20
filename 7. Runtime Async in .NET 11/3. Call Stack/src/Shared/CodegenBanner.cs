using System.Reflection;
using System.Runtime.CompilerServices;

namespace CallStack;

static class CodegenBanner
{
	// A misspelled Features value builds without a warning,
	// so every sample proves its own codegen instead of trusting the .csproj.
	public static void Print()
	{
		const BindingFlags allDeclaredMethods = BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		var assembly = typeof(CodegenBanner).Assembly;
		var asyncMethods = assembly.GetTypes()
			.SelectMany(type => type.GetMethods(allDeclaredMethods))
			.Where(IsAsync)
			.ToList();
		var runtimeAsyncCount = asyncMethods.Count(IsRuntimeAsync);
		var assemblyName = assembly.GetName().Name;

		Console.WriteLine($"[{assemblyName}] {runtimeAsyncCount} of {asyncMethods.Count} async methods are runtime async");
#if DEBUG
		// Debug builds keep extra locals and Tasks alive, so the lesson's numbers and stacks are only guaranteed in Release.
		Console.WriteLine("Debug build: numbers and stacks can differ from the video. Run with -c Release.");
#endif
	}

	static bool IsAsync(MethodInfo method) =>
		IsRuntimeAsync(method)
		|| method.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false)
		|| method.IsDefined(typeof(AsyncIteratorStateMachineAttribute), inherit: false);

	static bool IsRuntimeAsync(MethodInfo method) =>
		method.MethodImplementationFlags.HasFlag(MethodImplAttributes.Async);
}