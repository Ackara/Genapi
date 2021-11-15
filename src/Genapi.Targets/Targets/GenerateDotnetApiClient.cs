using Microsoft.Build.Framework;
using Microsoft.OpenApi.Models;
using System.IO;

namespace Tekcari.Genapi.Targets
{
	public class GenerateDotnetApiClient : ITask
	{
		public GenerateDotnetApiClient()
		{
		}

		[Required]
		public ITaskItem SourceFile { get; set; }

		[Required]
		public string DestinationFile { get; set; }

		public bool Execute()
		{
			// STEP: De-serialize Open API specification.

			string fullPath = SourceFile.GetMetadata("FullPath");
			OpenApiDocument document = Transformation.DocumentLoader.Read(fullPath);

			// STEP: Generate source code.

			//using (var outStream = new FileStream(DestinationFile, FileMode.Create, FileAccess.Write, FileShare.Read))
			//{
			//Transformation.CSharp.CSharpApiClientWriter writer = new Transformation.CSharp.CSharpApiClientWriter(outStream);
			//}

			WriteMessage(DestinationFile);
			WriteMessage(fullPath);

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
				nameof(GenerateDotnetApiClient),
				severity
				));
		}

		#endregion Backing Members
	}
}