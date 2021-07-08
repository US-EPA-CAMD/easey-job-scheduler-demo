using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECMPS.Checks.CheckEngine;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Epa.Camd.Easey.JobScheduler
{
    [Route("quartz/api/triggers")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        [HttpPost("monitor-plans")]
        public void TriggerMPEvaluation([FromBody] string value)
        {
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
