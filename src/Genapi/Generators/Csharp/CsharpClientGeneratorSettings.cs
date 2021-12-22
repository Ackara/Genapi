namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientGeneratorSettings : IGeneratorSettings
	{
		public CsharpClientGeneratorSettings()
		{
			RootNamespace = string.Concat(nameof(Tekcari), '.', nameof(Genapi));
			ClientClassName = "GeneratedClient";
			CollectionTypeFormat = "{0}[]";
		}

		public string OutputFolder { get; set; }

		public string RootNamespace { get; set; }

		public string ClientClassName { get; set; }

		public string CollectionTypeFormat { get; set; }

		public string BaseUrl { get; set; }
	}
}