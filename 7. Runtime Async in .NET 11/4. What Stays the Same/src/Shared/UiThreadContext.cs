using System.Collections.Concurrent;

namespace WhatStaysTheSame;

// Plays the part of chapter 06's UI thread, so the demo runs in a console without MAUI.
sealed class UiThreadContext : SynchronizationContext, IDisposable
{
	readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _work = [];
	readonly Thread _thread;

	public UiThreadContext()
	{
		// A deadlocked UI thread never finishes; a background thread lets the process exit anyway.
		_thread = new Thread(RunPostedCallbacks) { IsBackground = true, Name = "UI thread" };
		_thread.Start();
	}

	public bool IsCurrentThread => Thread.CurrentThread == _thread;

	public override void Post(SendOrPostCallback callback, object? state)
	{
		try
		{
			_work.Add((callback, state));
		}
		catch (InvalidOperationException) when (_work.IsAddingCompleted)
		{
			// A check that gave up waiting disposed this context while a continuation was still on its way; throwing here would crash the process instead of letting the check report the timeout.
		}
	}

	public void Dispose() => _work.CompleteAdding();

	void RunPostedCallbacks()
	{
		SetSynchronizationContext(this);

		foreach (var (callback, state) in _work.GetConsumingEnumerable())
		{
			callback(state);
		}
	}
}