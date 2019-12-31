
namespace Eon.Net.Http {

	public interface IHttpMessageQuotas
		:IAsReadOnly<IHttpMessageQuotas>, IValidatable {

		int? MaxContentLength { get; set; }

	}

}