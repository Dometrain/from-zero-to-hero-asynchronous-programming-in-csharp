using System.Reflection;
using System.Runtime.CompilerServices;

namespace OptingIn;

static class CodegenReport
{
	record Row(string Name, string Description, MethodInfo Method);

	public static void Print()
	{
		var requestedFeatures = GetRequestedFeatures();
		var rows = GetRows();

		PrintRequested(requestedFeatures);
		PrintYourCode(rows);

		if (IsRequestedButNotApplied(requestedFeatures, rows))
		{
			Console.WriteLine("Runtime async was requested but not applied: the value must be exactly runtime-async=on");
		}
	}

	static string GetRequestedFeatures()
	{
		var metadata = typeof(CodegenReport).Assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(attribute => attribute.Key == "RequestedFeatures");

		return metadata?.Value?.Trim(';', ' ') ?? "";
	}

	static Row[] GetRows() =>
	[
		Method(nameof(Shapes.Prep), "async Task"),
		Method(nameof(Shapes.CountPlates), "async ValueTask<int>"),
		Method(nameof(Shapes.CountChairs), "pooled ValueTask<int>"),
		Lambda(nameof(Shapes.SetTable), "async lambda as Func<Task>", Shapes.SetTable),
		Lambda(nameof(Shapes.ClearTable), "async lambda as Action", Shapes.ClearTable),
		Method(nameof(Shapes.RingTheBell), "async void"),
		Method(nameof(Shapes.ServeCourses), "async iterator"),
		Method(nameof(Shapes.CountForks), "opted out by attribute")
	];

	static Row Method(string name, string description)
	{
		var method = typeof(Shapes).GetMethod(name)
			?? throw new InvalidOperationException($"Shapes has no method named {name}");

		return new Row(name, description, method);
	}

	static Row Lambda(string name, string description, Delegate lambda) => new(name, description, lambda.Method);

	static void PrintRequested(string requestedFeatures)
	{
		var requested = requestedFeatures.Length == 0 ? "(nothing)" : requestedFeatures;
		Console.WriteLine($"Requested: {requested}");
	}

	static void PrintYourCode(Row[] rows)
	{
		Console.WriteLine("Your code");

		foreach (var row in rows)
		{
			Console.WriteLine($"  {row.Name,-14}{row.Description,-30}{DescribeCodegen(row.Method)}");
		}
	}

	static string DescribeCodegen(MethodInfo method)
	{
		if (IsRuntimeAsync(method))
		{
			// The compiler keeps [AsyncMethodBuilder] on a runtime async method but doesn't use the builder (dotnet/roslyn#84061).
			var isPooled = method.IsDefined(typeof(AsyncMethodBuilderAttribute));
			return isPooled ? "runtime async (pooling builder ignored)" : "runtime async";
		}

		if (method.IsDefined(typeof(AsyncIteratorStateMachineAttribute)))
		{
			return "compiler async (iterator)";
		}

		if (method.IsDefined(typeof(AsyncStateMachineAttribute)))
		{
			return "compiler async";
		}

		throw new InvalidOperationException($"{method.Name} is not an async method");
	}

	static bool IsRuntimeAsync(MethodInfo method) =>
		method.MethodImplementationFlags.HasFlag(MethodImplAttributes.Async);

	// A wrong value (true, On, 1) compiles without a warning, so the report is where you find out.
	static bool IsRequestedButNotApplied(string requestedFeatures, Row[] rows) =>
		requestedFeatures.Contains("runtime-async") && !rows.Any(row => IsRuntimeAsync(row.Method));
}