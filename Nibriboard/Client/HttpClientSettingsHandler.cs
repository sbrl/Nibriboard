using System;
using System.IO;

using SBRL.GlidingSquirrel.Http;
using SBRL.GlidingSquirrel.Websocket;

using Newtonsoft.Json;

namespace Nibriboard.Client
{
	public class HttpClientSettingsHandler : WebsocketServer
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

