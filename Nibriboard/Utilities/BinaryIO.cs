using System;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Nibriboard.Utilities
{
	public static class BinaryIO
	{
		/// <summary>
		/// Deserialises an object from it's binary representation.
		/// </summary>
		/// <typeparam name="T">The type to cast the deseralised object into.</typeparam>
		/// <param name="sourceStream">The source stream to deseralise an object from.</param>
		/// <returns>The object deserialised from the given stream.</returns>
		public static async Task<T> DeserialiseBinaryObject<T>(Stream sourceStream)
		{
			return await Task.Run(() => {
				BinaryFormatter formatter = new BinaryFormatter();
				return (T)formatter.Deserialize(sourceStream);
			});
		}

		/// <summary>
		/// Serialises an object and outputs a binary representation to the given stream.
		/// </summary>
		/// <param name="target">The target to serialise.</param>
		/// <param name="destinationStream">The destination stream to send the binary representation to.</param>
		public static async Task SerialiseToBinary(object target, Stream destinationStream)
		{
			await Task.Run(() => {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(destinationStream, target);
			});
		}
	}
}
