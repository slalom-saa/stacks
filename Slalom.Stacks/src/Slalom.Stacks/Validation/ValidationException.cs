﻿using System;
using System.Linq;

namespace Slalom.Stacks.Validation
{
    /// <summary>
    /// The exception that is raised when a validation error is found.
    /// </summary>
    /// <seealso cref="System.InvalidOperationException" />
    public class ValidationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="messages">The validation messages to add to the exception.</param>
        public ValidationException(params ValidationError[] messages)
            : base(string.Join(Environment.NewLine, messages.Select(e => e.Message)))
        {
            this.ValidationMessages = messages;
        }

        /// <summary>
        /// Gets the validation messages.
        /// </summary>
        /// <value>The validation messages.</value>
        public ValidationError[] ValidationMessages { get; private set; }
    }
}