using System;
using System.IO;
using Nibriboard.RippleSpace;

namespace Nibriboard.Utilities
{
	public static class CalcPaths
	{
		/// <summary>
		/// Returns the directory in which a plane's data should be unpacked to.
		/// </summary>
		/// <param name="storageRoot">The root directory to which everything is going to be unpacked.</param>
		/// <param name="planeName">The name of the plane that will be unpacked.</param>
		/// <returns>The directory to which a plane should unpack it's data to.</returns>
		public static string PlaneDirectory(string storageRoot, string planeName)
		{
			string result = Path.Combine(storageRoot, "Planes", planeName);
			return result;
		}

		/// <summary>
		/// Returns the path to the plane index file given a directory that a plane has been unpacked to.
		/// </summary>
		/// <param name="planeDirectory">The directory to which a plane's data has been unpacked.</param>
		/// <returns>The path to the plane index file.</returns>
		public static string PlaneIndex(string planeDirectory)
		{
			return Path.Combine(planeDirectory, "plane-index.json");
		}

		public static string RippleSpaceAccountData(string rippleSpaceRoot)
		{
			return Path.Combine(rippleSpaceRoot, "user-data.json");
		}


		public static string ChunkFilePath(string planeStorageDirectory, ChunkReference chunkRef)
		{
			return Path.Combine(planeStorageDirectory, chunkRef.AsFilepath());
		}
	}
}
