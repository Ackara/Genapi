namespace Tekcari.Gapi.Generators
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
			if (string.IsNullOrEmpty(input))
				return input;
			else if (input.Length >= 2)
				return string.Concat(char.ToUpperInvariant(input[0]), input.Substring(1));
			else
				return input;
		}

		public static string CamelCase(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;
			else if (input.Length >= 2)
				return string.Concat(char.ToLowerInvariant(input[0]), input.Substring(1));
			else
				return input;
		}
	}
}