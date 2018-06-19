using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using SharpCompress.Writers;
using SharpCompress.Common;
using SharpCompress.Readers;
using Nibriboard.Utilities;
using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	public class MultiChunkUpdateEventArgs : EventArgs
	{
		/// <summary>
		/// The type of updates being made to these chunks.
		/// </summary>
		public ChunkUpdateType UpdateType;

		/// <summary>
		/// The chunks that have been updated.
		/// </summary>
		public IEnumerable<Chunk> UpdatedChunks;
	}

	public delegate void MultiChunkUpdateEvent(object sender, MultiChunkUpdateEventArgs eventArgs);

	/// <summary>
	/// Represents a single infinite plane.
	/// </summary>
	public class Plane
	{
		/// <summary>
		/// Whether we're currently buffering chunk updates or not.
		/// </summary>
		private bool currentlyBufferingChunkUpdates = false;
		/// <summary>
		/// A temporary storage list for chunks that have been updated but haven't yet been included in 
		/// a plane-wide chunk update event.
		/// </summary>
		private List<Chunk> updatedChunksBuffer = new List<Chunk>();

		/// <summary>
		/// The name of this plane.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The size of the chunks on this plane.
		/// </summary>
		public readonly int ChunkSize;

		/// <summary>
		/// A list of usernames who are able to both view and update the membership list of this plane.
		/// </summary>
		public List<string> Creators = new List<string>();
		/// <summary>
		/// A list of usernames who are able to view this plane, but not update the membership list.
		/// </summary>
		public List<string> Members = new List<string>();

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
		public int PrimaryChunkAreaSize = 5;

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
		/// Fired when one or more of the chunks on this plane updates.
		/// </summary>
		public event MultiChunkUpdateEvent OnChunkUpdates;

		/// <summary>
		/// The chunkspace that holds the currently loaded and active chunks.
		/// </summary>
		protected Dictionary<ChunkReference, Chunk> loadedChunkspace = new Dictionary<ChunkReference, Chunk>();

		public PlaneInfo Info {
			get {
				return PlaneInfo.FromPlane(this);
			}
		}

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
				foreach(KeyValuePair<ChunkReference, Chunk> chunkEntry in loadedChunkspace)
				{
					if(chunkEntry.Value.CouldUnload)
						result++;
				}
				return result;
			}
		}
		/// <summary>
		/// Calculates the total number of chunks created that are on this plane - including those
		/// that are currently unloaded.
		/// </summary>
		/// <description>
		/// This is, understandably, a rather expensive operation - so use with caution!
		/// Also, this count is only accurate to the last save.
		/// </description>
		public int TotalChunks {
			get {
				return Directory.GetFileSystemEntries(StorageDirectory, "*.chunk").Length;
			}
		}

		/// <summary>
		/// Initialises a new plane.
		/// </summary>
		/// <param name="inInfo">The settings to use to initialise the new plane.</param>
		/// <param name="inStorageDirectory">The storage directory in which we should store the plane's chunks (may be prepopulated).</param>
		public Plane(PlaneInfo inInfo, string inStorageDirectory)
		{
			Name = inInfo.Name;
			ChunkSize = inInfo.ChunkSize;
			StorageDirectory = inStorageDirectory;
			Creators = inInfo.Creators ?? new List<string>();
			Members = inInfo.Members ?? new List<string>();

			// Set the soft loaded chunk limit to double the number of chunks in the
			// primary chunks area
			// Note that the primary chunk area is a radius around (0, 0) - not the diameter
			SoftLoadedChunkLimit = PrimaryChunkAreaSize * PrimaryChunkAreaSize * 16;
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

			await FetchChunks(primaryChunkRefs, false);
		}

		/// <summary>
		/// Fetches a list of chunks by a list of chunk refererences.
		/// </summary>
		/// <param name="chunkRefs">The chunk references to fetch the attached chunks for.</param>
		/// <returns>The chunks attached to the specified chunk references.</returns>
		public async Task<List<Chunk>> FetchChunks(IEnumerable<ChunkReference> chunkRefs, bool autoCreate)
		{
			// Todo Paralellise loading with https://www.nuget.org/packages/AsyncEnumerator
			List<Chunk> chunks = new List<Chunk>();
			foreach(ChunkReference chunkRef in chunkRefs) {
				Chunk nextChunk = await FetchChunk(chunkRef, true);
				if(nextChunk != null) // Might be null if we're not allowed to create new chunks
					chunks.Add(nextChunk);
			}
			return chunks;
		}
		public async Task<List<Chunk>> FetchChunks(IEnumerable<ChunkReference> chunkRefs)
		{
			return await FetchChunks(chunkRefs, true);
		}


		public async Task<Chunk> FetchChunk(ChunkReference chunkLocation, bool autoCreate)
		{
			// If the chunk is in the loaded chunk-space, then return it immediately
			if(loadedChunkspace.ContainsKey(chunkLocation))
			{
				return loadedChunkspace[chunkLocation];
			}

			// Uh-oh! The chunk isn't loaded at moment. Load it quick & then
			// return it fast.
			string chunkFilePath = Path.Combine(StorageDirectory, chunkLocation.AsFilepath());
			Chunk loadedChunk;
			if(File.Exists(chunkFilePath)) // If the chunk exists on disk, load it
				loadedChunk = await Chunk.FromFile(this, chunkFilePath);
			else
			{
				// Ooooh! It's a _new_, never-before-seen one! Create a brand new chunk :D
				// ....but only if we've been told it's ok to create new chunks.
				if(!autoCreate) return null;
				loadedChunk = new Chunk(this, ChunkSize, chunkLocation);
			}
			loadedChunk.OnChunkUpdate += HandleChunkUpdate;
			loadedChunkspace.Add(chunkLocation, loadedChunk);

			return loadedChunk;
		}
		public async Task<Chunk> FetchChunk(ChunkReference chunkLocation)
		{
			return await FetchChunk(chunkLocation, true);
		}

		/// <summary>
		/// Works out whether a chunk currently exists.
		/// </summary>
		/// <param name="chunkLocation">The chunk location to check.</param>
		/// <returns>Whether the chunk at specified location exists or not.</returns>
		public bool HasChunk(ChunkReference chunkLocation)
		{
			if(loadedChunkspace.ContainsKey(chunkLocation))
				return true;

			string chunkFilePath = Path.Combine(StorageDirectory, chunkLocation.AsFilepath());
			if(File.Exists(chunkFilePath))
				return true;

			return false;
		}

		public async Task SaveChunk(ChunkReference chunkLocation)
		{
			// It doesn't exist, so we can't save it :P
			if (!loadedChunkspace.ContainsKey(chunkLocation))
				return;

			Chunk chunk = loadedChunkspace[chunkLocation];
			string chunkFilePath = Path.Combine(StorageDirectory, chunkLocation.AsFilepath());

			// If it's empty, then there's no point in saving it
			if (chunk.IsEmpty)
			{
				// Delete the existing chunk file, if it 
				if (File.Exists(chunkFilePath))
					File.Delete(chunkFilePath);
				return;
			}


			using (Stream chunkDestination = File.Open(chunkFilePath, FileMode.OpenOrCreate))
				await chunk.SaveTo(chunkDestination);
		}

		public async Task AddLine(DrawnLine newLine)
		{
			if(newLine.Points.Count == 0) {
				Log.WriteLine("[Plane/{0}] Lines that don't contain any points can't be added to a chunk!", Name);
				return;
			}
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

			// Add each segment to the appropriate chunk, buffering the chunk update message
			BeginChunkUpdateBuffering();
			foreach(DrawnLine newLineSegment in chunkedLineParts)
			{
				Chunk containingChunk = await FetchChunk(newLineSegment.ContainingChunk);
				containingChunk.Add(newLineSegment);
			}
			FinishChunkUpdateBuffering();
		}

		public async Task<bool> RemoveLineSegment(ChunkReference containingChunk, string targetLineUniqueId)
		{
			Chunk chunk = await FetchChunk(containingChunk);
			return chunk.Remove(targetLineUniqueId);
		}

		/// <summary>
		/// Whether the specified username is allowed to view this Plane.
		/// </summary>
		/// <remarks>
		/// In other words, whether the specified username can be found in either the Creators
		/// or Members username lists.
		/// </remarks>
		/// <param name="targetUsername">The target username to search for.</param>
		/// <returns>Whether the specified username is allowed to view this plane.</returns>
		public bool HasMember(string targetUsername)
		{
			return Creators.Contains(targetUsername) || Members.Contains(targetUsername);
		}

		/// <summary>
		/// Whether the specified username is listed as a creator of this plane.
		/// </summary>
		/// <param name="targetUsername">The target username to search for.</param>
		/// <returns>Whether the specified username is listed as a creator of this plane.</returns>
		public bool HasCreator(string targetUsername)
		{
			return Creators.Contains(targetUsername);
		}


		public async Task PerformMaintenance()
		{
			// Be lazy and don't bother to perform maintenance if it's not needed
			if(LoadedChunks < SoftLoadedChunkLimit ||
			   UnloadableChunks < MinUnloadeableChunks)
				return;

			int unloadedChunks = 0;
			foreach(KeyValuePair<ChunkReference, Chunk> chunkEntry in loadedChunkspace)
			{
				if(!chunkEntry.Value.CouldUnload)
					continue;

				// This chunk has been inactive for a while - let's serialise it and save it to disk
				Stream chunkSerializationSink = File.Open(
					Path.Combine(StorageDirectory, chunkEntry.Key.AsFilepath()),
					FileMode.Create
				);
				await chunkEntry.Value.SaveTo(chunkSerializationSink);

				// Remove the chunk from the loaded chunkspace
				loadedChunkspace.Remove(chunkEntry.Key);

				unloadedChunks++;
			}

			if (unloadedChunks > 0)
				Log.WriteLine($"[RippleSpace/{Name}] Unloaded {unloadedChunks} inactive chunks.");
		}

		public async Task<long> Save()
		{
			// Create the storage directory on disk if required
			Directory.CreateDirectory(StorageDirectory);

			// Save all the chunks to disk
			List<Task> chunkSavers = new List<Task>();
			foreach(KeyValuePair<ChunkReference, Chunk> loadedChunkItem in loadedChunkspace)
			{
				// Figure out where to put the chunk and create the relevant directories
				string chunkDestinationFilename = CalcPaths.ChunkFilePath(StorageDirectory, loadedChunkItem.Key);
				Directory.CreateDirectory(Path.GetDirectoryName(chunkDestinationFilename));

				// Ask the chunk to save itself, but only if it isn't empty
				if (loadedChunkItem.Value.IsEmpty) {
					// Delete the existing chunk file, if it exists
					if (File.Exists(chunkDestinationFilename))
						File.Delete(chunkDestinationFilename);
					
					continue;
				}

				Stream chunkDestination = File.Open(chunkDestinationFilename, FileMode.OpenOrCreate);
				chunkSavers.Add(loadedChunkItem.Value.SaveTo(chunkDestination));
			}
			await Task.WhenAll(chunkSavers);

			// Save the plane information
			StreamWriter planeInfoWriter = new StreamWriter(CalcPaths.PlaneIndex(StorageDirectory));
			await planeInfoWriter.WriteLineAsync(JsonConvert.SerializeObject(Info));
			planeInfoWriter.Close();

			// Calculate the total number bytes written
			long totalSize = 0;
			foreach (KeyValuePair<ChunkReference, Chunk> loadedChunkItem in loadedChunkspace)
			{
				string destFilename = CalcPaths.ChunkFilePath(StorageDirectory, loadedChunkItem.Key);
				if (!File.Exists(destFilename)) // Don't assume that the file exists - it might be an empty chunk
					continue;
				totalSize += (new FileInfo(destFilename)).Length;
			}

			return totalSize;
		}

		public void BeginChunkUpdateBuffering() {
			Log.WriteLine("Beginning chunk update buffer");
			updatedChunksBuffer.Clear();
			currentlyBufferingChunkUpdates = true;
		}
		public void FinishChunkUpdateBuffering() {
			currentlyBufferingChunkUpdates = false;
			Log.WriteLine("Captured {0} chunk updates whilst buffering", updatedChunksBuffer.Count);
			OnChunkUpdates(this, new MultiChunkUpdateEventArgs() {
				UpdateType = ChunkUpdateType.Combination,
				UpdatedChunks = updatedChunksBuffer.ToArray()
			});
			updatedChunksBuffer.Clear();
		}

		/// <summary>
		/// Handles chunk updates from the individual loaded chunks on this plane.
		/// Re-emits chunk updates it catches wind of at plane-level.
		/// </summary>
		/// <param name="sender">The chunk responsible for the update.</param>
		/// <param name="eventArgs">The event arguments associated with the chunk update.</param>
		public void HandleChunkUpdate(object sender, ChunkUpdateEventArgs eventArgs)
		{
			Chunk updatingChunk = sender as Chunk;
			if(updatingChunk == null)
			{
				Log.WriteLine("[Plane {0}] Invalid chunk update event captured - ignoring.");
				return;
			}


			Log.WriteLine("[Plane {0}] Chunk at {1} updated because {2}", Name, updatingChunk.Location, eventArgs.UpdateType);

			if (!currentlyBufferingChunkUpdates) {
				// Make the chunk update bubble up to plane-level
				OnChunkUpdates(this, new MultiChunkUpdateEventArgs()
				{
					UpdateType = eventArgs.UpdateType,
					UpdatedChunks = new Chunk[] { updatingChunk },
				});
			}
			else {
				updatedChunksBuffer.Add(updatingChunk);
			}
		}

		/// <summary>
		/// Loads a plane from a given nplane file.
		/// </summary>
		/// <param name="planeDirectory">The directory from which the plane should be loaded.</param>
		/// <returns>The loaded plane.</returns>
		public static async Task<Plane> FromDirectory(string planeDirectory)
		{
			PlaneInfo planeInfo = JsonConvert.DeserializeObject<PlaneInfo>(
				File.ReadAllText(CalcPaths.PlaneIndex(planeDirectory))
			);

			Plane loadedPlane = new Plane(planeInfo, planeDirectory);

			// Load the primary chunks into the plane
			await loadedPlane.LoadPrimaryChunks();

			return loadedPlane;
		}
	}
}
