﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Slalom.Stacks.Messaging.Validation;

namespace Slalom.Stacks.Messaging.Pipeline.Steps
{
    public class ValidateMessage : IMessageExecutionStep
    {
        private readonly IComponentContext _context;

        public ValidateMessage(IComponentContext context)
        {
            _context = context;
        }

        public async Task Execute(IMessage message, MessageContext context)
        {
            if (message is ICommand)
            {
                var validator = (ICommandValidator)_context.Resolve(typeof(CommandValidator<>).MakeGenericType(message.GetType()));
                var results = await validator.Validate((ICommand)message);
                context.AddValidationErrors(results);
            }
        }
    }
}
