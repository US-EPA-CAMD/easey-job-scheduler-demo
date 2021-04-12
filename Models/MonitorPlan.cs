using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("monitor_plan", Schema = "camdecmps")]
	public class MonitorPlan : EvaluationBase
	{
 		[Key]
		[Column("mon_plan_id")]
 		public int Id { get; set; }

		[Column("fac_id")]
 		public int FacilityId { get; set; }

 		public List<MonitorPlanLocation> Locations { get; set; }
	}
}
