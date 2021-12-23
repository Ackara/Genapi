using Microsoft.OpenApi.Models;

namespace Tekcari.Genapi.Generators
{
	public interface IGenerator
	{
		FileResult[] Generate(OpenApiDocument document);
	}

	public interface IGenerator<TSettings> : IGenerator
	{
		FileResult[] Generate(OpenApiDocument document, TSettings settings);
	}
}