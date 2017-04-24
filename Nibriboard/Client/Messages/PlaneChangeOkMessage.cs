using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Sent by the server to tell a client that they have switched planes successfully.
	/// </summary>
	public class PlaneChangeOkMessage : Message
	{
		/// <summary>
		/// The name of the plane that the client is now on.
		/// </summary>
		public string NewPlaneName;

		/// <summary>
		/// The size of the grid on the plane that the client has switched to.
		/// </summary>
		public int GridSize;

		public PlaneChangeOkMessage()
		{
		}
	}
}
