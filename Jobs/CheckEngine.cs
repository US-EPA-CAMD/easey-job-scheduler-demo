using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Quartz;

using Epa.Camd.Easey.RulesApi.Models;

namespace Epa.Camd.Easey.JobScheduler.Jobs
{
  public class CheckEngine : IJob
  {
    private NpgSqlContext _dbContext = null;

    public CheckEngine(NpgSqlContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      await Console.Out.WriteLineAsync($"Check Engine checking submission queue for work...");
      List<CheckQueue> submissions = _dbContext.Submissions.Where(item => item.StatusCode == "Submitted").ToList();

      if (submissions.Count > 0)
        await Console.Out.WriteLineAsync($"...scheduling {submissions.Count} submissions immediately");
      else
        await Console.Out.WriteLineAsync($"...no submissions to schedule");

      foreach(var item in submissions)
      {
        item.StatusCode = "Processing";
        _dbContext.Submissions.Update(item);
        _dbContext.SaveChanges();

        string key = $"Monitor Plan {item.Id} Evaluation";
        IJobDetail job = JobBuilder.Create<MonitorPlanEvaluation>()
          .WithIdentity(key)
          .UsingJobData("id", item.Id)
          .Build();

        ITrigger trigger = TriggerBuilder.Create()
          .WithIdentity(key)
          .StartNow()
          .WithSimpleSchedule(x => x
            .WithRepeatCount(0)
          ).Build();

        await context.Scheduler.ScheduleJob(job, trigger);
      }

      await Console.Out.WriteLineAsync($"Check Engine process ended...");      
    }
  }
}