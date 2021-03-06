﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using Autofac;
using Slalom.Stacks.Services.Inventory;

namespace Slalom.Stacks.Services
{
    /// <summary>
    /// EndPoint extention methods for Stacks.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Gets the local service inventory.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>Slalom.Stacks.Services.Inventory.ServiceInventory.</returns>
        public static ServiceInventory GetServices(this Stack instance)
        {
            return instance.Container.Resolve<ServiceInventory>();
        }
    }
}