using Microsoft.Build.Framework;
using Microsoft.OpenApi.Models;
using System.IO;

namespace Tekcari.Genapi.Targets
{
	public class GenerateCsharpApiClient : ITask
	{
		[Required]
		public ITaskItem SourceFile { get; set; }

		[Required]
		public ITaskItem DestinationFile { get; set; }

		public string Namespace { get; set; }

		public bool Execute()
		{
			// STEP: De-serialize Open API specification.

			string source = SourceFile.GetMetadata("FullPath");
			string destination = DestinationFile.GetMetadata("FullPath");

			var settings = new TranspilerSettings
			{
				Namespace = Namespace,
				OutputFile = destination,
				ClientClassName = Path.GetFileNameWithoutExtension(destination)
			};

			WriteMessage($"Loading '{source}'");
			OpenApiDocument document = Transformation.DocumentLoader.Read(source);

			// STEP: Generate source code.

			using (var outStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				var writer = new Transformation.CSharp.CSharpApiClientWriter(outStream, settings);
				writer.Write(document);
				writer.Flush();
			}
			WriteMessage($"Generated '{destination}'.");

			return true;
		}

		#region Backing Members

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		private void WriteMessage(string message, MessageImportance severity = MessageImportance.Normal)
		{
			BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
				message,
				null,
				nameof(GenerateCsharpApiClient),
				severity
				));
		}

		#endregion Backing Members
	}
}