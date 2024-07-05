using System.Text;

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
			{
				var builder = new StringBuilder();
				for (int i = 0; i < input.Length; i++)
				{
					char c = input[i];
					if (i == 0) builder.Append(char.ToUpperInvariant(c));
					else if (c == '_' || c == ' ') builder.Append(char.ToUpperInvariant(input[++i]));
					else builder.Append(input[i]);
				}
				return builder.ToString();
			}
			else return input;
		}

		public static string CamelCase(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;
			else if (input.Length >= 2)
			{
				var builder = new StringBuilder();
				for (int i = 0; i < input.Length; i++)
				{
					char c = input[i];
					if (i == 0) builder.Append(char.ToLowerInvariant(c));
					else if (c == '_' || c == ' ') builder.Append(char.ToUpperInvariant(input[++i]));
					else builder.Append(input[i]);
				}
				return builder.ToString();
			}
			else return input;
		}
	}
}