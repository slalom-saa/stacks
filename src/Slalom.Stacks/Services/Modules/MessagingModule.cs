﻿using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Autofac;
using Slalom.Stacks.Services.Logging;
using Slalom.Stacks.Services.Pipeline;
using Slalom.Stacks.Services.Validation;
using Slalom.Stacks.Reflection;
using Slalom.Stacks.Services.Inventory;
using Slalom.Stacks.Services.Messaging;
using Slalom.Stacks.Validation;
using Module = Autofac.Module;

namespace Slalom.Stacks.Services.Modules
{
    /// <summary>
    /// An Autofac module to configure the communication dependencies.
    /// </summary>
    /// <seealso cref="Autofac.Module" />
    internal class MessagingModule : Module
    {
        private readonly Stack _stack;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingModule" /> class.
        /// </summary>
        public MessagingModule(Stack stack)
        {
            _stack = stack;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">
        /// The builder through which components can be
        /// registered.
        /// </param>
        /// <remarks>Note that the ContainerBuilder parameter is unique to this module.</remarks>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c => new MessageGateway(c.Resolve<IComponentContext>()))
                .As<IMessageGateway>()
                .SingleInstance();

            builder.RegisterType<LocalDispatcher>().As<ILocalMessageDispatcher>();
            builder.RegisterType<RemoteEndPointInventory>().AsSelf().SingleInstance();

            builder.RegisterType<InMemoryEventStore>().As<IEventStore>().SingleInstance();

            builder.RegisterAssemblyTypes(_stack.Assemblies.Union(new[] { typeof(IMessageExecutionStep).GetTypeInfo().Assembly }).ToArray())
                .Where(e => e.GetInterfaces().Any(x => x == typeof(IMessageExecutionStep)))
                .AsSelf();

            builder.Register(c => new ServiceInventory())
                .AsSelf()
                .SingleInstance()
                .OnActivated(e => { e.Instance.RegisterLocal(_stack.Assemblies.ToArray()); });

            builder.Register(c => new Request())
                .As<IRequestContext>();

            builder.RegisterType<InMemoryRequestLog>().As<IRequestLog>().SingleInstance();
            builder.RegisterType<InMemoryResponseLog>().As<IResponseLog>().SingleInstance();
            builder.RegisterType<InMemoryEventStore>().As<IEventStore>().SingleInstance();

            builder.RegisterGeneric(typeof(MessageValidator<>));

            this.RegisterAssemblyTypes(builder, this._stack.Assemblies.ToArray());

            _stack.Assemblies.CollectionChanged += this.HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _stack.Use(builder =>
            {
                this.RegisterAssemblyTypes(builder, e.NewItems.OfType<Assembly>().ToArray());
            });
        }

        private void RegisterAssemblyTypes(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies)
                   .Where(e => e.GetBaseAndContractTypes().Any(x => x == typeof(IValidate<>)))
                   .As(instance => instance.GetBaseAndContractTypes())
                   .AllPropertiesAutowired();

            builder.RegisterAssemblyTypes(assemblies)
                   .Where(e => e.GetBaseAndContractTypes().Any(x => x == typeof(IEndPoint<>) || x == typeof(IEndPoint<,>)))
                   .AsBaseAndContractTypes().AsSelf()
                   .AllPropertiesAutowired();

            builder.RegisterAssemblyTypes(assemblies)
                   .Where(e => e.GetInterfaces().Contains(typeof(IEventPublisher)))
                   .As<IEventPublisher>().AsSelf().SingleInstance();

        }
    }
}