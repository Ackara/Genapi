using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class CSharpTransformationTest
	{
		[TestMethod]
		public void Can_write_csharp_method()
		{
			// Arrange

			var me = new Transformation.CSharp.CSharpMethod
			{
				Name = "GetItems",

				Body = @""
			};

			// Act

			// Assert
		}

		#region Backing Members

		private static IEnumerable<object[]> GetDocuments()
		{
			foreach (var file in TestData.GetFilePaths("*.yml"))
				yield return new object[] { file };

			foreach (var file in TestData.GetFilePaths("*.json"))
				yield return new object[] { file };

			var urls = new string[]
			{
				"https://raw.githubusercontent.com/plaid/plaid-openapi/master/2020-09-14.yml"
			};
			foreach (string u in urls) yield return new object[] { u };
		}

		#endregion Backing Members
	}
}