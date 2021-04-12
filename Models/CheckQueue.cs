using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[Table("check_queue", Schema = "camdecmpsaux")]
	public class CheckQueue
	{
 		[Key]
		[Column("check_queue_id")]    
 		public int Id { get; set; }

		[Column("check_queue_process_cd")]
		public string ProcessCode { get; set; }

		[Column("check_queue_status_cd")]
		public string StatusCode { get; set; }
	}
}
