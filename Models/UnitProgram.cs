using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("unit_program", Schema = "camd")]
	public class UnitProgram : EvaluationBase
	{
 		[Key]
		[Column("up_id")]
 		public int Id { get; set; }

		[Column("end_date")]
 		public DateTime? EndDate { get; set; }

 		public Unit Unit { get; set; }

	}
}
