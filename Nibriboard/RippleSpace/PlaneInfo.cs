using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	[Serializable]
	[JsonObject(MemberSerialization.OptOut)]
	public class PlaneInfo
	{
		public string Name { get; set; }
		public int ChunkSize { get; set; }
		public List<string> Creators { get; set; } = new List<string>();
		public List<string> Members { get; set; } = new List<string>();

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
			result.Creators = plane.Creators;
			result.Members = plane.Members;
			return result;
		}
	}
}
