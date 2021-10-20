using System.Text;

namespace Tekcari.Genapi.Transformation.CSharp
{
	public class CSharpWriter : WriterBase
	{
		public CSharpWriter()
		{
			Encoding = System.Text.Encoding.UTF8;
		}

		public override Encoding Encoding { get; }

		
	}
}