using System;
using System.Drawing;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SBRL.Utilities.JsonConverters
{
	/// <summary>
	/// Deserialises objects into points from the System.Drawing namespace.
	/// </summary>
	public class PointConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);

			return new Point(
				jsonObject.Value<int>("X"),
				jsonObject.Value<int>("Y")
			);
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType != typeof(Rectangle))
				return false;
			return true;
		}
	}
}

