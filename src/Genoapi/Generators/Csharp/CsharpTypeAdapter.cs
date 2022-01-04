using Microsoft.OpenApi.Models;

namespace Tekcari.Gapi.Generators.Csharp
{
	public class CsharpTypeAdapter : ITypeNameAdapter<CsharpClientGeneratorSettings>, ITypeNameAdapter<CsharpClientTestSuiteGeneratorSettings>
	{
		public string Map(OpenApiSchema typeInfo, CsharpClientGeneratorSettings settings)
		{
			if (!string.IsNullOrEmpty(typeInfo.Reference?.Id))
				return typeInfo.Reference.Id;
			else if (typeInfo.Enum.Count > 0)
				return Map(typeInfo.Type, null);
			else if (string.Equals(typeInfo.Type, "array", System.StringComparison.InvariantCultureIgnoreCase))
				if (string.IsNullOrEmpty(typeInfo.Items.Reference?.Id))
					return string.Format(settings.CollectionTypeFormat, typeInfo.Items.Type);
				else
					return string.Format(settings.CollectionTypeFormat, typeInfo.Items.Reference.Id);
			else
				return Map(typeInfo.Type, typeInfo.Format);
		}

		public string Map(OpenApiSchema typeInfo, CsharpClientTestSuiteGeneratorSettings settings)
		{
			return Map(typeInfo, new CsharpClientGeneratorSettings
			{
				CollectionTypeFormat = settings.CollectionTypeFormat
			});
		}

		public string Map(string typeName, string format)
		{
			switch (format?.ToLowerInvariant() ?? typeName)
			{
				case "int64": return "long";
				case "int32": return "int";
				case "double": return "double";
				case "money": return "decimal";
				case "boolean": return "bool";
				case "binary": return "byte[]";
				case "date-time": return nameof(System.DateTime);

				case "decimal": return "decimal";
				case "integer": return "int";
				default: return "string";
			}
		}
	}
}