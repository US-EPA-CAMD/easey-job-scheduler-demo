using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("unit", Schema = "camd")]
	public class Unit
	{
 		[Key]
		[Column("unit_id")]
 		public int Id { get; set; }

		[Column("unitid")]
 		public int Name { get; set; }

		[Column("fac_id")]
 		public int FacilityId { get; set; }

 		public List<MonitorLocation> Locations { get; set; }

 		public List<UnitProgram> Programs { get; set; }
	}
}
