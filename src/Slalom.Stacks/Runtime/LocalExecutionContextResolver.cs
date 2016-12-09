﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading;
using Microsoft.Extensions.Configuration;

#if !core
using System.Runtime.Remoting.Messaging;
#endif

namespace Slalom.Stacks.Runtime
{
    /// <summary>
    /// Resolves the message execution context containing information at the current time in processing. This information is otherwise lost
    /// when processing is multi-threaded or distributed.
    /// </summary>
    /// <seealso cref="Slalom.Stacks.Runtime.IExecutionContextResolver" />
    public class LocalExecutionContextResolver : IExecutionContextResolver
    {
        private const string Key = "CorrelationId";

        private static Guid session = Guid.NewGuid();
        private readonly IConfiguration _configuration;
        private readonly LocalExecutionContext _context;

#if core
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalExecutionContextResolver"/> class.
        /// </summary>
        public LocalExecutionContextResolver()
        {
            _context = new LocalExecutionContext();
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalExecutionContextResolver"/> class.
        /// </summary>
        public LocalExecutionContextResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalExecutionContextResolver"/> class.
        /// </summary>
        /// <param name="context">The context to return.</param>
        public LocalExecutionContextResolver(LocalExecutionContext context)
        {
            _context = context;
        }

#if !core
        private Guid GetCorrelationId()
        {
            if (CallContext.GetData(Key) == null)
            {
                var id = Guid.NewGuid();
                CallContext.SetData(Key, id);
            }
            return new Guid(CallContext.GetData(Key).ToString());
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
#else
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
#endif

        /// <summary>
        /// Resolves the current execution context.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public ExecutionContext Resolve()
        {
#if !core

            return new LocalExecutionContext(_configuration["Application"],
                _configuration["Environment"], this.GetLocalIPAddress(),
                "",
                this.GetCorrelationId().ToString(),
                session.ToString(),
                new ClaimsPrincipal(Thread.CurrentPrincipal.Identity), this.GetLocalIPAddress(),
                Environment.MachineName,
                Environment.CurrentManagedThreadId);
#else
            return new LocalExecutionContext(_configuration["Application"],
                _configuration["Environment"], this.GetLocalIPAddress(),
                "",
                Guid.NewGuid().ToString(),
                session.ToString(),
                ClaimsPrincipal.Current, this.GetLocalIPAddress(),
                Environment.MachineName,
                Environment.CurrentManagedThreadId);
#endif
        }
    }
}