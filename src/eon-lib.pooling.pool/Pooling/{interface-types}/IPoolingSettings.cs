using Eon.Description;
using Eon.Threading;

namespace Eon.Pooling {

	/// <summary>
	/// Basic pooling settings.
	/// </summary>
	public interface IPoolingSettings
		:IAsReadOnly<IPoolingSettings>, ISettings {

		string PoolDisplayName { get; set; }

		int? PoolSize { get; set; }

		TimeoutDuration PreferredSlidingTtl { get; set; }

	}

}