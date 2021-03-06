﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Slalom.Stacks.Services.Logging;
using Slalom.Stacks.Services.Messaging;

namespace Slalom.Stacks.Services.Pipeline
{
    /// <summary>
    /// The publish events step of the EndPoint execution pipeline.
    /// </summary>
    /// <seealso cref="Slalom.Stacks.Services.Pipeline.IMessageExecutionStep" />
    internal class PublishEvents : IMessageExecutionStep
    {
        private readonly IEventStore _eventStore;
        private readonly IMessageGateway _messageGateway;
        private readonly IEnumerable<IEventPublisher> _eventPublishers;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishEvents" /> class.
        /// </summary>
        /// <param name="components">The current component context.</param>
        public PublishEvents(IComponentContext components)
        {
            _messageGateway = components.Resolve<IMessageGateway>();
            _eventStore = components.Resolve<IEventStore>();
            _eventPublishers = components.ResolveAll<IEventPublisher>();
        }

        /// <inheritdoc />
        public async Task Execute(ExecutionContext context)
        {
            if (context.IsSuccessful)
            {
                var raisedEvents = context.RaisedEvents.Union(new[] {context.Response as EventMessage}).Where(e => e != null).ToArray();
                foreach (var instance in raisedEvents)
                {
                    await _eventStore.Append(instance);

                    await _messageGateway.Publish(instance, context);
                }

                await Task.WhenAll(_eventPublishers.Select(e => e.Publish(raisedEvents)));
            }
        }
    }
}