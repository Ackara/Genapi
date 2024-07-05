namespace Tekcari.Gapi.Generators.Csharp
{
	public class CsharpEndpointGeneratorSettings : ICodeGeneratorSettings
	{
		public CsharpEndpointGeneratorSettings()
		{
			RootNamespace = string.Concat(nameof(Tekcari), '.', nameof(Gapi));
			ClientClassName = DEFAULT_CLIENT_NAME;
			CollectionTypeFormat = "{0}[]";
		}

		internal const string DEFAULT_CLIENT_NAME = "GeneratedApiClient";

		public string OutputFolder { get; set; }

		public string RootNamespace { get; set; }

		public string ClientClassName { get; set; }

		public string CollectionTypeFormat { get; set; }

		public string BaseUrl { get; set; }

		public string[] References { get; set; }

		public string[] ClassesToExludeFromComponents { get; set; }
	}
}