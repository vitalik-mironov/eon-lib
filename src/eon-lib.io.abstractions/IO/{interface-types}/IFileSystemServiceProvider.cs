using System;
using System.IO;

using Eon.ComponentModel.Dependencies;
using Eon.Text;

namespace Eon.IO {

	public interface IFileSystemServiceProvider
		:IDisposableDependencySupport {

		bool ChooseSingleFileForOpenUI(string prompt, string fileFilter, out IFileInfo chosenFile);

		bool ChooseSingleFileForSaveUI(string prompt, string fileFilter, string defaultFileName, out IFileInfo chosenFile);

		bool ChooseSingleFileForSaveUI(string prompt, string fileFilter, string defaultFileName, out Stream chosenFileStream);

		string EscapeInvalidFileNameChars(string value);

		IFileInfo GetFileInfo(string filePath);

		string CombinePath(string path1, string path2);

		string CombinePath(params string[ ] paths);

		bool TrySelectNonexistentFileName(DirectoryInfo directory, string fileNameTemplate, int maxIterations, Func<ITextTemplateVarSubstitution, int, ITextTemplateVarSubstitution> fileNameTemplateSubstitutionSelector, out FileInfo selectedFileName);

		bool TrySelectNonexistentFileName(DirectoryInfo directory, string fileName, int maxIterations, out FileInfo selectedFileName);

	}
}