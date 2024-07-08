using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tekcari.Genapi.Serialization;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientTestSuiteGenerator : ICodeGenerator<CsharpClientTestSuiteGeneratorSettings>
	{
		public CsharpClientTestSuiteGenerator()
			: this(new CsharpClientTestSuiteGeneratorSettings(), new CsharpTypeAdapter()) { }

		public CsharpClientTestSuiteGenerator(CsharpClientTestSuiteGeneratorSettings settings)
			: this(settings, new CsharpTypeAdapter()) { }

		public CsharpClientTestSuiteGenerator(CsharpClientTestSuiteGeneratorSettings settings, ITypeNameAdapter<CsharpClientTestSuiteGeneratorSettings> typeNameAdapter)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_mapper = typeNameAdapter ?? throw new ArgumentNullException(nameof(typeNameAdapter));

			DotLiquid.Template.RegisterFilter(typeof(CsharpFilters));
			DotLiquid.Template.RegisterFilter(typeof(CustomFilters));
		}

		public string Id { get => "http-client-test"; }

		public FileResult[] Generate(OpenApiDocument document) => Generate(document, _settings);

		public FileResult[] Generate(OpenApiDocument document, CsharpClientTestSuiteGeneratorSettings settings)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
			_settings = settings;

			BuildGlobalModel();
			GenerateTestMethods();

			return _fileList.ToArray();
		}

		// ==================== INTERNAL MEMBERS ==================== //

		private void BuildGlobalModel()
		{
			_globalModel.Add("rootnamespace", _settings.RootNamespace);
			_globalModel.Add("client_class_name", _settings.ClientClassName);
			_globalModel.Add("test_class_name", _settings.TestClassName);
			_globalModel.Add("service_url", _settings.ServiceUrl);
		}

		private void GenerateTestMethods()
		{
			DotLiquid.Template template = DotLiquid.Template.Parse(ResourceLoader.GetContent(EmbeddedResourceName.test));
			var endpoints = new List<object>();
			var model = new DotLiquid.Hash();

			foreach (KeyValuePair<string, OpenApiPathItem> path in _document.Paths)
				foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
				{
					endpoints.Add(DotLiquid.Hash.FromAnonymousObject(GetEndpointModel(operation.Value)));
				}

			model.Add("endpoints", endpoints);
			model.Merge(_globalModel);

			string content = template.Render(model);
			_fileList.Add(new FileResult($"{_settings.TestClassName}.cs", content, "tests"));
		}

		private object GetEndpointModel(OpenApiOperation operation)
		{
			List<object> parameters = GetEndpointParameterList(operation);
			return new
			{
				operationName = operation.OperationId,
				testName = string.Format(_settings.TestNameFormat, operation.OperationId),
				parameters
			};
		}

		private List<object> GetEndpointParameterList(OpenApiOperation operation)
		{
			static string safeName(string x) => CustomFilters.CamelCase(CsharpFilters.SafeName(x));
			var parameters = new List<object>();

			if (operation.RequestBody != null)
			{
				KeyValuePair<string, OpenApiMediaType> first = operation.RequestBody.Content.FirstOrDefault();
				string mimeType = first.Key.ToLowerInvariant();
				OpenApiMediaType mediaType = first.Value;
				string name, kind = "body", type = _mapper.Map(mediaType.Schema, _settings);

				switch (mimeType)
				{
					case "application/xml":
					case "application/json":
					case "application/x-www-form-urlencoded":
						name = safeName(type);
						parameters.Add(new { kind, type, name, value = $"{name}: {name}", mimeType });
						break;

					case "application/octet-stream":
						name = operation.Parameters.Any(x => x.Name == "data") ? "data1" : "data";
						parameters.Add(new { kind, type, name, value = $"{name}: {name}", mimeType });
						break;
				}
			}

			string lastItem = operation.Parameters?.LastOrDefault()?.Name;
			foreach (OpenApiParameter arg in operation.Parameters.OrderByDescending(x => x.Required))
			{
				parameters.Add(GetEndpointParameter(arg));
			}

			return parameters;
		}

		private object GetEndpointParameter(OpenApiParameter parameter)
		{
			string name = parameter.Name;
			string type = _mapper.Map(parameter.Schema, _settings);
			string value = $"{name}: {name}";
			string kind = Enum.GetName(typeof(ParameterLocation), parameter.In);

			return new { name, type, value, kind };
		}

		#region Backing Members

		private readonly DotLiquid.Hash _globalModel = new DotLiquid.Hash();
		private readonly ITypeNameAdapter<CsharpClientTestSuiteGeneratorSettings> _mapper;
		private CsharpClientTestSuiteGeneratorSettings _settings;
		private OpenApiDocument _document;
		private IList<FileResult> _fileList = new List<FileResult>();

		#endregion Backing Members
	}
}