using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tekcari.Genapi.Transformation
{
	public class DocumentLoader
	{
		public static OpenApiDocument Load(string uri)
		{
			if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException(nameof(uri));
			return Load(new Uri(uri));
		}

		public static OpenApiDocument Load(Uri uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));

			switch (uri.Scheme.ToLowerInvariant())
			{
				case "http":
				case "https":
					return DownloadFile(uri.AbsoluteUri);

				default:
					using (Stream stream = File.OpenRead(uri.LocalPath))
					{
						return ReadFile(stream, null);
					}
			}
		}

		public static OpenApiDocument ReadFile(Stream stream, OpenApiReaderSettings settings)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			var reader = new OpenApiStreamReader(settings);
			OpenApiDocument document = reader.Read(stream, out OpenApiDiagnostic diagnostic);

			foreach (var error in diagnostic.Errors)
			{
				System.Diagnostics.Debug.WriteLine($"{error.Pointer}: {error.Message}");
			}

			return document;
		}

		public static OpenApiDocument DownloadFile(string url)
		{
			return DownloadFileAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;
		}

		public static async Task<OpenApiDocument> DownloadFileAsync(HttpRequestMessage message)
		{
			using (var client = new HttpClient())
			using (HttpResponseMessage response = await client.SendAsync(message))
			{
				if (response.IsSuccessStatusCode)
				{
					return ReadFile(await response.Content.ReadAsStreamAsync(), null);
				}
			}

			return default;
		}
	}
}