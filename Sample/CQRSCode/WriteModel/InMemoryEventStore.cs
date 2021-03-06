﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.WriteModel
{
	public class InMemoryEventStore : IEventStore<ISingleSignOnToken>
	{
		private readonly Dictionary<Guid, List<IEvent<ISingleSignOnToken>>> _inMemoryDb = new Dictionary<Guid, List<IEvent<ISingleSignOnToken>>>();

		public void Save(Type aggregateRootType, IEvent<ISingleSignOnToken> @event)
		{
			List<IEvent<ISingleSignOnToken>> list;
			_inMemoryDb.TryGetValue(@event.Id, out list);
			if (list == null)
			{
				list = new List<IEvent<ISingleSignOnToken>>();
				_inMemoryDb.Add(@event.Id, list);
			}
			list.Add(@event);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof(T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<ISingleSignOnToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			List<IEvent<ISingleSignOnToken>> events;
			_inMemoryDb.TryGetValue(aggregateId, out events);
			return events != null ? events.Where(x => x.Version > fromVersion) : new List<IEvent<ISingleSignOnToken>>();
		}

		public IEnumerable<EventData> Get(Guid correlationId)
		{
			return Enumerable.Empty<EventData>();
		}

		public void Save<T>(IEvent<ISingleSignOnToken> @event)
		{
			Save(typeof(T), @event);
		}
	}
}