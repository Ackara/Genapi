namespace Tekcari.Genapi.Generators
{
	public readonly struct FileResult
	{
		public FileResult(string fileName, string content)
		{
			Name = fileName;
			Content = content;
		}

		public string Name { get; }

		public string Content { get; }
	}
}