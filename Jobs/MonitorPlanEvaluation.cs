using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

using Quartz;
using Npgsql;
using NpgsqlTypes;

namespace Epa.Camd.Easey.RulesApi.Models
{
  public class MonitorPlanEvaluation : IJob
  {
    private NpgSqlContext _dbContext = null;

    public int Id { private get; set; }

    public string MonitorPlanId { private get; set; }

    public MonitorPlanEvaluation(NpgSqlContext dbContext)
    {
      _dbContext = dbContext;
    }  

    public async Task Execute(IJobExecutionContext context)
    {
      try {
        JobKey key = context.JobDetail.Key;
        JobDataMap dataMap = context.MergedJobDataMap;

        this.Id = dataMap.GetInt("Id");
		    this.MonitorPlanId = dataMap.GetString("MonitorPlanId");        

        await Console.Out.WriteLineAsync($"{key.Group} process started for {key.Name}...");

        CheckQueue item = _dbContext.Submissions.Find(this.Id);

        System.Threading.Thread.Sleep(30000);

        //List<NpgsqlParameter> paramList = new List<NpgsqlParameter>();
        //paramList.Add(_dbContext.CreateParameter('monPlanID', '', NpgsqlDbType.Text, ParameterDirection.Input));
        //NpgsqlCommand command = _dbContext.ExecuteProcedure('camdecmps.getinitialvalues', paramList);

        item.StatusCode = "Complete";
        _dbContext.Submissions.Update(item);
        _dbContext.SaveChanges();

        await Console.Out.WriteLineAsync($"{key.Group} process ended for {key.Name}...");
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}