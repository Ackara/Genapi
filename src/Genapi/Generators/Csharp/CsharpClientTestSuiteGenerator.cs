using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpClientTestSuiteGenerator : IGenerator<CsharpClientTestSuiteGeneratorSettings>
	{
		public CsharpClientTestSuiteGenerator()
		{
		}

		public FileResult[] Generate(OpenApiDocument document, CsharpClientTestSuiteGeneratorSettings settings)
		{
			throw new NotImplementedException();
		}
	}
}