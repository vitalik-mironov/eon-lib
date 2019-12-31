using System;
using System.Runtime.Serialization;

namespace Eon.Runtime.Serialization {

	public class MemberDeserializationException
		:SerializationException {

		// TODO: Put strings into the resources.
		//
		public MemberDeserializationException(string message, Type type, string memberName, Exception innerException)
			: base(message: $"Ошибка десериализации элемента '{memberName}' (тип объекта '{type}')." + (string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine + message), innerException: innerException) { }

		public MemberDeserializationException(string message, Type type, string memberName)
			: this(message: message, type: type, memberName: memberName, innerException: null) { }

		public MemberDeserializationException(Type type, string memberName, Exception innerException)
			: this(message: null, type: type, memberName: memberName, innerException: innerException) { }

	}

}