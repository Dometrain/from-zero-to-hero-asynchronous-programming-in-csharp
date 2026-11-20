using WhatStaysTheSame;

CodegenBanner.Print();

if (args.Contains("--check"))
{
	CodegenBanner.PrintAsyncMethods();
}

Console.WriteLine($"1. AsyncLocal: The initial AsyncLocal.Value is Turkey, and after await the caller still sees {await Checks.AsyncLocalStaysWithTheCaller()}");
Console.WriteLine($"2. UI thread -> await -> {Checks.ResumeOnTheUiThread(continueOnCapturedContext: true)}");
Console.WriteLine($"3. UI thread -> await ConfigureAwait(false) -> {Checks.ResumeOnTheUiThread(continueOnCapturedContext: false)}");
Console.WriteLine($"4. Blocking on the UI thread : {Checks.BlockOnTheUiThread(continueOnCapturedContext: true)}");
Console.WriteLine($"5. Blocking on the UI thread with ConfigureAwait(false) : {Checks.BlockOnTheUiThread(continueOnCapturedContext: false)}");
Console.WriteLine($"6. Throw before the first await : {Checks.ThrowBeforeTheFirstAwait()}");
Console.WriteLine($"7. Non-Awaited Task that faults : {Checks.ForgetATaskThatFaults()}");
Console.WriteLine($"8. AsyncLocal using DomeTrainTask: The initial AsyncLocal.Va lue is Turkey : {await Checks.AwaitDomeTrainTask()}");