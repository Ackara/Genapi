using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tekcari.Genapi.Generators.Csharp
{
	public class CsharpGenerator
	{
		public static void ExtractUsingStatements(string sourceCode, out string codeWithoutUsings, out string[] usings)
		{
			var regex = new Regex(@"(?i)using\s+[A-Z0-9_\.]+;");
			codeWithoutUsings = regex.Replace(sourceCode, string.Empty);

			var statements = new List<string>();
			foreach (Match match in regex.Matches(sourceCode)) statements.Add(match.Value);
			usings = statements.Distinct().ToArray();
		}

		public static byte[] Merge(IEnumerable<FileResult> files)
		{
			var usings = new List<string>();
			var builder = new StringBuilder();

			foreach (FileResult file in files)
			{
				ExtractUsingStatements(file.Content, out string code, out string[] statements);
				usings.AddRange(statements);
				builder.AppendLine(code.Trim());
				builder.AppendLine();
			}

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				foreach (string statment in usings.Distinct().Reverse())
				{
					writer.WriteLine(statment);
				}

				writer.WriteLine();
				writer.Write(builder.ToString());
				writer.Flush();
				return stream.ToArray();
			}
		}
	}
}