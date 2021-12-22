using ApprovalTests;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tekcari.Genapi.Generators.Csharp;
using Tekcari.Genapi.Transformation;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

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

			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(Path.GetFileName(specificationFilePath));

			var sut = new CsharpClientGenerator();
			var spec = DocumentLoader.Load(specificationFilePath);

			string defaultResultFolder = Path.Combine(Path.GetTempPath());
			var settings = new CsharpClientGeneratorSettings
			{
				OutputFolder = defaultResultFolder
			};

			// Act

			var fileList = sut.Generate(spec, settings);

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

		[TestMethod]
		public async Task MyTestMethod()
		{
			const string endpoint = "https://petstore3.swagger.io/api/v3";
			const string body = @"
{
  ""id"": 10,
  ""name"": ""doggie"",
  ""category"": { ""id"": 1, ""name"": ""Dogs"" },
  ""photoUrls"": [  ""string"" ],
  ""tags"": [{ ""id"": 0, ""name"": ""string""  }],
  ""status"": ""available""
}
";

			var req = new HttpRequestMessage(HttpMethod.Post, string.Concat(endpoint, "/pet"));
			req.Content = new StringContent(body, Encoding.UTF8, "application/json");

			var client = new HttpClient();
			var res = await client.SendAsync(req);
			string json = await res.Content.ReadAsStringAsync();

			res.IsSuccessStatusCode.ShouldBeTrue(res.ReasonPhrase);
			json.ShouldNotBeNullOrEmpty();
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