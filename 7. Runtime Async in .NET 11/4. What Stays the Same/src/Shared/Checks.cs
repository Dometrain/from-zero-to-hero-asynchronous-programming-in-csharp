using System.Runtime.CompilerServices;
using CreatingTaskFromScratch;

namespace WhatStaysTheSame;

static class Checks
{
	const string ForgottenTaskMessage = "Nobody awaited this Task";

	static readonly AsyncLocal<string?> _dish = new();

	public static async Task<string?> AsyncLocalStaysWithTheCaller()
	{
		_dish.Value = "Turkey";

		await SwitchToHam();

		return _dish.Value;
	}

	static async Task SwitchToHam()
	{
		_dish.Value = "Ham";

		await Task.Yield();
	}

	public static string ResumeOnTheUiThread(bool continueOnCapturedContext)
	{
		using var ui = new UiThreadContext();
		var resumed = new TaskCompletionSource<string>();

		// A sync callback starts a Task-returning method: an async lambda here would be async void, which stays compiler async in both builds and would test nothing.
		ui.Post(async _ => await ResumeAfterDelay(ui, continueOnCapturedContext, resumed), null);

		// Unlike BlockOnTheUiThread, this wait can't deadlock: ResumeAfterDelay never needs this thread to finish.
		return resumed.Task.Wait(TimeSpan.FromSeconds(2)) ? resumed.Task.Result : "never resumed";
	}

	static async Task ResumeAfterDelay(UiThreadContext ui, bool continueOnCapturedContext, TaskCompletionSource<string> resumed)
	{
		_dish.Value = "Turkey";

		await Task.Delay(50).ConfigureAwait(continueOnCapturedContext);

		resumed.SetResult($"back on the UI thread: {ui.IsCurrentThread}");
	}

	public static string BlockOnTheUiThread(bool continueOnCapturedContext)
	{
		// A fresh context per check because a deadlocked one never recovers.
		using var ui = new UiThreadContext();
		using var unblocked = new ManualResetEventSlim();

		ui.Post(_ =>
		{
			// Chapter 05's anti-pattern on purpose: runtime async doesn't make blocking safe.
			Simmer(continueOnCapturedContext).GetAwaiter().GetResult();
			unblocked.Set();
		}, null);

		return unblocked.Wait(TimeSpan.FromSeconds(1)) ? "completed with no deadlocks" : "DEADLOCK DETECTED (calling thread still blocked)";
	}

	static async Task Simmer(bool continueOnCapturedContext) => await Task.Delay(50).ConfigureAwait(continueOnCapturedContext);

	public static string ThrowBeforeTheFirstAwait()
	{
		var task = Burn(burn: true);

		return $"the call returned a {task.Status} Task";
	}

	static async Task Burn(bool burn)
	{
		if (burn)
		{
			throw new InvalidOperationException("Burnt");
		}

		await Task.Yield();
	}

	public static string ForgetATaskThatFaults()
	{
		using var exceptionReported = new ManualResetEventSlim();

		// Check 6 left a faulted Task behind as well, so only the exception of the Task forgotten here counts.
		void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs)
		{
			if (eventArgs.Exception.InnerException?.Message == ForgottenTaskMessage)
			{
				exceptionReported.Set();
			}
		}

		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		StartAndForget();

		var stopwatch = System.Diagnostics.Stopwatch.StartNew();

		// The runtime only reports an unobserved exception when the forgotten Task is garbage collected.
		// The thread that completed the Task can still hold it when StartAndForget's wait returns, so one collection can miss the Task, or free it after the finalizer pass has already finished.
		// So collect again until the exception is reported, pausing so the finalizer thread and the completing thread can run; the time limit only keeps a run where it never is reported from hanging.
		while (!exceptionReported.IsSet && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			exceptionReported.Wait(TimeSpan.FromMilliseconds(10));
		}

		TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

		return $"UnobservedTaskException fired: {exceptionReported.IsSet}";
	}

	// NoInlining keeps the call out of the check's own frame, where a local or temporary would keep the Task reachable during the collections.
	[MethodImpl(MethodImplOptions.NoInlining)]
	static void StartAndForget()
	{
		var task = FaultAfterYield();

		// A Task that is still running can't be collected, so wait for this one to fault; Wait() would observe the exception, and then it is never reported.
		((IAsyncResult)task).AsyncWaitHandle.WaitOne();
	}

	static async Task FaultAfterYield()
	{
		await Task.Yield();

		throw new InvalidOperationException(ForgottenTaskMessage);
	}

	public static async Task<string> AwaitDomeTrainTask()
	{
		_dish.Value = "Turkey";

		await DomeTrainTask.Delay(TimeSpan.FromMilliseconds(50));

		return $"completed, AsyncLocal: {_dish.Value}";
	}

	public static string ElideAsync()
	{
		var elidedException = WhereTheExceptionWent(() => ElidedWrapper(fail: true));
		var elidedAsyncLocal = WhatHappenedToTheAsyncLocal(() => ElidedWrapper(fail: false));
		var awaitedException = WhereTheExceptionWent(() => AwaitedWrapper(fail: true));
		var awaitedAsyncLocal = WhatHappenedToTheAsyncLocal(() => AwaitedWrapper(fail: false));

		return $"elided {elidedException} and {elidedAsyncLocal} | async/await {awaitedException} and {awaitedAsyncLocal}";
	}

	static string WhereTheExceptionWent(Func<Task<int>> call)
	{
		try
		{
			_ = call();

			return "stored it in the Task";
		}
		catch (InvalidOperationException)
		{
			return "threw at the call";
		}
	}

	static string WhatHappenedToTheAsyncLocal(Func<Task<int>> call)
	{
		_dish.Value = "Caller";

		_ = call();

		return _dish.Value == "Caller" ? "kept AsyncLocal" : "leaked AsyncLocal";
	}

	static Task<int> ElidedWrapper(bool fail)
	{
		_dish.Value = "Wrapper";

		if (fail)
		{
			throw new InvalidOperationException("Burnt");
		}

		return Leaf();
	}

	static async Task<int> AwaitedWrapper(bool fail)
	{
		_dish.Value = "Wrapper";

		if (fail)
		{
			throw new InvalidOperationException("Burnt");
		}

		return await Leaf();
	}

	static async Task<int> Leaf()
	{
		await Task.Yield();

		return 350;
	}
}