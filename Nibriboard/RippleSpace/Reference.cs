using System;

using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// An abstract class representing a coordinate reference to a location.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class Reference : ICloneable
	{
		public readonly Plane Plane;

        /// <summary>
        /// The name of the plane that this reference is located on.
        /// Mainly used by the serialisation system that sends things to the client.
        /// </summary>
        [JsonProperty]
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
		public int X { get; set; }
        /// <summary>
        /// The y position of this reference.
        /// </summary>
		[JsonProperty]
		public int Y { get; set; }

		public Reference(Plane inPlane, int inX, int inY)
		{
			Plane = inPlane;
			X = inX; Y = inY;
		}

		public override string ToString()
		{
			return $"({X}, {Y}, {Plane.Name})";
		}

		public abstract object Clone();
	}
}
