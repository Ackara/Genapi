using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using Tekcari.Genapi.Transformation;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class SerializationTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSpecifications), DynamicDataSourceType.Method)]
		public void Can_convert_file_to_openapi_spec_model(string filePath)
		{
			var document = DocumentLoader.Read(filePath);
			document.ShouldNotBeNull();
			document.Paths.ShouldNotBeEmpty();
		}

		#region Backing Members

		private static IEnumerable<object[]> GetSpecifications() => TestData.GetSpecifications();

		#endregion Backing Members
	}
}