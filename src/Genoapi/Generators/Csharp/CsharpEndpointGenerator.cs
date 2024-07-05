using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tekcari.Gapi.Generators.Csharp
{
	public class CsharpEndpointGenerator : ICodeGenerator<CsharpEndpointGeneratorSettings>
	{
		public FileResult[] Generate(OpenApiDocument document, CsharpEndpointGeneratorSettings settings)
		{
			throw new NotImplementedException();
		}

		public string Id { get; }

		public FileResult[] Generate(OpenApiDocument document)
		{
			throw new NotImplementedException();
		}
	}
}