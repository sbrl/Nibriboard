using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a single chunk of an infinite <see cref="Nibriboard.RippleSpace.Plane" />.
	/// </summary>
	public class Chunk : IEnumerable<DrawnLine>
	{
		/// <summary>
		/// The lines that this chunk currently contains.
		/// </summary>
		private List<DrawnLine> lines = new List<DrawnLine>();

		/// <summary>
		/// The plane that this chunk is located on.
		/// </summary>
		public readonly Plane Plane;

		/// <summary>
		/// The size of this chunk.
		/// </summary>
		public readonly int Size;

		/// <summary>
		/// The time at which this chunk was loaded.
		/// </summary>
		public readonly DateTime TimeLoaded = DateTime.Now;
		/// <summary>
		/// The time at which this chunk was last accessed.
		/// </summary>
		public DateTime TimeLastAccessed { get; private set; } = DateTime.Now;

		public Chunk(Plane inPlane, int inSize)
		{
			Plane = inPlane;
			Size = inSize;
		}

		public void UpdateAccessTime()
		{
			TimeLastAccessed = DateTime.Now;
		}

		public DrawnLine this[int i]
		{
			get {
				UpdateAccessTime();
				return lines[i];
			}
			set {
				UpdateAccessTime();
				lines[i] = value;
			}
		}

		public IEnumerator<DrawnLine> GetEnumerator()
		{
			UpdateAccessTime();
			return lines.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			UpdateAccessTime();
			return GetEnumerator();
		}

		public static async Task<Chunk> FromFile(Plane plane, string filename)
		{
			StreamReader chunkSource = new StreamReader(filename);
			return await FromStream(plane, chunkSource);
		}
		public static async Task<Chunk> FromStream(Plane plane, StreamReader chunkSource)
		{
			Chunk result = new Chunk(
				plane,
				int.Parse(chunkSource.ReadLine())
			);

			string nextLine = string.Empty;
			while((nextLine = await chunkSource.ReadLineAsync()) != null)
			{
				throw new NotImplementedException();
			}

			return result;
		}
	}
}
