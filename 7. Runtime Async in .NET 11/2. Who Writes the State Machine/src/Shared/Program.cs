using WhoWritesTheStateMachine;

CodegenBanner.Print();

var getLibraries = typeof(LibraryService).GetMethod(nameof(LibraryService.GetLibraries))
	?? throw new InvalidOperationException("LibraryService.GetLibraries not found");

StateMachineReport.Print(getLibraries);

using var httpClient = new HttpClient(new OfflineLibrariesHandler());
var libraryService = new LibraryService(httpClient);

// Holding the Task before awaiting it lets us print what a paused call really returns.
var pausedTask = libraryService.GetLibraries();
var pausedTaskType = pausedTask.GetType();
Console.WriteLine($"  Manages Execution:     {pausedTaskType.DeclaringType?.Name}+{pausedTaskType.Name}");