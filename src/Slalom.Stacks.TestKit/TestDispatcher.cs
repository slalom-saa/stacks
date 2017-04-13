﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Messaging.Registry;
using ExecutionContext = Slalom.Stacks.Messaging.ExecutionContext;

namespace Slalom.Stacks.TestKit
{
    public class TestDispatcher : LocalDispatcher, IRemoteMessageDispatcher
    {
        private Dictionary<Type, Action<object>> _endPoints = new Dictionary<Type, Action<object>>();
        private Dictionary<string, Action<Request>> _namedEndPoints = new Dictionary<string, Action<Request>>();

        public void UseEndPoint<T>(Action<T> action)
        {
            _endPoints.Add(typeof(T), a =>
            {
                action((T)a);
            });
        }


        public TestDispatcher(IComponentContext components) : base(components)
        {
        }

        public override Task<MessageResult> Dispatch(Request request, EndPointMetaData endPoint, ExecutionContext parentContext, TimeSpan? timeout = default(TimeSpan?))
        {
            if (_endPoints.ContainsKey(request.Message.MessageType))
            {
                var context = new ExecutionContext(request, endPoint, CancellationToken.None, parentContext);

                _endPoints[request.Message.MessageType](request.Message.Body);

                return Task.FromResult(new MessageResult(context));
            }

            if (_namedEndPoints.ContainsKey(endPoint.Path))
            {
                var context = new ExecutionContext(request, endPoint, CancellationToken.None, parentContext);

                _namedEndPoints[endPoint.Path](request);

                return Task.FromResult(new MessageResult(context));
            }

            return base.Dispatch(request, endPoint, parentContext, timeout);
        }

        public override Task<MessageResult> Dispatch(Request request, ExecutionContext context)
        {
            Console.WriteLine("Y");

            return base.Dispatch(request, context);
        }

        public void UseEndPoint(string path, Action<Request> action)
        {
            _namedEndPoints.Add(path, action);
        }

        public bool CanDispatch(Request request)
        {
            return true;
        }

        public Task<MessageResult> Dispatch(Request request, ExecutionContext parentContext, TimeSpan? timeout = null)
        {
            if (_endPoints.ContainsKey(request.Message.MessageType))
            {
                var context = new ExecutionContext(request, parentContext);

                _endPoints[request.Message.MessageType](request.Message.Body);

                return Task.FromResult(new MessageResult(context));
            }

            if (_namedEndPoints.ContainsKey(request.Path))
            {
                var context = new ExecutionContext(request, parentContext);

                _namedEndPoints[request.Path](request);

                return Task.FromResult(new MessageResult(context));
            }

            return base.Dispatch(request, parentContext);
        }
    }
}