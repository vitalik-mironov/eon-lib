using System;
using System.Diagnostics;

namespace Eon.Metadata.Tree {


	//[DebuggerStepThrough]
	//[DebuggerNonUserCode]
	public static class MetadataTreeElementUtilities {

		// TODO: Put strings into the resources.
		//
		public static TMetadata EnsureHasMetadataOfType<TMetadata>(this IMetadataTreeElement element)
			where TMetadata : class, IMetadata {
			element.EnsureNotNull(nameof(element));
			//
			var elementMetadata = element.Metadata;
			if (elementMetadata == null)
				throw new EonException($"Элемент дерева метаданных '{element}' не связан с объектом метаданных.");
			var elementMetadataAsTResult = elementMetadata as TMetadata;
			if (elementMetadataAsTResult == null)
				throw new EonException($"Тип объекта метаданных элемента дерева метаданных не совместим с требуемым типом '{typeof(TMetadata)}' (элемент дерева метаданных '{element}' связан с объектом метаданных типа '{elementMetadata.GetType()}').");
			else
				return elementMetadataAsTResult;
		}

	}

}