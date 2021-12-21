using Microsoft.OpenApi.Models;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpTypeAdapter
	{
		public string Map(OpenApiSchema typeInfo, CsharpClientGeneratorSettings settings)
		{
			if (!string.IsNullOrEmpty(typeInfo.Reference?.Id))
				return typeInfo.Reference.Id;
			else if (string.Equals(typeInfo.Type, "array", System.StringComparison.InvariantCultureIgnoreCase))
				if (string.IsNullOrEmpty(typeInfo.Items.Reference?.Id))
					return string.Format(settings.CollectionTypeFormat, typeInfo.Items.Type);
				else
					return string.Format(settings.CollectionTypeFormat, typeInfo.Items.Reference.Id);
			else
				return Map(typeInfo.Type, typeInfo.Format);
		}

		public string Map(string typeName, string format)
		{
			switch (format?.ToLowerInvariant() ?? typeName)
			{
				case "int64": return "long";
				case "int32": return "int";
				case "boolean": return "bool";
				case "date-time": return nameof(System.DateTime);

				case "integer": return "int";
				default: return "string";
			}
		}
	}
}