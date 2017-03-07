﻿using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Slalom.Stacks.Configuration.Actors;
using Slalom.Stacks.ConsoleClient.Application.Products.Add;
using Slalom.Stacks.Logging;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Messaging.Serialization;
using Slalom.Stacks.Services;
using Slalom.Stacks.Services.Registry;
using Slalom.Stacks.TestKit;
using Slalom.Stacks.Text;

namespace Slalom.Stacks.ConsoleClient
{
    public class A : EndPoint<ProductAdded>
    {
        public override void Receive(ProductAdded command)
        {
            Console.WriteLine("...");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var stack = new Stack())
                {
                    stack.UseInMemoryPersistence();

                    stack.UseSimpleConsoleLogging();

                    stack.Send(new AddProductCommand("adf", "Adf")).Wait();

                    //Console.WriteLine(stack.Send("_systems/events").Result.ToJson());


                    Console.WriteLine("Complete");
                    Console.ReadKey();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}