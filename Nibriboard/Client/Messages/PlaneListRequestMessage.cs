using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a complaint (usually by the server) about something that the other party
	/// has done that they probably shouldn't have.
	/// </summary>
	public class PlaneListRequestMessage : Message
	{
		/// <summary>
		/// Whether a list of planes that the currently logged in user is a creator is desired, rather than a list of
		/// planes that the currently logged in user can view.
		/// </summary>
		public bool IsCreator = false;

		public PlaneListRequestMessage()
		{
		}
	}
}
