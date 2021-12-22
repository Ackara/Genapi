using System.Text.RegularExpressions;

namespace Tekcari.Genapi.Generators.Csharp
{
	internal static class CsharpFilters
	{
		public static string AsTypeParam(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			else return $"<{input}>";
		}

		public static string SafeName(string input)
		{
			return _memberInvalidCharacters.Replace(input, string.Empty);
		}

		private static readonly Regex _memberInvalidCharacters = new Regex(@"[^A-Z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}