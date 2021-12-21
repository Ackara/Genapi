using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tekcari.Genapi.Generators.Csharp;
using Tekcari.Genapi.Transformation;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class CsharpClientGeneratorTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetClientGenerationCases), DynamicDataSourceType.Method)]
		public void Can_generate_csharp_api_client(string specificationFilePath)
		{
			// Arrange

			DotLiquid.Template.RegisterFilter(typeof(CsharpFilters));
			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(Path.GetFileName(specificationFilePath));

			var sut = new CsharpClientGenerator();
			var apiSpec = DocumentLoader.Load(specificationFilePath);

			string defaultResultFolder = Path.Combine(Path.GetTempPath());
			var settings = new CsharpClientGeneratorSettings
			{
				OutputFolder = defaultResultFolder
			};

			// Act

			var fileList = sut.Generate(apiSpec, settings);

			var builder = new StringBuilder();
			foreach (var file in fileList.Reverse())
			{
				builder.Append("== ");
				builder.AppendLine(file.Name);
				builder.AppendLine(string.Concat(Enumerable.Repeat('=', 50)));
				builder.AppendLine();

				AppendErrors(builder, file.Content);
				builder.AppendLine(file.Content);
				builder.AppendLine().AppendLine();
			}

			// Assert

			fileList.ShouldNotBeEmpty();
			Approvals.Verify(builder);
		}

		[TestMethod]
		public void MyTestMethod()
		{
			DotLiquid.Template.RegisterFilter(typeof(CsharpFilters));

			var ooo = new
			{
				name = "cat",
				list = new string[] {"a", "b", "c"},
				models = new object[] { new { id = 0, age = 3 } }
			};

			var hash =  DotLiquid.Hash.FromAnonymousObject(ooo);
			
		}

		#region Backing Members

		private static void AppendErrors(StringBuilder builder, string csharp, params string[] ignore)
		{
			if (string.IsNullOrEmpty(csharp)) return;

			var snippet = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp);
			var errors = from x in snippet.GetDiagnostics()
						 where
							x.Severity >= Microsoft.CodeAnalysis.DiagnosticSeverity.Warning
							&&
							ignore.Contains(x.Id) == false
						 select $"[{x.Id}] Line {x.Location.GetMappedLineSpan().StartLinePosition.Line + 1}: {x.GetMessage()}";

			if (errors.Any())
			{
				builder
					.Append(string.Concat(Enumerable.Repeat('*', 21)))
					.Append(" ERRORS ")
					.AppendLine(string.Concat(Enumerable.Repeat('*', 21)))
					.AppendLine()
					.AppendJoin("\r\n", errors)
					.AppendLine()
					.AppendLine()
					.AppendLine(string.Concat(Enumerable.Repeat('*', 50)))
					.AppendLine()
					.AppendLine();
			}
		}

		public static IEnumerable<object[]> GetClientGenerationCases()
		{
			yield return new object[] { TestData.GetFilePath("petstore.json") };
			//yield return new object[] { TestData.GetFilePath("plaid.yml") };
		}

		#endregion Backing Members
	}
}