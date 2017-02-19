﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cqrs.Entities;
using Cqrs.DataStores;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	public abstract class Repository<TQueryStrategy, TQueryBuilder, TData> : IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity
	{
		protected Func<IDataStore<TData>> CreateDataStoreFunction { get; private set; }

		protected TQueryBuilder QueryBuilder { get; private set; }

		protected Repository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
		{
			CreateDataStoreFunction = createDataStoreFunction;
			QueryBuilder = queryBuilder;
		}

		#region Implementation of IRepository<TData>

		public virtual void Create(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		public virtual void Create(IEnumerable<TData> data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Add(data);
		}

		public virtual ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true)
		{
			// The .Select(i => i) is necessary due to inheritance
			// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
			IQueryable<TData> query = QueryBuilder.CreateQueryable(singleResultQuery).Select(i => i);

			IEnumerable<TData> result = query.AsEnumerable();

			try
			{
				return new SingleResultQuery<TQueryStrategy, TData>
				{
					QueryStrategy = singleResultQuery.QueryStrategy,
					Result = throwExceptionWhenNoQueryResults
						? result.Single()
						: result.SingleOrDefault()
				};
			}
			finally
			{
				// This is disabled until I can figure out a better way to handle disposing these... as it will most likely dispose the data store... and that's not cool.
				/*
				var disposable = result as IDisposable;
				if (disposable != null)
					disposable.Dispose();
				*/
			}
		}

		public virtual ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery)
		{
			// The .Select(i => i) is necessary due to inheritance
			// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
			IQueryable<TData> result = QueryBuilder.CreateQueryable(resultQuery).Select(i => i);

			try
			{
				return new CollectionResultQuery<TQueryStrategy, TData>
				{
					QueryStrategy = resultQuery.QueryStrategy,
					Result = result.ToList()
				};
			}
			finally
			{
				// This is disabled until I can figure out a better way to handle disposing these... as it will most likely dispose the data store... and that's not cool.
				/*
				var disposable = result as IDisposable;
				if (disposable != null)
					disposable.Dispose();
				*/
			}
		}

		public virtual void Update(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Update(data);
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public virtual void Delete(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Remove(data);
		}

		public virtual void DeleteAll()
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.RemoveAll();
		}

		public void Destroy(TData data)
		{
			using (var dataStore = CreateDataStoreFunction())
				dataStore.Destroy(data);
		}

		public virtual TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (IDataStore<TData> dataStore = CreateDataStoreFunction())
			{
				IEnumerable<TData> query = dataStore
					// The .Select(i => i) is necessary due to inheritance
					// http://stackoverflow.com/questions/1021274/linq-to-sql-mapping-exception-when-using-abstract-base-classes
					.Select(i => i)
					.Where(entity => entity.Rsn == rsn)
					.AsEnumerable();

				return throwExceptionOnMissingEntity
					? query.Single()
					: query.SingleOrDefault();
			}
		}

		#endregion

		protected virtual IQueryable<TData> CreateQueryable(Expression<Func<TData, bool>> predicate)
		{
			return CreateDataStoreFunction().Where(predicate);
		}
	}
}