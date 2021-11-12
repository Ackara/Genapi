using Microsoft.Build.Framework;

namespace Tekcari.Genapi.Targets
{
	public class GenerateDotnetApiClient : ITask
	{
		public GenerateDotnetApiClient()
		{
		}

		[Required]
		public ITaskItem SourceFile { get; set; }

		public bool Execute()
		{
			

			return true;
		}

		#region Backing Members

		public ITaskHost HostObject { get; set; }

		public IBuildEngine BuildEngine { get; set; }

		#endregion Backing Members
	}
}