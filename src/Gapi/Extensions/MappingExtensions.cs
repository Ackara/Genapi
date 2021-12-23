namespace Tekcari.Gapi.Extensions
{
	internal static class MappingExtensions
	{
		public static string ToCSharpType(this string text)
		{
			if (string.IsNullOrEmpty(text)) return default;

			switch (text.ToLowerInvariant())
			{
				case "integer": return "int";
				case "boolean": return "bool";
				default: return "string";
			}
		}
	}
}