using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;

namespace Nibriboard.RippleSpace
{
	public enum ChunkUpdateType
	{
		/// <summary>
		/// Something was added to the chunk.
		/// </summary>
		Addition,
		/// <summary>
		/// Something was deleted form the chunk.
		/// </summary>
		Deletion,
		/// <summary>
		/// A combination of additions and deletions were made to the chunk's contents.
		/// </summary>
		Combination
	}

	public class ChunkUpdateEventArgs : EventArgs
	{
		/// <summary>
		/// The type of update made to the chunk
		/// </summary>
		public ChunkUpdateType UpdateType { get; set; }
	}

	public delegate void ChunkUpdateEvent(object sender, ChunkUpdateEventArgs eventArgs);

	/// <summary>
	/// Represents a single chunk of an infinite <see cref="NibriboardServer.RippleSpace.Plane" />.
	/// </summary>
	[Serializable]
	public class Chunk : IEnumerable<DrawnLine>, IDeserializationCallback
	{
		/// <summary>
		/// The plane upon which this chunk is located.
		/// </summary>
		private Plane plane;

		/// <summary>
		/// The lines that this chunk currently contains.
		/// </summary>
		private List<DrawnLine> lines = new List<DrawnLine>();

		/// <summary>
		/// The size of this chunk.
		/// </summary>
		public readonly int Size;

		/// <summary>
		/// The location of this chunk chunk on the plane.
		/// </summary>
		public readonly ChunkReference Location;

		/// <summary>
		/// Fired when this chunk is updated.
		/// </summary>
		public event ChunkUpdateEvent OnChunkUpdate;
		/// <summary>
		/// The time at which this chunk was loaded.
		/// </summary>
		public readonly DateTime TimeLoaded = DateTime.Now;
		/// <summary>
		/// The time at which this chunk was last accessed.
		/// </summary>
		public DateTime TimeLastAccessed { get; private set; } = DateTime.Now;

		/// <summary>
		/// Whether this <see cref="T:Nibriboard.RippleSpace.Chunk"/> is primary chunk.
		/// Primary chunks are always loaded.
		/// </summary>
		public bool IsPrimaryChunk
		{
			get {
				if(Location.X < Location.Plane.PrimaryChunkAreaSize &&
				   Location.X > -Location.Plane.PrimaryChunkAreaSize &&
				   Location.Y < Location.Plane.PrimaryChunkAreaSize &&
				   Location.Y > -Location.Plane.PrimaryChunkAreaSize)
				{
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Whether this chunk is inactive or not.
		/// </summary>
		/// <remarks>
		/// Note that even if a chunk is inactive, it's not guaranteed that
		/// it will be unloaded. It's possible that the server will keep it
		/// loaded anyway - it could be a primary chunk, or the server may not
		/// have many chunks loaded at a particular time.
		/// </remarks>
		public bool Inactive
		{
			get {
				// If the time we were last accessed + the inactive timer is
				// still less than the current time, then we're inactive.
				if (TimeLastAccessed.AddMilliseconds(plane.InactiveMillisecs) < DateTime.Now)
					return false;
				
				return true;
			}
		}

		/// <summary>
		/// Whether this chunk could, theorectically, be unloaded.
		/// This method takes into account whether this is a primary chunk or not.
		/// </summary>
		public bool CouldUnload
		{
			get {
				// If we're a primary chunk or not inactive, then we shouldn't
				// unload it.
				if (IsPrimaryChunk || !Inactive)
					return false;
				
				return true;
			}
		}

		public Chunk(Plane inPlane, int inSize, ChunkReference inLocation)
		{
			plane = inPlane;
			Size = inSize;
			Location = inLocation;
		}

		/// <summary>
		/// Updates the time the chunk was last accessed, thereby preventing it
		/// from becoming inactive.
		/// </summary>
		public void UpdateAccessTime()
		{
			TimeLastAccessed = DateTime.Now;
		}

		#region Enumerator

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

		/// <summary>
		/// Adds one or more new drawn lines to the chunk.
		/// Note that new lines added must not cross chunk borders.
		/// </summary>
		/// <param name="newLines">The new line(s) to add.</param>
		public void Add(params DrawnLine[] newLines)
		{
			int i = 0;
			foreach (DrawnLine newLine in newLines)
			{
				if (newLine.SpansMultipleChunks == true)
					throw new ArgumentException("Error: A line you tried to add spans multiple chunks.", $"newLines[{i}]");

				if (newLine.ContainingChunk != Location)
					throw new ArgumentException($"Error: A line you tried to add isn't in this chunk ({Location}).", $"newLine[{i}]");

				lines.Add(newLine);
				i++;
			}

			OnChunkUpdate(this, new ChunkUpdateEventArgs() { UpdateType = ChunkUpdateType.Addition });
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

		#endregion

		#region Serialisation

		public static async Task<Chunk> FromFile(Plane plane, string filename)
		{
			FileStream chunkSource = new FileStream(filename, FileMode.Open);
			return await FromStream(plane, chunkSource);
		}
		public static async Task<Chunk> FromStream(Plane plane, Stream chunkSource)
		{
			Chunk loadedChunk = await Utilities.DeserialiseBinaryObject<Chunk>(chunkSource);
			loadedChunk.plane = plane;

			return loadedChunk;
		}

		public void OnDeserialization(object sender)
		{
			UpdateAccessTime();
		}

		#endregion
	}
}
