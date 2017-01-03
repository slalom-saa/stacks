﻿using System;
using Slalom.Stacks.Runtime;

namespace Slalom.Stacks.Messaging
{
    /// <summary>
    /// An imperative message to perform an action.
    /// </summary>
    /// <seealso href="http://bit.ly/2d01rc7">Reactive Messaging Patterns with the Actor Model: Applications and Integration in Scala and Akka</seealso>
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Gets the command type.
        /// </summary>
        /// <value>The command type.</value>
        Type Type { get; }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        /// <value>The command name.</value>
        string CommandName { get; }

        /// <summary>
        /// Gets the current execution context.
        /// </summary>
        /// <value>The current execution context.</value>
        ExecutionContext Context { get; }

        /// <summary>
        /// Sets the current execution context.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        void SetExecutionContext(ExecutionContext context);
    }
}