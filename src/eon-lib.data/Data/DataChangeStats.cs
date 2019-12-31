using System;

namespace Eon.Data {

	public readonly struct DataChangeStats
		:IEquatable<DataChangeStats> {

		#region Static members

		public static readonly DataChangeStats Zero = default;

		public static DataChangeStats operator +(DataChangeStats a, DataChangeStats b) {
			checked {
				return
					new DataChangeStats(
						inserted: a.Inserted + b.Inserted,
						updated: a.Updated + b.Updated,
						deleted: a.Deleted + b.Deleted);
			}
		}

		public static bool operator ==(DataChangeStats a, DataChangeStats b)
			=> a.Inserted == b.Inserted && a.Updated == b.Updated && a.Deleted == b.Deleted;

		public static bool operator !=(DataChangeStats a, DataChangeStats b)
			=> !(a.Inserted == b.Inserted && a.Updated == b.Updated && a.Deleted == b.Deleted);


		#endregion

		public readonly int Inserted;

		public readonly int Updated;

		public readonly int Deleted;

		// TODO: Put strings into the resources.
		//
		public DataChangeStats(int inserted, int updated) {
			inserted.Arg(nameof(inserted)).EnsureNotLessThanZero();
			updated.Arg(nameof(updated)).EnsureNotLessThanZero();
			//
			Inserted = inserted;
			Updated = updated;
			Deleted = 0;
		}

		public DataChangeStats(int inserted, int updated, int deleted) {
			inserted.Arg(nameof(inserted)).EnsureNotLessThanZero();
			updated.Arg(nameof(updated)).EnsureNotLessThanZero();
			deleted.Arg(nameof(deleted)).EnsureNotLessThanZero();
			//
			Inserted = inserted;
			Updated = updated;
			Deleted = deleted;
		}

		public override int GetHashCode() {
			if (this == default)
				return 0;
			else
				unchecked {
					var code = 17;
					code = code * 23 + Inserted.GetHashCode();
					code = code * 23 + Updated.GetHashCode();
					code = code * 23 + Deleted.GetHashCode();
					return code;
				}
		}

		public override bool Equals(object other)
			=> other is DataChangeStats otherDataChangeStats ? this == otherDataChangeStats : false;

		public bool Equals(DataChangeStats other)
			=> this == other;

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Создано:{Environment.NewLine}{Inserted.ToString("d").IndentLines()}"
			+ $"{Environment.NewLine}Обновлено:{Environment.NewLine}{Updated.ToString("d").IndentLines()}"
			+ $"{Environment.NewLine}Удалено:{Environment.NewLine}{Deleted.ToString("d").IndentLines()}";

	}

}