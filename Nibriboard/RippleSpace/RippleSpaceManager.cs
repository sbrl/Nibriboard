using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Nibriboard.RippleSpace
{
	public class RippleSpaceManager
	{
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
		protected Plane CreatePlane(string newPlaneName)
		{
			if(this[newPlaneName] != null)
				throw new InvalidOperationException($"Error: A plane with the name '{newPlaneName}' already exists in this RippleSpaceManager.");
			
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
	}
}
