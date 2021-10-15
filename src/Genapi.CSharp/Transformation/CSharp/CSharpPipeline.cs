using Microsoft.OpenApi.Models;
using System;

namespace Tekcari.Genapi.Transformation.CSharp
{
	public class CSharpPipeline : IPipeline
	{
		public CSharpPipeline(OpenApiDocument document)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public void foo()
		{
			
		}

		#region Backing Members

		private readonly OpenApiDocument _document;

		#endregion Backing Members
	}
}