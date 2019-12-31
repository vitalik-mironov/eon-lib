namespace Eon.Runtime.Options {

	public sealed class DotDirectoryNameOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '16'.
		/// </summary>
		public static readonly int MaxLengthOfName;

		/// <summary>
		/// Value: '.settings' (see <see cref="Name"/>).
		/// </summary>
		public static readonly DotDirectoryNameOption Fallback;

		static DotDirectoryNameOption() {
			MaxLengthOfName = 16;
			//
			Fallback = new DotDirectoryNameOption(name: ".settings");
			RuntimeOptions.Option<DotDirectoryNameOption>.SetFallback(option: Fallback);
		}

		public static string Require()
			=> RuntimeOptions.Option<DotDirectoryNameOption>.Require().Name;

		#endregion

		readonly string _name;

		public DotDirectoryNameOption(string name) {
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty().EnsureHasMaxLength(maxLength: MaxLengthOfName);
			//
			_name = name;
		}

		public string Name
			=> _name;

	}

}