using Microsoft.Build.Framework;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekcari.Genapi.Generators;

namespace Tekcari.Genapi.Targets
{
	public class GenerateCsharpApiClient : ITask
	{
		[Required]
		public ITaskItem SourceFile { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public string RootNamespace { get; set; }

		public bool Execute()
		{
			// STEP: De-serialize arguments.

			string source = SourceFile.GetMetadata("FullPath");
			string destination = DestinationFile.GetMetadata("FullPath");

			var settings = new Generators.Csharp.CsharpClientGeneratorSettings
			{
				OutputFolder = Path.GetDirectoryName(destination),
				ClientClassName = Path.GetFileNameWithoutExtension(destination)
			};

			if (!string.IsNullOrEmpty(RootNamespace)) settings.RootNamespace = RootNamespace;
			if (!Directory.Exists(settings.OutputFolder)) Directory.CreateDirectory(settings.OutputFolder);

			WriteMessage($"input: '{source}'.");
			OpenApiDocument document = Transformation.DocumentLoader.Load(source);

			// STEP: Generate source code.

			var generator = new Generators.Csharp.CsharpClientGenerator();
			IEnumerable<FileResult> files = generator.Generate(document, settings);
			foreach (FileResult file in files) WriteMessage($"output: '{file.Name}'");

			byte[] data = Generators.Csharp.CsharpGenerator.Merge(files.ToArray());
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