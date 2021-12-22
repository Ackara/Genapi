using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekcari.Genapi.Transformation;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientGenerator : IGenerator<CsharpClientGeneratorSettings>
	{
		public CsharpClientGenerator()
		{
			DotLiquid.Template.RegisterFilter(typeof(CsharpFilters));
			DotLiquid.Template.RegisterFilter(typeof(CustomFilters));
		}

		public FileResult[] Generate(OpenApiDocument document, CsharpClientGeneratorSettings settings)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
			_settings = settings;

			BuildTemplateModel();
			GenerateSupportClasses();
			GenerateComponents();
			GenerateClientClass();

			return _fileList.ToArray();
		}

		// ==================== Write Files ==================== //

		private void BuildTemplateModel()
		{
			_globalModel.Add("rootnamespace", _settings.RootNameSpace);
		}

		private void GenerateSupportClasses()
		{
			var template = CreateTemplate(EmbeddedResourceName.Response);
			string content = template.Render(_globalModel);

			_fileList.Add(new FileResult("Response.cs", content));
		}

		private void GenerateComponents()
		{
			DotLiquid.Template template = CreateTemplate(EmbeddedResourceName.components);

			foreach (KeyValuePair<string, OpenApiSchema> schema in _document.Components.Schemas)
				if (string.Equals(schema.Value.Type, "object", StringComparison.InvariantCultureIgnoreCase))
				{
					DotLiquid.Hash model = DotLiquid.Hash.FromAnonymousObject(FromSchemaToModel(schema.Key, schema.Value));
					model.Merge(_globalModel);

					_fileList.Add(new FileResult($"{MemberName(schema.Key)}.cs", template.Render(model)));
				}
		}

		private object FromSchemaToModel(string className, OpenApiSchema schema)
		{
			var properties = new List<object>();
			foreach (KeyValuePair<string, OpenApiSchema> member in schema.Properties)
			{
				properties.Add(FromPropertyToModel(member.Key, member.Value));
			}

			return new { className, properties };
		}

		private object FromPropertyToModel(string propertyName, OpenApiSchema schema)
		{
			return new
			{
				name = propertyName,
				type = _mapper.Map(schema, _settings),
				summary = schema.Description,
				example = schema.Example
			};
		}

		private void GenerateClientClass()
		{
			DotLiquid.Template template = CreateTemplate(EmbeddedResourceName.httpClient);
			var endpoints = new List<object>();
			var model = new DotLiquid.Hash();

			foreach (KeyValuePair<string, OpenApiPathItem> path in _document.Paths)
				foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
				{
					object endpoint = DotLiquid.Hash.FromAnonymousObject(FromOperationToModel(path.Key, operation.Key, operation.Value));
					endpoints.Add(endpoint);
				}

			model.Add("endpoints", endpoints);
			model.Merge(_globalModel);

			string content = template.Render(model);
			_fileList.Add(new FileResult($"{_settings.ClientClassName}.cs", content));
		}

		private object FromOperationToModel(string path, OperationType method, OpenApiOperation operation)
		{
			static bool isArrary(OpenApiSchema x) => string.Equals(x.Type, "array", StringComparison.InvariantCultureIgnoreCase);
			var url = new StringBuilder(path);

			// STEP: Determine the endpoint response type.

			object successResponse = null;
			var parameters = new List<object>();

			foreach (KeyValuePair<string, OpenApiResponse> response in operation.Responses)
				if (int.TryParse(response.Key, out int code))
				{
					if (code >= 200 && code <= 299)
					{
						successResponse = FromResponseToModel(code, response.Value);
					}
				}

			// STEP: Determine the endpoint parameter list.

			if (operation.RequestBody != null)
			{
				OpenApiMediaType mediaType = operation.RequestBody.Content.FirstOrDefault().Value;
				string type = _mapper.Map(mediaType.Schema, _settings);
				parameters.Add(new { type, name = CustomFilters.CamelCase(type), usage = "body" });
			}

			foreach (OpenApiParameter arg in operation.Parameters.OrderBy(x => x.In))
			{
				string type = _mapper.Map(arg.Schema, _settings);
				parameters.Add(new { type, name = arg.Name, usage = Enum.GetName(typeof(ParameterLocation), arg.In) });
			}

			// STEP: Ensure endpoint path has parameters.

			var routeValues = from x in operation.Parameters
							  where x.In == ParameterLocation.Path
							  select CustomFilters.CamelCase(x.Name);
			path = string.Concat(path, string.Join("/", routeValues));

			if (operation.Parameters.Any(x => x.In == ParameterLocation.Query))
				path = string.Concat(path, "?");

			var queryValues = from x in operation.Parameters
							  where x.In == ParameterLocation.Query && !isArrary(x.Schema)
							  select $"{x.Name}={{{CustomFilters.CamelCase(x.Name)}}}";
			//path = string.Concat(path, string.Join("&", queryValues));

			queryValues = from x in operation.Parameters
						  where x.In == ParameterLocation.Query && isArrary(x.Schema)
						  select $"{{GetQueryList(\"{x.Name}\", {CustomFilters.CamelCase(x.Name)})}}";
			//path = string.Concat(path, string.Join("&", queryValues));

			// STEP: Build model for endpoint method.

			return new
			{
				path,
				method = Enum.GetName(typeof(OperationType), method),
				operationName = operation.OperationId,
				summary = operation.Summary,
				remarks = operation.Description,
				success = successResponse,
				parameters
			};
		}

		private object FromResponseToModel(int status, OpenApiResponse response)
		{
			OpenApiMediaType mediaType = response.Content.FirstOrDefault().Value;
			string type = _mapper.Map(mediaType.Schema, _settings);

			return new
			{
				statusCode = Convert.ToString(status),
				type
			};
		}

		#region Backing Members

		private readonly DotLiquid.Hash _globalModel = new DotLiquid.Hash();
		private readonly CsharpTypeAdapter _mapper = new CsharpTypeAdapter();

		private OpenApiDocument _document;
		private CsharpClientGeneratorSettings _settings;
		private ICollection<FileResult> _fileList = new List<FileResult>();

		private DotLiquid.Template CreateTemplate(string fileName) => DotLiquid.Template.Parse(ResourceLoader.GetContent(fileName));

		private string MemberName(string text) => text;

		#endregion Backing Members
	}
}