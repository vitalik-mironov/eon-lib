using Eon.Description;

namespace Eon.MessageFlow.Local.Description {

	public interface IXAppLocalPublisherDescription
		:IDescription {

		bool UseDefaultSettings { get; set; }

		ILocalPublisherSettings CustomSettings { get; set; }

	}

}