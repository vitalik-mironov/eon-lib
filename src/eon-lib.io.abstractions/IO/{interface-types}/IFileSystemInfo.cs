using System;

namespace Eon.IO {

	public interface IFileSystemInfo {

		DateTime CreationTime { get; }

		string Name { get; }

		void Refresh();

		bool Exists { get; }

		string FullName { get; }

	}

}