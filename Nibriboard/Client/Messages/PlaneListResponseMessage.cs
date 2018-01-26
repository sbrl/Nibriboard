using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client.Messages
{
	public class PlaneListItem
	{
		public string Name { get; set; }
		public string Role { get; set; }

		public PlaneListItem(Plane plane, string inRole) : this(plane.Name, inRole) { }
		public PlaneListItem(string inName, string inRole)
		{
			Name = inName;
			Role = inRole;
		}
	}

		
	/// <summary>
	/// Represents a request from a client for a list of planes that that they are allowed to interact with.
	/// </summary>
	public class PlaneListResponseMessage : Message
	{
		public List<PlaneListItem> Planes = new List<PlaneListItem>();

		public PlaneListResponseMessage()
		{
		}
	}
}
