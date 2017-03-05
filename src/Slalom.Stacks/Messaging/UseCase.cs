﻿using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Slalom.Stacks.Domain;
using Slalom.Stacks.Messaging.Pipeline;
using Slalom.Stacks.Search;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Messaging
{
    /// <summary>
    /// Defines a use case actor that performs a defined function.
    /// </summary>
    /// <typeparam name="TCommand">The type of message.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public abstract class UseCase<TCommand, TResult> : UseCase<TCommand>, IHandle where TCommand : class
    {
        /// <summary>
        /// Executes the use case given the specified message.
        /// </summary>
        /// <param name="command">The message containing the input.</param>
        /// <returns>The message result.</returns>
        public new virtual TResult Execute(TCommand command)
        {
            throw new NotImplementedException($"The execution methods for the {this.GetType().Name} use case actor have not been implemented.");
        }

        /// <summary>
        /// Executes the use case given the specified message.
        /// </summary>
        /// <param name="command">The message containing the input.</param>
        /// <returns>A task for asynchronous programming.</returns>
        public new virtual Task<TResult> ExecuteAsync(TCommand command)
        {
            return Task.FromResult(this.Execute(command));
        }

        /// <inheritdoc />
        async Task IHandle.Handle(IMessage instance)
        {
            this.Message = instance;

            await this.Prepare(instance);

            if (!this.Context.ValidationErrors.Any())
            {
                try
                {
                    var message = instance.Body as TCommand;
                    if (instance.Body is string)
                    {
                        message = JsonConvert.DeserializeObject<TCommand>((string)instance.Body);
                    }

                    if (!this.Context.CancellationToken.IsCancellationRequested)
                    {
                        var result = await this.ExecuteAsync(message);

                        this.Context.Response = result;
                    }
                }
                catch (Exception exception)
                {
                    this.Context.SetException(exception);
                }
            }

            await this.Complete(instance);
        }
    }

    /// <summary>
    /// Defines a use case actor that performs a defined function.
    /// </summary>
    /// <typeparam name="TCommand">The type of message.</typeparam>
    public abstract class UseCase<TCommand> : IHandle, IUseMessageContext where TCommand : class
    {
        /// <summary>
        /// Gets the configured <see cref="IDomainFacade"/> instance.
        /// </summary>
        /// <value>The configured <see cref="IDomainFacade"/> instance.</value>    
        public IDomainFacade Domain => this.Components.Resolve<IDomainFacade>();

        /// <summary>
        /// Gets the message that is being executed.
        /// </summary>
        /// <value>The message that is being executed.</value>
        public IMessage Message { get; protected internal set; }

        /// <summary>
        /// Gets the configured <see cref="ISearchFacade"/> instance.
        /// </summary>
        /// <value>The configured <see cref="ISearchFacade"/> instance.</value>
        public ISearchFacade Search => this.Components.Resolve<ISearchFacade>();

        /// <summary>
        /// Gets the user making the request.
        /// </summary>
        /// <value>The user making the request.</value>
        public IPrincipal User => this.Context.Request.User;

        /// <summary>
        /// Gets the configured <see cref="IComponentContext"/> instance.
        /// </summary>
        /// <value>The configured <see cref="IComponentContext"/> instance.</value>
        internal IComponentContext Components { get; set; }

        /// <summary>
        /// Gets the current <see cref="MessageExecutionContext"/> instance.
        /// </summary>
        /// <value>The current <see cref="MessageExecutionContext"/> instance.</value>
        internal MessageExecutionContext Context { get; private set; }

        /// <summary>
        /// Adds the raised event that will fire on completion.
        /// </summary>
        /// <param name="instance">The instance to raise.</param>
        public void AddRaisedEvent(Event instance)
        {
            Argument.NotNull(instance, nameof(instance));

            this.Context.AddRaisedEvent(instance);
        }

        /// <summary>
        /// Executes the use case given the specified message.
        /// </summary>
        /// <param name="command">The message containing the input.</param>
        /// <returns>The message result.</returns>
        public virtual void Execute(TCommand command)
        {
            throw new NotImplementedException($"The execution methods for the {this.GetType().Name} use case actor have not been implemented.");
        }

        /// <summary>
        /// Executes the use case given the specified message.
        /// </summary>
        /// <param name="command">The message containing the input.</param>
        /// <returns>A task for asynchronous programming.</returns>
        public virtual Task ExecuteAsync(TCommand command)
        {
            this.Execute(command);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task for asynchronous programming.</returns>
        protected Task<MessageResult> Send(object message)
        {
            var stream = this.Components.Resolve<IMessageGateway>();

            return stream.Send(message, this.Context);
        }

        /// <summary>
        /// Completes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task for asynchronous programming.</returns>
        internal async Task Complete(IMessage message)
        {
            var steps = new List<IMessageExecutionStep>
            {
                this.Components.Resolve<HandleException>(),
                this.Components.Resolve<Complete>(),
                this.Components.Resolve<PublishEvents>(),
                this.Components.Resolve<LogCompletion>()
            };
            foreach (var step in steps)
            {
                await step.Execute(message, this.Context);
            }
        }

        /// <summary>
        /// Prepares the usecase for execution.
        /// </summary>
        /// <param name="message">The current message.</param>
        /// <returns>A task for asynchronous programming.</returns>
        internal async Task Prepare(IMessage message)
        {
            var steps = new List<IMessageExecutionStep>
            {
                this.Components.Resolve<LogStart>(),
                this.Components.Resolve<ValidateMessage>()
            };
            foreach (var step in steps)
            {
                await step.Execute(message, this.Context);
            }
        }

        /// <inheritdoc />
        async Task IHandle.Handle(IMessage instance)
        {
            this.Message = instance;

            await this.Prepare(instance);

            if (!this.Context.ValidationErrors.Any())
            {
                try
                {
                    var message = instance.Body as TCommand;
                    if (instance.Body is string)
                    {
                        message = JsonConvert.DeserializeObject<TCommand>((string)instance.Body);
                    }
                    if (!this.Context.CancellationToken.IsCancellationRequested)
                    {
                        await this.ExecuteAsync(message);
                    }
                }
                catch (Exception exception)
                {
                    this.Context.SetException(exception);
                }
            }

            await this.Complete(instance);
        }

        /// <inheritdoc />
        void IUseMessageContext.UseContext(MessageExecutionContext context)
        {
            this.Context = context;
        }
    }
}