using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tekcari.eCommerce.Billing;
using Tekcari.eCommerce.Data;

namespace Tekcari.eCommerce
{
	public class Client
	{
		public Client(string baseUrl, IHttpClientFactory httpClientFactory = default)
		{
			if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

			_baseUrl = baseUrl.Trim('/');
			_httpClientFactory = httpClientFactory ?? GetHttpClientFactory() ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}
		
{%- for endpoint in endpoints -%}
	{%- if endpoint.summary -%}
		/// <summary>{{ endpoint.summary | end_with_period }}</summary>
	{%- endif -%}
	{%- if endpoint.remarks -%}
		/// <remarks>{{ endpoint.remarks | end_with_period }}</remarks>
	{%- endif -%}
		public Task<Response{{ endpoint.success.type | as_type_param }}> {{ endpoint.operationName | safe_name }}Async({%- for arg in endpoint.parameters -%}{{ arg.type }} {{ arg.name | safe_name }}{%- endfor -%})
		{
			var request = new HttpRequestMessage(HttpMethod.{{ endpoint.method }}, CreateUrl($"{{ endpoint.path }}"));
			return SendRequestAsync{{ endpoint.success.type | as_type_param }}(request);
		}

{%- endfor -%}

		internal async Task<Response> SendRequestAsync(HttpRequestMessage request)
		{
			SetHeaders(request);
#if DEBUG
			PrintToDebugWindow(request);
#endif
			HttpClient client = _httpClientFactory.CreateClient();
			using (HttpResponseMessage response = await client.SendAsync(request))
			{
#if DEBUG
				PrintToDebugWindow(response);
#endif
				if (response.IsSuccessStatusCode)
				{
					return new Response((int)response.StatusCode, response.ReasonPhrase);
				}
				else
				{
					System.IO.Stream json = await response.Content.ReadAsStreamAsync();
					if (json.Length == 0) return new Response((int)response.StatusCode, response.ReasonPhrase);
					else
					{
						Fault error = await JsonSerializer.DeserializeAsync<Fault>(json, _serializerOptions);
						return new Response(error.Code, error.Message);
					}
				}
			}
		}

		internal async Task<Response<T>> SendRequestAsync<T>(HttpRequestMessage request)
		{
			SetHeaders(request);
#if DEBUG
			PrintToDebugWindow(request);
#endif
			HttpClient client = _httpClientFactory.CreateClient();
			using (HttpResponseMessage response = await client.SendAsync(request))
			{
#if DEBUG
				PrintToDebugWindow(response);
#endif
				string json = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(json)) return new Response<T>(default, (int)response.StatusCode, response.ReasonPhrase);

				if (response.IsSuccessStatusCode)
				{
					return new Response<T>(
						JsonSerializer.Deserialize<T>(json, _serializerOptions),
						(int)response.StatusCode,
						response.ReasonPhrase
						);
				}
				else
				{
					Fault fault = JsonSerializer.Deserialize<Fault>(json, _serializerOptions);
					return new Response<T>(
						default,
						(int)response.StatusCode,
						fault.Message);
				}
			}
		}

		internal void SetHeaders(HttpRequestMessage request)
		{
		}

		internal StringContent ToJson(object obj)
		{
			string json = System.Text.Json.JsonSerializer.Serialize(obj, _serializerOptions);
			return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
		}

		#region Backing Members

		private readonly string _baseUrl;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly JsonSerializerOptions _serializerOptions = Configuration.GlobalSettings.GetSerializerOptions();

		private Response BadRequest(string paramName) => new Response(401, $"'{paramName}' cannot be null or empty.");
		private Response<T> BadRequest<T>(string paramName) => new Response<T>(default, 401, $"'{paramName}' cannot be null or empty.");
		private static IHttpClientFactory GetHttpClientFactory() => new ServiceCollection().AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>();

		private static GetQueryList(string name, object[] args) => string.Join("&", from x in args select $"{name}={args}");

		private static string GetBaseUrl(Environment environment)
		{
			return environment switch
			{
				Environment.Production => "http://api.ackeem.dotnetcloud.co.uk",
				_ => "http://localhost.com/"
			};
		}

		private void PrintToDebugWindow(HttpRequestMessage message)
		{
#if DEBUG
			System.Diagnostics.Debug.Write("===== HTTP Request ");
			System.Diagnostics.Debug.WriteLine(string.Concat(Enumerable.Repeat('=', 50)));
			System.Diagnostics.Debug.WriteLine($"{message.Method.Method}: {message.RequestUri}");
			System.Diagnostics.Debug.WriteLine(message.Content?.ReadAsStringAsync().Result);
			System.Diagnostics.Debug.WriteLine(string.Empty);
#endif
		}

		private void PrintToDebugWindow(HttpResponseMessage message)
		{
#if DEBUG
			System.Diagnostics.Debug.Write($"===== HTTP Response ({(int)message.StatusCode}) ");
			System.Diagnostics.Debug.WriteLine(string.Concat(Enumerable.Repeat('=', 50)));
			System.Diagnostics.Debug.WriteLine($"{message.RequestMessage.Method.Method}: {message.RequestMessage.RequestUri}");
			System.Diagnostics.Debug.WriteLine($"Status: {message.ReasonPhrase} ({(int)message.StatusCode})");
			System.Diagnostics.Debug.WriteLine(message.Content?.ReadAsStringAsync().Result);
			System.Diagnostics.Debug.WriteLine(string.Empty);
#endif
		}


		#endregion Backing Members
	}
}