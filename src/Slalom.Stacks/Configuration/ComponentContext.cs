﻿using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Slalom.Stacks.Validation;

namespace Slalom.Stacks.Configuration
{
    /// <summary>
    /// Context that contains current components from the container.
    /// </summary>
    /// <seealso cref="Slalom.Stacks.Configuration.IComponentContext" />
    internal class ComponentContext : IComponentContext
    {
        private readonly Autofac.IComponentContext _context;
        private IPropertySelector _selector = new AllUnsetPropertySelector();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentContext"/> class.
        /// </summary>
        /// <param name="context">The configured <see cref="Autofac.IComponentContext"/> instance.</param>
        /// <exception>Thrown when the <paramref name="context"/> argument is null.</exception>
        internal ComponentContext(Autofac.IComponentContext context)
        {
            Argument.NotNull(context, nameof(context));

            _context = context;
        }

        /// <summary>
        /// Resolves an instance of the specified type from the context.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The resolved instance.</returns>
        /// <exception>Thrown when the <paramref name="type"/> argument is null.</exception>
        public object Resolve(Type type)
        {
            object instance;

            if (!_context.TryResolve(type, out instance))
            {
                if (!type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface)
                {
                    instance = Activator.CreateInstance(type);
                }
            }

            if (instance != null)
            {
                _context.InjectProperties(instance, _selector);
            }

            return instance;
        }

        /// <summary>
        /// Resolves an instance of the specified type from the context.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved instance.</returns>
        public T Resolve<T>()
        {
            T instance;

            if (!_context.TryResolve(out instance))
            {
                if (!typeof(T).GetTypeInfo().IsAbstract && !typeof(T).GetTypeInfo().IsInterface)
                {
                    instance = Activator.CreateInstance<T>();
                }
            }

            if (instance != null)
            {
                _context.InjectProperties(instance, _selector);
            }

            return instance;
        }

        public T BuildUp<T>(T instance)
        {
            _context.InjectProperties(instance, _selector);

            return instance;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _context.Resolve<IEnumerable<T>>();
        }

        /// <summary>
        /// Resolves all instance of the specified type from the container.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The resolved instances.</returns>
        /// <exception>Thrown when the <paramref name="type"/> argument is null.</exception>
        public IEnumerable<object> ResolveAll(Type type)
        {
            var target = (IEnumerable<object>)_context.Resolve(typeof(IEnumerable<>).MakeGenericType(type));

            foreach (var instance in target)
            {
                _context.InjectProperties(instance, _selector);
            }

            return target;
        }
    }
}