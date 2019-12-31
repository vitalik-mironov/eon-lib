using System;

namespace Eon.Context {

	/// <summary>
	/// Base type of context (see <see cref="IContext"/>) property.
	/// </summary>
	public abstract class ContextProperty {

		#region Static & constant members

		// TODO: Put strings into the resources.
		//
		protected static readonly Exception MissingValueSentinel = new EonException(message: "Value of the specified property is not defined for this context.");

		protected static readonly Exception ContextDisposeSentinel = DisposableUtilities.NewObjectDisposedException(disposable: null, disposeRequestedException: false, disposableName: $"{typeof(IContext).FullName}");

		#endregion

		readonly string _name;

		readonly Type _type;

		protected ContextProperty(string name, Type type) {
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			type.EnsureNotNull(nameof(type));
			//
			_name = name;
			_type = type;
		}

		public string Name
			=> _name;

		public Type Type
			=> _type;

		public override string ToString()
			=> $"{_type.Name} {_name}";

	}

}