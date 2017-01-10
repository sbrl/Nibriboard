using System;
using System.Reflection;
using System.IO;
using System.Net.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SBRLUtilities
{
	/// <summary>
	/// A collection of static methods for manipulating embedded resources.
	/// </summary>
	/// <description>
	/// v0.2, by Starbeamrainbowlabs <feedback@starbeamrainbowlabs.com>
	/// Last updated 8th August 2016.
	/// Licensed under MPL-2.0.
	/// 
	/// Changelog:
	/// v0.1 (25th July 2016):
	/// 	- Initial release.
	/// v0.2 (8th August 2016):
	/// 	- Changed namespace.
	/// </description>
	public static class EmbeddedFiles
	{
		/// <summary>
		/// An array of the filenames of all the resources embedded in the calling assembly.
		/// </summary>
		/// <value>The resource list.</value>
		public static string[] ResourceList {
			get {
				return Assembly.GetCallingAssembly().GetManifestResourceNames();
			}
		}

		public static string GetResourceListText()
		{
			StringWriter result = new StringWriter();
			result.WriteLine("Files embedded in {0}:", Assembly.GetCallingAssembly().GetName().Name);
			foreach (string filename in ResourceList)
				result.WriteLine(" - {0}", filename);
			return result.ToString();
		}
		/// <summary>
		/// Writes a list of embedded resources to the Console's standard output.
		/// </summary>
		public static void WriteResourceList()
		{
			Console.WriteLine(GetResourceListText());
		}

		/// <summary>
		/// Gets a StreamReader attached to the specified embedded resource.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to a StreamReader for.</param>
		/// <returns>A StreamReader attached to the specified embedded resource.</returns>
		public static StreamReader GetReader(string filename)
		{
			return new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(filename));
		}

		/// <summary>
		/// Gets the specified embedded resource's content as a byte array.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to get conteent of.</param>
		/// <returns>The specified embedded resource's content as a byte array.</returns>
		public static byte[] ReadAllBytes(string filename)
		{
			// Referencing the Result property will block until the async method completes
			return ReadAllBytesAsync(filename).Result;
		}
		/// <summary>
		/// Gets the specified embedded resource's content as a byte array asynchronously.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to get conteent of.</param>
		/// <returns>The specified embedded resource's content as a byte array.</returns>
		public static async Task<byte[]> ReadAllBytesAsync(string filename)
		{
			using (Stream resourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(filename))
			using (MemoryStream temp = new MemoryStream())
			{
				await resourceStream.CopyToAsync(temp);
				return temp.ToArray();
			}
		}

		/// <summary>
		/// Gets all the text stored in the specified embedded resource.
		/// </summary>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static string ReadAllText(string filename)
		{
			using (StreamReader resourceReader = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(filename)))
			{
				return resourceReader.ReadToEnd();
			}
		}
		/// <summary>
		/// Gets all the text stored in the specified embedded resource asynchronously.
		/// </summary>
		/// <param name="filename">The filename to fetch the content of.</param>
		/// <returns>All the text stored in the specified embedded resource.</returns>
		public static async Task<string> ReadAllTextAsync(string filename)
		{
			using (StreamReader resourceReader = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(filename)))
			{
				return await resourceReader.ReadToEndAsync();
			}
		}

		/// <summary>
		/// Enumerates the lines of text in the specified embedded resource.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerator<string> EnumerateLines(string filename)
		{
			using (StreamReader resourceReader = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(filename)))
			{
				string nextLine;
				while ((nextLine = resourceReader.ReadLine()) != null)
				{
					yield return nextLine;
				}
			}
		}
		/// <summary>
		/// Enumerates the lines of text in the specified embedded resource asynchronously.
		/// Each successive call returns a task that, when complete, returns the next line of text stored
		/// in the embedded resource.
		/// </summary>
		/// <param name="filename">The filename of the embedded resource to enumerate.</param>
		/// <returns>An IEnumerator that enumerates the specified embedded resource.</returns>
		public static IEnumerable<Task<string>> EnumerateLinesAsync(string filename)
		{
			using (StreamReader resourceReader = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(filename)))
			{
				while (!resourceReader.EndOfStream)
				{
					yield return resourceReader.ReadLineAsync();
				}
			}
		}

		/// <summary>
		/// Gets all the lines of text in the specified embedded resource.
		/// You might find EnumerateLines(string filename) more useful depending on your situation.
		/// </summary>
		/// <param name="filename">The filename to obtain the lines of text from.</param>
		/// <returns>A list of lines in the specified embedded resource.</returns>
		public static List<string> GetAllLines(string filename)
		{
			// Referencing the Result property will block until the async method completes
			return GetAllLinesAsync(filename).Result;
		}
		/// <summary>
		/// Gets all the lines of text in the specified embedded resource asynchronously.
		/// </summary>
		/// <param name="filename">The filename to obtain the lines of text from.</param>
		/// <returns>A list of lines in the specified embedded resource.</returns>
		public static async Task<List<string>> GetAllLinesAsync(string filename)
		{
			List<string> lines = new List<string>();
			IEnumerable<Task<string>> lineIterator = EnumerateLinesAsync(filename);
			foreach (Task<string> nextLine in lineIterator)
			{
				lines.Add(await nextLine);
			}
			return lines;
		}
	}
}
