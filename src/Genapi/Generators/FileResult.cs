namespace Tekcari.Genapi.Generators
{
	public readonly struct FileResult
	{
		public FileResult(string fileName, string content)
			: this(fileName, content, null) { }

		public FileResult(string fileName, string content, string tag)
		{
			Name = fileName;
			Content = content;
			Tag = tag;
		}

		public string Name { get; }

		public string Content { get; }

		public string Tag { get; }
	}
}