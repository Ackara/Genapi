using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tekcari.Genapi
{
	public class Transpiler
	{
		public Transpiler()
		{

		}

		public void Run(OpenApiDocument document, TranspilerSettings settings)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));

			foreach (var item in document.Paths)
			{
				
			}

			throw new System.NotImplementedException();
		}

		

		#region Backing Members



		#endregion
	}
}