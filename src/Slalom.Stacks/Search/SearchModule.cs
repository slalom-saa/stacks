﻿using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Slalom.Stacks.Configuration;
using Slalom.Stacks.Reflection;
using IComponentContext = Autofac.IComponentContext;
using Module = Autofac.Module;

namespace Slalom.Stacks.Search
{
    public class SearchModule : Module
    {
        public SearchModule(Assembly[] assemblies)
        {
            this.Assemblies = assemblies;
        }

        public Assembly[] Assemblies { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c => new NullSearchContext())
                   .As<ISearchContext>();

            builder.Register(c => new SearchFacade(new ComponentContext(c.Resolve<IComponentContext>())))
                   .As<ISearchFacade>();

            builder.RegisterAssemblyTypes(this.Assemblies)
                   .Where(e => e.GetBaseAndContractTypes().Any(x => x == typeof(ISearchIndexer<>)))
                   .As(e => typeof(ISearchIndexer<>).MakeGenericType(e.GetTypeInfo().BaseType.GetGenericArguments()[0]));
        }
    }
}