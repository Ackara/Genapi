namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientGeneratorSettings : IGeneratorSettings
	{
		public CsharpClientGeneratorSettings()
		{
			RootNameSpace = string.Concat(nameof(Tekcari), '.', nameof(Genapi));
			CollectionTypeFormat = "{0}[]";
		}

		public string RootNameSpace { get; set; }

		public string CollectionTypeFormat { get; set; }

		public string OutputFolder { get; set; }
	}
}