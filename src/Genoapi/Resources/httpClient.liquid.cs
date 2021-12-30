using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace {{rootnamespace}}
{
	public partial class {{client_class_name}}
	{
		public {{client_class_name}} (string baseUrl, IHttpClientFactory httpClientFactory = default)
		{
			if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

			_baseUrl = baseUrl.Trim('/');
			_httpClientFactory = httpClientFactory ?? GetHttpClientFactory() ?? throw new ArgumentNullException(nameof(httpClientFactory));
			Initialize();
		}
		
{%- for endpoint in endpoints -%}
{%- assign arguments = endpoint.parameters | map: "value" -%}
	{%- if endpoint.summary -%}
		/// <summary>{{endpoint.summary | end_with_period}}</summary>
	{%- endif -%}
	{%- if endpoint.remarks -%}
		/// <remarks>{{endpoint.remarks | end_with_period}}</remarks>
	{%- endif -%}
	{%- for param in endpoint.parameters -%}
		/// <param name="{{param.name}}">{{param.description | end_with_period}}</param>
	{%- endfor -%}
		public Task<Response{{endpoint.returnType | as_type_param}}> {{endpoint.operationName | safe_name | pascal_case}}Async({{arguments | join: ", "}})
		{
			var request = new HttpRequestMessage(HttpMethod.{{endpoint.method}}, Url($"{{endpoint.path}}"));
		{%- assign headers = endpoint.parameters | where: "kind", "Header" -%}
		{%- for header in headers -%}
			request.Headers.Add("{{header.name}}", {{header.name}});
		{%- endfor -%}
		{%- assign body = endpoint.parameters | where: "kind", "body" | first -%}
		{%- if body -%}
		{%- case body.mimeType -%}
			{%- when "application/json" -%}
			request.Content = ToJson({{body.name}});
		{%- endcase -%}
		{%- endif -%}
			return SendRequestAsync{{endpoint.returnType | as_type_param}}(request);
		}

{%- endfor -%}

		internal async Task<Response> SendRequestAsync(HttpRequestMessage request)
		{
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
						return new Response((int)response.StatusCode, response.ReasonPhrase);
					}
				}
			}
		}

		internal async Task<Response<T>> SendRequestAsync<T>(HttpRequestMessage request)
		{
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
					return new Response<T>(default, (int)response.StatusCode, response.ReasonPhrase);
				}
			}
		}

		internal StringContent ToJson(object obj)
		{
			string json = System.Text.Json.JsonSerializer.Serialize(obj, _serializerOptions);
			return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
		}

		partial void Initialize();

		#region Backing Members

		private readonly string _baseUrl;
		private readonly IHttpClientFactory _httpClientFactory;
		private JsonSerializerOptions _serializerOptions = null;

		private string Url(string path) => string.Concat(_baseUrl, path);
		private static IHttpClientFactory GetHttpClientFactory() => new ServiceCollection().AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>();

		private static string GetQueryList(string name, object[] args) => string.Join("&", from x in args select $"{name}={args}");

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