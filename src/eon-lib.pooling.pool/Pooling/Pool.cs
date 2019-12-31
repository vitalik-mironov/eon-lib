using System.Collections.Generic;
using Eon.Threading;

namespace Eon.Pooling {

	using IPool = IPool<object>;

	public static class Pool {

		/// <summary>
		/// Defines the default value of the pool item sliding time-to-live.
		/// <para>Value: max(<see cref="Leasable.MinTtl"/>, '00:30:00.0').</para>
		/// </summary>
		public static readonly TimeoutDuration DefaultOfItemPreferredSlidingTtl = Leasable.MinTtl.Max(b: TimeoutDuration.FromSeconds(seconds: 1800));

		/// <summary>
		/// Defines the default value of the pool max size.
		/// <para>Value: 64.</para>
		/// </summary>
		public static readonly int DefaultOfMaxSize = 64;

		/// <summary>
		/// Defines the default value of the pool item owning by the pool.
		/// <para>Value: <see langword="true"/>.</para>
		/// </summary>
		public static readonly bool DefaultOfOwnsItem = true;

		/// <summary>
		/// Defines the default value of the pool display name.
		/// <para>Value: <see langword="96"/>.</para>
		/// </summary>
		public static readonly int DisplayNameMaxLength = 96;

		static readonly DisposeRegistry __RegisteredPools;

		static Pool() {
			__RegisteredPools = new DisposeRegistry(cleanupThreshold: 32);
		}

		/// <summary>
		/// Gets alive pools that were registered with <see cref="Register(IPool{object})"/>.
		/// </summary>
		public static IReadOnlyList<IPool> GetRegisteredAlive()
			=> __RegisteredPools.GetAlive<IPool>();

		/// <summary>
		/// Registers the pool instance.
		/// </summary>
		/// <param name="pool">
		/// Pool.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static void Register(IPool pool) {
			pool.EnsureNotNull(nameof(pool));
			//
			__RegisteredPools.Register(disposable: pool);
		}

	}

}