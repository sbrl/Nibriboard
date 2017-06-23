using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client.Messages
{
	public class ChunkUpdateRequestMessage : Message
	{
		/// <summary>
		/// A list of chunks that the client has intentionally forgotten about, and will need
		/// to be resent to the client.
		/// </summary>
		public List<dynamic> ForgottenChunks = new List<dynamic>();

		public ChunkUpdateRequestMessage()
		{
		}
	}
}
