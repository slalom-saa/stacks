﻿using System;
using System.Collections.Generic;
using Slalom.Stacks.Runtime;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Messaging.Validation
{
    /// <summary>
    /// Performs security validation on a command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public abstract class SecurityValidationRule<TCommand> : ISecurityValidationRule<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Gets the execution context.
        /// </summary>
        /// <value>The execution context.</value>
        public ExecutionContext Context { get; private set; }

        /// <summary>
        /// Validates the specified command instance.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A task for asynchronous programming.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="instance"/> argument is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> argument is null.</exception>
        public IEnumerable<ValidationError> Validate(TCommand instance, ExecutionContext context)
        {
            Argument.NotNull(instance, nameof(instance));
            Argument.NotNull(context, nameof(context));

            this.Context = context;

            return this.Validate(instance);
        }

        /// <summary>
        /// Validates the specified command instance.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>A task for asynchronous programming.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="instance"/> argument is null.</exception>
        protected abstract IEnumerable<ValidationError> Validate(TCommand instance);
    }
}