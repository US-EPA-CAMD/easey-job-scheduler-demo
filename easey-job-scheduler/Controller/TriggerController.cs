using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Quartz;
using SilkierQuartz;

using ECMPS.Checks.CheckEngine;

namespace Epa.Camd.Easey.JobScheduler
{
  [ApiController]
  [Route("quartz/api/triggers")]
  [Produces("application/json")]  
  public class TriggerController : ControllerBase
  {
    private async Task<ActionResult> TriggerEvaluation(
      string processCode,
      string group,
      string key,
      string jobDesc,
      string triggerDesc,
      EvaluationRequest request
    ) {

      Services Services = (Services)Request.HttpContext.Items[typeof(Services)];
      IScheduler Scheduler = Services.Scheduler;

      IJobDetail job = JobBuilder.Create<cCheckEngine>()
        .WithIdentity("Monitor Plan Evaluation", "DEFAULT")
        .UsingJobData("ProcessCode", processCode)
        .Build();

      ITrigger trigger = TriggerBuilder.Create()
        .WithIdentity("Monitor Plan Evaluation", "DEFAULT")
        .UsingJobData("MonitorPlanId", request.MonitorPlanId)
        .UsingJobData("ConfigurationName", request.ConfigurationName)
        .StartNow()
        .Build();

      await Scheduler.ScheduleJob(job, trigger);

      return CreatedAtAction("EvaluationResponse)", new {
        processCode = processCode,
        facilityId = request.FacilityId,
        facilityname = request.FacilityName,
        monitorPlanId = request.MonitorPlanId,
        configurationName = request.ConfigurationName
      });
    }

    [HttpPost("monitor-plans")]
    public async Task<ActionResult> TriggerMPEvaluation([FromBody] EvaluationRequest request) {
      return await TriggerEvaluation(
        "MP",
        "DEFAULT",
        "Monitor Plan Evaluation",
        "Evaluates a Monitor Plan configuration",
        $"Monitor Plan Configuration: {request.ConfigurationName}, Monitor Plan Id: {request.MonitorPlanId}",
        request
      );
    }

    [HttpPost("qa-certifications")]
    public async Task<ActionResult> TriggerQAEvaluation([FromBody] EvaluationRequest request)
    {
      return await TriggerEvaluation(
        "QA",
        "DEFAULT",
        "",
        "",
        "",
        request
      );
    }

    [HttpPost("emissions")]
    public async Task<ActionResult> TriggerEMEvaluation([FromBody] EvaluationRequest request)
    {
      return await TriggerEvaluation(
        "EM",
        "DEFAULT",
        "",
        "",
        "",
        request
      );
    }
  }
}
