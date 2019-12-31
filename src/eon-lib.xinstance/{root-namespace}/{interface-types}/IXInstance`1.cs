using System;
using Eon.Description;

namespace Eon {

	/// <summary>
	/// Определяет программный интерфейс компонента <seealso cref="IXInstance"/>, тип описания (конфигурации) которого ограничен типом <typeparamref name="TDescription"/>.
	/// </summary>
	/// <typeparam name="TDescription">Ограничение типа описания (конфигурации) <seealso cref="IDescription"/> компонента.</typeparam>
	public interface IXInstance<out TDescription>
		:IXInstance
		where TDescription : class, IDescription {

		/// <summary>
		/// Возвращает описание (конфигурацию) данного компонента.
		/// <para>Необходимо учитывать, что для некоторых компонентов в зависимости от их реализации наличие описания не является обазательным. В этом случае данное свойство вместо возврата null при обращении к нему вызовет исключение <see cref="InvalidOperationException"/>. Поэтому перед обращением к данному свойтсву наличие описания необходимо проверять посредством свойства <see cref="IXInstance.HasDescription"/>.</para>
		/// </summary>
		new TDescription Description { get; }

	}

}