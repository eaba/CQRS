﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.DataStores;
using Cqrs.Entities;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Factories
{
	public interface IAzureDocumentDbDataStoreConnectionStringFactory
	{
		DocumentClient GetAzureDocumentDbConnectionClient();

		string GetAzureDocumentDbDatabaseName();

		string GetAzureDocumentDbCollectionName();

		/// <summary>
		/// Indicates if a different collection should be used per <see cref="IEntity"/>/<see cref="IDataStore{TData}"/> or a single collection used for all instances of <see cref="IDataStore{TData}"/> and <see cref="IDataStore{TData}"/>.
		/// Setting this to true can become expensive as each <see cref="IEntity"/> will have it's own collection. Check the relevant SDK/pricing models.
		/// </summary>
		bool UseSingleCollectionForAllDataStores();
	}
}