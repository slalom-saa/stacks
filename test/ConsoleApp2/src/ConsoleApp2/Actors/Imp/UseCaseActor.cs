using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Slalom.Stacks.Actors.Imp.Messages;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Domain;
using Slalom.Stacks.Validation;
using Akka.DI.Core;

namespace Slalom.Stacks.Actors
{
    public class DomainActor : ReceiveActor
    {
        public IDomainFacade Domain { get; set; }

        public DomainActor(IDomainFacade domain)
        {
            this.Domain = domain;
            this.ReceiveAsync<AddMessage>(this.HandleAdd);
        }

        public class AddMessage
        {
            public IEnumerable<IAggregateRoot> Items { get; set; }

            public AddMessage(IEnumerable<IAggregateRoot> items)
            {
                this.Items = items;
            }
        }

        private async Task HandleAdd(AddMessage message)
        {
            await this.Domain.AddAsync(message.Items);

            this.Sender.Tell("");
        }
    }

    public abstract class UseCaseActor<TCommand, TResult> : ReceiveActor where TCommand : ICommand
    {
        private IActorRef _domain;

        protected UseCaseActor()
        {
            this.ReceiveAsync<ExecuteUseCaseMessage>(this.HandleExecute);
        }

        protected override void PreStart()
        {
            base.PreStart();

            _domain = Context.ActorOf(Context.DI().Props<DomainActor>(), "domain");
        }

        protected async Task AddAync(IAggregateRoot aggregate)
        {
            await _domain.Ask(new DomainActor.AddMessage(new[] { aggregate }));

            this.Sender.Tell("");
        }

        private async Task HandleExecute(ExecuteUseCaseMessage message)
        {
            var errors = this.Validate((TCommand)message.Command).ToList();
            errors.AddRange(await this.ValidateAsync((TCommand)message.Command));
            if (errors.Any())
            {
                this.Sender.Tell(new UseCaseExecutionFailedMessage(message.Command, message.Caller, errors));
            }
            else
            {
                var target = await this.ExecuteAsync((TCommand)message.Command);
                this.Sender.Tell(new UseCaseExecutionSucceededMessage(message.Command, message.Caller, target));
            }
        }

        public virtual Task<TResult> ExecuteAsync(TCommand command)
        {
            return Task.FromResult(this.Execute(command));
        }

        public virtual TResult Execute(TCommand command)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<ValidationError> Validate(TCommand command)
        {
            yield break;
        }

        public virtual Task<IEnumerable<ValidationError>> ValidateAsync(TCommand command)
        {
            return Task.FromResult(Enumerable.Empty<ValidationError>());
        }
    }
}