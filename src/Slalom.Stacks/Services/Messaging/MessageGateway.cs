﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Slalom.Stacks.Reflection;
using Slalom.Stacks.Services.Inventory;
using Slalom.Stacks.Services.Logging;
using Slalom.Stacks.Utilities.NewId;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Services.Messaging
{
    /// <summary>
    /// A default <see cref="IMessageGateway" /> implementation.
    /// </summary>
    public class MessageGateway : IMessageGateway
    {
        private readonly Lazy<IRequestRouter> _dispatcher;
        private readonly Lazy<IEnumerable<IRemoteRouter>> _dispatchers;
        private readonly Lazy<IRequestContext> _requestContext;
        private readonly Lazy<IRequestLog> _requests;
        private readonly Lazy<ServiceInventory> _services;
        private readonly Lazy<IEnumerable<IEventPublisher>> _publishers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageGateway" /> class.
        /// </summary>
        /// <param name="components">The components.</param>
        public MessageGateway(IComponentContext components)
        {
            Argument.NotNull(components, nameof(components));

            _services = new Lazy<ServiceInventory>(components.Resolve<ServiceInventory>);
            _requestContext = new Lazy<IRequestContext>(components.Resolve<IRequestContext>);
            _requests = new Lazy<IRequestLog>(components.Resolve<IRequestLog>);
            _dispatcher = new Lazy<IRequestRouter>(components.Resolve<IRequestRouter>);
            _dispatchers = new Lazy<IEnumerable<IRemoteRouter>>(components.ResolveAll<IRemoteRouter>);
            _publishers = new Lazy<IEnumerable<IEventPublisher>>(components.ResolveAll<IEventPublisher>);
        }

        /// <inheritdoc />
        public virtual async Task Publish(EventMessage instance, ExecutionContext context)
        {
            Argument.NotNull(instance, nameof(instance));

            var request = _requestContext.Value.Resolve(instance, context.Request);
            await this.LogRequest(request);

            var endPoints = _services.Value.Find(instance);
            foreach (var endPoint in endPoints)
            {
                if (endPoint.InvokeMethod.GetParameters().FirstOrDefault()?.ParameterType == instance.MessageType)
                {
                    await _dispatcher.Value.Route(request, endPoint, context);
                }
                else
                {
                    var attribute = endPoint.EndPointType.GetAllAttributes<SubscribeAttribute>().FirstOrDefault();
                    if (attribute != null)
                    {
                        if (attribute.Channel == instance.Name)
                        {
                            await _dispatcher.Value.Route(request, endPoint, context);
                        }
                    }
                }
            }

            foreach (var publisher in _publishers.Value)
            {
                await publisher.Publish(instance);
            }
        }

        /// <inheritdoc />
        public async Task Publish(IEnumerable<EventMessage> instances, ExecutionContext context = null)
        {
            Argument.NotNull(instances, nameof(instances));

            foreach (var item in instances)
            {
                await this.Publish(item, context);
            }
        }

        /// <inheritdoc />
        public void Publish(string channel, string message)
        {
            EventMessage current;
            if (message.StartsWith("{"))
            {
                var instance = JsonConvert.DeserializeObject<JObject>(message);

                var requestId = instance["requestId"]?.Value<string>() ?? NewId.NextId();
                var body = instance["body"]?.ToObject<object>() ?? instance;

                current = new EventMessage(requestId, body);
            }
            else
            {
                current = new EventMessage(NewId.NextId(), message);
            }

            foreach (var endPoint in _services.Value.EndPoints.Where(e => e.EndPointType.GetAllAttributes<SubscribeAttribute>().Any(x => x.Channel == channel)))
            {
                var request = _requestContext.Value.Resolve(current, endPoint);

                _dispatcher.Value.Route(request, endPoint, null);
            }

            foreach (var publisher in _publishers.Value)
            {
                publisher.Publish(current);
            }
        }

        /// <inheritdoc />
        public void Publish(Event instance)
        {
            var current = new EventMessage(NewId.NextId(), instance);

            foreach (var endPoint in _services.Value.EndPoints.Where(e => e.EndPointType.GetAllAttributes<SubscribeAttribute>().Any(x => x.Channel == current.MessageType.FullName.Split('.').Last())))
            {
                var request = _requestContext.Value.Resolve(current, endPoint);

                _dispatcher.Value.Route(request, endPoint, null);
            }

            foreach (var publisher in _publishers.Value)
            {
                publisher.Publish(current);
            }
        }

        /// <inheritdoc />
        public Task<MessageResult> Send(object message, ExecutionContext parentContext = null, TimeSpan? timeout = null)
        {
            return this.Send(null, message, parentContext, timeout);
        }

        /// <inheritdoc />
        public Task<MessageResult> Send(string path, ExecutionContext parentContext = null, TimeSpan? timeout = null)
        {
            return this.Send(path, null, parentContext, timeout);
        }

        /// <inheritdoc />
        public virtual async Task<MessageResult> Send(string path, object instance, ExecutionContext parentContext = null, TimeSpan? timeout = null)
        {
            var endPoint = _services.Value.Find(path, instance);
            if (endPoint != null)
            {
                var request = _requestContext.Value.Resolve(instance, endPoint, parentContext?.Request);
                await this.LogRequest(request);
                return await _dispatcher.Value.Route(request, endPoint, parentContext, timeout);
            }
            else
            {
                var request = _requestContext.Value.Resolve(path, instance, parentContext?.Request);
                await this.LogRequest(request);
                var dispatcher = _dispatchers.Value.FirstOrDefault(e => e.CanRoute(request));
                if (dispatcher != null)
                {
                    return await dispatcher.Route(request, parentContext, timeout);
                }
            }

            var current = _requestContext.Value.Resolve(path, instance);
            var context = new ExecutionContext(current, null);
            context.SetException(new EndPointNotFoundException(current));
            return new MessageResult(context);
        }

        /// <inheritdoc />
        public virtual async Task<MessageResult> Send(string path, string command, ExecutionContext parentContext = null, TimeSpan? timeout = null)
        {
            var endPoint = _services.Value.Find(path);
            if (endPoint != null)
            {
                var request = _requestContext.Value.Resolve(command, endPoint, parentContext?.Request);
                await this.LogRequest(request);
                return await _dispatcher.Value.Route(request, endPoint, parentContext, timeout);
            }
            else
            {
                var request = _requestContext.Value.Resolve(path, command, parentContext?.Request);
                await this.LogRequest(request);
                var dispatcher = _dispatchers.Value.FirstOrDefault(e => e.CanRoute(request));
                if (dispatcher != null)
                {
                    return await dispatcher.Route(request, parentContext, timeout);
                }
            }

            var current = _requestContext.Value.Resolve(path, command);
            var context = new ExecutionContext(current, null);
            context.SetException(new EndPointNotFoundException(current));
            return new MessageResult(context);
        }

        private async Task LogRequest(Request request)
        {
            if (request.Path?.StartsWith("_") == false)
            {
                await _requests.Value.Append(request);
            }
        }
    }
}