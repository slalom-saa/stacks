using System;
using System.Diagnostics;

namespace Slalom.Stacks.Utilities.NewId.NewIdProviders
{
    internal class CurrentProcessIdProvider :
        IProcessIdProvider
    {
        public byte[] GetProcessId()
        {
            var processId = BitConverter.GetBytes(Process.GetCurrentProcess().Id);

            if(processId.Length < 2)
            {
                throw new InvalidOperationException("Current Process Id is of insufficient length");
            }

            return processId;
        }
    }
}