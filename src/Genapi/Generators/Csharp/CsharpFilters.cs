using System;
using System.Text.RegularExpressions;

namespace Tekcari.Genapi.Generators.Csharp
{
	public static class CsharpFilters
	{
		public static string SafeName(string input)
		{
			return _memberInvalidCharacters.Replace(input, string.Empty);
		}

		public static string PascalCase(object input)
		{
			return Convert.ToString(input)?.ToUpperInvariant();
		}

		private static readonly Regex _memberInvalidCharacters = new Regex(@"[^A-Z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}