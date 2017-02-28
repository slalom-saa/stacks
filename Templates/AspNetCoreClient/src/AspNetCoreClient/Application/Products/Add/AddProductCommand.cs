﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slalom.Stacks.Messaging;

namespace AspNetCoreClient.Application.Products.Add
{
    public class AddProductCommand : Command
    {
    }

    [Path("products/add")]
    public class AddProduct : UseCase<AddProductCommand>
    {
        public override void Execute(AddProductCommand command)
        {
            Console.WriteLine("Adding");
        }
    }
}
