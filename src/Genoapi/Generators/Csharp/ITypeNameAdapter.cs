using Microsoft.OpenApi.Models;

namespace Tekcari.Gapi.Generators
{
	public interface ITypeNameAdapter
	{
		string Map(OpenApiSchema typeInfo, ICodeGeneratorSettings settings);
	}

	public interface ITypeNameAdapter<TSettings> where TSettings : ICodeGeneratorSettings
	{
		string Map(OpenApiSchema typeInfo, TSettings settings);
	}
}