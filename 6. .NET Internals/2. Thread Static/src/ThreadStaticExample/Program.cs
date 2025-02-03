static class Program
{
	// Apply ThreadStatic attribute to make this variable thread-local.
	[ThreadStatic]
	static int threadSpecificValue;

	static void Main(string[] args)
	{
		// Initializing thread-specific value for the main thread
		threadSpecificValue = 100;

		// Output from the main thread
		Console.WriteLine($"Main thread - threadSpecificValue: {threadSpecificValue}");

		// Create two new threads
		Thread thread1 = new Thread(ThreadMethod);
		Thread thread2 = new Thread(ThreadMethod);

		// Start the threads
		thread1.Start();
		thread2.Start();

		// Wait for threads to finish
		thread1.Join();
		thread2.Join();

		// Output from the main thread after the other threads have finished
		Console.WriteLine($"Main thread after threads finished - threadSpecificValue: {threadSpecificValue}");
	}

	// Method to be run by each thread
	static void ThreadMethod()
	{
		// Initialize thread-specific value for this thread
		threadSpecificValue = Random.Shared.Next(1, 100);

		// Output from each thread
		Console.WriteLine(
			$"Thread {Environment.CurrentManagedThreadId} {nameof(threadSpecificValue)}: {threadSpecificValue}");
	}
}