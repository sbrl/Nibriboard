using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpCompress.Readers;

namespace Nibriboard.RippleSpace
{
	public class RippleSpaceManager
	{
		/// <summary>
		/// The temporary directory in which we are currently storing our unpacked planes temporarily.
		/// </summary>
		public string UnpackedDirectory;

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
			UnpackedDirectory += "/";
			Directory.CreateDirectory(UnpackedDirectory);

			Log.WriteLine("[RippleSpace] New blank ripplespace initialised.");
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
		/// <param name="newPlaneName">The name of the new plane to create.</param>
		/// <returns>The newly created plane.</returns>
		public Plane CreatePlane(string newPlaneName)
		{
			if(this[newPlaneName] != null)
				throw new InvalidOperationException($"Error: A plane with the name '{newPlaneName}' already exists in this RippleSpaceManager.");

			Log.WriteLine("[RippleSpace] Creating plane {0}", newPlaneName);

			Plane newPlane = new Plane(newPlaneName, DefaultChunkSize);
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

		public async Task<RippleSpaceManager> FromFile(string filename)
		{
			if(!File.Exists(filename))
				throw new FileNotFoundException($"Error: Couldn't find the packed ripplespace at {filename}");

			RippleSpaceManager rippleSpace = new RippleSpaceManager();

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
			List<Task> planeReaders = new List<Task>();
			string nextPlane;
			while((nextPlane = await planes.ReadLineAsync()) != null)
			{
				planeReaders.Add(Plane.FromFile(rippleSpace.UnpackedDirectory + nextPlane));
			}
			await Task.WhenAll(planeReaders);

			Log.WriteLine("[Core] done!");

			return rippleSpace;
		}
	}
}
