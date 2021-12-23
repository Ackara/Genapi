using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Threading.Tasks;

namespace Genapi.Example.Tests
{
	[TestClass]
	public class ClientTest
	{
		[TestMethod]
		public async Task Method1()
		{
			// Arrange
			const string baseUrl = "https://petstore3.swagger.io/api/v3";
			var sut = new GeneratedCode.GeneratedClient(baseUrl);

			var pet = AutoBogus.AutoFaker.Generate<GeneratedCode.Pet>();

			

			// Act

			var response = await sut.addPetAsync(pet);

			// Assert

			response.Succeeded.ShouldBeTrue(response.Message);
		}
	}
}