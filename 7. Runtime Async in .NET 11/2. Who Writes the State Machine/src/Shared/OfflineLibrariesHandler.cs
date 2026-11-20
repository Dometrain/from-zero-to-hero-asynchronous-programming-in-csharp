using System.Net;
using System.Text;

namespace WhoWritesTheStateMachine;

sealed class OfflineLibrariesHandler : HttpMessageHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		const string librariesJson =
			"""[{"Name":"CommunityToolkit.Maui"},{"Name":"CommunityToolkit.Mvvm"},{"Name":"AsyncAwaitBestPractices"}]""";

		await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(librariesJson, Encoding.UTF8, "application/json")
		};
	}
}