using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("monitor_plan_location", Schema = "camdecmps")]
	public class MonitorPlanLocation
	{
 		[Key]
		[Column("monitor_plan_location_id")]
 		public int Id { get; set; }

		[Column("mon_plan_id")]
 		public int PlanId { get; set; }

		[Column("mon_loc_id")]
 		public int LocationId { get; set; }

 		public MonitorPlan Plan { get; set; }

		public MonitorLocation Location { get; set; }
	}
}
