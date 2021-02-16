using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Quartz;

namespace EPA.CAMPD.Scheduler
{
  public class MainJob : IJob
  {
    private Dictionary<string, Dictionary<string, string>> dataSets = new Dictionary<string, Dictionary<string, string>>();

    public async Task Execute(IJobExecutionContext context)
    {
      await Console.Out.WriteLineAsync("Determining datasets to generate...");

      var periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-01-01");
      periods.Add("endDate", "2019-03-31");
      dataSets.Add("emissions/hourly/2019-Q1.json", periods);

      periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-04-01");
      periods.Add("endDate", "2019-06-30");
      dataSets.Add("emissions/hourly/2019-Q2.json", periods);

      periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-07-01");
      periods.Add("endDate", "2019-09-30");
      dataSets.Add("emissions/hourly/2019-Q3.json", periods);

      periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-10-01");
      periods.Add("endDate", "2019-12-31");
      dataSets.Add("emissions/hourly/2019-Q4.json", periods);

      periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-01-01");
      periods.Add("endDate", "2019-12-31");
      periods.Add("state", "AL");
      dataSets.Add("emissions/hourly/AL-2019.json", periods);

      periods = new Dictionary<string, string>();
      periods.Add("beginDate", "2019-01-01");
      periods.Add("endDate", "2019-12-31");
      periods.Add("state", "TX");
      dataSets.Add("emissions/hourly/TX-2019.json", periods);
     
      foreach(var ds in dataSets)
      {
        string key = ds.Key;

        IJobDetail job = JobBuilder.Create<SubJob>()
            .WithIdentity(key)
            .UsingJobData(new JobDataMap(ds.Value))
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(key)
            .StartNow()
            .WithSimpleSchedule(x => x
              .WithRepeatCount(0)
            )
            .Build();

        await Console.Out.WriteLineAsync($"Scheduling {key} dataset...");
        await context.Scheduler.ScheduleJob(job, trigger);
      }
    }
  }
}