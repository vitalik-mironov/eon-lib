using System;
using System.Linq;
using Eon.ComponentModel.Dependencies;
using Eon.Linq;

namespace Eon.Data.Querying {

	/// <summary>
	/// Контекст запросов данных к к.л. источнику данных.
	/// </summary>
	public interface IDataQueryingContext
		:IDisposable, IDependencySupport {

		IQueryable CreateQuery(Type elementType);

		IQueryExecutionJob<T> CreateQueryExecutionJob<T>(IQueryable<T> query)
			where T : class;

		IQueryable<T> CreateQuery<T>()
			where T : class;

	}

}