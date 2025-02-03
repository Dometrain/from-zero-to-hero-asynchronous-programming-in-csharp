using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace ExecutionContextExample;

public static class Program
{
	static readonly AsyncLocal<string> asyncLocalData = new();

	public static async Task Main()
	{
		Console.WriteLine("Main thread starts");

		// Assign data controlled by ExecutionContext
		CultureInfo.CurrentCulture = new CultureInfo("es-ES");
		Thread.CurrentPrincipal = new ClaimsPrincipal();
		asyncLocalData.Value = "Initial Value";

		PrintThreadValues();

		var mainThreadExecutionContext = ExecutionContext.Capture() ?? throw new InvalidOperationException("ExecutionContext only null when suppressed");

		var thread = new Thread(() =>
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-UK");
			Thread.CurrentPrincipal = new DomeTrainPrinciple();
			asyncLocalData.Value = "AsyncLocalData in Thread";

			Console.WriteLine("Background Thread after assigning values");
			PrintThreadValues();

			ExecutionContext.Run(mainThreadExecutionContext, _ =>
			{
				Console.WriteLine("Same Background Thread, but using MainThread's ExecutionContext");
				PrintThreadValues();
			}, null);
		});

		// Execute Thread
		thread.Start();

		// Wait for Thread to Complete
		thread.Join();

		// Print Main Thread values again
		Console.WriteLine("Main Thread Values");
		PrintThreadValues();

		await Task.Run(() =>
		{
			Console.WriteLine("Print Values from Task.Run()");
			PrintThreadValues();
		});

		// Prevent async/await from automatically flowing the ExecutionContext
		ExecutionContext.SuppressFlow();

		await Task.Run(() =>
		{
			Console.WriteLine("Print Values from Task.Run() With Execution Context Suppressed");
			PrintThreadValues();
		});
	}

	static void PrintThreadValues()
	{
		Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
		Console.WriteLine($"Culture: {CultureInfo.CurrentCulture.DisplayName}");
		Console.WriteLine($"Principal: {Thread.CurrentPrincipal?.GetType()}");
		Console.WriteLine($"AsyncLocalData: {asyncLocalData.Value}");

		Console.WriteLine();
	}
}

sealed class DomeTrainPrinciple() : GenericPrincipal(new DomeTrainIdentity(), null)
{
	sealed class DomeTrainIdentity : IIdentity
	{
		public string? AuthenticationType { get; } = "Fake";
		public bool IsAuthenticated { get; } = true;
		public string? Name { get; } = nameof(DomeTrainIdentity);
	}
}