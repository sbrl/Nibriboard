using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using SharpCompress.Readers;
using Nibriboard.Utilities;
using SharpCompress.Writers;
using SharpCompress.Common;

namespace Nibriboard.RippleSpace
{
	public class RippleSpaceManager
	{
		/// <summary>
		/// The filename from which this ripplespace was loaded, and the filename to which it should be saved again.
		/// </summary>
		/// <value>The source filename.</value>
		public string SourceFilename { get; set; }
		
		/// <summary>
		/// The temporary directory in which we are currently storing our unpacked planes temporarily.
		/// </summary>
		public string UnpackedDirectory { get; set; }

		/// <summary>
		/// The master list of planes that this PlaneManager is in charge of. 
		/// </summary>
		public List<Plane> Planes = new List<Plane>();

		/// <summary>
		/// The number of milliseconds between each maintenance run.
		/// </summary>
		public readonly int MaintenanceInternal = 5000;

		/// <summary>
		/// The number of milliseconds the last maintenance run took.
		/// </summary>
		public long LastMaintenanceDuration = 0;

		public int DefaultChunkSize { get; set; } = 512;

		public RippleSpaceManager()
		{
			// Create a temporary directory in which to store our unpacked planes
			UnpackedDirectory = Path.GetTempFileName();
			File.Delete(UnpackedDirectory);
			UnpackedDirectory = Path.GetDirectoryName(UnpackedDirectory) + "/ripplespace-" + Path.GetFileName(UnpackedDirectory) + "/";
			Directory.CreateDirectory(UnpackedDirectory);

			Log.WriteLine("[RippleSpace] New blank ripplespace initialised.");
		}
		~RippleSpaceManager()
		{
			Directory.Delete(UnpackedDirectory, true);
		}

		/// <summary>
		/// Gets the plane with the specified name from this RippleSpace.
		/// </summary>
		/// <param name="planeName">The plane name to retrieve.</param>
		public Plane this[string planeName] {
			get {
				return GetById(planeName);
			}
		}

		/// <summary>
		/// Gets the plane with the specified name from this RippleSpace. 
		/// </summary>
		/// <param name="targetName">The plane name to retrieve.</param>
		/// <returns>The plane wwith the specified name.</returns>
		protected Plane GetById(string targetName)
		{
			foreach (Plane plane in Planes)
			{
				if (plane.Name == targetName)
					return plane;
			}

			return null;
		}

		/// <summary>
		/// Creates a new plane, adds it to this RippleSpaceManager, and then returns it.
		/// </summary>
		/// <param name="newPlaneInfo">The settings for the new plane to create.</param>
		/// <returns>The newly created plane.</returns>
		public Plane CreatePlane(PlaneInfo newPlaneInfo)
		{
			if(this[newPlaneInfo.Name] != null)
				throw new InvalidOperationException($"Error: A plane with the name '{newPlaneInfo.Name}' already exists in this RippleSpaceManager.");

			Log.WriteLine("[RippleSpace] Creating plane {0}", newPlaneInfo.Name);

			Plane newPlane = new Plane(
				newPlaneInfo,
				CalcPaths.UnpackedPlaneDir(UnpackedDirectory, newPlaneInfo.Name)
			);
			Planes.Add(newPlane);
			return newPlane;
		}

		public async Task StartMaintenanceMonkey()
		{
			Log.WriteLine("[RippleSpace/Maintenance] Automated maintenance monkey created.");

			while (true)
			{
				Stopwatch maintenanceStopwatch = Stopwatch.StartNew();

				foreach (Plane plane in Planes)
					plane.PerformMaintenance();

				LastMaintenanceDuration = maintenanceStopwatch.ElapsedMilliseconds;

				await Task.Delay(MaintenanceInternal);
			}

		}

		public async Task Save()
		{
			Stopwatch timer = Stopwatch.StartNew();

			// Save the planes to disk
			List<Task> planeSavers = new List<Task>();
			foreach(Plane item in Planes)
			{
				// Figure out where the plane should save itself to and create the appropriate directories
				string planeSavePath = CalcPaths.UnpackedPlaneFile(UnpackedDirectory, item.Name);
				Directory.CreateDirectory(Path.GetDirectoryName(planeSavePath));

				// Ask the plane to save to the directory
				planeSavers.Add(item.Save(File.OpenWrite(planeSavePath)));
			}
			await Task.WhenAll(planeSavers);

			// Pack the planes into the ripplespace archive
			Stream destination = File.OpenWrite(SourceFilename);
			string[] planeFiles = Directory.GetFiles(UnpackedDirectory, "*.nplane.zip", SearchOption.TopDirectoryOnly);

			using(IWriter rippleSpacePacker = WriterFactory.Open(destination, ArchiveType.Zip, new WriterOptions(CompressionType.Deflate)))
			{
				foreach(string planeFilename in planeFiles)
				{
					rippleSpacePacker.Write(Path.GetFileName(planeFilename), planeFilename);
				}
			}
			destination.Close();

			Log.WriteLine("[Command/Save] Save complete in {0}ms", timer.ElapsedMilliseconds);
		}

		public async Task<RippleSpaceManager> FromFile(string filename)
		{
			if(!File.Exists(filename))
				throw new FileNotFoundException($"Error: Couldn't find the packed ripplespace at {filename}");

			RippleSpaceManager rippleSpace = new RippleSpaceManager();
			rippleSpace.SourceFilename = filename;

			using(Stream packedRippleSpaceStream = File.OpenRead(filename))
			using(IReader rippleSpaceUnpacker = ReaderFactory.Open(packedRippleSpaceStream))
			{
				Log.WriteLine($"[Core] Unpacking ripplespace packed with {rippleSpaceUnpacker.ArchiveType} from {filename}.");
				rippleSpaceUnpacker.WriteAllToDirectory(UnpackedDirectory);
			}
			Log.WriteLine("[Core] done!");

			if(!File.Exists(rippleSpace.UnpackedDirectory + "index.list"))
				throw new InvalidDataException($"Error: The packed ripplespace at {filename} doesn't appear to contain an index file.");

			Log.WriteLine("[Core] Importing planes");

			StreamReader planes = new StreamReader(rippleSpace.UnpackedDirectory + "index.list");
			List<Task<Plane>> planeReaders = new List<Task<Plane>>();
			string nextPlane;
			int planeCount = 0;
			while((nextPlane = await planes.ReadLineAsync()) != null)
			{
				planeReaders.Add(Plane.FromFile(
					planeName: nextPlane,
					storageDirectoryRoot: rippleSpace.UnpackedDirectory,
					sourceFilename: CalcPaths.UnpackedPlaneFile(rippleSpace.UnpackedDirectory, nextPlane),
					deleteSource: true
				));
				planeCount++;
			}
			await Task.WhenAll(planeReaders);

			rippleSpace.Planes.AddRange(
				planeReaders.Select((Task<Plane> planeReader) => planeReader.Result)
			);

			Log.WriteLine("[Core] done! {0} planes loaded.", planeCount);

			return rippleSpace;
		}

}
}
