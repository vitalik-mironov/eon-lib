using Eon.Description.Tree;

namespace Eon.Runtime.Options {

	/// <summary>
	/// Defines EON runtime option of <see cref="DescriptionTreeFactory"/>.
	/// </summary>
	public sealed class DescriptionTreeFactoryOption
		:RuntimeOptionBase {

		#region Static members

		static DescriptionTreeFactoryOption() { }

		public static DescriptionTreeFactory Require()
			=> RuntimeOptions.Option<DescriptionTreeFactoryOption>.Require().Factory;

		#endregion

		readonly DescriptionTreeFactory _factory;

		public DescriptionTreeFactoryOption(DescriptionTreeFactory factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			_factory = factory;
		}

		public DescriptionTreeFactory Factory
			=> _factory;

	}

}