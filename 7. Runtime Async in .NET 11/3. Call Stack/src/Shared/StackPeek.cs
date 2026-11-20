using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CallStack;

static class StackPeek
{
	public static bool ShowAllFrames { get; set; }

	// new StackTrace(1, ...) skips Print's own frame by position, so the JIT must not inline Print into its caller.
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void Print(string checkpoint)
	{
		var trace = new StackTrace(1, false);

		// GetFrames() also returns frames that ToString() hides: the runtime's dispatcher, marked [StackTraceHidden],
		// and AggressiveInlining helpers such as AsyncTaskMethodBuilder.Start that optimized code inlines away.
		// We show the first, skip the second, and count exactly what ToString() prints.
		var methods = trace.GetFrames()
			.Select(frame => frame.GetMethod())
			.OfType<MethodBase>()
			.Where(method => !method.MethodImplementationFlags.HasFlag(MethodImplAttributes.AggressiveInlining))
			.ToList();
		var hiddenCount = methods.Count(IsHidden);
		var visibleCount = methods.Count - hiddenCount;

		PrintHeader(checkpoint, visibleCount, hiddenCount);
		WarnIfCountDiffersFromToString(trace, visibleCount);
		PrintFrames(methods);
	}

	static void PrintHeader(string checkpoint, int visibleCount, int hiddenCount)
	{
		var thread = Thread.CurrentThread.IsThreadPoolThread ? "thread-pool thread" : "main thread";
		var hiddenNote = hiddenCount > 0 ? $" (+{hiddenCount} hidden)" : "";
		Console.WriteLine($"[{checkpoint}] {thread}, {visibleCount} frames{hiddenNote}");
	}

	static void WarnIfCountDiffersFromToString(StackTrace trace, int visibleCount)
	{
		var printedByToString = trace.ToString().Split('\n')
			.Count(line => line.StartsWith("   at ", StringComparison.Ordinal));
		if (printedByToString != visibleCount)
		{
			Console.WriteLine($"  (warning: StackTrace.ToString printed {printedByToString} frames)");
		}
	}

	static void PrintFrames(List<MethodBase> methods)
	{
		var frameworkRun = 0;
		foreach (var method in methods)
		{
			if (IsCollapsedIntoRun(method))
			{
				frameworkRun++;
				continue;
			}

			PrintFrameworkRun(frameworkRun);
			frameworkRun = 0;

			var hiddenNote = IsHidden(method) ? " (hidden)" : "";
			Console.WriteLine($"  {Describe(method)}{hiddenNote}");
		}

		PrintFrameworkRun(frameworkRun);
	}

	static void PrintFrameworkRun(int frameCount)
	{
		if (frameCount > 0)
		{
			Console.WriteLine($"  ... {frameCount} framework {(frameCount == 1 ? "frame" : "frames")}");
		}
	}

	static bool IsCollapsedIntoRun(MethodBase method) => !ShowAllFrames && IsFramework(method) && !IsHidden(method);

	static bool IsFramework(MethodBase method) => method.Module != typeof(StackPeek).Module;

	static bool IsHidden(MethodBase method) =>
		IsMarkedStackTraceHidden(method)
		|| (method.DeclaringType is Type declaringType && IsMarkedStackTraceHidden(declaringType));

	static bool IsMarkedStackTraceHidden(MemberInfo member) =>
		member.IsDefined(typeof(StackTraceHiddenAttribute), inherit: false);

	static string Describe(MethodBase method) => $"{method.DeclaringType?.Name}.{method.Name}";
}