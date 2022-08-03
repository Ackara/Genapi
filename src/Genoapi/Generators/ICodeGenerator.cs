using Microsoft.OpenApi.Models;

namespace Tekcari.Gapi.Generators
{
	public interface ICodeGenerator
	{
		public string Id { get; }

		FileResult[] Generate(OpenApiDocument document);
	}

	public interface ICodeGenerator<TSettings> : ICodeGenerator where TSettings : ICodeGeneratorSettings
	{
		FileResult[] Generate(OpenApiDocument document, TSettings settings);
	}
}