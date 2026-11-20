using System.Runtime.CompilerServices;

namespace OptingIn;

// Nothing calls these methods: the report only reads how the compiler built each one.
static class Shapes
{
	public static async Task Prep() => await Task.Yield();

	public static async ValueTask<int> CountPlates()
	{
		await Task.Yield();
		return 12;
	}

	[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
	public static async ValueTask<int> CountChairs()
	{
		await Task.Yield();
		return 12;
	}

	public static async void RingTheBell() => await Task.Yield();

	public static async IAsyncEnumerable<string> ServeCourses()
	{
		await Task.Yield();
		yield return "Soup";
	}

	[RuntimeAsyncMethodGeneration(false)]
	public static async Task<int> CountForks()
	{
		await Task.Yield();
		return 12;
	}

	public static readonly Func<Task> SetTable = async () => await Task.Yield();

	public static readonly Action ClearTable = async () => await Task.Yield();
}