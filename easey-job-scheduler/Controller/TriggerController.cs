using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECMPS.Checks.CheckEngine;
using Epa.Camd.Easey.JobScheduler.Jobs;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using SilkierQuartz;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Epa.Camd.Easey.JobScheduler
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()

        {
           ;
            Services Services = (Services)Request.HttpContext.Items[typeof(Services)];
              IScheduler Scheduler = Services.Scheduler;
              IJobDetail job = JobBuilder.Create<cCheckEngine>()
               .WithIdentity("CHECK ENGINEX", "DEFAULT")
          .Build();

            
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("ExecutionX", "DEFAULT")
               .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(40)
                  .RepeatForever())
               .Build();

            Scheduler.ScheduleJob(job, trigger);
        

            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {       
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
