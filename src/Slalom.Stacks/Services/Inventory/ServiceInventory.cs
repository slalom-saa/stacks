﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Slalom.Stacks.Configuration;
using Slalom.Stacks.Reflection;
using Slalom.Stacks.Services.Messaging;

namespace Slalom.Stacks.Services.Inventory
{
    /// <summary>
    /// A service inventory containing available services.
    /// </summary>
    public class ServiceInventory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInventory"/> class.
        /// </summary>
        /// <param name="application">The application that contain the endpoints.</param>
        public ServiceInventory(Application application)
        {
            this.Application = application;
        }

        /// <summary>
        /// Gets the application that contain the endpoints.
        /// </summary>
        /// <value>
        /// The application that contain the endpoints.
        /// </value>
        public Application Application { get; }

        /// <summary>
        /// Gets the end points in the inventory.
        /// </summary>
        /// <value>The end points in the inventory.</value>
        public IEnumerable<EndPointMetaData> EndPoints => this.Hosts.SelectMany(e => e.Services).SelectMany(e => e.EndPoints);

        /// <summary>
        /// Gets the registered services.
        /// </summary>
        /// <value>The registered services.</value>
        public List<ServiceHost> Hosts { get; } = new List<ServiceHost>();

        /// <summary>
        /// Finds the endpoint for the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the endpoint for the specified message.</returns>
        public IEnumerable<EndPointMetaData> Find(object message)
        {
            if (message == null)
            {
                return Enumerable.Empty<EndPointMetaData>();
            }
            return this.Hosts.SelectMany(e => e.Services).SelectMany(e => e.EndPoints).Where(e => e.RequestType == message.GetType());
        }

        /// <summary>
        /// Finds the endpoint for the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Returns the endpoint for the specified path.</returns>
        public EndPointMetaData Find(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var endPoints = this.Hosts.SelectMany(e => e.Services).SelectMany(e => e.EndPoints).ToList();

            return endPoints.FirstOrDefault(e => $"v{e.Version}/{e.Path}" == path) ?? endPoints.Where(e => e.Path == path).OrderBy(e => e.Version).LastOrDefault();
        }

        /// <summary>
        /// Finds the endpoint for the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the endpoint for the specified message.</returns>
        public IEnumerable<EndPointMetaData> Find(EventMessage message)
        {
            return this.EndPoints.Where(e => e.RequestType.FullName == message.MessageType || e.EndPointType.GetAllAttributes<SubscribeAttribute>().Any());
        }

        /// <summary>
        /// Finds the endpoint that can handle the specified path.  If there is no path, then the message will be used.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="message">The message.</param>
        /// <returns>Returns the endpoint.</returns>
        public EndPointMetaData Find(string path, object message)
        {
            var target = this.Find(path);
            if (message != null)
            {
                if (target == null)
                {
                    target = this.Find(message).FirstOrDefault();
                }
                if (target == null)
                {
                    var attribute = message.GetType().GetAllAttributes<RequestAttribute>().FirstOrDefault();
                    if (attribute != null)
                    {
                        target = this.Find(attribute.Path);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// Registers all local services using the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to use to scan.</param>
        public void RegisterLocal(params Assembly[] assemblies)
        {
            var host = new ServiceHost();
            foreach (var service in assemblies.SafelyGetTypes(typeof(IEndPoint)).Distinct())
            {
                if (!service.GetTypeInfo().IsGenericType && !service.IsDynamic() && !service.GetTypeInfo().IsAbstract)
                {
                    host.Add(service);
                }
            }
            this.Hosts.Add(host);
        }
    }
}