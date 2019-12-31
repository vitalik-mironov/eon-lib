using System;
using System.Diagnostics;

namespace Eon.Metadata {

	//[DebuggerStepThrough]
	//[DebuggerNonUserCode]
	public struct MetadataPropertyHandle<TMetadata, TPropertyValue>
	where TMetadata : class, IMetadata {

		readonly bool _isHandleValid;

		public readonly TMetadata Metadata;

		public readonly string PropertyName;

		public readonly TPropertyValue PropertyValue;

		public MetadataPropertyHandle(TMetadata metadata, string propertyName, TPropertyValue propertyValue) {
			metadata.EnsureNotNull(nameof(metadata));
			propertyName
				.Arg(nameof(propertyName))
				.EnsureNotEmptyOrWhiteSpace();
			//
			Metadata = metadata;
			PropertyName = propertyName;
			PropertyValue = propertyValue;
			_isHandleValid = true;
		}

		// TODO: Put strings into the resources.
		//
		public void EnsureHandleValid() {
			if (!_isHandleValid)
				throw new EonException($"Данный дескриптор свойства метаданных не валиден.{Environment.NewLine}\tДескриптор:{this.FmtStr().GNLI2()}");
		}

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Тип:{Environment.NewLine}{GetType().FmtStr().G().IndentLines()}"
			+ $"{Environment.NewLine}Метаданные:{Metadata.FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Свойство:{PropertyName.FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Значение свойства:{PropertyValue.FmtStr().GNLI()}";

	}

}