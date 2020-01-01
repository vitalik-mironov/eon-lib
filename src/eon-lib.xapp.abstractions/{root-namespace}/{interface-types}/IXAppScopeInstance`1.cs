using System;

using Eon.Description;

namespace Eon {

	/// <summary>
	/// Компонент <seealso cref="IXInstance"/>, входящий в скоуп Eon-приложению <seealso cref="IXApp{TDescription}"/>.
	/// </summary>
	/// <typeparam name="TDescription"></typeparam>
	public interface IXAppScopeInstance<out TDescription>
		:IXAppScopeInstance, IXInstance<TDescription>
		where TDescription : class, IDescription {

		/// <summary>
		/// Возвращает описание (конфигурацию) данного компонента.
		/// <para>Необходимо учитывать, что для некоторых компонентов в зависимости от их реализации наличие описания не является обазательным. В этом случае данное свойство вместо возврата <see langword="null"/> при обращении к нему вызовет исключение <see cref="InvalidOperationException"/>. Поэтому перед обращением к данному свойтсву наличие описания необходимо проверять посредством свойства <see cref="IXInstance.HasDescription"/>.</para>
		/// </summary>
		new TDescription Description { get; }

	}

}