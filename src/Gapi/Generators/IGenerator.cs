using Microsoft.OpenApi.Models;

namespace Tekcari.Gapi.Generators
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