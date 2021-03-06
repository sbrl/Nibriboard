﻿using System;
using System.Configuration;
using System.IO;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a location in absolute plane-space.
	/// </summary>
	public class LocationReference : Reference<double>
	{
		/// <summary>
		/// The chunk that this location reference fall inside.
		/// </summary>
		public ChunkReference ContainingChunk {
			get {
				if(Plane == null)
					return null;
				
				return new ChunkReference(
					Plane,
					(int)Math.Floor(X / Plane.ChunkSize),
					(int)Math.Floor(Y / Plane.ChunkSize)
				);
			}
		}
		public LocationReference(Plane inPlane, double inX, double inY) : base(inPlane, inX, inY)
		{
			
		}
		/// <summary>
		/// Creates a new blank <see cref="Nibriboard.RippleSpace.LocationReference" />.
		/// Don't use this yourself! This is only for Newtonsoft.Json to use when deserialising references.
		/// </summary>
		public LocationReference() : base()
		{

		}

		public override bool Equals(object obj)
		{
			LocationReference otherLocationReference = obj as LocationReference;
			if (otherLocationReference == null)
				return false;
			
			if(X == otherLocationReference.X && Y == otherLocationReference.Y &&
			   Plane == otherLocationReference.Plane)
			{
				return true;
			}
			return false;
		}
		public override int GetHashCode()
		{
			return $"({Plane.Name})+{X}+{Y}".GetHashCode();
		}

		public override string ToString()
		{
			return $"LocationReference: {base.ToString()}";
		}

		public static LocationReference Parse(Plane plane, string source)
		{
			if (!source.StartsWith("LocationReference:"))
				throw new InvalidDataException($"Error: That isn't a valid location reference. Location references start with 'ChunkReference:'.");

			// Trim the extras off the reference
			source = source.Substring("LocationReference:".Length);
			source = source.Trim("() \v\t\r\n".ToCharArray());

			int x = int.Parse(source.Substring(0, source.IndexOf(",")));
			int y = int.Parse(source.Substring(source.IndexOf(",") + 1));
			return new LocationReference(
				plane,
				x,
				y
			);
		}

		/// <summary>
		/// Returns a clone of this LocationReference.
		/// </summary>
		/// <returns>The newly-cloned instance.</returns>
		public override object Clone()
		{
			return new LocationReference(Plane, X, Y);
		}
	}
}
