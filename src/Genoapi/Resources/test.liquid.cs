using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace {{rootnamespace}}
{
	[TestClass]
	public partial class {{test_class_name}}
	{
	{%- for endpoint in endpoints -%}
	{%- assign parameterList = endpoint.parameters | map: "value" -%}
		[TestMethod]
		public async Task {{endpoint.testName}}()
		{
			var sut = new {{client_class_name}}("{{service_url}}");
		{%- for parameter in endpoint.parameters -%}
			var {{parameter.name}} = AutoBogus.AutoFaker.Generate<{{parameter.type}}>();
		{%- endfor-%}

			var response = await sut.{{endpoint.operationName}}Async({{parameterList | join: ", "}});

			Assert.AreEqual(true, response.Succeeded, response.Message);
		}

	{%- endfor -%}
	}
}
