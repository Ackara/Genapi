namespace Tekcari.Genapi.Generators
{
	internal static class CustomFilters
	{
		public static string EndWithPeriod(string input)
		{
			if (!string.IsNullOrEmpty(input) && input[input.Length - 1] != '.') return string.Concat(input, '.');
			else return input;
		}

		public static string PascalCase(string input)
		{
			return input;
		}

		public static string CamelCase(string input)
		{
			return input?.ToLowerInvariant();
		}
	}
}