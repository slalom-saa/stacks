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
using Autofac;
using Newtonsoft.Json;
using Slalom.Stacks.Services.Inventory;
using Slalom.Stacks.Services.Pipeline;

namespace Slalom.Stacks.Services.Messaging
{
    /// <summary>
    /// A local <see cref="ILocalMessageDispatcher" /> implementation.
    /// </summary>
    /// <seealso cref="ILocalMessageDispatcher" />
    public class LocalDispatcher : ILocalMessageDispatcher
    {
        private readonly IComponentContext _components;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDispatcher" /> class.
        /// </summary>
        /// <param name="components">The configured <see cref="IComponentContext" />.</param>
        public LocalDispatcher(IComponentContext components)
        {
            _components = components;
        }

        /// <inheritdoc />
        public virtual async Task<MessageResult> Dispatch(Request request, EndPointMetaData endPoint, ExecutionContext parentContext, TimeSpan? timeout = null)
        {
            CancellationTokenSource source;
            if (timeout.HasValue || endPoint.Timeout.HasValue)
            {
                source = new CancellationTokenSource(timeout ?? endPoint.Timeout.Value);
            }
            else
            {
                source = new CancellationTokenSource();
            }

            var context = new ExecutionContext(request, endPoint, source.Token, parentContext);

            var handler = _components.Resolve(endPoint.ServiceType);
            var service = handler as IEndPoint;
            if (service != null)
            {
                service.Context = context;
            }

            var body = request.Message.Body;
            var parameterType = endPoint.Method.GetParameters().First().ParameterType;
            if (body == null || body.GetType() != parameterType)
            {
                body = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(body ?? ""), parameterType);
            }

            await (Task) endPoint.Method.Invoke(handler, new[] {body});

            await this.Complete(context);

            return new MessageResult(context);
        }

        /// <summary>
        /// Runs steps to complete the current execution context.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <returns>Returns a task for asynchronous programming.</returns>
        protected virtual async Task Complete(ExecutionContext context)
        {
            var steps = new List<IMessageExecutionStep>
            {
                _components.Resolve<HandleException>(),
                _components.Resolve<Complete>(),
                _components.Resolve<PublishEvents>(),
                _components.Resolve<LogCompletion>()
            };
            foreach (var step in steps)
            {
                await step.Execute(context);
            }
        }
    }
}