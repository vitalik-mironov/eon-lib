namespace Eon.Text {

	public interface IStringBuilderBufferSetSettings
		:IAsReadOnly<IStringBuilderBufferSetSettings>, IValidatable {

		int? BufferMaxCapacity { get; set; }

		int? BufferMinCapacity { get; set; }

		int? BufferSetMaxSize { get; set; }

	}

}