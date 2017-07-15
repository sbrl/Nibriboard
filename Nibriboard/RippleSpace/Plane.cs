using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using SharpCompress.Writers;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents an infinite plane.
	/// </summary>
	public class Plane
	{
		/// <summary>
		/// The name of this plane.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The size of the chunks on this plane.
		/// </summary>
		public readonly int ChunkSize;

		/// <summary>
		/// The path to the directory that the plane's information will be stored in.
		/// </summary>
		public readonly string StorageDirectory;

		/// <summary>
		/// The number of milliseconds that should pass since a chunk's last
		/// access in order for it to be considered inactive.
		/// </summary>
		public int InactiveMillisecs = 60 * 1000;

		/// <summary>
		/// The number of chunks in a square around (0, 0) that should always be
		/// loaded.
		/// Works like a radius.
		/// </summary>
		public int PrimaryChunkAreaSize = 10;

		/// <summary>
		/// The minimum number of potentially unloadable chunks that we should have
		/// before considering unloading some chunks
		/// </summary>
		public int MinUnloadeableChunks = 50;

		/// <summary>
		/// The soft limit on the number of chunks we can have loaded before we start
		/// bothering to try and unload any chunks
		/// </summary>
		public int SoftLoadedChunkLimit;

		/// <summary>
		/// Fired when one of the chunks on this plane updates.
		/// </summary>
		public event ChunkUpdateEvent OnChunkUpdate;

		/// <summary>
		/// The chunkspace that holds the currently loaded and active chunks.
		/// </summary>
		protected Dictionary<ChunkReference, Chunk> loadedChunkspace = new Dictionary<ChunkReference, Chunk>();

		/// <summary>
		/// The number of chunks that this plane currently has laoded into active memory.
		/// </summary>
		public int LoadedChunks {
			get {
				return loadedChunkspace.Count;
			}
		}
		/// <summary>
		/// The number of potentially unloadable chunks this plane currently has.
		/// </summary>
		public int UnloadableChunks {
			get {
				int result = 0;
				foreach(KeyValuePair<ChunkReference, Chunk> chunkEntry in loadedChunkspace) {
					if(chunkEntry.Value.CouldUnload)
						result++;
				}
				return result;
			}
		}

		public Plane(string inName, int inChunkSize, string inStorageDirectoryRoot)
		{
			Name = inName;
			ChunkSize = inChunkSize;

			// Set the soft loaded chunk limit to double the number of chunks in the
			// primary chunks area
			// Note that the primary chunk area is a radius around (0, 0) - not the diameter
			SoftLoadedChunkLimit = PrimaryChunkAreaSize * PrimaryChunkAreaSize * 4;

			StorageDirectory = inStorageDirectoryRoot + Name;
			if(File.Exists(StorageDirectory))
				throw new InvalidOperationException($"Error: The unpacked storage directory {StorageDirectory} already exists!");
			Directory.CreateDirectory(StorageDirectory);
		}

		private async Task LoadPrimaryChunks()
		{
			List<ChunkReference> primaryChunkRefs = new List<ChunkReference>();

			ChunkReference currentRef = new ChunkReference(this, -PrimaryChunkAreaSize, -PrimaryChunkAreaSize);
			while(currentRef.Y < PrimaryChunkAreaSize)
			{
				primaryChunkRefs.Add(currentRef.Clone() as ChunkReference);

				currentRef.X++;

				if(currentRef.X > PrimaryChunkAreaSize)
				{
					currentRef.X = -PrimaryChunkAreaSize;
					currentRef.Y++;
				}
			}

			await FetchChunks(primaryChunkRefs);
		}

		/// <summary>
		/// Fetches a list of chunks by a list of chunk refererences.
		/// </summary>
		/// <param name="chunkRefs">The chunk references to fetch the attached chunks for.</param>
		/// <returns>The chunks attached to the specified chunk references.</returns>
		public async Task<List<Chunk>> FetchChunks(List<ChunkReference> chunkRefs)
		{
			// todo Paralellise loading with https://www.nuget.org/packages/AsyncEnumerator
			List<Chunk> chunks = new List<Chunk>();
			foreach(ChunkReference chunkRef in chunkRefs)
				chunks.Add(await FetchChunk(chunkRef));
			return chunks;
		}

		public async Task<Chunk> FetchChunk(ChunkReference chunkLocation)
		{
			// If the chunk is in the loaded chunk-space, then return it immediately
			if(loadedChunkspace.ContainsKey(chunkLocation))
			{
				return loadedChunkspace[chunkLocation];
			}

			// Uh-oh! The chunk isn't loaded at moment. Load it quick & then
			// return it fast.
			string chunkFilePath = Path.Combine(StorageDirectory, chunkLocation.AsFilename());
			Chunk loadedChunk;
			if(File.Exists(chunkFilePath)) // If the chunk exists on disk, load it
				loadedChunk = await Chunk.FromFile(this, chunkFilePath);
			else // Ooooh! It's a _new_, never-before-seen one! Create a brand new chunk :D
				loadedChunk = new Chunk(this, ChunkSize, chunkLocation);
			loadedChunk.OnChunkUpdate += handleChunkUpdate;
			loadedChunkspace.Add(chunkLocation, loadedChunk);

			return loadedChunk;
		}

		public async Task AddLine(DrawnLine newLine)
		{
			List<DrawnLine> chunkedLineParts;
			// Split the line up into chunked pieces if neccessary
			if(newLine.SpansMultipleChunks)
				chunkedLineParts = newLine.SplitOnChunks();
			else
				chunkedLineParts = new List<DrawnLine>() { newLine };

			foreach(DrawnLine linePart in chunkedLineParts)
			{
				if(linePart.Points.Count == 0)
					Log.WriteLine("[Plane/{0}] Warning: A line part has no points in it O.o", Name);
			}

			// Add each segment to the appropriate chunk
			foreach(DrawnLine newLineSegment in chunkedLineParts)
			{
				Chunk containingChunk = await FetchChunk(newLineSegment.ContainingChunk);
				containingChunk.Add(newLineSegment);
			}
		}

		public void PerformMaintenance()
		{
			// Be lazy and don't bother to perform maintenance if it's not needed
			if(LoadedChunks < SoftLoadedChunkLimit ||
			   UnloadableChunks < MinUnloadeableChunks)
				return;
			
			foreach(KeyValuePair<ChunkReference, Chunk> chunkEntry in loadedChunkspace)
			{
				if(!chunkEntry.Value.CouldUnload)
					continue;

				// This chunk has been inactive for a while - let's serialise it and save it to disk
				Stream chunkSerializationSink = new FileStream(
					Path.Combine(StorageDirectory, chunkEntry.Key.AsFilename()),
					FileMode.Create,
					FileAccess.Write,
					FileShare.None
				);
				IFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(chunkSerializationSink, chunkEntry.Value);

				// Remove the chunk from the loaded chunkspace
				loadedChunkspace.Remove(chunkEntry.Key);
			}
		}

		public void Save(Stream destination)
		{
			WriterOptions packingOptions = new WriterOptions(CompressionType.GZip);

			IEnumerable<string> chunkFiles = Directory.GetFiles(StorageDirectory);
			using(IWriter packer = WriterFactory.Open(destination, ArchiveType.Tar, packingOptions))
			{
				foreach(string nextChunkFile in chunkFiles)
				{
					packer.Write($"{Name}/{Path.GetFileName(nextChunkFile)}", nextChunkFile);
				}
			}
		}

		/// <summary>
		/// Handles chunk updates from the individual loaded chunks on this plane.
		/// Re-emits chunk updates it catches wind of at plane-level.
		/// </summary>
		/// <param name="sender">The chunk responsible for the update.</param>
		/// <param name="eventArgs">The event arguments associated with the chunk update.</param>
		protected void handleChunkUpdate(object sender, ChunkUpdateEventArgs eventArgs)
		{
			Chunk updatingChunk = sender as Chunk;
			if(updatingChunk == null)
			{
				Log.WriteLine("[Plane {0}] Invalid chunk update event captured - ignoring.");
				return;
			}

			Log.WriteLine("[Plane {0}] Chunk at {1} updated because {2}", Name, updatingChunk.Location, eventArgs.UpdateType);

			// Make the chunk update bubble up to plane-level
			OnChunkUpdate(sender, eventArgs);
		}

		public static async Task<Plane> FromFile(string inName, int inChunkSize, string inStorageDirectoryRoot, string sourceFilename)
		{
			Plane loadedPlane = new Plane(inName, inChunkSize, inStorageDirectoryRoot);

			// Unpack the plane to the temporary directory
			using(Stream sourceStream = File.OpenRead(sourceFilename))
			using(IReader unpacker = ReaderFactory.Open(sourceStream))
			{
				unpacker.WriteAllToDirectory(loadedPlane.StorageDirectory);
			}

			// Load the primary chunks from disk inot the plane
			await loadedPlane.LoadPrimaryChunks();

			return loadedPlane;
		}

}
}
