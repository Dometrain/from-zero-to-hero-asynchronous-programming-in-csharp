namespace System.Runtime.CompilerServices;

// The compiler recognizes this attribute by name and the BCL doesn't ship it, so the project declares its own.
// Roslyn calls it an experiment that may be removed, so the report shows whether your SDK still honors it.
[AttributeUsage(AttributeTargets.Method)]
sealed class RuntimeAsyncMethodGenerationAttribute(bool runtimeAsync) : Attribute
{
	public bool RuntimeAsync { get; } = runtimeAsync;
}