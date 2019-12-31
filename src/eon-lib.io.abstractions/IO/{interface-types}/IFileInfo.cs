using System.IO;

namespace Eon.IO {

	public interface IFileInfo
		:IFileSystemInfo {

		long Length { get; }

		Stream OpenRead();

		Stream OpenWrite();

		string DirectoryFullName { get; }

	}

}