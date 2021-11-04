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

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class CSharpTransformationTest
	{
		[TestMethod]
		[DynamicData(nameof(GetComponents), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
		public void Can_transform_component_to_csharp_class(OpenApiSchema schema)
		{
			// Arrange

			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario(schema.Title);

			var output = new MemoryStream();
			var writer = new Transformation.CSharp.CSharpApiClientWriter(output);

			// Act

			writer.Write(schema);
			writer.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = VerifyCode(code);

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			ApprovalTests.Approvals.Verify(code);
		}

		[TestMethod]
		[DynamicData(nameof(GetOperations), DynamicDataSourceType.Method)]
		public void Can_transform_operation_to_csharp_method(string path, OperationType method, OpenApiOperation endpoint)
		{
			// Arrange

			using var scenario = ApprovalTests.Namers.ApprovalResults.ForScenario($"{method}: {path}");

			var output = new MemoryStream();
			var writer = new Transformation.CSharp.CSharpApiClientWriter(output);

			// Act

			writer.Write(path, method, endpoint);
			writer.Flush();
			string code = Encoding.UTF8.GetString(output.ToArray());
			code = VerifyCode(code, "CS0106");

			// Assert

			code.ShouldNotBeNullOrWhiteSpace();
			ApprovalTests.Approvals.Verify(code);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetComponents()
		{
			var filter = new Regex("Item.+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			OpenApiDocument document = DocumentLoader.Read(TestData.GetFilePath("*.yml"));
			var testCases = from x in document.Components.Schemas.Values
							where
							 !string.IsNullOrEmpty(x.Title)
							 &&
							 string.Equals(x.Type, "object", StringComparison.InvariantCultureIgnoreCase)
							 &&
							 filter.IsMatch(x.Title)
							select x;

			foreach (OpenApiSchema item in testCases.Take(2))
			{
				yield return new object[] { item };
			}
		}

		private static IEnumerable<object[]> GetOperations()
		{
			var filter = new Regex("Item.+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			foreach (string filePath in TestData.GetFilePaths("*.json"))
			{
				OpenApiDocument document = DocumentLoader.Read(filePath);

				foreach (var item in document.Paths)
					foreach (var method in item.Value.Operations)
					{
						yield return new object[] { item.Key, method.Key, method.Value };
					}
			}
		}

		public static string GetTestDisplayName(MethodInfo methodInfo, object[] data)
		{
			string defaultName = $"{methodInfo.Name}";

			if (data == null) return defaultName;
			else if (data[0] is OpenApiSchema s) return s?.Title;
			else return Convert.ToString(data[0]);
		}

		private static string VerifyCode(string csharp, params string[] ignore)
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