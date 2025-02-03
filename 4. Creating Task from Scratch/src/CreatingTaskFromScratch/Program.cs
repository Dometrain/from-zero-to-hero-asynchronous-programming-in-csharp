// See https://aka.ms/new-console-template for more information

using CreatingTaskFromScratch;

Console.WriteLine($"Starting Thread Id: {Environment.CurrentManagedThreadId}");

await DomeTrainTask.Run(() => Console.WriteLine($"First DomeTrainTask Id: {Environment.CurrentManagedThreadId}"));

await DomeTrainTask.Delay(TimeSpan.FromSeconds(1));

Console.WriteLine($"Second DomeTrainTask Id: {Environment.CurrentManagedThreadId}");

await DomeTrainTask.Delay(TimeSpan.FromSeconds(1));

await DomeTrainTask.Run(() => Console.WriteLine($"Third DomeTrainTask Id: {Environment.CurrentManagedThreadId}"));