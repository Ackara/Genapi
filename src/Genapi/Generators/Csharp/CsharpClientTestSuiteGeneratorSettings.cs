namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientTestSuiteGeneratorSettings : IGeneratorSettings
	{
		public CsharpClientTestSuiteGeneratorSettings()
		{
			RootNamespace = string.Concat(nameof(Tekcari), '.', nameof(Genapi));
			CollectionTypeFormat = "{0}[]";
		}

		public string CollectionTypeFormat { get; set; }

		public object RootNamespace { get; set; }
	}
}