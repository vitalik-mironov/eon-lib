using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Linq;
using Eon.Text;
using Eon.Threading;
using Eon.Threading.Tasks;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.IO {

	public static class FileSystemAccessUtilities {

		#region Nested types

		sealed class P_DefaultPathComparerData {

			public readonly StringComparer Comparer;

			public readonly StringComparison Comparison;

			internal P_DefaultPathComparerData(StringComparer comparer) {
				if (comparer == StringComparer.OrdinalIgnoreCase)
					Comparison = StringComparison.OrdinalIgnoreCase;
				else if (comparer == StringComparer.Ordinal)
					Comparison = StringComparison.Ordinal;
				else
					throw new ArgumentOutOfRangeException(paramName: nameof(comparer));
				Comparer = comparer;
			}

		}

		#endregion

		static P_DefaultPathComparerData __DefaultPathComparer;

		public static bool ChooseSingleFileForOpenUI(IDependencySupport dependencies, string prompt, string fileFilter, out IFileInfo chosenFile)
			=> P_RequireFileSystemServiceProvider(dependencies).ChooseSingleFileForOpenUI(prompt, fileFilter, out chosenFile);

		public static bool ChooseSingleFileForSaveUI(IDependencySupport dependencies, string prompt, string fileFilter, string defaultFileName, out IFileInfo chosenFile)
			=> P_RequireFileSystemServiceProvider(dependencies).ChooseSingleFileForSaveUI(prompt, fileFilter, defaultFileName, out chosenFile);

		public static bool ChooseSingleFileForSaveUI(IDependencySupport dependencies, string prompt, string fileFilter, string defaultFileName, out Stream chosenFileStream)
			=> P_RequireFileSystemServiceProvider(dependencies).ChooseSingleFileForSaveUI(prompt, fileFilter, defaultFileName, out chosenFileStream);

		public static string EscapeInvalidFileNameChars(IDependencySupport dependencies, string value)
			=> P_RequireFileSystemServiceProvider(dependencies).EscapeInvalidFileNameChars(value);

		public static IFileInfo GetFileInfo(IDependencySupport dependencies, string filePath)
			=> P_RequireFileSystemServiceProvider(dependencies).GetFileInfo(filePath);

		public static string CombinePath(IDependencySupport dependencies, string path1, string path2)
			=> P_RequireFileSystemServiceProvider(dependencies).CombinePath(path1, path2);

		public static string CombinePath(IDependencySupport dependencies, params string[ ] paths)
			=> P_RequireFileSystemServiceProvider(dependencies).CombinePath(paths);

		// TODO: Put exception messages into the resources.
		//
		static IFileSystemServiceProvider P_RequireFileSystemServiceProvider(IDependencySupport dependencies) {
			dependencies.EnsureNotNull(nameof(dependencies));
			//
			var fileSystemAccessProvider = dependencies.GetService<IFileSystemServiceProvider>();
			if (fileSystemAccessProvider == null)
				throw new EonException(message: $"Невозможно выполнить операцию, так как поставщик функциональных зависимостей не предоставил компонент доступа к файловой системе.{Environment.NewLine}\tПоставщик функциональных зависимостей:{Environment.NewLine}{dependencies.FmtStr().G().IndentLines2()}");
			else
				return fileSystemAccessProvider;
		}

#if TRG_NETSTANDARD1_5

		public static StringComparison DefaultPathComparison
			=> P_DefaultPathComparer.Comparison;

		public static StringComparer DefaultPathComparer
			=> P_DefaultPathComparer.Comparer;

		static P_DefaultPathComparerData P_DefaultPathComparer {
			get {
				var comparer = VolatileUtilities.Read(ref __DefaultPathComparer);
				if (comparer is null) {
					// Платформа не поставляет никакого способа узнать о чувствительности к регистру в именах файловой системы...
					//
					bool isCaseInsensitive;
					var fileName = (GuidUtilities.NewGuidString().Left(8) + ".txt").ToLowerInvariant();
					var fileDirecory = Path.GetTempPath();
					var file = new FileInfo(Path.Combine(fileDirecory, fileName));
					using (var fileStream = new FileStream(file.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read)) {
						fileStream.WriteByte(255);
						fileStream.Flush();
						isCaseInsensitive = File.Exists(Path.Combine(fileDirecory, fileName.ToUpperInvariant()));
						fileStream.Dispose();
						try { file.Delete(); }
						catch { };
					}
					comparer = isCaseInsensitive ? new P_DefaultPathComparerData(StringComparer.OrdinalIgnoreCase) : new P_DefaultPathComparerData(StringComparer.Ordinal);
					comparer = Interlocked.CompareExchange(ref __DefaultPathComparer, comparer, null) ?? comparer;
				}
				return comparer;
			}
		}

		public static bool TryDeleteFile(string filePath) {
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentException(FormatXResource(typeof(string), "CanNotNullOrEmpty"), nameof(filePath));
			try {
				File.Delete(filePath);
				return true;
			}
			catch (Exception firstException) {
				if (firstException is PathTooLongException)
					throw;
				return false;
			}
		}

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Копирует всё содержимое (включая подкаталоги) указанного каталога (директории) <paramref name="sourceDirectory"/> в указанный каталог <paramref name="destinationDirectory"/>.
		/// <para>Сам каталог-источник в каталог-приёмнике не создаётся.</para>
		/// </summary>
		/// <param name="sourceDirectory">
		/// Каталог-источник копирования.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="destinationDirectory">
		/// Каталог-приёмник копирования.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="taskCreationOptions">Опции создания задачи <seealso cref="Task"/> выполнения операции.</param>
		/// <returns>Объект <seealso cref="Task"/>.</returns>
		public static Task DeepCopyAsync(this DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
			=>
			Task.Factory.StartNew(action: () => DeepCopy(sourceDirectory: sourceDirectory, destinationDirectory: destinationDirectory), cancellationToken: CancellationToken.None, creationOptions: taskCreationOptions, scheduler: TaskScheduler.Default);

		// TODO: Add component info into log
		//
		/// <summary>
		/// Создает ZIP-архив файла <paramref name="sourceFile"/>, сохраняет его по пути <paramref name="archiveFile"/>.
		/// <para>При <paramref name="deleteSourceFileOnSuccess"/> = <see langword="true"/>, исходный файл удаляется.</para>
		/// </summary>
		/// <param name="sourceFile">
		/// Файл-источник архивирования.
		/// <para>Не может быть <see langword="null"/>. Файл должен существовать.</para>
		/// </param>
		/// <param name="archiveFile">
		/// Файл-приёмник архивирования.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="overwrite">
		/// Флаг перезаписи файла-приемника <paramref name="archiveFile"/>, если такой существует.
		/// </param>
		/// <param name="deleteSourceFileOnSuccess">
		/// Флаг удаления файла-источника при успешном архивировании (исключения при удалении не выбрасываются).
		/// </param>
		/// <param name="overrideSourceFileName">
		/// Переопределяет имя исходного файла в ZIP-архиве.
		/// <para>Если не указано, то имя соответствует имени исходного файла <paramref name="sourceFile"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		public static async Task ZipArchiveFileAsync(FileInfo sourceFile, FileInfo archiveFile, bool deleteSourceFileOnSuccess, string overrideSourceFileName = default, bool overwrite = default, IContext ctx = default) {
			sourceFile.EnsureNotNull(nameof(sourceFile));
			archiveFile.EnsureNotNull(nameof(archiveFile));
			overrideSourceFileName.Arg(nameof(overrideSourceFileName)).EnsureNotEmpty();
			//
			if (!sourceFile.Exists)
				throw new FileNotFoundException(FormatXResource(typeof(FileNotFoundException), null, sourceFile.FullName));
			try {
				var ct = ctx.Ct();
				FileStream sourceFileStream = default;
				try {
					var retryOpen = true;
					for (; ; ) {
						try {
							sourceFileStream = new FileStream(path: sourceFile.FullName, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read);
							break;
						}
						catch (IOException ex) {
							if (retryOpen) {
								retryOpen = false;
								await TaskUtilities.Delay(duration: TimeoutDuration.FromMilliseconds(937), cancellationToken: ct, cancellationBreak: true).ConfigureAwait(false);
								continue;
							}
							else
								throw new EonException(message: $"Ошибка при архивировании: файл-источник невозможно открыть для чтения.{Environment.NewLine}\tФайл:{sourceFile.FullName.FmtStr().GNLI2()}", innerException: ex);
						}
					}
					archiveFile.Directory.Refresh();
					if (!archiveFile.Directory.Exists)
						archiveFile.Directory.Create();
					var archiveFileStream = default(FileStream);
					var zipArchive = default(ZipArchive);
					try {
						try {
							archiveFileStream = new FileStream(path: archiveFile.FullName, mode: overwrite ? FileMode.Create : FileMode.CreateNew, access: FileAccess.ReadWrite, share: FileShare.Read);
						}
						catch (Exception ex) {
							throw
								new EonException(message: $"Ошибка при архивировании: невозможно открыть поток для записи.{Environment.NewLine}\tФайл:{archiveFile.FullName.FmtStr().GNLI2()}", innerException: ex);
						}
						zipArchive = new ZipArchive(stream: archiveFileStream, mode: ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding: Encoding.UTF8);
						var zipEntry = zipArchive.CreateEntry(entryName: overrideSourceFileName ?? sourceFile.Name, compressionLevel: CompressionLevel.Optimal);
						using (var zipEntryStream = zipEntry.Open()) {
							await sourceFileStream.CopyToAsync(destination: zipEntryStream, cancellationToken: ct).ConfigureAwait(false);
							await zipEntryStream.FlushAsync(cancellationToken: ct).ConfigureAwait(false);
						}
					}
					catch (Exception ex) {
						throw
							new EonException(
								message: $"Ошибка при архивировании.{Environment.NewLine}\tФайл-источник:{sourceFile.FullName.FmtStr().GNLI2()}{Environment.NewLine}\tФайл-приемник:{archiveFile.FullName.FmtStr().GNLI2()}",
								innerException: ex);
					}
					finally {
						zipArchive?.Dispose();
						archiveFileStream?.Dispose();
					}
				}
				finally {
					sourceFileStream?.Dispose();
				}
				if (deleteSourceFileOnSuccess) {
					var retryDelete = true;
					for (; ; ) {
						try {
							sourceFile.Delete();
							break;
						}
						catch (IOException) {
							if (retryDelete) {
								retryDelete = false;
								await TaskUtilities.Delay(duration: TimeoutDuration.FromMilliseconds(937), cancellationToken: ct, cancellationBreak: true).ConfigureAwait(false);
								continue;
							}
							else
								break;
						}
					}
				}
			}
			catch (Exception exception) {
				throw
					new EonException(
						message: $"Ошибка при архивировании.{Environment.NewLine}\tФайл-источник:{sourceFile.FullName.FmtStr().GNLI2()}{Environment.NewLine}\tФайл-приемник:{archiveFile.FullName.FmtStr().GNLI2()}",
						innerException: exception);
			}
		}

		/// <summary>
		/// Копирует всё содержимое (включая подкаталоги) указанного каталога-источника (директории) <paramref name="sourceDirectory"/> в указанный каталог-приёмник <paramref name="destinationDirectory"/>.
		/// <para>Сам каталог-источник в каталог-приёмнике не создаётся.</para>
		/// </summary>
		/// <param name="sourceDirectory">
		/// Каталог-источник копирования.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="destinationDirectory">
		/// Каталог-приёмник копирования.
		/// <para>Не может быть null.</para>
		/// </param>
		public static void DeepCopy(this DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory) {
			sourceDirectory.EnsureNotNull(nameof(sourceDirectory));
			sourceDirectory.EnsureNotNull(nameof(destinationDirectory));
			//
			sourceDirectory.Refresh();
			if (!sourceDirectory.Exists)
				throw
					new DirectoryNotFoundException(message: FormatXResource(typeof(DirectoryNotFoundException), subpath: null, args: new[ ] { sourceDirectory.FullName }));
			destinationDirectory.Refresh();
			if (sourceDirectory.FullName.StringEquals(destinationDirectory.FullName, DefaultPathComparer))
				throw new EonException(message: $"Путь каталога-источника '{sourceDirectory.FullName}' совпадает с путем каталога-приемника '{destinationDirectory.FullName}'.");
			var traversalList =
				new {
					SourceBase = sourceDirectory,
					DestinationBase = destinationDirectory
				}
				.Sequence()
				.ToList();
			for (var traversalCounter = 0; traversalCounter < traversalList.Count; traversalCounter++) {
				var traversalItem = traversalList[ traversalCounter ];
				//
				var currentDestinationBase = traversalItem.DestinationBase;
				var currentSourceBase = traversalItem.SourceBase;
				// Копирование структуры каталогов (директорий).
				//
				foreach (var sourceBaseSubdirectory in currentSourceBase.GetDirectories("*", SearchOption.TopDirectoryOnly)) {
					var destinationBaseSubdirectory = new DirectoryInfo(Path.Combine(currentDestinationBase.FullName, sourceBaseSubdirectory.Name));
					destinationBaseSubdirectory.Refresh();
					if (!destinationBaseSubdirectory.Exists)
						destinationBaseSubdirectory.Create();
					traversalList.Add(new { SourceBase = sourceBaseSubdirectory, DestinationBase = destinationBaseSubdirectory });
				}
				// Копирование файлов.
				//
				if (!currentDestinationBase.Exists)
					currentDestinationBase.Create();
				foreach (var sourceFile in currentSourceBase.GetFiles("*", SearchOption.TopDirectoryOnly))
					sourceFile.CopyTo(destFileName: Path.Combine(currentDestinationBase.FullName, sourceFile.Name), overwrite: true);
			}
		}

		public static void DirectoryDeepCopy(string sourceDirectoryPath, string destinationDirectoryPath) {
			sourceDirectoryPath.EnsureNotNullOrEmpty(nameof(sourceDirectoryPath));
			destinationDirectoryPath.EnsureNotNullOrEmpty(nameof(destinationDirectoryPath));
			//
			(new DirectoryInfo(sourceDirectoryPath)).DeepCopy(new DirectoryInfo(destinationDirectoryPath));
		}

		// TODO: Put strings into the resources.
		//
		public static string GetBasePath(this string fullPath1, string fullPath2) {
			fullPath1.EnsureNotNullOrEmpty(nameof(fullPath1));
			fullPath2.EnsureNotNullOrEmpty(nameof(fullPath2));
			if (!Path.IsPathRooted(fullPath1))
				throw new ArgumentException($"Указанный путь не является полным.{Environment.NewLine}\tПуть:{Environment.NewLine}{fullPath1.IndentLines2()}", nameof(fullPath1));
			else if (!Path.IsPathRooted(fullPath2))
				throw new ArgumentException($"Указанный путь не является полным.{Environment.NewLine}\tПуть:{Environment.NewLine}{fullPath2.IndentLines2()}", nameof(fullPath2));
			//
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				throw new NotSupportedException($"Операция не поддерживается на текущей платформе ОС.");
			//
			if (!fullPath1[ fullPath1.Length - 1 ].P_IsDirectorySeparator())
				fullPath1 += Path.AltDirectorySeparatorChar;
			if (!fullPath2[ fullPath2.Length - 1 ].P_IsDirectorySeparator())
				fullPath2 += Path.AltDirectorySeparatorChar;
			var minLength = Math.Min(fullPath1.Length, fullPath2.Length);
			string pathPart1 = null;
			string pathPart2 = null;
			var lastEqualCharIndex = -1;
			var nonEmptyPathPartComponents = 0;
			for (var i = 0; i < minLength; i++) {
				if (fullPath1[ i ].P_IsDirectorySeparator() || fullPath2[ i ].P_IsDirectorySeparator()) {
					if (fullPath1[ i ].P_IsDirectorySeparator() && fullPath2[ i ].P_IsDirectorySeparator()) {
						if (string.Equals(pathPart1, pathPart2, DefaultPathComparison)) {
							if (i > 0) {
								if (!string.IsNullOrEmpty(pathPart1))
									nonEmptyPathPartComponents++;
								lastEqualCharIndex = i;
							}
							pathPart1 = null;
							pathPart2 = null;
						}
						else
							break;
					}
					else
						break;
				}
				else {
					pathPart1 += fullPath1[ i ];
					pathPart2 += fullPath2[ i ];
				}
			}
			if (lastEqualCharIndex < 0 || nonEmptyPathPartComponents < 1)
				return null;
			return fullPath1.Left(lastEqualCharIndex + 1);
		}

		static bool P_IsDirectorySeparator(this char value)
			=> value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar;

		/// <summary>
		/// Создает относительный путь от одной директории или файла к другой(-ому).
		/// </summary>
		/// <param name="fromPath">Путь, определяющий "верхнюю" точку, относительно которой строится путь.</param>
		/// <param name="toPath">Путь, определяющий конечный элемент, на который должен указывать относительный путь.</param>
		public static string GetRelativePath(string fromPath, string toPath) {
			fromPath.Arg(nameof(fromPath)).EnsureNotNullOrEmpty();
			toPath.Arg(nameof(toPath)).EnsureNotNullOrEmpty();

			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);

			fromUri.Arg(nameof(fromUri)).EnsureAbsolute();
			toUri.Arg(nameof(toUri)).EnsureAbsolute();

			if (fromUri.Scheme != toUri.Scheme)
				throw new EonException($"Операция получения относительного пути не может быть выполнена. Входные параметры представляют Uri с различными свойствами 'Scheme'.{Environment.NewLine}\tFromPath:{fromPath.FmtStr().GNLI2()}{Environment.NewLine}\tToPath:{toPath.FmtStr().GNLI2()}");

			if (fromUri.IsLoopback != toUri.IsLoopback)
				throw new EonException($"Операция получения относительного пути не может быть выполнена. Входные параметры представляют Uri с различными свойствами 'IsLoopback'.{Environment.NewLine}\tFromPath:{fromPath.FmtStr().GNLI2()}{Environment.NewLine}\tToPath:{toPath.FmtStr().GNLI2()}");

			if (fromUri.IsUnc != toUri.IsUnc)
				throw new EonException($"Операция получения относительного пути не может быть выполнена. Входные параметры представляют Uri с различными свойствами 'IsUnc'.{Environment.NewLine}\tFromPath:{fromPath.FmtStr().GNLI2()}{Environment.NewLine}\tToPath:{toPath.FmtStr().GNLI2()}");

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			return relativePath;
		}

		public static bool IsDescendantOrSelfOf(this DirectoryInfo descandantDirectory, DirectoryInfo ascendantDirectory) {
			descandantDirectory.EnsureNotNull(nameof(descandantDirectory));
			ascendantDirectory.EnsureNotNull(nameof(ascendantDirectory));
			//
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				throw new NotSupportedException($"Операция не поддерживается на текущей платформе ОС.");
			//
			return string.Equals(ascendantDirectory.FullName, descandantDirectory.FullName, DefaultPathComparison) || IsDescendantOf(descandantDirectory, ascendantDirectory);
		}

		public static bool IsDescendantOf(this DirectoryInfo descandantDirectory, DirectoryInfo ascendantDirectory) {
			descandantDirectory.EnsureNotNull(nameof(descandantDirectory));
			ascendantDirectory.EnsureNotNull(nameof(ascendantDirectory));
			//
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				throw new NotSupportedException($"Операция не поддерживается на текущей платформе ОС.");
			//
			var ascendants = ascendantDirectory.TreeNode().SelfAndAncestors(i => i.Parent).ToLinkedList();
			var descandantParents = descandantDirectory.TreeNode().Ancestors(i => i.Parent).ToLinkedList();
			if (ascendants.Count > descandantParents.Count)
				return false;
			else {
				var ascendantsNode = ascendants.Last;
				var descendantParentsNode = descandantParents.Last;
				var result = true;
				for (; ascendantsNode != null && result;) {
					result = string.Equals(ascendantsNode.Value.Name, descendantParentsNode.Value.Name, DefaultPathComparison);
					ascendantsNode = ascendantsNode.Previous;
					descendantParentsNode = descendantParentsNode.Previous;
				}
				return result;
			}
		}

		public static bool TrySelectNonexistentFileName(IDependencySupport dependencies, DirectoryInfo directory, string fileNameTemplate, int maxIterations, Func<ITextTemplateVarSubstitution, int, ITextTemplateVarSubstitution> fileNameTemplateSubstitutionSelector, out FileInfo selectedFileName)
			=>
			P_RequireFileSystemServiceProvider(dependencies: dependencies)
			.TrySelectNonexistentFileName(directory: directory, fileNameTemplate: fileNameTemplate, maxIterations: maxIterations, fileNameTemplateSubstitutionSelector: fileNameTemplateSubstitutionSelector, selectedFileName: out selectedFileName);

		public static bool TrySelectNonexistentFileName(
			IDependencySupport dependencies,
			DirectoryInfo directory,
			string fileName,
			int maxIterations,
			out FileInfo selectedFileName)
			=>
			P_RequireFileSystemServiceProvider(dependencies: dependencies)
			.TrySelectNonexistentFileName(
				directory: directory,
				fileName: fileName,
				maxIterations: maxIterations,
				selectedFileName: out selectedFileName);

#endif

	}
}