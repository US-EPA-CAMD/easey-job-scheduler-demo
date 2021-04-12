using System;
using System.Threading.Tasks;

using Quartz;

namespace Epa.Camd.Easey.RulesApi.Models
{
  public class MonitorPlanEvaluation : IJob
  {
    private NpgSqlContext _dbContext = null;

    public MonitorPlanEvaluation(NpgSqlContext dbContext)
    {
      _dbContext = dbContext;
    }  

    public async Task Execute(IJobExecutionContext context)
    {
      JobDataMap dataMap = context.JobDetail.JobDataMap;
      int id = dataMap.GetInt("id");
      await Console.Out.WriteLineAsync($"Monitor Plan Evaluation process started for Id {id}...");

      CheckQueue item = _dbContext.Submissions.Find(id);

      System.Threading.Thread.Sleep(new Random(12654879).Next(5000, 30000));

      item.StatusCode = "Complete";
      _dbContext.Submissions.Update(item);
      _dbContext.SaveChanges();

      await Console.Out.WriteLineAsync($"Monitor Plan Evaluation process ended for Id {id}...");
    }
  }
}