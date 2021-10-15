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

		public void WriteMember(CSharpMethod method)
		{
			base.WriteIndent();
			this.Write("public void name()");
			this.WriteLine("{");



			this.WriteLine("}");
		}
	}
}