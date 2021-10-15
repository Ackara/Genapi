namespace Tekcari.Genapi.Transformation
{
	public abstract class WriterBase : System.IO.TextWriter
	{
		public string Indentation { get; private set; }

		protected void WriteIndent()
		{
			base.Write(Indentation);
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

		public void PopIndent()
		{
			if (!string.IsNullOrEmpty(Indentation))
			{
				Indentation = Indentation.Remove(0, 1);
			}
		}
	}
}