using System;
using System.Threading.Tasks;

using Quartz;
using SilkierQuartz;

namespace Epa.Camd.Easey.JobScheduler.Jobs
{
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello");
            return Task.CompletedTask;
        }
    }
}
