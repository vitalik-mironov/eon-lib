using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Globalization;
using Eon.Runtime.Serialization;
using Eon.Threading;
using Eon.Threading.Tasks;

using Newtonsoft.Json;

using static Eon.Resources.XResource.XResourceUtilities;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Reflection {

	/// <summary>
	/// Представляет немутабельный объект ссылки на какой-либо тип по его полному имени.
	/// </summary>
	[DataContract(Name = DataContractName, Namespace = Xml.Schema.EonXmlNamespaces.Core)]
	[DebuggerDisplay("{ToString()}")]
	public class TypeNameReference
		:IEquatable<TypeNameReference>, IFormattable {

		#region Nested types

		sealed class P_ResolvedState {

			readonly Type _type;

			readonly Task<Type> _typeAsCompletedTask;

			readonly ILocalizable<string> _singularTextView;

			internal P_ResolvedState(Type type, ILocalizable<string> singularTextView) {
				type.EnsureNotNull(nameof(type));
				singularTextView.EnsureNotNull(nameof(singularTextView));
				//
				_type = type;
				_typeAsCompletedTask = Task.FromResult(result: type);
				_singularTextView = singularTextView;
			}

			public Type Type => _type;

			public Task<Type> TypeAsCompletedTask => _typeAsCompletedTask;

			public ILocalizable<string> SingularTextView => _singularTextView;

		}

		#endregion

		#region Static & constant members

		public const string DataContractName = "TypeNameReference";

		public const string SerializationStringRegexPattern = @"^(assembly=[^\;\n]+\;[\s]*type=[^\;\n]+\;?)$";

		public const int SerializationStringMaxLength = PartMaxLength * 2 + 16;

		public const string PartRegexPattern = @"^([^\s]+.*[^\s]+)$|^([^\s]?)$";

		/// <summary>
		/// Значение: '1024'.
		/// </summary>
		public const int PartMaxLength = 1024;

		/// <summary>
		/// Значение: '2050'.
		/// </summary>
		public const int NameMaxLength = PartMaxLength * 2 + 2;

		/// <summary>
		/// Значение: <see cref="EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiter"/>.
		/// </summary>
		public const char PartDelimiter = EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiter;

		/// <summary>
		/// Значение: <seealso cref="EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiterString"/>.
		/// </summary>
		public const string PartDelimiterString = EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiterString;

		public static readonly Regex SerializationStringRegex = new Regex(pattern: SerializationStringRegexPattern, options: RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		public static readonly Regex PartRegex = new Regex(pattern: PartRegexPattern, options: RegexOptions.Compiled);

		public static bool operator ==(TypeNameReference a, TypeNameReference b)
			=> Equals(a, b);

		public static bool operator !=(TypeNameReference a, TypeNameReference b)
			=> !Equals(a, b);

		public static bool Equals(TypeNameReference a, TypeNameReference b) {
			if (ReferenceEquals(objA: a, objB: b))
				return true;
			else if (a is null)
				return false;
			else
				return a.Equals(b);
		}

		public static implicit operator TypeNameReference(Type type)
			=> type is null ? null : new TypeNameReference(target: type);

		static int P_EvaluateHashCode(string assemblyName, string typeFullName) {
			if (string.IsNullOrEmpty(assemblyName))
				throw new ArgumentException(FormatXResource(typeof(string), "CanNotNullOrEmpty"), nameof(assemblyName));
			else if (string.IsNullOrEmpty(typeFullName))
				throw new ArgumentException(FormatXResource(typeof(string), "CanNotNullOrEmpty"), nameof(typeFullName));
			else if (assemblyName.Length > PartMaxLength)
				throw new ArgumentException(FormatXResource(typeof(string), "TooLong/MaxLength", PartMaxLength.ToString("d")), nameof(assemblyName));
			else if (typeFullName.Length > PartMaxLength)
				throw new ArgumentException(FormatXResource(typeof(string), "TooLong/MaxLength", PartMaxLength.ToString("d")), nameof(typeFullName));
			var charBuffer = new char[ assemblyName.Length + typeFullName.Length + 2 ];
			assemblyName.CopyTo(0, charBuffer, 0, assemblyName.Length);
			charBuffer[ assemblyName.Length ] = ',';
			charBuffer[ assemblyName.Length + 1 ] = ' ';
			typeFullName.CopyTo(0, charBuffer, assemblyName.Length + 2, typeFullName.Length);
			return Encoding.UTF8.GetBytes(charBuffer, 0, charBuffer.Length).EvaluateHashCode();
		}

		static ILocalizable<string> P_GetSingularText(Type type, string defaultText) {
			type.EnsureNotNull(nameof(type));
			//
			return new InvariantLocalizable<string>(value: defaultText ?? string.Empty);
		}

		public static void ParseSerializationString(ArgumentUtilitiesHandle<string> value, out string assembly, out string type) {
			value.EnsureNotNull().EnsureNotEmpty().EnsureHasMaxLength(maxLength: SerializationStringMaxLength).EnsureMatchRegex(regex: SerializationStringRegex);
			//
			var valueString = value.Value;
			var index1 = valueString.IndexOf(value: '=');
			var index2 = valueString.IndexOf(value: ';', startIndex: index1 + 1);
			var index3 = valueString.IndexOf(value: '=', startIndex: index2);
			var locAssembly = valueString.Substring(startIndex: index1 + 1, index2 - index1 - 1);
			var locType = valueString.Substring(startIndex: index3 + 1, valueString.Length - (index3 + 1) - (valueString[ valueString.Length - 1 ] == ';' ? 1 : 0));
			assembly = locAssembly;
			type = locType;
		}

		#endregion

		string _assembly;
		[DataMember(Order = 0, Name = nameof(Assembly), IsRequired = false, EmitDefaultValue = false)]
		string P_Assembly_DataMember {
			get => _serializationString is null ? _assembly : null;
			set {
				if (!(value is null)) {
					if (!(_serializationString is null))
						throw new MemberDeserializationException(message: "Ожидалось значение null.", type: GetType(), memberName: nameof(Assembly));
					try {
						P_SetAssembly(value.Arg(nameof(value)));
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(type: GetType(), memberName: nameof(Assembly), innerException: exception);
					}
				}
			}
		}

		string _type;
		[DataMember(Order = 1, Name = nameof(Type), IsRequired = false, EmitDefaultValue = false)]
		string P_Type_DataMember {
			get => _serializationString is null ? _type : null;
			set {
				if (!(value is null)) {
					if (!(_serializationString is null))
						throw new MemberDeserializationException(message: "Ожидалось значение null.", type: GetType(), memberName: nameof(Type));
					try {
						P_SetType(value.Arg(nameof(value)));
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(GetType(), nameof(Type), exception);
					}
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		string _serializationString;
		[DataMember(Order = 2, Name = "SerializationString", IsRequired = false, EmitDefaultValue = false)]
		string P_SerializationString_DataMember {
			get => _serializationString;
			set {
				if (!(value is null)) {
					if (!(_assembly is null && _type is null))
						throw new MemberDeserializationException(message: "Ожидалось значение null.", type: GetType(), memberName: "SerializationString");
					try {
						ParseSerializationString(value: value.Arg(nameof(value)), assembly: out var assembly, type: out var type);
						P_SetAssembly(assembly: assembly.Arg(nameof(assembly)));
						P_SetType(type: type.Arg(nameof(type)));
						_serializationString = value;
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(type: GetType(), memberName: "SerializationString", innerException: exception);
					}
				}
			}
		}

		readonly Type _constructedFromTarget;

		DisposableLazy<P_ResolvedState> _state;

		int _hashCode;

		public TypeNameReference(Type target) {
			target.EnsureNotNull(nameof(target));
			//
			InspectTarget(target);
			_constructedFromTarget = target;
			P_SetAssembly(target.GetTypeInfo().Assembly.FullName.Arg(nameof(target)));
			P_SetType(target.FullName.Arg(nameof(target)));
		}

		public TypeNameReference(string type, string assembly) {
			P_SetType(type: type.Arg(nameof(type)));
			P_SetAssembly(assembly: assembly.Arg(nameof(assembly)));
		}

		public TypeNameReference(string serializationString) {
			ParseSerializationString(value: serializationString.Arg(nameof(serializationString)), assembly: out var assembly, type: out var type);
			P_SetAssembly(assembly: assembly.Arg(nameof(assembly)));
			P_SetType(type: type.Arg(nameof(type)));
			_serializationString = serializationString;
		}

		[JsonConstructor]
		protected TypeNameReference(SerializationContext ctx) { }

		public string Assembly
			=> _assembly;

		public string Type
			=> _type;

		public string Name
			=> $"{_type}{PartDelimiterString} {_assembly}";

		void P_SetAssembly(ArgumentUtilitiesHandle<string> assembly) {
			assembly.EnsureNotNullOrWhiteSpace().EnsureHasMaxLength(maxLength: PartMaxLength).EnsureMatchRegex(regex: PartRegex);
			//
			_assembly = assembly.Value;
		}

		void P_SetType(ArgumentUtilitiesHandle<string> type) {
			type.EnsureNotNullOrWhiteSpace().EnsureHasMaxLength(maxLength: PartMaxLength).EnsureMatchRegex(regex: PartRegex);
			//
			_type = type.Value;
		}

		protected virtual void InspectTarget(Type target)
			=> target.EnsureNotNull(nameof(target));

		public Type Resolve() {
			var state = itrlck.Get(ref _state);
			if (state is null)
				return ResolveAsync().WaitResultWithTimeout();
			else
				return state.Value.Type;
		}

		public Task<Type> ResolveAsync(IContext ctx = default) {
			try {
				var state = itrlck.Get(ref _state);
				if (state is null) {
					var newState = default(DisposableLazy<P_ResolvedState>);
					try {
						newState =
							new DisposableLazy<P_ResolvedState>(
								factory:
									() => {
										// TODO: Put exception messages into the resources.
										//
										Type locType;
										ILocalizable<string> locResolvedTypeSingularText;
										try {
											if (_constructedFromTarget is null) {
												locType = EonTypeUtilitiesCoreL2.GetType(typeName: Name, throwOnError: true);
												InspectTarget(locType);
											}
											else
												locType = _constructedFromTarget;
											locResolvedTypeSingularText = P_GetSingularText(type: locType, defaultText: P_GetDefaultText());
										}
										catch (Exception locException) {
											throw
												new EonException(message: $"Ошибка разрешения ссылки на тип.{Environment.NewLine}\tСсылка:{this.FmtStr().GNLI2()}", innerException: locException);
										}
										return new P_ResolvedState(locType, locResolvedTypeSingularText);
									});
						//
						state = itrlck.UpdateIfNull(ref _state, newState);
					}
					finally {
						if (!ReferenceEquals(newState, state))
							newState?.Dispose();
					}
				}
				return state.Value.TypeAsCompletedTask;
			}
			catch (Exception exception) {
				return Task.FromException<Type>(exception: exception);
			}
		}

		public sealed override string ToString()
			=> ToString(string.Empty, null);

		public virtual bool Equals(TypeNameReference other) {
			if (ReferenceEquals(other, null))
				return false;
			else
				return GetHashCode() == other.GetHashCode();
		}

		public sealed override bool Equals(object obj)
			=> Equals(obj as TypeNameReference);

		public override int GetHashCode() {
			var hashCode = VolatileUtilities.Read(ref _hashCode);
			if (hashCode == 0)
				VolatileUtilities.Write(ref _hashCode, hashCode = P_EvaluateHashCode(_assembly, _type));
			return hashCode;
		}

		public virtual string ToString(string format, IFormatProvider formatProvider) {
			if (string.IsNullOrEmpty(format))
				return P_GetDefaultText();
			else if (format == FormatStringUtilities.ShortFormatSpecifier || format == FormatStringUtilities.LongFormatSpecifier) {
				var state = itrlck.Get(ref _state);
				if (state is null || !state.IsValueCreated) {
					if (_constructedFromTarget is null)
						return P_GetDefaultText();
					else
						return P_GetSingularText(_constructedFromTarget, P_GetDefaultText()).GetForCulture(formatProvider as CultureInfo);
				}
				return state.Value.SingularTextView.GetForCulture(culture: formatProvider as CultureInfo);
			}
			throw new ArgumentException(message: FormatXResource(locator: typeof(FormatException), subpath: "ExceptionMessages/InvalidFormatSpecifier", args: format), paramName: nameof(format));
		}

		string P_GetDefaultText() => Name;

		// TODO: Put strings into the resources.
		//
		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			if (_assembly is null)
				throw
					new SerializationException(message: $"Ожидалось, что свойство '{nameof(Assembly)}' после десериализации будет иметь значение, отличное от null.{Environment.NewLine}\tТип объекта:{GetType().ToString().FmtStr().GNLI2()}");
			else if (_type is null)
				throw
					new SerializationException(message: $"Ожидалось, что свойство '{nameof(Type)}' после десериализации будет иметь значение, отличное от null.{Environment.NewLine}\tТип объекта:{GetType().ToString().FmtStr().GNLI2()}");
			OnDeserialized(context: context);
		}

		protected virtual void OnDeserialized(StreamingContext context) { }

	}

}