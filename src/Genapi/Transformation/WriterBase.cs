using System.IO;
using System.Text;

namespace Tekcari.Genapi.Transformation
{
	public abstract class WriterBase : System.IO.StreamWriter
	{
		public WriterBase(Stream stream, Encoding encoding)
			: base(stream, encoding) { }

		protected Stream Data;

		public string Indentation { get; private set; }

		protected void WriteIndent()
		{
			Write(Indentation);
		}

		protected new void WriteLine(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				base.Write(Indentation);
				base.WriteLine(text);
			}
		}

		protected void PushIndent()
		{
			Indentation += '\t';
		}

		protected void PopIndent()
		{
			if (!string.IsNullOrEmpty(Indentation))
			{
				Indentation = Indentation.Remove(0, 1);
			}
		}

		protected string GetName(string name)
		{
			return name;
		}
	}
}