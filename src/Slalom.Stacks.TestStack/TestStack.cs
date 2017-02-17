﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using Slalom.Stacks.Domain;
using Slalom.Stacks.Messaging;
using System.Linq;

namespace Slalom.Stacks.TestStack
{
    public class TestStack : Stack, IHandleEvent
    {
        public readonly List<IEvent> RaisedEvents = new List<IEvent>();


        public TestStack(object instance = null, [CallerMemberName] string callerName = "")
            : base(typeof(TestStack))
        {
            this.Use(builder =>
            {
                builder.RegisterInstance(this).As<IHandleEvent>();
            });

            if (instance != null && callerName != null)
            {
                var method = instance.GetType().GetTypeInfo().GetMethod(callerName);
                var attribute = method.GetCustomAttributes<GivenAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    var scenario = (Scenario)Activator.CreateInstance(attribute.Name);
                    this.UseScenario(scenario);
                }
            }
        }

        public Task HandleAsync(IEvent instance)
        {
            RaisedEvents.Add(instance);

            return Task.FromResult(0);
        }

        public CommandResult Send(ICommand command)
        {
            return SendAsync(command).Result;
        }

        public void UseScenario(Scenario scenario)
        {
            this.Use(builder =>
            {
                builder.RegisterInstance(scenario.EntityContext).As<IEntityContext>();
            });
        }

        public void UseScenario(Type scenario)
        {
            this.Use(builder =>
            {
                var instance = Activator.CreateInstance(scenario) as Scenario;
                builder.RegisterInstance(instance.EntityContext).As<IEntityContext>();
            });
        }
    }
}