using System;

using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// An abstract class representing a coordinate reference to a location.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class Reference<T> : ICloneable
	{
		public Plane Plane { get; set; }

        /// <summary>
        /// The name of the plane that this reference is located on.
        /// Mainly used by the serialisation system that sends things to the client.
        /// </summary>
        public string PlaneName
        {
            get {
                return Plane.Name;
            }
        }

        /// <summary>
        /// The x position of this reference.
        /// </summary>
        [JsonProperty]
		public T X { get; set; }
        /// <summary>
        /// The y position of this reference.
        /// </summary>
		[JsonProperty]
		public T Y { get; set; }

		public Reference(Plane inPlane, T inX, T inY)
		{
			Plane = inPlane;
			X = inX; Y = inY;
		}
		/// <summary>
		/// Creates a new blank <see cref="Nibriboard.RippleSpace.Reference" />.
		/// Don't use this yourself! This is only for Newtonsoft.Json to use when deserialising references.
		/// </summary>
		public Reference() {


		}

		public override string ToString()
		{
			return $"({X}, {Y}, {Plane.Name})";
		}

		public abstract object Clone();
	}
}
