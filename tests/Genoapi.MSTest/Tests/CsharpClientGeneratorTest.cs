using ApprovalTests;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tekcari.Gapi.Generators;
using Tekcari.Gapi.Generators.Csharp;
using Tekcari.Gapi.Serialization;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Tekcari.Gapi.Tests
{
	[TestClass]
	public class CsharpClientGeneratorTest
	{
		[TestMethod]
		[DynamicData(nameof(GetClientGeneratorCases), DynamicDataSourceType.Method)]
		public void Can_generate_csharp_api_client_support_classes(string documentPath, string serviceUrl)
		{
			var settings = new CsharpClientGeneratorSettings
			{
				BaseUrl = serviceUrl
			};
			RunGeneratorTest(new CsharpClientGenerator(settings), documentPath, (x) => x.Tag == "native");
		}

		[TestMethod]
		[DynamicData(nameof(GetClientGeneratorCases), DynamicDataSourceType.Method)]
		public void Can_generate_csharp_api_client_components(string documentPath, string serviceUrl)
		{
			var settings = new CsharpClientGeneratorSettings
			{
				BaseUrl = serviceUrl
			};
			RunGeneratorTest(new CsharpClientGenerator(settings), documentPath, (x) => x.Tag == "component");
		}

		[TestMethod]
		[DynamicData(nameof(GetClientGeneratorCases), DynamicDataSourceType.Method)]
		public void Can_generate_csharp_api_client_endpoints(string documentPath, string serviceUrl)
		{
			var settings = new CsharpClientGeneratorSettings
			{
				BaseUrl = serviceUrl
			};
			RunGeneratorTest(new CsharpClientGenerator(settings), documentPath, (x) => x.Tag == "client");
		}

		[TestMethod]
		[DynamicData(nameof(GetClientGeneratorCases), DynamicDataSourceType.Method)]
		public void Can_generate_csharp_api_client_test_suite(string documentPath, string serviceUrl)
		{
			var settings = new CsharpClientTestSuiteGeneratorSettings
			{
				ServiceUrl = serviceUrl
			};
			RunGeneratorTest(new CsharpClientTestSuiteGenerator(settings), documentPath);
		}

		[TestMethod]
		public void Can_invoke_generate_csharp_api_client_task()
		{
			// Arrange

			var specificationFile = Mock.Create<ITaskItem>();
			specificationFile.Arrange(x => x.GetMetadata(Arg.Is("FullPath")))
				.Returns(TestData.GetFilePath("petstore.json"));

			string resultFilePath = Path.Combine(Path.GetTempPath(), "GeneratedClient.txt");
			var outFile = Mock.Create<ITaskItem>();
			outFile.Arrange(x => x.GetMetadata(Arg.Is("FullPath")))
				.Returns(resultFilePath);

			var task = new Targets.GenerateCsharpApiClient
			{
				SourceFile = specificationFile,
				DestinationFile = outFile
			};

			// Act

			var sucess = task.Execute();

			// Assert

			sucess.ShouldBeTrue();
			Approvals.VerifyFile(resultFilePath);
		}

		[TestMethod]
		public void Can_invoke_generate_csharp_api_client_test_suite_task()
		{
			// Arrange

			var specificationFile = Mock.Create<ITaskItem>();
			specificationFile.Arrange(x => x.GetMetadata(Arg.Is("FullPath")))
				.Returns(TestData.GetFilePath("petstore.json"));

			string resultFilePath = Path.Combine(Path.GetTempPath(), "GeneratedClientTest.txt");
			var outFile = Mock.Create<ITaskItem>();
			outFile.Arrange(x => x.GetMetadata(Arg.Is("FullPath")))
				.Returns(resultFilePath);

			var task = new Targets.GenerateCsharpApiClientTestSuite
			{
				ServiceUrl = "https://petstore3.swagger.io/api/v3",
				SourceFile = specificationFile,
				DestinationFile = outFile,
			};

			// Act

			var sucess = task.Execute();

			// Assert

			sucess.ShouldBeTrue();
			Approvals.VerifyFile(resultFilePath);
		}

		[TestMethod]
		public void Can_extract_using_statments()
		{
			// Arrange

			string sample = @"
using System;
using System.Linq;
using System.Data;

namespace foo
{
	publice class ClassA {}
	publice class ClassA {}
}

using System;
using System.Linq;
using System.Data;

namespace bar
{
	publice class ClassC {}
	publice class ClassD {}
}
";

			// Act

			CsharpGenerator.ExtractUsingStatements(sample, out string code, out string[] usings);

			// Assert
			code.ShouldNotMatch(@"(?i)using\s+[A-Z0-9_\.]+;");
			usings.ShouldNotBeEmpty();
			usings.Length.ShouldBe(3);
		}

		internal static void RunGeneratorTest(IGenerator generator, string documentPath, Func<FileResult, bool> filter = default)
		{
			var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(Path.GetFileName(documentPath));
			if (filter == default) filter = (x) => !string.IsNullOrEmpty(x.Name);

			var spec = DocumentLoader.Load(documentPath);
			var fileList = generator.Generate(spec);
			var generatedCode = MergeAndAnalyze(fileList.Where(filter));

			fileList.ShouldNotBeEmpty();
			Approvals.Verify(generatedCode);
		}

		#region Backing Members

		public static IEnumerable<object[]> GetClientGeneratorCases()
		{
			yield return new object[] { TestData.GetFilePath("openapi.json"), "" };
			//yield return new object[] { TestData.GetFilePath("petstore.json"), "https://petstore3.swagger.io/api/v3" };
			//yield return new object[] { TestData.GetFilePath("plaid.yml") };
		}

		internal static string MergeAndAnalyze(IEnumerable<FileResult> fileList)
		{
			var builder = new StringBuilder();
			foreach (var file in fileList)
			{
				builder.Append("== ");
				builder.AppendLine(file.Name);
				builder.AppendLine(string.Concat(Enumerable.Repeat('=', 50)));
				builder.AppendLine();

				AppendErrors(builder, file.Content);
				builder.AppendLine(file.Content);
				builder.AppendLine().AppendLine();
			}
			return builder.ToString();
		}

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

		#endregion Backing Members
	}
}