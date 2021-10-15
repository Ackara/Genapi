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
			
		}

        internal static readonly IConfiguration Configuration;

        public static readonly string Directory = Path.Combine(AppContext.BaseDirectory, "test-data");

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
        {
            return Configuration.GetValue<T>(key, default);
        }
    }
}
