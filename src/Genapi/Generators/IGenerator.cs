using Microsoft.OpenApi.Models;

namespace Tekcari.Genapi.Generators
{
	public interface IGenerator<TSettings>
	{
		FileResult[] Generate(OpenApiDocument document, TSettings settings);
	}
}