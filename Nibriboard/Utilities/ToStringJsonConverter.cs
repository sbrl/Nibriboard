using System;

using Newtonsoft.Json;

/// <summary>
/// Contains a number classes that I (SBRL) have found on the internet.
/// While they have usually been adapted from one form or another, full credit is always given.
/// </summary>
namespace SBRL.Utilities.Solutions
{
	/// <summary>
	/// A JsonConverter that converts properties to a string via their inbuilt ToString() method.
	/// From http://stackoverflow.com/a/22355712/1460422 by 
	/// </summary>
	class ToStringJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}

