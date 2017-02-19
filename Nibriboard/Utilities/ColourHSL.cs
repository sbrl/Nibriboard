using System;

namespace SBRL.Utilities
{
	public class ColourHSL 
	{
		/// <summary>
		/// A random number generator.Used to generate random colours in RandomSaturated().
		/// </summary>
		private static Random random = new Random();

		#region Properties

		/// <summary>
		/// The hue of the colour, between 0 and 360.
		/// </summary>
		public float Hue { get; set; }
		/// <summary>
		/// The saturation of the colour, between 0 and 100.
		/// </summary>
		public float Saturation { get; set; }
		/// <summary>
		/// The lightness of the colour, between 0 and 100.
		/// </summary>
		public float Lightness { get; set; }
		/// <summary>
		/// The opacity of the colour, between 0 and 1.
		/// </summary>
		public float Alpha { get; set; }

		#endregion

		#region Aliases

		/// <summary>
		/// Alias of Hue.
		/// </summary>
		public float H {
			get { return Hue; }
			set { Hue = value; }
		}
		/// <summary>
		/// Alias of Saturation.
		/// </summary>
		public float S {
			get { return Saturation; }
			set { Saturation = value; }
		}
		/// <summary>
		/// Alias of Lightness.
		/// </summary>
		public float L {
			get { return Lightness; }
			set { Lightness = value; }
		}
		/// <summary>
		/// Alias of Alpha.
		/// </summary>
		public float A {
			get { return Alpha; }
			set { Alpha = value; }
		}

		#endregion

		/// <summary>
		/// Whether this colour is opaque or not.
		/// </summary>
		public bool IsOpaque {
			get {
				return Alpha == 1;
			}
		}

		/// <summary>
		/// Whether this colour is translucent or not.
		/// </summary>
		public bool IsTranslucent {
			get {
				return !IsOpaque && !IsTransparent;
			}
		}

		/// <summary>
		/// Whether this colour is completely transparent or not.
		/// </summary>
		public bool IsTransparent {
			get {
				return Alpha == 0;
			}
		}

		/// <summary>
		/// Whether this colour is currently valid or not. A valid colour has a hue of 0-360,
		/// a saturation of 0-100, a lightness of 0-100, and an opacity of 0-1.
		/// </summary>
		public bool IsValidColour
		{
			get {
				return Hue >= 0 && Hue <= 360 &&
					Saturation >= 0 && Saturation <= 100 &&
					Lightness >= 0 && Lightness <= 100 &&
					Alpha >= 0 && Alpha <= 1;
			}
		}

		/// <summary>
		/// Initializes a new ColourHSL instance, with the colour set to bright white.
		/// </summary>
		public ColourHSL() : this(0, 100, 100, 1)
		{
		}
		/// <summary>
		/// Initializes a new <see cref="SBRL.Utilities.ColourHSL"/>  class instance.
		/// </summary>
		/// <param name="inHue">The hue of the colour.</param>
		/// <param name="inSaturation">The saturation of the colour.</param>
		/// <param name="inLightness">The lightness of the colour.</param>
		public ColourHSL(float inHue, float inSaturation, float inLightness) : this(inHue, inSaturation, inLightness, 1)
		{
		}
		/// <summary>
		/// Initializes a new <see cref="SBRL.Utilities.ColourHSL"/>  class instance.
		/// </summary>
		/// <param name="inHue">The hue of the colour.</param>
		/// <param name="inSaturation">The saturation of the colour.</param>
		/// <param name="inLightness">The lightness of the colour.</param>
		/// <param name="inAlpha">The opacity of the colour.</param>
		public ColourHSL(float inHue, float inSaturation, float inLightness, float inAlpha)
		{
			Hue = inHue;
			Saturation = inSaturation;
			Lightness = inLightness;
			Alpha = inAlpha;
		}

		/// <summary>
		/// Returns a random bright colour.
		/// </summary>
		public static ColourHSL RandomSaturated()
		{
			return new ColourHSL() {
				Hue = random.Next(0, 360),
				Lightness = random.Next(45, 55),
				Saturation = random.Next(90, 100)
			};
		}

		#region Overrides

		/// <summary>
		/// Returns a <see cref="string"/> that represents this <see cref="SBRL.Utilities.ColourHSL"/> instance.
		/// </summary>
		/// <returns>A <see cref="string"/> that represents this <see cref="SBRL.Utilities.ColourHSL"/> instance.</returns>
		public override string ToString()
		{
			string format = string.Empty;
			if (IsOpaque)
				format = "hsl({0}, {1}%, {2}%)";
			else
				format = "hsla({0}, {1}%, {2}%, {3})";
			
			return string.Format(format, Hue, Saturation, Lightness, Math.Round(Alpha, 3));
		}

		#endregion
	}
}