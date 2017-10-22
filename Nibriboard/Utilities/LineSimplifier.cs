using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;

namespace Nibriboard.Utilities
{
	public static class LineSimplifier
	{
		public static List<LocationReference> SimplifyLine(List<LocationReference> points, float minArea)
		{
			// This algorithm requires more than 3 points
			if(points.Count < 3)
				return points;

			points = new List<LocationReference>(points); // Shallow clone the list

			while(true)
			{
				float smallestArea = float.MaxValue;
				int smallestAreaI = 1;

				for(int i = 1; i < points.Count - 1; i++)
				{
					float nextArea = TriangleArea(points[i - 1], points[i], points[i + 1]);
					if(nextArea < smallestArea) {
						smallestArea = nextArea;
						smallestAreaI = i;
					}
				}


				if(smallestArea >= minArea || points.Count <= 3)
					break;

				// Remove the central point of the smallest triangle
				points.RemoveAt(smallestAreaI);
			}

			return points;
		}

		public static float TriangleArea(LocationReference a, LocationReference b, LocationReference c)
		{
			return Math.Abs(
				(
					(float)a.X * (b.Y - c.Y) +
					(float)b.X * (c.Y - a.Y) +
					(float)c.X * (a.Y - b.Y)
				) / 2f
			);
		}
	}
}
