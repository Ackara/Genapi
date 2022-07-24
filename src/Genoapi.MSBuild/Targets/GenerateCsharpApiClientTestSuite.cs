using Microsoft.Build.Framework;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekcari.Gapi.Generators;

namespace Tekcari.Gapi.Targets
{
	public class GenerateCsharpApiClientTestSuite : ITask
	{
		[Required]
		public ITaskItem SourceFile { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public string RootNamespace { get; set; }

		public string ClientClassName { get; set; }

		public string TestNameFormat { get; set; }

		public string ServiceUrl { get; set; }

		public bool Execute()
		{
			// STEP: Assign arguments.

			string source = SourceFile.GetMetadata("FullPath");
			string destination = DestinationFile.GetMetadata("FullPath");
			string rootFolder = Path.GetDirectoryName(destination);

			var settings = new Generators.Csharp.CsharpClientTestSuiteGeneratorSettings
			{
				TestClassName = Path.GetFileNameWithoutExtension(destination)
			};

			if (!string.IsNullOrEmpty(ServiceUrl)) settings.ServiceUrl = ServiceUrl;
			if (!string.IsNullOrEmpty(RootNamespace)) settings.RootNamespace = RootNamespace;
			if (!string.IsNullOrEmpty(TestNameFormat)) settings.TestNameFormat = TestNameFormat;
			if (!string.IsNullOrEmpty(ClientClassName)) settings.ClientClassName = ClientClassName;
			if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);

			// STEP: Generate source code.

			WriteMessage($"source: '{source}'.");
			OpenApiDocument document = Serialization.DocumentLoader.Load(source);

			var generator = new Generators.Csharp.CsharpClientTestSuiteGenerator();
			IEnumerable<FileResult> files = generator.Generate(document, settings);
			foreach (FileResult file in files) WriteMessage($"found: '{file.Name}'");

			byte[] data = Generators.Csharp.CsharpGenerator.Merge(files.Reverse());
			using (var fileStrem = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				fileStrem.Write(data, 0, data.Length);
				fileStrem.Flush();
			}

			WriteMessage($"Generated '{destination}'.", MessageImportance.High);
			return true;
		}

		#region Backing Members

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		private void WriteMessage(string message, MessageImportance severity = MessageImportance.Normal)
		{
			BuildEngine?.LogMessageEvent(new BuildMessageEventArgs(
				message,
				null,
				nameof(GenerateCsharpApiClient),
				severity
				));
		}

		#endregion Backing Members
	}
}