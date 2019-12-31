using System;
using System.IO;
using Eon.Text;

namespace Eon.IO {

	public static partial class PathUtilities {

		static readonly char[ ] __InvalidFileNameChars;

		public static readonly char InvalidPathCharEscapeChar = '_';

		static PathUtilities() {
			__InvalidFileNameChars = Path.GetInvalidFileNameChars();
		}

		public static string EscapeInvalidFileNameChars(string fileName) {
			if (string.IsNullOrEmpty(fileName))
				return fileName;
			using (var buf = StringBuilderUtilities.AcquireBuffer()) {
				var sb = buf.StringBuilder;
				sb.Append(fileName);
				for (var i = 0; i < __InvalidFileNameChars.Length; i++)
					sb.Replace(__InvalidFileNameChars[ i ], InvalidPathCharEscapeChar);
				return sb.ToString();
			}
		}

		public static string ExpandPath(string path, bool trimTrailingDirectorySeparator = false)
			=> ExpandPath(path: path, basePath: AppContext.BaseDirectory, trimTrailingDirectorySeparator: trimTrailingDirectorySeparator);

		// TODO: Put strings into the resources.
		//
		public static string ExpandPath(string path, string basePath, bool trimTrailingDirectorySeparator = false) {
			basePath.EnsureNotNull(nameof(basePath)).EnsureNotEmpty();
			if (!Path.IsPathRooted(basePath))
				throw
					new ArgumentException(
						paramName: nameof(basePath),
						message: $"Указанный путь не является абсолютным путём.{Environment.NewLine}\tПуть:{basePath.FmtStr().GNLI2()}");
			//
			path = Environment.ExpandEnvironmentVariables(path);
			if (trimTrailingDirectorySeparator)
				path = path.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (!Path.IsPathRooted(path: path))
				path =
					Path
					.Combine(
						path1: trimTrailingDirectorySeparator ? basePath.TrimEndSingle(trimChar1: Path.DirectorySeparatorChar, trimChar2: Path.AltDirectorySeparatorChar) : basePath,
						path2: path);
			return path;
		}

	}

}