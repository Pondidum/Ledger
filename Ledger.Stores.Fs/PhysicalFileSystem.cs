using System.IO;

namespace Ledger.Stores.Fs
{
	public class PhysicalFileSystem : IFileSystem
	{
		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public Stream ReadFile(string path)
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		public Stream AppendTo(string path)
		{
			return new FileStream(path, FileMode.Append);
		}
	}
}
