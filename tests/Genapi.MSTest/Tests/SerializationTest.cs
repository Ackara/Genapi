using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tekcari.Genapi.Transformation;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class SerializationTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSpecifications), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
		public void Can_convert_file_to_openapi_spec_model(string filePath)
		{
			var document = DocumentLoader.Read(filePath);
			document.ShouldNotBeNull();
			document.Paths.ShouldNotBeEmpty();
		}

		#region Backing Members

		private static IEnumerable<object[]> GetSpecifications() => TestData.GetSpecifications();

		public static string GetTestDisplayName(MethodInfo method, object[] args)
		{
			string path = Convert.ToString(args[0]);
			return $"{method.Name}('{Path.GetFileName(path)}')";
		}

		#endregion Backing Members
	}
}