using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tekcari.Genapi.Transformation;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientGenerator : IGenerator<CsharpClientGeneratorSettings>
	{
		public CsharpClientGenerator()
		{
			
		}

		public FileResult[] Generate(OpenApiDocument document, CsharpClientGeneratorSettings settings)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
			_settings = settings;

			BuildTemplateModel();
			GenerateSupportClasses();
			GenerateComponents();
			//GenerateClientClass();

			return _fileList.ToArray();
		}

		// ==================== Build Model ==================== //

		private void BuildTemplateModel()
		{
			_globalModel.Add("rootnamespace", _settings.RootNameSpace);
		}

		private void BuildComponentModel()
		{
			foreach (KeyValuePair<string, OpenApiSchema> schema in _document.Components.Schemas)
				if (string.Equals(schema.Value.Type, "object", StringComparison.InvariantCultureIgnoreCase))
				{
					FromSchemaToModel(schema.Key, schema.Value);
				}
		}

		// ==================== Write Files ==================== //
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
			throw new NotImplementedException();
		}

		#region Backing Members

		private readonly DotLiquid.Hash _globalModel = new DotLiquid.Hash();
		private readonly CsharpTypeAdapter _mapper = new CsharpTypeAdapter();

		private OpenApiDocument _document;
		private CsharpClientGeneratorSettings _settings;
		private ICollection<FileResult> _fileList = new List<FileResult>();

		private DotLiquid.Template CreateTemplate(string fileName) => DotLiquid.Template.Parse(ResourceLoader.GetContent(fileName));

		private KeyValuePair<string, object> CreateProp(string name, object value) => new KeyValuePair<string, object>(name, value);

		private string MemberName(string text) => text;

		#endregion Backing Members
	}
}