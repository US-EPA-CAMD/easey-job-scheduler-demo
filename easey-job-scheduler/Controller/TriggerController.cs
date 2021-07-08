using System;
using Microsoft.AspNetCore.Mvc;

using Quartz;
using SilkierQuartz;

using ECMPS.Checks.CheckEngine;

namespace Epa.Camd.Easey.JobScheduler
{
  [Route("quartz/api/triggers")]
  [ApiController]
  public class TriggerController : ControllerBase
  {
    [HttpPost("monitor-plans")]
    public void TriggerMPEvaluation([FromBody] EvaluationRequest request) {
      const string key = "Monitor Plan Evaluation";
      const string group = "DEFAULT";
      const string processCode = "MP";

      string jobDesc = "Evaluates a Monitor Plan configuration";
      string triggerDesc = $"Monitor Plan Configuration: {request.ConfigurationName}, Monitor Plan Id: {request.MonitorPlanId}";

      Services Services = (Services)Request.HttpContext.Items[typeof(Services)];
      IScheduler Scheduler = Services.Scheduler;
      IJobDetail job = JobBuilder.Create<cCheckEngine>()
        .WithIdentity(key, group)
        .WithDescription(jobDesc)
        .UsingJobData("ProcessCode", processCode)
        .Build();

      ITrigger trigger = TriggerBuilder.Create()
        .WithIdentity($"{key} trigger", group)
        .WithDescription(triggerDesc)
        .UsingJobData("MonitorPlanId", request.MonitorPlanId)
        .UsingJobData("ConfigurationName", request.ConfigurationName)
        .StartNow()
        .Build();

      Scheduler.ScheduleJob(job, trigger);
    }

    [HttpPost("qa-certifications")]
    public void TriggerQAEvaluation([FromBody] string value)
    {
    }

    [HttpPost("emissions")]
    public void TriggerEMEvaluation([FromBody] string value)
    {
    }
  }
}
