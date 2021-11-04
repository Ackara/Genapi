using Microsoft.OpenApi.Models;
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
			: this(new MemoryStream()) { }

		public CSharpApiClientWriter(Stream stream)
			: base(stream, Encoding.UTF8) { }

		public void Write(OpenApiDocument document)
		{
			foreach (KeyValuePair<string, OpenApiPathItem> item in document.Paths)
			{
				
			}

			throw new System.NotImplementedException();
		}

		public void Write(OpenApiSchema classDeclaration)
		{
			WriteLine($"public partial class {GetName(classDeclaration.Title)}");
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
			WriteLine($"public Task<ApiResponse> {GetMemberName(operation.OperationId)}({GetParameterList(operation)})");
			WriteLine("{");
			PushIndent();

			WriteLine($"var request = new HttpRequestMessage(HttpMethod.{method}, GetEndpoint($\"{GetPath(path, operation)}\"));");
			WriteLine("return SendRequestAsync(request);");

			PopIndent();
			WriteLine("}");
		}

		protected void WriteXmlDoc(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				WriteLine($"/// <summary>");
				WriteLine($"/// {text.TrimEnd('.')}.");
				WriteLine($"/// </summary>");
			}
		}

		protected void WriteXmlDoc(OpenApiOperation operation)
		{
			WriteXmlDoc(operation.Summary);
			foreach (OpenApiParameter arg in operation.Parameters)
			{
				string csName = GetParameterName(arg.Name);
				WriteLine($"// <param name=\"{csName}\">{arg.Description.TrimEnd('.')}.</param>");
			}
		}

		#region Backing Members

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