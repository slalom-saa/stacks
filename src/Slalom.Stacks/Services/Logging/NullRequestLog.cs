﻿/* 
 * Copyright (c) Stacks Contributors
 * 
 * This file is subject to the terms and conditions defined in
 * the LICENSE file, which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slalom.Stacks.Services.Messaging;

namespace Slalom.Stacks.Services.Logging
{
    internal class NullRequestLog : IRequestLog
    {
        public Task Append(Request entry)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<RequestEntry>> GetEntries(DateTimeOffset? start, DateTimeOffset? end)
        {
            throw new NotImplementedException();
        }
    }
}