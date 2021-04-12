using System;

using CodeEffects.Rule.Attributes;

using Epa.Camd.Easey.RulesApi.Services;

namespace Epa.Camd.Easey.RulesApi.Models
{
	[ExternalAction(typeof(EvaluationService), "ReturnResult")]

	public class EvaluationBase
	{
		public DateTime? EvaluationBeginDate { get; set; }
		public DateTime? EvaluationEndDate { get; set; }
		public readonly string MaximumFutureDate = DateTime.Now.AddYears(1).ToString("YYYY-MM-DD");

		public EvaluationBase() { }

    public bool IsValidDate(DateTime? value)
		{
			return value == null || (value != null && value >= DateTime.MinValue);
		}

		public string DateToString(DateTime? value)
		{
			return value.ToString();
		}		
	}
}
