﻿
using System;

namespace Eon.ComponentModel {

	/// <summary>
	/// Определяет опции работы управляющего элемента запуска/перезапуска/остановки компонента (<see cref="IRunControl"/>).
	/// </summary>
	public enum RunControlOptions {

		/// <summary>
		/// Опции осутствуют.
		/// </summary>
		None = 0,

		/// <summary>
		/// Режим многократного запуска (перезапуска).
		/// <para>Должен использоваться с компонентом, который реализует переходы: запуск -> остановка -> запуск -> остановка -> ... -> запуск -> остановка.</para>
		/// </summary>
		MultipleStart = 1,

		/// <summary>
		/// Режим однократного запуска.
		/// <para>Должен использоваться с компонентом, который реализует переход: запуск -> остановка.</para>
		/// </summary>
		SingleStart = 2,

		/// <summary>
		/// Авто-запуск операции остановки при поступлении сигнала от токена остановки (если таковой был явно указан).
		/// </summary>
		AutoStopOnTokenSignal = 4,

		/// <summary>
		/// Опция, запрещающая выполнение операции запуска.
		/// <para>При попытке запуска будет вызвано исключение <see cref="InvalidOperationException"/>.</para>
		/// </summary>
		ForbidStart = 8

	}

}