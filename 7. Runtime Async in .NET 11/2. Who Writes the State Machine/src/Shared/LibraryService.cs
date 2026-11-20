using System.Text.Json;

namespace WhoWritesTheStateMachine;

public class LibraryService(HttpClient httpClient)
{
	readonly HttpClient _httpClient = httpClient;

	public async Task<List<LibraryModel>> GetLibraries()
	{
		var response = await _httpClient.GetAsync("https://codeTraveler.io/MauiLibraries");
		response.EnsureSuccessStatusCode();

		var stream = await response.Content.ReadAsStreamAsync();
		var libraries = await JsonSerializer.DeserializeAsync<List<LibraryModel>>(stream);

		return libraries ?? throw new InvalidOperationException("Libraries cannot be null");
	}
}