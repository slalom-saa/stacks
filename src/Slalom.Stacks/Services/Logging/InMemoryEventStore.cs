﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Slalom.Stacks.Configuration;
using Slalom.Stacks.Services.Messaging;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Services.Logging
{
    internal class InMemoryEventStore : IEventStore
    {
        private readonly Application _environment;

        /// <summary>
        /// The lock for the instances.
        /// </summary>
        protected readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The in-memory items.
        /// </summary>
        protected readonly List<EventEntry> Instances = new List<EventEntry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStore" /> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public InMemoryEventStore(Application environment)
        {
            _environment = environment;
        }

        public Task<IEnumerable<EventEntry>> GetEvents(DateTimeOffset? start, DateTimeOffset? end)
        {
            CacheLock.EnterReadLock();
            try
            {
                start = start ?? DateTimeOffset.Now.LocalDateTime.AddDays(-1);
                end = end ?? DateTimeOffset.Now.LocalDateTime;
                return Task.FromResult(Instances.Where(e => e.TimeStamp >= start && e.TimeStamp <= end).AsEnumerable());
            }
            finally
            {
                CacheLock.ExitReadLock();
            }
        }

        public Task Append(EventMessage instance)
        {
            Argument.NotNull(instance, nameof(instance));

            CacheLock.EnterWriteLock();
            try
            {
                Instances.Add(new EventEntry(instance, _environment));
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
            return Task.FromResult(0);
        }
    }
}