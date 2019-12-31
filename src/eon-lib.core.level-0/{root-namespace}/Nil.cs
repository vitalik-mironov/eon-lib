using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon {

	/// <summary>
	/// Void (nothing, null) marker.
	/// </summary>
	[DataContract]
	public sealed class Nil
		:IEquatable<Nil> {

		#region Static members

		public static readonly Nil Value = new Nil();

		public static readonly Type Type = typeof(Nil);

		public static bool operator ==(Nil a, Nil b)
			=> (a is null) ? (b is null) : !(b is null);

		public static bool operator !=(Nil a, Nil b)
			=> (a is null) ? !(b is null) : (b is null);

		#endregion

		[JsonConstructor]
		Nil() { }

		public bool Equals(Nil other)
			=> !(other is null);

		public override int GetHashCode()
			=> int.MaxValue;

		public override bool Equals(object other)
			=> Equals(other: other as Nil);

	}

}