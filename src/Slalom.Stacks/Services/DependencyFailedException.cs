﻿using System;
using System.Linq;
using Slalom.Stacks.Services.Messaging;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Services
{
    /// <summary>
    /// Exception that should be raised when the execution of a dependency fails.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyFailedException" /> class.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="dependency">The dependency call result.</param>
        public DependencyFailedException(Request request, MessageResult dependency)
            : base($"Failed to complete request {request.Message.Id} because of a failed dependent request {dependency.RequestId}.", dependency.RaisedException ?? new ValidationException(dependency.ValidationErrors.ToArray()))
        {
            this.Request = request;
            this.Dependency = dependency;
        }

        /// <summary>
        /// Gets the dependency execution result.
        /// </summary>
        /// <value>The dependency execution result.</value>
        public MessageResult Dependency { get; }

        /// <summary>
        /// Gets the current request.
        /// </summary>
        /// <value>The current request.</value>
        public Request Request { get; }
    }
}