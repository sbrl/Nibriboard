using System;
using System.IO;
using IotWeb.Common.Http;
using MimeSharp;

using SBRLUtilities;
using System.Collections.Generic;
using System.Reflection;

namespace Nibriboard
{
	public class HttpEmbeddedFileHandler : IHttpRequestHandler
	{
		private string filePrefix;

		private Mime mimeTypeFinder = new Mime();
		private Dictionary<string, string> mimeTypeOverrides = new Dictionary<string, string>() {
			["application/xhtml+xml"] = "text/html",
			["application/tei+xml"] = "image/x-icon"
		};

		private List<string> embeddedFiles = new List<string>(EmbeddedFiles.ResourceList);

		public HttpEmbeddedFileHandler(string inFilePrefix)
		{
			filePrefix = inFilePrefix;
		}

		public void HandleRequest(string uri, HttpRequest request, HttpResponse response, HttpContext context) {
			StreamWriter responseData = new StreamWriter(response.Content) { AutoFlush = true };
			if (request.Method != HttpMethod.Get) {
				response.ResponseCode = HttpResponseCode.MethodNotAllowed;
				response.ContentType = "text/plain";
				responseData.WriteLine("Error: That method isn't supported yet.");
				logRequest(request, response);
				return;
			}

			response.ContentType = getMimeType(request.URI);
			response.Headers.Add("content-type", response.ContentType);

			string expandedFilePath = getEmbeddedFileReference(request.URI);
			if (!embeddedFiles.Contains(expandedFilePath)) {
				response.ResponseCode = HttpResponseCode.NotFound;
				response.ContentType = "text/plain";
				responseData.WriteLine("Can't find {0}.", expandedFilePath);
				logRequest(request, response);
				return;
			}
			byte[] embeddedFile = EmbeddedFiles.ReadAllBytes(expandedFilePath);
			response.ContentLength = embeddedFile.Length;
			response.Content.Write(embeddedFile, 0, embeddedFile.Length);

			logRequest(request, response);
		}

		protected string getEmbeddedFileReference(string uri) {
			return filePrefix + "." + uri.TrimStart("/".ToCharArray());
		}

		protected string getMimeType(string uri) {
			string mimeType = mimeTypeFinder.Lookup(uri);
			foreach (KeyValuePair<string, string> mimeMapping in mimeTypeOverrides) {
				if (mimeType == mimeMapping.Key)
					mimeType = mimeMapping.Value;
			}
			return mimeType;
		}

		private void logRequest(HttpRequest request, HttpResponse response) {
			Log.WriteLine("[Http/FileHandler] {0} {1} {2} {3}", response.ResponseCode.ResponseCode(), response.ContentType, request.Method.ToString().ToUpper(), request.URI);
		}
	}
}

