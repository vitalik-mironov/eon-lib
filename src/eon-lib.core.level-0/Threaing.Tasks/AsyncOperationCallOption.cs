﻿namespace Eon.Threading.Tasks {

	/// <summary>
	/// Defines an async operation option(s).
	/// </summary>
	public enum AsyncOperationCallOption {

		/// <summary>
		/// Транслируется результат уже существующего сеанса выполнения асинхронной операции.
		/// <para>Если сеанса нет, то запускается новый сеанс выполнения операции, транслируется его результат.</para>
		/// </summary>
		ExistingOrNew = 0,

		/// <summary>
		/// Если уже существует сеанс выполнения асинхронной операции, то текущий вызов операции должен быть отменён.
		/// <para>Если сеанса нет, то запускается новый сеанс выполнения операции, транслируется его результат.</para>
		/// </summary>
		CancelOrNew,

		/// <summary>
		/// Вызов асинхронной операции всегда должен запускать новый сеанс выполнения операции.
		/// </summary>
		New,

		/// <summary>
		/// Если уже существует сеанс выполнения асинхронной операции, то текущий вызов операции должен быть завершён, указывая в качестве результата значение по умолчанию (если применимо к операции).
		/// <para>Если сеанса нет, то запускается новый сеанс выполнения операции, транслируется его результат.</para>
		/// </summary>
		DefaultOrNew,

	}

}