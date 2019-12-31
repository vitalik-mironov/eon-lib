using Eon.Triggers.Description;

namespace Eon.Triggers {

	/// <summary>
	/// XInstance-компонент триггера (см. <seealso cref="ITrigger"/>).
	/// </summary>
	/// <typeparam name="TDescription">Тип описания (конфигурации) компонента.</typeparam>
	public interface ITrigger<out TDescription>
		:IXAppScopeInstance<TDescription>, ITrigger
		where TDescription : class, ITriggerDescription { }

}