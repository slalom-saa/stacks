﻿using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using System.Threading.Tasks;
using Slalom.Stacks.Domain;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Messaging.Events;
using Slalom.Stacks.Messaging.Pipeline;
using Slalom.Stacks.Search;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Services
{
    public abstract class SystemEndPoint<T, R> : Service, IEndPoint<T>
    {
        public abstract Task<R> Execute(T instance);

        public async Task Receive(T instance)
        {
            var result = await this.Execute(instance);

            ((IService)this).Context.Response = result;
        }
    }

    public interface IService
    {
        ExecutionContext Context { get; set; }

        Request Request { get; }
    }

    public abstract class Service : IService
    {
        /// <summary>
        /// Gets the configured <see cref="IDomainFacade"/> instance.
        /// </summary>
        /// <value>The configured <see cref="IDomainFacade"/> instance.</value>    
        public IDomainFacade Domain => this.Components.Resolve<IDomainFacade>();

        /// <summary>
        /// Gets the configured <see cref="ISearchFacade"/> instance.
        /// </summary>
        /// <value>The configured <see cref="ISearchFacade"/> instance.</value>
        public ISearchFacade Search => this.Components.Resolve<ISearchFacade>();

        /// <summary>
        /// Gets the configured <see cref="IComponentContext"/> instance.
        /// </summary>
        /// <value>The configured <see cref="IComponentContext"/> instance.</value>
        internal IComponentContext Components { get; set; }

        private ExecutionContext Context => ((IService)this).Context;

        public Request Request => ((IService)this).Context.Request;

        ExecutionContext IService.Context { get; set; }

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
        /// Sends the specified message.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="command">The command.</param>
        /// <returns>A task for asynchronous programming.</returns>
        protected Task<MessageResult> Send(string path, Command command)
        {
            var messages = this.Components.Resolve<IMessageGateway>();

            return messages.Send(path, command, this.Context);
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A task for asynchronous programming.</returns>
        protected Task<MessageResult> Send(Command command)
        {
            var messages = this.Components.Resolve<IMessageGateway>();

            return messages.Send(command, this.Context);
        }

        /// <summary>
        /// Completes the specified message.
        /// </summary>
        /// <returns>A task for asynchronous programming.</returns>
        internal virtual async Task Complete()
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
                await step.Execute(this.Request.Message, this.Context);
            }
        }

        /// <summary>
        /// Prepares the usecase for execution.
        /// </summary>
        /// <returns>A task for asynchronous programming.</returns>
        internal virtual async Task Prepare()
        {
            var steps = new List<IMessageExecutionStep>
            {
                this.Components.Resolve<LogStart>(),
                this.Components.Resolve<ValidateMessage>()
            };
            foreach (var step in steps)
            {
                await step.Execute(this.Request.Message, this.Context);
            }
        }
    }

    public abstract class UseCase<TCommand, TResult> : UseCase<TCommand>, IEndPoint<TCommand> where TCommand : Command where TResult : class
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
        async Task IEndPoint<TCommand>.Receive(TCommand instance)
        {
            await this.Prepare();

            var context = ((IService)this).Context;

            if (!context.ValidationErrors.Any())
            {
                try
                {
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        var result = await this.ExecuteAsync(instance);

                        context.Response = result;

                        if (result is Event)
                        {
                            this.AddRaisedEvent(result as Event);
                        }
                    }
                }
                catch (Exception exception)
                {
                    context.SetException(exception);
                }
            }

            await this.Complete();
        }
    }

    public abstract class UseCase<TCommand> : Service, IEndPoint<TCommand> where TCommand : Command
    {
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

        async Task IEndPoint<TCommand>.Receive(TCommand instance)
        {
            await this.Prepare();

            var context = ((IService)this).Context;

            if (!context.ValidationErrors.Any())
            {
                try
                {
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        await this.ExecuteAsync(instance);
                    }
                }
                catch (Exception exception)
                {
                    context.SetException(exception);
                }
            }

            await this.Complete();
        }
    }

    public abstract class EventUseCase<TCommand> : Service, IEndPoint<TCommand> where TCommand : Event
    {
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

        async Task IEndPoint<TCommand>.Receive(TCommand instance)
        {
            await this.Prepare();

            var context = ((IService)this).Context;

            if (!context.ValidationErrors.Any())
            {
                try
                {
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        await this.ExecuteAsync(instance);
                    }
                }
                catch (Exception exception)
                {
                    context.SetException(exception);
                }
            }

            await this.Complete();
        }
    }
}