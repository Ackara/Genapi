using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneratedCode
{
	[TestClass]
	public partial class GeneratedClientTest
	{
		[TestMethod]
		public async Task Can_Checkout()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");
			var shoppingCart = AutoBogus.AutoFaker.Generate<ShoppingCart>();

			var response = await sut.CheckoutAsync(shoppingCart: shoppingCart);

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

		[TestMethod]
		public async Task Can_CreateProduct()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");
			var product = AutoBogus.AutoFaker.Generate<Product>();

			var response = await sut.CreateProductAsync(product: product);

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

		[TestMethod]
		public async Task Can_GetProductByKeywords()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");
			var keywords = AutoBogus.AutoFaker.Generate<string>();
			var take = AutoBogus.AutoFaker.Generate<int>();
			var skip = AutoBogus.AutoFaker.Generate<int>();

			var response = await sut.GetProductByKeywordsAsync(keywords: keywords, take: take, skip: skip);

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

		[TestMethod]
		public async Task Can_CreateProductFromPackage()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");

			var response = await sut.CreateProductFromPackageAsync();

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

		[TestMethod]
		public async Task Can_GetProducts()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");
			var productSearchRequest = AutoBogus.AutoFaker.Generate<ProductSearchRequest>();

			var response = await sut.GetProductsAsync(productSearchRequest: productSearchRequest);

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

		[TestMethod]
		public async Task Can_GetProductBanner()
		{
			var sut = new GeneratedClient("https://petstore3.swagger.io/api/v3");
			var productId = AutoBogus.AutoFaker.Generate<string>();

			var response = await sut.GetProductBannerAsync(productId: productId);

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

	}
}

