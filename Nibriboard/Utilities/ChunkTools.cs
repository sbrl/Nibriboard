using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;
using SBRL.Utilities;

namespace SBRL.Utilities
{
	/// <summary>
	/// A collection of tools aid in the manipulation of chunks.
	/// </summary>
	public static class ChunkTools
	{
		/// <summary>
		/// Gets a list of chunk references that cross inside the specified rectangle.
		/// </summary>
		/// <param name="plane">The plane to operate on.</param>
		/// <param name="area">The rectangle to find the containing chunks for.</param>
		/// <returns>All the chunk references that fall inside the specified area.</returns>
		public static List<ChunkReference> GetContainingChunkReferences(Plane plane, Rectangle area)
		{
			List<ChunkReference> result = new List<ChunkReference>();

			Vector2 currentLocation = area.TopLeft;
			while(currentLocation.X < area.BottomRight.X &&
				  currentLocation.Y < area.BottomRight.Y)
			{
				result.Add(new ChunkReference(
					plane,
					(int)Math.Floor(currentLocation.X / plane.ChunkSize),
					(int)Math.Floor(currentLocation.Y / plane.ChunkSize)
				));

				currentLocation.X += plane.ChunkSize;

				if(currentLocation.X > area.Right)
				{
					currentLocation.X = area.Left;
					currentLocation.Y += plane.ChunkSize;
				}
			}

			return result;
		}
	}
}
