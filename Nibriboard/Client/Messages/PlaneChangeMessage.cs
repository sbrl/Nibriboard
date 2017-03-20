using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a plane change requested by a nirbi client.
	/// </summary>
	public class PlaneChangeMessage : Message
	{
		/// <summary>
		/// Whether the plane change is ok or not.
		/// Used when the server is replying to a plane change request.
		/// </summary>
		public bool IsOK = false;
		/// <summary>
		/// Ţhe new plane name that the nibri client would like to switch to.
		/// </summary>
		public string NewPlaneName;

		/// <summary>
		/// Create a new plane change message.
		/// </summary>
		/// <param name="inNewPlaneName">The new plane name that the nibri client would like to switch to.</param>
		public PlaneChangeMessage(string inNewPlaneName)
		{
			NewPlaneName = inNewPlaneName;
		}
	}
}
