using System;
using IotWeb.Common.Http;
using System.IO;
using Newtonsoft.Json;

namespace Nibriboard.Client
{
	public class HttpClientSettingsHandler : IHttpRequestHandler
	{
		private ClientSettings settings;

		public HttpClientSettingsHandler(ClientSettings inSettings)
		{
			settings = inSettings;
		}

		public void HandleRequest(string uri, HttpRequest request, HttpResponse response, HttpContext context) {
			StreamWriter responseData = new StreamWriter(response.Content) { AutoFlush = true };

			string settingsJson = JsonConvert.SerializeObject(settings);
			response.ContentLength = settingsJson.Length;
			response.Headers.Add("content-type", "application/json");

			responseData.Write(settingsJson);

			Log.WriteLine("[Http/ClientSettings] Sent settings");
		}
	}
}

