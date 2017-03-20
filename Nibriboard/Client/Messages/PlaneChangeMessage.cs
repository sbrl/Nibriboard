using System;
namespace Nibriboard.Client.Messages
{
	public class PlaneChangeMessage : Message
	{
		public string NewPlaneName;

		public PlaneChangeMessage(string inNewPlaneName)
		{
			NewPlaneName = inNewPlaneName;
		}
	}
}
