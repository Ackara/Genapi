using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Tekcari.Genapi.Generators;

namespace Tekcari.Genapi.Tests
{
	[TestClass]
	public class CustomFilterTest
	{
		[TestMethod]
		[DataRow("normal", "Normal")]
		[DataRow("camelCase", "CamelCase")]
		[DataRow("snake_case", "SnakeCase")]
		[DataRow("Title Case", "TitleCase")]
		[DataRow("PascalCase", "PascalCase")]
		[DataRow("order.cancel", "OrderCancel")]
		public void Can_convert_text_pascal_case(string original, string expected)
		{
			CustomFilters.PascalCase(original).ShouldBe(expected);
		}

		[TestMethod]
		[DataRow("normal", "normal")]
		[DataRow("Normal", "normal")]
		[DataRow("camelCase", "camelCase")]
		[DataRow("snake_case", "snakeCase")]
		[DataRow("Title Case", "titleCase")]
		[DataRow("PascalCase", "pascalCase")]
		public void Can_convert_text_camel_case(string original, string expected)
		{
			CustomFilters.CamelCase(original).ShouldBe(expected);
		}
	}
}