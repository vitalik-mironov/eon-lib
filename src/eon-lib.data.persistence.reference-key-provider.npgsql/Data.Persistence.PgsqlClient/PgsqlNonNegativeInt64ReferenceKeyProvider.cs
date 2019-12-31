#pragma warning disable CS3001 // Argument type is not CLS-compliant

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Data.Storage;
using Eon.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using static Eon.Transactions.TransactionUtilities;

namespace Eon.Data.Persistence.PgsqlClient {

	public class PgsqlNonNegativeInt64ReferenceKeyProvider
		:StorageNonNegativeInt64ReferenceKeyProviderBase {

		#region Static & constant members

		/// <summary>
		/// Value: '96'.
		/// </summary>
		static readonly int __EntityTypeIdMaxLength = 96;

		static async Task<TakeNextKeyResult> P_TryTakeNextKeyFromStoreAsync(IStorageNonNegativeInt64ReferenceKeyProviderSettings settings, int count, NpgsqlConnection storeConnection, Guid instanceUid, IContext ctx = default) {
			settings = settings.EnsureNotNull(nameof(settings)).AsReadOnly().EnsureValid().Value;
			storeConnection.EnsureNotNull(nameof(storeConnection));
			count.Arg(nameof(count)).EnsureNotLessThan(operand: 1);
			//
			var ct = ctx.Ct();
			var caughtException = default(Exception);
			var storeConnectionMustClosed = false;
			var tempTableName = $@"""{instanceUid.ToString(format: "n", provider: CultureInfo.InvariantCulture)}_temp_table""";
			var keysStorageObjectFullName = (settings.KeysStorageObjectSchemaName is null ? string.Empty : settings.KeysStorageObjectSchemaName + ".") + $@"""{settings.KeysStorageObjectName}""";
			var paramlessPrepareStatementTemplates =
				new List<string>(
					collection:
						new string[ ]{
							// Открываем транзакцию
							@"BEGIN;",
							// Внутри транзакции снова получаем id транзакции, если он был инкрементирован, значит действительно окружающие транзакции отсутствуют. Иначе, если statement 'begin' был вызван ранее - инкрементирование не произойдет
							@"DO
							$$
							DECLARE
								previous_txid xid := (SELECT txid_current FROM {1});
								current_txid xid := TXID_CURRENT();
							BEGIN
								IF previous_txid = current_txid THEN
									RAISE 'Absence of any open transaction is required to perform this operation.';
								END IF;
							END;
							$$ LANGUAGE plpgsql;"
			});
			var paramfulPrepareStatementTemplate =
				// Передаем в данный запрос параметры, они будут записаны в таблицу. Результат работы запроса так же будет помещен в данную таблицу.
				// ВАЖНО: имена параметров регистро-независимо связаны с именами передаваемых и получаемых параметров из кода clr. При правке параметров в данной команде, необходимо также изменить имена параметров в последующем коде. См. ниже '// Исполнение комманды с параметрами'.
				//
				@"DROP TABLE IF EXISTS {1};
				CREATE TEMPORARY TABLE {1} AS
					SELECT * FROM (
						VALUES (txid_current(), @storeNodeId::uuid, @entityTypeId::text, @keyValueFirst::bigint, @keyValueLast::bigint, @takeKeysCount::bigint, null::bigint, null::bigint, FALSE::bool))
							AS t (txid_current, storeNodeId, entityTypeId, keyValueFirst, keyValueLast, takeKeysCount, firstTakenKeyValue, lastTakenKeyValue, commit_txn);";
			var bodyStatementTemplate =
				@"DO
				$$
				DECLARE
					var_storeNodeId UUID = (SELECT t.storeNodeId FROM {1} as t);
					var_entityTypeId VARCHAR = (SELECT t.entityTypeId FROM {1} as t);
					var_keyValueFirst BIGINT = (SELECT t.keyValueFirst FROM {1} as t);
					var_keyValueLast BIGINT = (SELECT t.keyValueLast FROM {1} as t);
					var_takeKeysCount INT = (SELECT t.takeKeysCount FROM {1} as t);
					var_canTakeKeysCount INT;
					var_generatorStateRef BIGINT;
					var_previousKeyValue BIGINT; 
				BEGIN
					IF var_keyValueFirst IS NULL THEN
						RAISE 'No value for parameter ''@keyValueFirst''.';
					END IF;
					IF var_keyValueLast IS NULL THEN
						RAISE 'No value for parameter ''@keyValueLast''.';
					END IF;
					IF var_keyValueLast < var_keyValueFirst THEN
						RAISE 'No value for parameter ''@keyValueLast''.';
					END IF;
					IF var_takeKeysCount IS NULL THEN
						RAISE 'No value for parameter ''@takeKeysCount''.';
					END IF;
					IF var_takeKeysCount < 1 OR (var_takeKeysCount - 1) > var_keyValueLast - var_keyValueFirst THEN
						RAISE 'Invalid value of parameter ''@takeKeysCount''.';
					END IF;

					var_generatorStateRef = (SELECT
						T.""Ref""
					FROM
						{0} T
					WHERE
						T.""StoreNodeId"" = var_storeNodeId
						AND T.""EntityTypeId"" = var_entityTypeId);

					IF (var_generatorStateRef IS NULL) THEN
						INSERT INTO
							{0} (
								""Ref""
								,""StoreNodeId""
								,""EntityTypeId""
								,""KeyValueFirst""
								,""KeyValueLast""
								,""PreviousKeyValue"")
							VALUES (
								var_keyValueFirst
								,var_storeNodeId
								,var_entityTypeId
								,var_keyValueFirst
								,var_keyValueLast
								,var_keyValueFirst);
						var_generatorStateRef = var_keyValueFirst;
					ELSE
						IF (NOT EXISTS(
							SELECT
								NULL
							FROM
								{0} T
							WHERE
								T.""Ref"" = var_generatorStateRef
								AND T.""KeyValueFirst"" = var_keyValueFirst
								AND T.""KeyValueLast"" = var_keyValueLast))
						THEN
							RAISE 'Specified values of parameters ''@keyValueFirst'' and ''@keyValueLast'' are not match to the according values of store node row.';
						END IF;
					END IF;

					var_previousKeyValue = (
						SELECT
							""PreviousKeyValue""
						FROM
							{0}
						WHERE
							""Ref"" = var_generatorStateRef);

					var_canTakeKeysCount = (
						SELECT
							MIN(T.KeysCount)
						FROM (
							VALUES
								(var_takeKeysCount),
								(CASE
										WHEN var_previousKeyValue IS NULL THEN var_keyValueLast - var_keyValueFirst + 1
										ELSE var_keyValueLast - var_previousKeyValue
								 END))
							AS T (KeysCount));

					IF var_canTakeKeysCount > 1 THEN
						UPDATE {0} SET
							""PreviousKeyValue"" =
								CASE
									WHEN var_previousKeyValue IS NULL THEN var_keyValueFirst + (var_canTakeKeysCount - 1)
									ELSE var_previousKeyValue + var_canTakeKeysCount
								END
						WHERE
							""Ref"" = var_generatorStateRef;

						UPDATE {1} SET
								firstTakenKeyValue =
									CASE
										WHEN var_previousKeyValue IS NULL THEN var_keyValueFirst
										ELSE var_previousKeyValue + 1
									END,
								lastTakenKeyValue =
									CASE
										WHEN var_previousKeyValue IS NULL THEN var_keyValueFirst + (var_canTakeKeysCount - 1)
										ELSE var_previousKeyValue + var_canTakeKeysCount
									END,
								commit_txn = TRUE;
					END IF;
				END;
				$$ LANGUAGE plpgsql;";
			//
			try {
				if (storeConnection.State != ConnectionState.Open) {
					await storeConnection.OpenAsync(cancellationToken: ct).ConfigureAwait(false);
					storeConnectionMustClosed = true;
				}
				// Исполнение комманды с параметрами.
				//
				using (var storeCommand = createTextCommand()) {
					storeCommand.CommandText = string.Format(format: paramfulPrepareStatementTemplate, arg0: keysStorageObjectFullName, arg1: tempTableName);
					storeCommand
						.Parameters
						.AddRange(
							values:
								new NpgsqlParameter[ ] {
									new NpgsqlParameter() {
										ParameterName = "@storeNodeId",
										Direction = ParameterDirection.Input,
										Value = settings.ScopeUid.Value
									},
									new NpgsqlParameter() {
										ParameterName = "@entityTypeId",
										Direction = ParameterDirection.Input,
										Value = settings.EntityTypeId,
										Size = __EntityTypeIdMaxLength
									},
									new NpgsqlParameter() {
										ParameterName = "@keyValueFirst",
										Direction = ParameterDirection.Input,
										Value = settings.ScopeLowerKey.Value
									},
									new NpgsqlParameter() {
										ParameterName = "@keyValueLast",
										Direction = ParameterDirection.Input,
										Value = settings.ScopeUpperKey.Value
									},
									new NpgsqlParameter() {
										ParameterName = "@takeKeysCount",
										Direction = ParameterDirection.Input,
										Value = count
									}
								});
					await storeCommand.ExecuteNonQueryAsync(cancellationToken: ct).ConfigureAwait(false);
				}
				// Исполнение комманд без параметров.
				//
				foreach (var commandStatementText in paramlessPrepareStatementTemplates) {
					using (var storeCommand = createTextCommand()) {
						storeCommand.CommandText = string.Format(format: commandStatementText, arg0: keysStorageObjectFullName, arg1: tempTableName);
						await storeCommand.ExecuteNonQueryAsync(cancellationToken: ct).ConfigureAwait(false);
					}
				}
				// Исполнение тела (основной логики) запроса.
				//
				using (var storeCommand = createTextCommand()) {
					storeCommand.CommandText = string.Format(format: bodyStatementTemplate, arg0: keysStorageObjectFullName, arg1: tempTableName);
					await storeCommand.ExecuteNonQueryAsync(cancellationToken: ct).ConfigureAwait(false);
				}
				// Считываем результаты, сохраненные во временной таблице.
				//
				using (var storeCommand = createCommand(type: CommandType.TableDirect)) {
					storeCommand.CommandText = tempTableName;
					long? firstTakenKeyValue;
					long? lastTakenKeyValue;
					bool commit_txn;
					var reader = await storeCommand.ExecuteReaderAsync(cancellationToken: ct).ConfigureAwait(false);
					try {
						if (await reader.ReadAsync(cancellationToken: ct).ConfigureAwait(false)) {
							firstTakenKeyValue =
								reader[ nameof(firstTakenKeyValue) ] == DBNull.Value
								? throw new EonException($"Result table column '{nameof(firstTakenKeyValue)}' has invalid value '{nameof(DBNull.Value)}'.")
								: (long?)reader[ nameof(firstTakenKeyValue) ];
							lastTakenKeyValue =
								reader[ nameof(lastTakenKeyValue) ] == DBNull.Value
								? throw new EonException($"Result table column '{nameof(lastTakenKeyValue)}' has invalid value '{nameof(DBNull.Value)}'.") :
								(long?)reader[ nameof(lastTakenKeyValue) ];
							commit_txn =
								reader[ nameof(commit_txn) ] == DBNull.Value
								? throw new EonException($"Result table column '{nameof(commit_txn)}' has invalid value '{nameof(DBNull.Value)}'.")
								: (bool)reader[ nameof(commit_txn) ];
						}
						else
							throw new EonException(message: "Result table has no rows, but one expected.");
					}
					catch (IndexOutOfRangeException exception) {
						throw new EonException(message: "Result table has no required column.", innerException: exception);
					}
					reader.Close();
					if (commit_txn) {
						using (var storeCommand2 = createTextCommand()) {
							storeCommand2.CommandText = "COMMIT;";
							await storeCommand2.ExecuteNonQueryAsync(cancellationToken: ct).ConfigureAwait(false);
						}
						return new TakeNextKeyResult(hasTaken: true, lowerTakenKey: firstTakenKeyValue, upperTakenKey: lastTakenKeyValue);
					}
					else {
						using (var storeCommand2 = createTextCommand()) {
							storeCommand2.CommandText = "ROLLBACK;";
							await storeCommand2.ExecuteNonQueryAsync(cancellationToken: ct).ConfigureAwait(false);
						}
						return new TakeNextKeyResult(hasTaken: false, lowerTakenKey: null, upperTakenKey: null);
					}
				}
			}
			catch (Exception exception) {
				caughtException = new EonException(message: "An error has occurred while trying to take next reference key(s) from storage.", innerException: exception);
				throw caughtException;
			}
			finally {
				if (storeConnectionMustClosed)
					try {
						storeConnection.Close();
					}
					catch (Exception secondException) {
						throw new AggregateException(caughtException, secondException);
					}
			}
			//
			NpgsqlCommand createTextCommand()
				=> createCommand(type: CommandType.Text);
			NpgsqlCommand createCommand(CommandType type) {
				var command = storeConnection.CreateCommand();
				command.CommandType = type;
				command.CommandTimeout = settings.Storage.CommandExecutionTimeout.IsInfinite ? 0 : Math.Max(settings.Storage.CommandExecutionTimeout.Milliseconds / 1000, 1);
				return command;
			}
		}

		#endregion

		IServiceProvider _serviceProvider;

		readonly Guid _instanceUid;

		public PgsqlNonNegativeInt64ReferenceKeyProvider(IServiceProvider serviceProvider, IStorageNonNegativeInt64ReferenceKeyProviderSettings settings)
			: base(settings: settings) {
			serviceProvider.EnsureNotNull(nameof(serviceProvider));
			settings.EntityTypeId.ArgProp($"{nameof(settings)}.{nameof(settings.EntityTypeId)}").EnsureHasMaxLength(maxLength: __EntityTypeIdMaxLength);
			//
			_serviceProvider = serviceProvider;
			_instanceUid = Guid.NewGuid();
		}

		protected IServiceProvider ServiceProvider
			=> ReadDA(ref _serviceProvider);

		/// <summary>
		/// Creates the storage connection.
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		protected override async Task<DbConnection> CreateStorageConnectionAsync(IContext ctx = default) {
			ctx.ThrowIfCancellationRequested();
			var factory = ServiceProvider.GetRequiredService<IStorageDbConnectionFactory<NpgsqlConnection>>();
			return await factory.CreateAsync(arg: Settings.Storage, ctx: ctx).Unwrap().ConfigureAwait(false);
		}

		protected override sealed async Task<TakeNextKeyResult> TakeNextKeyAsync(DbConnection storeConnection, int count, IContext ctx = default) {
			var npgsqlConnection = storeConnection.EnsureNotNull(nameof(storeConnection)).EnsureOfType<DbConnection, NpgsqlConnection>().Value;
			//
			return await TakeNextKeyAsync(storeConnection: npgsqlConnection, count: count, ctx: ctx).ConfigureAwait(false);
		}

		protected virtual async Task<TakeNextKeyResult> TakeNextKeyAsync(NpgsqlConnection storeConnection, int count, IContext ctx = default) {
			storeConnection.EnsureNotNull(nameof(storeConnection));
			//
			TakeNextKeyResult result;
			using (var suppressTx = SuppressTx()) {
				result = await P_TryTakeNextKeyFromStoreAsync(settings: Settings, count: count, storeConnection: storeConnection, instanceUid: _instanceUid, ctx: ctx).ConfigureAwait(false);
				suppressTx.Complete();
			}
			return result;
		}

		protected override void Dispose(bool explicitDispose) {
			_serviceProvider = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}

#pragma warning restore CS3001 // Argument type is not CLS-compliant