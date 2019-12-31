using Eon.Text.RegularExpressions;

namespace Eon.IO {

	public interface IFilesArchivationOptions
			:IAsReadOnly<IFilesArchivationOptions>, IValidatable {

		string ArchiveDirectoryPath { get; set; }

		string DirectoryPath { get; set; }

		bool IncludeSubdirectories { get; set; }

		RegexConstructParameters RelativeFilePathRegex { get; set; }

	}

}