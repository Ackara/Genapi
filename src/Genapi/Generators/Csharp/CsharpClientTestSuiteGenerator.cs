using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientTestSuiteGenerator : IGenerator<CsharpClientTestSuiteGeneratorSettings>
	{
		public CsharpClientTestSuiteGenerator()
			: this(new CsharpClientTestSuiteGeneratorSettings(), new CsharpTypeAdapter()) { }

		public CsharpClientTestSuiteGenerator(CsharpClientTestSuiteGeneratorSettings settings, ITypeNameAdapter<CsharpClientTestSuiteGeneratorSettings> typeNameAdapter)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_mapper = typeNameAdapter ?? throw new ArgumentNullException(nameof(typeNameAdapter));

			DotLiquid.Template.RegisterFilter(typeof(CsharpFilters));
			DotLiquid.Template.RegisterFilter(typeof(CustomFilters));
		}

		public FileResult[] Generate(OpenApiDocument document) => Generate(document, _settings);

		public FileResult[] Generate(OpenApiDocument document, CsharpClientTestSuiteGeneratorSettings settings)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
			_settings = settings;

			BuildGlobalModel();
			GenerateSupportClasses();

			return _fileList.ToArray();
		}

		// ==================== INTERNAL MEMBERS ==================== //

		private void BuildGlobalModel()
		{
			_globalModel.Add("rootnamespace", _settings.RootNamespace);
		}

		private void GenerateSupportClasses()
		{
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