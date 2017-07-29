using System;

using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	[Serializable]
	[JsonObject(MemberSerialization.OptOut)]
	public class PlaneInfo
	{
		public string Name { get; set; }
		public int ChunkSize { get; set; }

		public PlaneInfo()
		{
		}
		public PlaneInfo(string inName) : this(inName, 1024)
		{
		}
		public PlaneInfo(string inName, int inChunkSize)
		{
			Name = inName;
			ChunkSize = inChunkSize;
		}

		public static PlaneInfo FromPlane(Plane plane)
		{
			PlaneInfo result = new PlaneInfo(plane.Name, plane.ChunkSize);

			return result;
		}
	}
}
