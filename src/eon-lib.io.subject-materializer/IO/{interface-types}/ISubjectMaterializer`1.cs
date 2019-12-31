using System;

using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon.IO {

	/// <summary>
	/// Материализатор какого-либо предмета (объекта).
	/// </summary>
	/// <typeparam name="TSubject">Ограничение типа материализуемого предмета (объекта).</typeparam>
	public interface ISubjectMaterializer<out TSubject>
		:IDisposable
		where TSubject : class {

		ITaskWrap<TSubject> MaterializeAsync(ISubjectLoadContext ctx);

	}

}