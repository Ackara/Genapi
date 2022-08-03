namespace Tekcari.Gapi.Generators.Csharp
{
	public class CsharpClientTestSuiteGeneratorSettings : ICodeGeneratorSettings
	{
		public CsharpClientTestSuiteGeneratorSettings()
		{
			CollectionTypeFormat = "{0}[]";
			RootNamespace = string.Concat(nameof(Tekcari), '.', nameof(Gapi));
			ClientClassName = CsharpClientGeneratorSettings.DEFAULT_CLIENT_NAME;
			TestClassName = string.Concat(ClientClassName, "Test");
			TestNameFormat = "Can_{0}";
		}

		public string CollectionTypeFormat { get; set; }

		public string RootNamespace { get; set; }

		public string TestClassName { get; set; }

		public string ClientClassName { get; set; }

		public string TestNameFormat { get; set; }

		public string ServiceUrl { get; set; }
	}
}