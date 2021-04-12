using System;

using CodeEffects.Rule.Attributes;
//using Epa.Camd.Easey.RulesApi.Models;

namespace Epa.Camd.Easey.RulesApi.Services
{
	public class EvaluationService
	{
		[Action(DisplayName = "Return Result")]
		public void ReturnResult(string msg)
		{
			// if (!string.IsNullOrEmpty(request.EvaluatedValue))
			// 	msg = msg.Replace("{value}", request.EvaluatedValue.ToString());

			// msg = msg.Replace("{recId}", request.UnitCapacity.Id.ToString());

			// msg = msg.Replace("&nbsp;", " ");
			// request.Results.Add(msg);
		}
	}
}