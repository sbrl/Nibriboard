using System;
using System.IO;
using Newtonsoft.Json;

namespace SBRL.Utilities
{
	public static class JsonUtilities
	{
		public static T DeserializeProperty<T>(string json, string targetPropertyName)
		{
			using (StringReader stringReader = new StringReader(json))
			using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
			{
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.PropertyName
						&& (string)jsonReader.Value == targetPropertyName)
					{
						jsonReader.Read();

						JsonSerializer serializer = new JsonSerializer();
						return serializer.Deserialize<T>(jsonReader);
					}
				}
				return default(T);
			}
		}
	}
}

