using System;
using Nibriboard.RippleSpace;

namespace Nibriboard.Utilities
{
	public static class CalcPaths
	{
		/// <summary>
		/// Returns the directory in which a plane's data should be unpacked to.
		/// </summary>
		/// <param name="unpackingRoot">The root directory to which everything is going to be unpacked.</param>
		/// <param name="planeName">The name of the plane that will be unpacked.</param>
		/// <returns>The directory to which a plane should unpack it's data to.</returns>
		public static string UnpackedPlaneDir(string unpackingRoot, string planeName)
		{
			string result = $"{unpackingRoot}/Planes/{planeName}/";
			return result;
		}

		/// <summary>
		/// Returns the path to the plane index file given a directory that a plane has been unpacked to.
		/// </summary>
		/// <param name="unpackingPlaneDir">The directory to which a plane's data has been unpacked.</param>
		/// <returns>The path to the plane index file.</returns>
		public static string UnpackedPlaneIndex(string unpackingPlaneDir)
		{
			return $"{unpackingPlaneDir}/plane-index.json";
		}

		/// <summary>
		/// Calculates the path to a packed plane file.
		/// </summary>
		/// <param name="unpackingDir">The directory to which the nplane files were unpacked.</param>
		/// <param name="planeName">The name of the plane to fetch the filepath for.</param>
		/// <returns>The path to the packed plane file.</returns>
		public static string UnpackedPlaneFile(string unpackingDir, string planeName)
		{
			return $"{unpackingDir}/{planeName}.nplane.tar.gz";
		}


		public static string ChunkFilepath(string planeStorageDirectory, ChunkReference chunkRef)
		{
			return $"{planeStorageDirectory}/{chunkRef.AsFilename()}";
		}
	}
}
