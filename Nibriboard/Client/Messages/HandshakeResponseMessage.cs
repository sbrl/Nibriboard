using System;

using Newtonsoft.Json;

using SBRL.Utilities;
using SBRL.Utilities.Solutions;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Holds the server's response to a HandshakeRequest.
	/// </summary>
	public class HandshakeResponseMessage : Message
	{
		/// <summary>
		/// The id that the server is assigning to the nibri client.
		/// </summary>
		public int Id;
		/// <summary>
		/// The colour that the server is assigning to the nibri client.
		/// </summary>
		[JsonConverter(typeof(ToStringJsonConverter))]
		public ColourHSL Colour;

		public HandshakeResponseMessage()
		{
		}
	}
}

