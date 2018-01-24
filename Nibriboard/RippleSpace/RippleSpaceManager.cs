using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Nibriboard.Utilities;
using Nibriboard.Userspace;

namespace Nibriboard.RippleSpace
{
	public class RippleSpaceManager
	{
		/// <summary>
		/// The temporary directory in which we are strong out data.
		/// </summary>
		public string SourceDirectory { get; set; }

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

		/// <summary>
		/// The size of the last save, in bytes.
		/// Returns 0 if this RippleSpace hasn't been saved yet.
		/// </summary>
		/// <value>The last size of the save file.</value>
		public long LastSaveSize {
			get {
				if(!Directory.Exists(SourceDirectory))
					return 0;

				return (new DirectoryInfo(SourceDirectory))
					.GetFiles("*", SearchOption.AllDirectories)
					.Sum(file => file.Length);
			}
		}

		public RippleSpaceManager(string inSourceDirectory)
		{
			SourceDirectory = inSourceDirectory;

			// Make sure that the source directory exists
			if (!Directory.Exists(SourceDirectory)) {
				Directory.CreateDirectory(SourceDirectory);

				Log.WriteLine("[RippleSpace] New blank ripplespace initialised.");
			}
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
		/// Compiles a list of planes that the specified user has access to, taking permissions into account.
		/// </summary>
		/// <returns>A list of planes that the specified user has access to.</returns>
		/// <param name="targetUser">The target user to get the list of planes for.</param>
		public IEnumerable<Plane> GetByUser(User targetUser)
		{
			// If they can't view any planes, then theres no point in iterating the list
			if (!targetUser.HasPermission("view-own-plane") && !targetUser.HasPermission("view-any-plane"))
				return new List<Plane>();

			// If the user is allowed to view *any* plane, then we don't need to filter them :P
			if (targetUser.HasPermission("view-any-plane"))
				return Planes;
			
			return Planes.Where((Plane nextPlane) => nextPlane.HasMember(targetUser.Username));
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
				CalcPaths.PlaneDirectory(SourceDirectory, newPlaneInfo.Name)
			);
			Planes.Add(newPlane);
			return newPlane;
		}

		public Plane GetByName(string targetPlaneName)
		{
			foreach(Plane plane in Planes) {
				if(plane.Name == targetPlaneName)
					return plane;
			}
			return null;
		}

		public async Task StartMaintenanceMonkey()
		{
			Log.WriteLine("[RippleSpace/Maintenance] Automated maintenance monkey created.");

			while (true)
			{
				Stopwatch maintenanceStopwatch = Stopwatch.StartNew();

				foreach (Plane plane in Planes)
					await plane.PerformMaintenance();

				LastMaintenanceDuration = maintenanceStopwatch.ElapsedMilliseconds;

				await Task.Delay(MaintenanceInternal);
			}
		}

		public async Task<long> Save()
		{
			Stopwatch timer = Stopwatch.StartNew();

			// Save the planes to disk
			List<Task<long>> planeSavers = new List<Task<long>>();
			StreamWriter indexWriter = new StreamWriter(Path.Combine(SourceDirectory, "index.list"));
			foreach(Plane currentPlane in Planes)
			{
				// Add the plane to the index
				await indexWriter.WriteLineAsync(currentPlane.Name);

				// Ask the plane to save to the directory
				planeSavers.Add(currentPlane.Save());
			}
			indexWriter.Close();
			await Task.WhenAll(planeSavers);

			long totalBytesWritten = planeSavers.Sum((Task<long> saver) => saver.Result);

			Log.WriteLine(
				"[Command/Save] Save complete - {0} written in {1}ms",
				Formatters.HumanSize(totalBytesWritten),
				timer.ElapsedMilliseconds
			);

			return totalBytesWritten;
		}

		public static async Task<RippleSpaceManager> FromDirectory(string sourceDirectory)
		{

			RippleSpaceManager rippleSpace = new RippleSpaceManager(sourceDirectory);

			if (!Directory.Exists(sourceDirectory)) {
				Log.WriteLine($"[Core] Creating new ripplespace in {sourceDirectory}.");
				return rippleSpace;
			}

			Log.WriteLine($"[Core] Loading ripplespace from {sourceDirectory}.");

            // Load the planes in
            if (!File.Exists(Path.Combine(rippleSpace.SourceDirectory, "index.list"))) {
                Log.WriteLine($"[Core] Warning: The ripplespace at {sourceDirectory} doesn't appear to contain an index file.");
                return rippleSpace;
            }

			Log.WriteLine("[Core] Importing planes");
			Stopwatch timer = Stopwatch.StartNew();

			StreamReader planeList = new StreamReader(Path.Combine(sourceDirectory, "index.list"));

			List<Task<Plane>> planeLoaders = new List<Task<Plane>>();
			string nextPlaneName = string.Empty;
			while ((nextPlaneName = await planeList.ReadLineAsync()) != null)
			{
				string nextPlaneDirectory = CalcPaths.PlaneDirectory(sourceDirectory, nextPlaneName);
				if (!Directory.Exists(nextPlaneDirectory)) {
					Log.WriteLine($"[Core] Warning: Couldn't find listed plane {nextPlaneName} when loading ripplespace.");
					continue;
				}
				planeLoaders.Add(Plane.FromDirectory(nextPlaneDirectory));
			}
			await Task.WhenAll(planeLoaders);


			rippleSpace.Planes.AddRange(
				planeLoaders.Select((Task<Plane> planeLoader) => planeLoader.Result)
			);

			long msTaken = timer.ElapsedMilliseconds;
			Log.WriteLine($"[Core] done! {rippleSpace.Planes.Count} plane{(rippleSpace.Planes.Count != 1?"s":"")} loaded in {msTaken}ms.");

			return rippleSpace;
		}

}
}
