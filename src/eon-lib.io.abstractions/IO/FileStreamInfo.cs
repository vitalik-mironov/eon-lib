using System;
using System.IO;

namespace Eon.IO {

	public class FileStreamInfo
		:StreamInfo {

		readonly bool _hasFileInfo;

		FileInfo _fileInfo;

		public FileStreamInfo(FileStream fileStream, bool ownsFileStream, string contentMediaTypeName = null)
			: base(fileStream, ownsFileStream, contentMediaTypeName) {
			//
			var fileStreamName = fileStream.Name;
			if (string.IsNullOrEmpty(fileStreamName)) {
				_hasFileInfo = false;
				_fileInfo = null;
			}
			else {
				_fileInfo = new FileInfo(fileStreamName);
				_hasFileInfo = true;
			}
		}

		public FileStreamInfo(FileInfo fileInfo, FileStream fileStream, bool ownsFileStream, string contentMediaTypeName = null)
			: base(fileStream, ownsFileStream, contentMediaTypeName) {
			//
			_fileInfo = fileInfo;
			_hasFileInfo = fileInfo != null;
		}

		public new FileStream Stream
			=> (FileStream)base.Stream;

		public bool HasFileInfo {
			get {
				EnsureNotDisposeState();
				return _hasFileInfo;
			}
		}

		// TODO: Put strings into the resources.
		//
		public FileInfo FileInfo {
			get {
				if (HasFileInfo)
					return ReadDA(ref _fileInfo);
				else
					throw
						new EonException(message: $"Объект сведений о файле в данном компоненте отсутствует.{Environment.NewLine}\tКомпонент:{this.FmtStr().GI2()}");
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_fileInfo = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}