﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System.Collections.Generic;
using Slalom.Stacks.Services.Messaging;

namespace Slalom.Stacks.Caching
{
    /// <summary>
    /// A message containing information about what was changed in the cache.
    /// </summary>
    /// <seealso cref="IMessage" />
    public class CacheUpdatedMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheUpdatedMessage" /> class.
        /// </summary>
        /// <param name="keysUpdated">The keys that were updated.</param>
        public CacheUpdatedMessage(IEnumerable<string> keysUpdated)
            : base(keysUpdated)
        {
        }

        /// <summary>
        /// Gets the keys updated.
        /// </summary>
        /// <value>The keys updated.</value>
        public IEnumerable<string> KeysUpdated => this.Body as IEnumerable<string>;
    }
}