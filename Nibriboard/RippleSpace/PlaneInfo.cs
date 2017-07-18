using System;

namespace Nibriboard.RippleSpace
{
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
	}

}
