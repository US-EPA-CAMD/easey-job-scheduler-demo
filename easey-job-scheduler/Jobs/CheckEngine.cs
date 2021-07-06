using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
      try {
        await Console.Out.WriteLineAsync($"Check Engine checking submission queue for work...");

        List<CheckQueue> submissions = _dbContext.Submissions.FromSqlRaw(
          @"SELECT * FROM camdecmpsaux.check_queue_evaluations_to_process({0})",
          Guid.Parse(context.Scheduler.SchedulerInstanceId)
        ).ToList();

        if (submissions.Count > 0)
          await Console.Out.WriteLineAsync($"...scheduling {submissions.Count} submissions immediately");
        else
          await Console.Out.WriteLineAsync($"...no submissions to schedule");

        foreach(var item in submissions)
        {
          item.StatusCode = "Processing";
          _dbContext.Submissions.Update(item);
          _dbContext.SaveChanges();

          string group = $"Check Engine Evaluation";
          string key = $"submitted check queue item [{item.Id}] Monitor Plan {item.MonitorPlanId}";

          IJobDetail job = JobBuilder.Create<MonitorPlanEvaluation>()
            .WithIdentity(key, group)
            .UsingJobData("Id", item.Id)
            .UsingJobData("MonitorPlanId", item.MonitorPlanId)
            .Build();

          ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(key, group)
            .StartNow()
            .WithSimpleSchedule(x => x
              .WithRepeatCount(0)
            ).Build();

          await context.Scheduler.ScheduleJob(job, trigger);
        }

        await Console.Out.WriteLineAsync($"Check Engine process ended...");
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}