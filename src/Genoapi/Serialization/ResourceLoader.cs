using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tekcari.Gapi.Serialization
{
	internal static class ResourceLoader
	{
		public static Stream GetStream(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
			fileName = string.Concat(nameof(Tekcari), '.', nameof(Gapi), '.', "Resources", '.', fileName);

			Assembly assembly = typeof(ResourceLoader).Assembly;
			foreach (string name in assembly.GetManifestResourceNames())
				if (string.Equals(fileName, name, StringComparison.InvariantCultureIgnoreCase))
				{
					return assembly.GetManifestResourceStream(name);
				}

			throw new FileNotFoundException($"Could not find embedded file with following name: '{fileName}'", fileName);
		}

		public static string GetContent(string fileName, Encoding encoding = default)
		{
			if (encoding == default) encoding = Encoding.UTF8;

			using (var reader = new StreamReader(GetStream(fileName), encoding, true))
			{
				return reader.ReadToEnd();
			}
		}
	}
}