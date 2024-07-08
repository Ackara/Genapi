using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tekcari.Genapi
{
	internal static class TestData
	{
		static TestData()
		{
			Configuration = null;
		}

		public static readonly string Directory = Path.Combine(AppContext.BaseDirectory, "test-data");
		internal static readonly IConfiguration Configuration;

		public static IEnumerable<object[]> GetSpecifications()
		{
			foreach (var file in GetFilePaths("*.yml"))
				yield return new object[] { file };

			foreach (var file in GetFilePaths("*.json"))
				yield return new object[] { file };

			var urls = new string[]
			{
				"https://raw.githubusercontent.com/plaid/plaid-openapi/master/2020-09-14.yml"
			};
			foreach (string u in urls) yield return new object[] { u };
		}

		public static string GetFilePath(string pattern)
		{
			return System.IO.Directory.EnumerateFiles(Directory, pattern, SearchOption.AllDirectories).First();
		}

		public static IEnumerable<string> GetFilePaths(string pattern)
		{
			return System.IO.Directory.EnumerateFiles(Directory, pattern, SearchOption.AllDirectories);
		}

		public static string GetValue(string key)
			=> GetValue<string>(key) ?? throw new ArgumentNullException(key);

		public static T GetValue<T>(string key)
			=> Configuration.GetValue<T>(key, default);
	}
}