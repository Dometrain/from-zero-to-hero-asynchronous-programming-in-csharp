using System.Net;

namespace OptingIn;

sealed class SlowMenuHandler : HttpMessageHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// The delay makes the framework's own async method really pause, so we can see what it returns.
		await Task.Delay(100, cancellationToken);

		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("Turkey, Gravy, Mashed Potatoes, Stuffing")
		};
	}
}