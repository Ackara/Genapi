using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Tekcari.Genapi.Base
{
	public class Client
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Client"/> class.
		/// </summary>
		/// <param name="baseUrl">The base URL.</param>
		/// <param name="clientFactory">The client factory.</param>
		/// <param name="logger">The logger.</param>
		/// <exception cref="System.ArgumentNullException">clientFactory</exception>
		public Client(string baseUrl, IHttpClientFactory clientFactory, ILogger logger)
		{
			var f = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
		}

		public int Id => 0;

		public object Getall()
		{
			IHttpClientFactory _fac = null;

			HttpClient client = _fac.CreateClient();
			using (HttpResponseMessage res = null)
			{
				switch ((int)res.StatusCode)
				{
					case 300:
						break;
				}

				System.Text.Json.JsonSerializerOptions f = new System.Text.Json.JsonSerializerOptions
				{
				};
			}

			int code = 0;
			switch (code)
			{
				case 0:

					break;

				default:

					break;
			}

			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Converts to string.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return base.ToString();
		}

		public object WriteSendRequest()
		{
			var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Patch, "");

			throw new System.NotImplementedException();
		}

		private static IHttpClientFactory GetHttpClientFactory() => new ServiceCollection().AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>();
	}
}