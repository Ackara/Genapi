using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tekcari.Genapi.Extensions;

namespace Tekcari.Genapi.Transformation.CSharp
{
	public class CSharpApiClientWriter : WriterBase
	{
		public CSharpApiClientWriter()
			: this(new MemoryStream(), default, default) { }

		public CSharpApiClientWriter(TranspilerSettings settings)
			: this(new MemoryStream(), default, settings) { }

		public CSharpApiClientWriter(Stream stream)
			: this(stream, default, default) { }

		public CSharpApiClientWriter(Stream stream, TranspilerSettings settings)
			: this(stream, default, settings) { }

		public CSharpApiClientWriter(Stream stream, OpenApiDocument document, TranspilerSettings settings)
			: base(stream, Encoding.UTF8)
		{
			_settings = settings;
			_document = document;
		}

		public void Write(OpenApiDocument document)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));

			WriteUsingStatments();

			string defaultNS = string.Concat(nameof(Tekcari), '.', nameof(Genapi));
			WriteLine($"namespace {_settings.Namespace ?? defaultNS}");
			WriteLine("{");
			PushIndent();

			WriteClientClass(document);
			WriteLine();
			WriteComponents(document.Components);
			WriteLine();
			WriteResponseClass();

			PopIndent();
			WriteLine("}");
		}

		public void Write(string className, OpenApiSchema classDeclaration)
		{
			if (string.Equals(classDeclaration.Type, "array", StringComparison.InvariantCultureIgnoreCase)) return;

			WriteLine($"public partial class {GetClassName(className)}");
			WriteLine("{");
			PushIndent();

			foreach (KeyValuePair<string, OpenApiSchema> property in classDeclaration.Properties)
			{
				string name = property.Key;
				OpenApiSchema info = property.Value;

				WriteLine($"[JsonPropertyName(\"{name}\")]");
				WriteLine($"public {GetType(info)} {GetMemberName(name)} {{ get; set; }}");
				WriteLine();
			}

			PopIndent();
			WriteLine("}");
		}

		public void Write(string path, OperationType method, OpenApiOperation operation)
		{
			WriteXmlDoc(operation);
			WriteLine($"public async Task<{GetApiResponseType(operation)}> {GetMemberName(operation.OperationId)}({GetParameterList(operation)})");
			WriteLine("{");
			PushIndent();

			WriteLine($"var request = new HttpRequestMessage(HttpMethod.{method}, GetEndpoint($\"{GetPath(path, operation)}\"));");
			WriteLine("HttpClient client = _factory.CreateClient();");
			WriteLine("using (HttpResponseMessage response = await client.SendAsync(request))");
			WriteLine("{");
			PushIndent();

			WriteLine("switch ((int)response.StatusCode)");
			WriteLine("{");
			PushIndent();

			foreach (KeyValuePair<string, OpenApiResponse> response in operation.Responses)
			{
				if (int.TryParse(response.Key, out int code))
					WriteLine($"case {code}:");
				else
					WriteLine("default:");
				PushIndent();

				KeyValuePair<string, OpenApiMediaType> contentType = response.Value.Content.FirstOrDefault();
				WriteResponse(contentType.Key, contentType.Value);

				PopIndent();
			}

			PopIndent();
			WriteLine("}");

			PopIndent();
			WriteLine("}");

			PopIndent();
			WriteLine("}");
		}

		public void WriteClientClass(OpenApiDocument document)
		{
			string clientName = GetClassName(_settings.ClientClassName ?? document.Info?.Title);
			WriteXmlSummary(document.Info.Description);
			WriteLine($"public partial class {clientName}");
			WriteLine("{");
			PushIndent();

			// Constructor

			WriteXmlSummary($"Initializes a new instance of the <see cref=\"{clientName}\"/> class.");
			WriteXmlParam("baseUrl", "The API server URL.");
			WriteXmlParam("httpClientFactory", "The client factory.");
			WriteXmlParam("logger", "The logger.");
			WriteXmlException("baseUrl");
			WriteXmlException("httpClientFactory");
			WriteLine($"public {clientName}(string baseUrl, IHttpClientFactory httpClientFactory, ILogger<{clientName}> logger)");
			WriteLine("{");
			PushIndent();

			WriteLine("_baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(httpClientFactory));");
			WriteLine("_factory = httpClientFactory ?? CreateHttpClientFactory() ?? throw new ArgumentNullException(nameof(httpClientFactory));");
			WriteLine("_logger = logger;");

			PopIndent();
			WriteLine("}");
			WriteLine();

			// Endpoints

			foreach (KeyValuePair<string, OpenApiPathItem> path in document.Paths)
				foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
				{
					Write(path.Key, operation.Key, operation.Value);
				}

			WritePrivateMembers();

			PopIndent();
			WriteLine("}");
		}

		public void WriteComponents(OpenApiComponents components)
		{
			foreach (KeyValuePair<string, OpenApiSchema> schema in components.Schemas)
			{
				Write(schema.Key, schema.Value);
				WriteLine();
			}
		}

		internal void WriteResponseClass()
		{
			WriteXmlSummary("Represents an API server response.");
			WriteLine("[System.Diagnostics.DebuggerDisplay(\"{GetDebuggerDisplay(),nq}\")]");
			WriteLine($"public readonly struct {RESPONSE_CLASS}");
			WriteLine("{");
			PushIndent();

			// Constructor

			WriteXmlSummary($"Initializes a new instance of the <see cref=\"{RESPONSE_CLASS}\"/> struct.");
			WriteLine("/// <param name=\"code\">The HTTP status code.</param>");
			WriteLine("/// <param name=\"message\">The error message.</param>");
			WriteLine($"public {RESPONSE_CLASS}(int code, string message = default)");
			WriteLine("{");
			PushIndent();

			WriteLine("StatusCode = code;");
			WriteLine("Message = message;");

			PopIndent();
			WriteLine("}");
			WriteLine();

			// Properties

			WriteXmlSummary("The HTTP status code.");
			WriteLine("public int StatusCode { get; }");
			WriteLine();

			WriteXmlSummary("The response message.");
			WriteLine("public string Message { get; }");
			WriteLine();

			WriteXmlSummary("Determines whether the HTTP response was successful.");
			WriteLine("public bool Succeeded =>  StatusCode >= 200 && StatusCode <= 299;");
			WriteLine();

			WriteXmlSummary("Determines whether the HTTP response was NOT successful.");
			WriteLine("public bool Failed => Succeeded == false;");
			WriteLine();

			// Methods

			WriteXmlSummary("Convert this object to its string representation.");
			WriteLine("public override string ToString() => Message;");
			WriteLine();

			WriteLine("private string GetDebuggerDisplay() => $\"[{StatusCode}] {Message}\".Trim();");
			WriteLine();

			WriteLine($"public static implicit operator bool({RESPONSE_CLASS} obj) => obj.Succeeded;");
			WriteLine();

			PopIndent();
			WriteLine("}");
			WriteLine();

			/// ==================================================

			WriteXmlSummary("Represents an API server response.");
			WriteLine("[System.Diagnostics.DebuggerDisplay(\"{GetDebuggerDisplay(),nq}\")]");
			WriteLine($"public readonly struct {RESPONSE_CLASS}<T>");
			WriteLine("{");
			PushIndent();

			// Constructor

			WriteXmlSummary($"Initializes a new instance of the <see cref=\"{RESPONSE_CLASS}\"/> struct.");
			WriteLine("/// <param name=\"code\">The HTTP status code.</param>");
			WriteLine("/// <param name=\"message\">The error message.</param>");
			WriteLine($"public {RESPONSE_CLASS}(int code, string message = default) : this(default, code, message) {{}}");
			WriteLine();

			WriteXmlSummary($"Initializes a new instance of the <see cref=\"{RESPONSE_CLASS}\"/> struct.");
			WriteLine("/// <param name=\"data\">The HTTP response object.</param>");
			WriteLine("/// <param name=\"code\">The HTTP status code.</param>");
			WriteLine("/// <param name=\"message\">The error message.</param>");
			WriteLine($"public {RESPONSE_CLASS}(T data, int code, string message = default)");
			WriteLine("{");
			PushIndent();

			WriteLine("StatusCode = code;");
			WriteLine("Message = message;");
			WriteLine("Data = data;");

			PopIndent();
			WriteLine("}");
			WriteLine();

			// Properties

			WriteXmlSummary("The HTTP response object.");
			WriteLine("public T Data { get; }");
			WriteLine();

			WriteXmlSummary("The HTTP status code.");
			WriteLine("public int StatusCode { get; }");
			WriteLine();

			WriteXmlSummary("The response message.");
			WriteLine("public string Message { get; }");
			WriteLine();

			WriteXmlSummary("Determines whether the HTTP response was successful.");
			WriteLine("public bool Succeeded =>  StatusCode >= 200 && StatusCode <= 299;");
			WriteLine();

			WriteXmlSummary("Determines whether the HTTP response was NOT successful.");
			WriteLine("public bool Failed => Succeeded == false;");
			WriteLine();

			// Methods

			WriteXmlSummary("Convert this object to its string representation.");
			WriteLine("public override string ToString() => Message;");
			WriteLine();

			WriteLine("private string GetDebuggerDisplay() => $\"[{StatusCode}] {Message}\".Trim();");
			WriteLine();

			WriteLine($"public static implicit operator bool({RESPONSE_CLASS}<T> obj) => obj.Succeeded;");
			WriteLine();

			WriteLine($"public static implicit operator T({RESPONSE_CLASS}<T> obj) => obj.Data;");
			WriteLine();

			WriteLine($"public static implicit operator {RESPONSE_CLASS}({RESPONSE_CLASS}<T> obj) => new {RESPONSE_CLASS}(obj.StatusCode, obj.Message);");
			WriteLine();

			PopIndent();
			WriteLine("}");
		}

		protected void WriteXmlSummary(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				WriteLine($"/// <summary>");
				WriteLine($"/// {text.TrimEnd('.').Trim()}.");
				WriteLine($"/// </summary>");
			}
		}

		protected void WriteXmlParam(string paramName, string text)
		{
			if (!string.IsNullOrEmpty(paramName))
			{
				WriteLine($"/// <param name=\"{paramName}\">{text.Trim().TrimEnd('.')}.</param>");
			}
		}

		protected void WriteXmlException(string paramName, string exception = "System.ArgumentNullException")
		{
			WriteLine($"/// <exception cref=\"{exception}\">{paramName}</exception>");
		}

		protected void WriteXmlDoc(OpenApiOperation operation)
		{
			WriteXmlSummary(operation.Summary);
			foreach (OpenApiParameter arg in operation.Parameters)
			{
				string csName = GetParameterName(arg.Name);
				WriteLine($"// <param name=\"{csName}\">{arg.Description.TrimEnd('.')}.</param>");
			}
		}

		private void WriteUsingStatments()
		{
			WriteLine("using Microsoft.Extensions.DependencyInjection;");
			WriteLine("using Microsoft.Extensions.Logging;");
			WriteLine("using System;");
			WriteLine("using System.Net.Http;");
			WriteLine("using System.Text.Json;");
			WriteLine("using System.Text.Json.Serialization;");
			WriteLine("using System.Threading.Tasks;");
			WriteLine();
		}

		private void WriteResponse(string content, OpenApiMediaType media)
		{
			switch (content)
			{
				case "application/json":
					WriteJsonResponse(media);
					break;

				default:
					WriteLine($"return new {RESPONSE_CLASS}((int)response.StatusCode, response.ReasonPhrase);");
					break;
			}
		}

		private void WriteJsonResponse(OpenApiMediaType info)
		{
			string responseType = GetResponseType(info.Schema);
			WriteLine($"return new {RESPONSE_CLASS}<{responseType}>(JsonSerializer.Deserialize<{responseType}>(await response.Content.ReadAsStringAsync(), _serializerOptions), (int)response.StatusCode);");
		}

		private void WritePrivateMembers()
		{
			WriteLine("#region Backing Members");
			WriteLine();

			WriteLine($"private readonly IHttpClientFactory _factory;");
			WriteLine($"private readonly string _baseUrl;");
			WriteLine($"private readonly ILogger _logger;");
			WriteLine($"private readonly JsonSerializerOptions _serializerOptions = new System.Text.Json.JsonSerializerOptions();");
			WriteLine();

			WriteLine("private string GetEndpoint(string path) => string.Concat(_baseUrl, path);");
			WriteLine();

			WriteLine("private static IHttpClientFactory GetHttpClientFactory() => new ServiceCollection().AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>();");
			WriteLine();

			WriteLine("#endregion Backing Members");
		}

		#region Backing Members

		private readonly TranspilerSettings _settings;
		private OpenApiDocument _document;
		private const string RESPONSE_CLASS = "ApiResponse";

		private string GetApiResponseType(OpenApiOperation operation)
		{
			foreach (KeyValuePair<string, OpenApiResponse> response in operation.Responses)
				if (int.TryParse(response.Key, out int code) && code >= 200 && code <= 299)
				{
					switch (response.Value.Content?.FirstOrDefault().Key?.ToLowerInvariant())
					{
						case "application/json":
							OpenApiMediaType media = response.Value.Content?.FirstOrDefault().Value;
							return string.Concat(RESPONSE_CLASS, '<', GetResponseType(media.Schema), '>');

						default: return RESPONSE_CLASS;
					}
				}

			return null;
		}

		private string GetResponseType(OpenApiSchema schema)
		{
			if (string.Equals(schema.Type, "array", StringComparison.InvariantCultureIgnoreCase))
			{
				return string.Concat(nameof(System.Collections.IEnumerable), '<', GetClassName(schema.Items.Reference.Id), '>');
			}
			else
			{
				return GetClassName(schema.Reference.Id);
			}
		}

		private string GetParameterList(OpenApiOperation operation)
		{
			var args = from x in operation.Parameters
					   select $"{x.Schema.Type.ToCSharpType()} {GetParameterName(x.Name)}";

			return string.Join(", ", args);
		}

		private string GetPath(string path, OpenApiOperation operation)
		{
			var builder = new StringBuilder();
			foreach (OpenApiParameter arg in operation.Parameters.Where(x => x.In == ParameterLocation.Path).OrderBy(x => x.Required))
			{
				path = Regex.Replace(path, $"\\{{{arg.Name}\\}}", string.Concat('{', GetParameterName(arg.Name), '}'));
			}
			builder.Append(path);

			var queryArgs = from x in operation.Parameters
							where x.In == ParameterLocation.Query
							orderby x.Required
							select x;

			if (queryArgs.Any())
			{
				builder.Append('?');
				foreach (OpenApiParameter arg in operation.Parameters.Where(x => x.In == ParameterLocation.Query).OrderBy(x => x.Required))
				{
					builder.Append(arg.Name).Append('=').AppendFormat("{{{0}}}", GetParameterName(arg.Name));
					builder.Append("&");
				}
			}

			return builder.ToString().TrimEnd('&', ' ');
		}

		private string GetType(OpenApiSchema property)
		{
			if (property.Type == "object")
			{
				return GetClassName(property.Reference.Id) ?? "object";
			}

			return property.Type;
		}

		private string GetClassName(string name)
		{
			return name;
		}

		private string GetMemberName(string name)
		{
			return name;
		}

		private string GetParameterName(string name)
		{
			return name;
		}

		#endregion Backing Members
	}
}