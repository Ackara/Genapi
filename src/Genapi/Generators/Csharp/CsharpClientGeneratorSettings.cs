namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientGeneratorSettings : IGeneratorSettings
	{
		public CsharpClientGeneratorSettings()
		{
			RootNameSpace = string.Concat(nameof(Tekcari), '.', nameof(Genapi));
			CollectionTypeFormat = "{0}[]";
			ClientClassName = "ApiClient";
		}

		public string RootNameSpace { get; set; }

		public string ClientClassName { get; set; }

		public string BaseUrl { get; set; }

		public string CollectionTypeFormat { get; set; }

		public string OutputFolder { get; set; }
	}
}