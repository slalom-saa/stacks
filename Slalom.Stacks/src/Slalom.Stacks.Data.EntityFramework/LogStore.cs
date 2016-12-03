﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Slalom.Stacks.Communication;
using Slalom.Stacks.Communication.Logging;
using Slalom.Stacks.Runtime;

namespace Slalom.Stacks.EntityFramework
{
    public class LogStore : ILogStore
    {
        private readonly DbContext _context;

        public LogStore(DbContext context)
        {
            _context = context;

            //_context.Database.ExecuteSqlCommand(
        }

        public Task AppendAsync(ICommand command, ICommandResult result, ExecutionContext context)
        {
            _context.Add(new Log(command, result, context));

            return _context.SaveChangesAsync();
        }
    }
}