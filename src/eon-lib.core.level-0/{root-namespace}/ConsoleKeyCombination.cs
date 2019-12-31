using System;

namespace Eon {

	public readonly struct ConsoleKeyCombination
		:IEquatable<ConsoleKeyCombination> {

		#region Static members

		public static bool operator ==(ConsoleKeyCombination a, ConsoleKeyCombination b)
			=> a.Key == b.Key && a.Modifiers == b.Modifiers;

		public static bool operator !=(ConsoleKeyCombination a, ConsoleKeyCombination b)
			=> a.Key != b.Key && a.Modifiers != b.Modifiers;

		public static implicit operator ConsoleKeyCombination(ConsoleKeyInfo consoleKeyInfo)
			=> new ConsoleKeyCombination(key: consoleKeyInfo.Key, modifiers: consoleKeyInfo.Modifiers);

		#endregion

		public readonly ConsoleKey Key;

		public readonly ConsoleModifiers Modifiers;

		public ConsoleKeyCombination(ConsoleKey key, ConsoleModifiers modifiers) {
			Key = key;
			Modifiers = modifiers;
		}

		public bool Equals(ConsoleKeyCombination other)
			=> Key == other.Key && Modifiers == other.Modifiers;

		public override bool Equals(object other)
			=> other is ConsoleKeyCombination otherStricted ? Equals(other: otherStricted) : false;

		public override int GetHashCode()
			=> (BitConverter.IsLittleEndian ? ((int)Key << 3) : ((int)Key >> 3)) | (int)Modifiers;

		public override string ToString() {
			var result = string.Empty;
			if ((Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
				result = ConsoleModifiers.Control.ToString();
			if ((Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
				result += (result.Length > 0 ? " + " : string.Empty) + ConsoleModifiers.Shift.ToString();
			if ((Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
				result += (result.Length > 0 ? " + " : string.Empty) + ConsoleModifiers.Alt.ToString();
			var keyName = Enum.GetName(typeof(ConsoleKey), Key);
			if (!string.IsNullOrEmpty(keyName))
				result += (result.Length > 0 ? " + " : string.Empty) + keyName;
			return result;
		}

	}

}