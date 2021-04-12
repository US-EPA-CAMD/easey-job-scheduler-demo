using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("monitor_location", Schema = "camdecmps")]
	public class MonitorLocation
	{
 		[Key]
		[Column("mon_loc_id")]
 		public int Id { get; set; }

 		public Unit Unit { get; set; }

 		public List<MonitorPlanLocation> Plans { get; set; }
	}
}
