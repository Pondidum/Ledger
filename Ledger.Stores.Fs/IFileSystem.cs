using System.IO;

namespace Ledger.Stores.Fs
{
	public interface IFileSystem
	{
		bool FileExists(string path);

		Stream ReadFile(string path);
		Stream AppendTo(string path);
	}
}
