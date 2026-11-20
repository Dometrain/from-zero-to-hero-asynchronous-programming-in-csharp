using System.Reflection;
using System.Runtime.CompilerServices;

namespace WhatStaysTheSame;

static class CodegenBanner
{
	// A misspelled Features value builds without a warning,
	// so every sample proves its own codegen instead of trusting the .csproj.
	public static void Print()
	{
		var asyncMethods = FindAsyncMethods();
		var runtimeAsyncCount = asyncMethods.Count(IsRuntimeAsync);
		var assemblyName = typeof(CodegenBanner).Assembly.GetName().Name;

		Console.WriteLine($"[{assemblyName}] {runtimeAsyncCount} of {asyncMethods.Count} async methods are runtime async");
#if DEBUG
		// Debug builds keep extra locals and Tasks alive, so the lesson's numbers and stacks are only guaranteed in Release.
		Console.WriteLine("Debug build: numbers and stacks can differ from the video. Run with -c Release.");
#endif
	}

	public static void PrintAsyncMethods()
	{
		// Ordinal, so the list is the same on every machine and in every culture.
		foreach (var line in FindAsyncMethods().Select(Describe).Order(StringComparer.Ordinal))
		{
			Console.WriteLine(line);
		}
	}

	static List<MethodInfo> FindAsyncMethods()
	{
		const BindingFlags allDeclaredMethods = BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		return typeof(CodegenBanner).Assembly.GetTypes()
			.SelectMany(type => type.GetMethods(allDeclaredMethods))
			.Where(IsAsync)
			.ToList();
	}

	static string Describe(MethodInfo method)
	{
		var model = IsRuntimeAsync(method) ? "runtime async" : "compiler async";

		return $"{method.DeclaringType?.Name}.{method.Name}: {model}";
	}

	static bool IsAsync(MethodInfo method) =>
		IsRuntimeAsync(method)
		|| method.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false)
		|| method.IsDefined(typeof(AsyncIteratorStateMachineAttribute), inherit: false);

	static bool IsRuntimeAsync(MethodInfo method) =>
		method.MethodImplementationFlags.HasFlag(MethodImplAttributes.Async);
}