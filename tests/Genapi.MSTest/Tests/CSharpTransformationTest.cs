using ApprovalTests;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tekcari.Genapi.Transformation;
using Tekcari.Genapi.Transformation.CSharp;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	///[ApprovalTests.Reporters.UseReporter(typeof(ApprovalTests.Reporters.FileLauncherReporter))]
	public class CSharpTransformationTest
	{
		[DataTestMethod]
		[DataRow("pet*")]
		public void Can_transform_open_api_document_to_csharp_client(string pattern)
		{
			// Arrange

			string filePath = TestData.GetFilePath(pattern);
			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(Path.GetFileName(filePath));

			var document = DocumentLoader.Read(filePath);

			var settings = new TranspilerSettings
			{
				ClientClassName = "GeneratedClient"
			};

			var output = new MemoryStream();
			var sut = new CSharpApiClientWriter(output, settings);

			// Act

			sut.Write(document);
			sut.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = AppendErrors(code);

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			Approvals.Verify(code);
		}

		[TestMethod]
		[DynamicData(nameof(GetComponents), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
		public void Can_transform_components_to_csharp_classes(string className, OpenApiSchema schema)
		{
			// Arrange

			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(className ?? Convert.ToString(_counter++));

			var output = new MemoryStream();
			var writer = new CSharpApiClientWriter(output);

			// Act

			writer.Write(className, schema);
			writer.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = AppendErrors(code);

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			Approvals.Verify(code);
		}

		[TestMethod]
		[DynamicData(nameof(GetOperations), DynamicDataSourceType.Method)]
		public void Can_transform_operation_to_csharp_method(string path, OperationType method, OpenApiOperation endpoint, OpenApiDocument document)
		{
			// Arrange

			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario($"{method}: {path}");

			var output = new MemoryStream();
			var writer = new CSharpApiClientWriter(output, document, default);

			// Act

			writer.Write(path, method, endpoint);
			writer.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = AppendErrors(code, "CS0106");

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			Approvals.Verify(code);
		}

		[TestMethod]
		public void Can_write_csharp_response_class()
		{
			// Arrange

			var output = new MemoryStream();
			var sut = new CSharpApiClientWriter(output);

			// Act

			sut.WriteResponseClass();
			sut.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = AppendErrors(code);

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			Approvals.Verify(code);
		}

		#region Backing Members

		private static int _counter = 0;

		private static IEnumerable<object[]> GetComponents()
		{
			var filter = new Regex(".+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			foreach (string specificationFile in TestData.GetFilePaths("pet*.json"))
			{
				OpenApiDocument document = DocumentLoader.Read(specificationFile);
				var testCases = from x in document.Components.Schemas
								select x;

				foreach (var item in testCases.Take(2))
				{
					yield return new object[] { item.Key, item.Value };
				}
			}
		}

		private static IEnumerable<object[]> GetOperations()
		{
			foreach (string specificationFile in TestData.GetFilePaths("pet*.json"))
			{
				OpenApiDocument document = DocumentLoader.Read(specificationFile);

				foreach (var item in document.Paths.Take(1))
					foreach (var method in item.Value.Operations.Take(2))
					{
						yield return new object[] { item.Key, method.Key, method.Value, document };
					}
			}
		}

		public static string GetTestDisplayName(MethodInfo methodInfo, object[] data)
		{
			string defaultName = $"{methodInfo.Name}";

			if (data == null) return defaultName;
			else return string.Concat(methodInfo.Name, "(", data[0], ")");
		}

		private static string AppendErrors(string csharp, params string[] ignore)
		{
			if (string.IsNullOrEmpty(csharp)) return csharp;

			var snippet = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp);
			var errors = from x in snippet.GetDiagnostics()
						 where
							x.Severity >= Microsoft.CodeAnalysis.DiagnosticSeverity.Warning
							&&
							ignore.Contains(x.Id) == false
						 select $"[{x.Id}] Line {x.Location.GetMappedLineSpan().StartLinePosition.Line + 1}: {x.GetMessage()}";

			if (errors.Any())
			{
				return string.Concat(
					csharp,
					"\r\n\r\n",
					"Errors\r\n",
					string.Concat(Enumerable.Repeat('=', 50)),
					"\r\n",
					string.Join("\r\n", errors));
			}
			else
				return csharp;
		}

		#endregion Backing Members
	}
}