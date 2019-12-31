using Eon.Description;

namespace Eon {

	/// <summary>
	/// Определяет программный интерфейс сводки состояния компонента <see cref="IXInstance"/>.
	/// <para>В общем случае используется для логирования.</para>
	/// </summary>
	public interface IXInstanceStateSummary {

		IDescriptionSummary Description { get; }

		XInstanceStates State { get; }

	}

}