using System;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace Nibriboard
{
	public static class Utilities
	{
		public static async Task<T> DeserialiseBinaryObject<T>(Stream sourceStream)
		{
			return await Task.Run(() => {
				BinaryFormatter formatter = new BinaryFormatter();
				return (T)formatter.Deserialize(sourceStream);
			});
		}
	}
}
