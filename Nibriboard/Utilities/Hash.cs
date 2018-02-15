using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nibriboard.Utilities
{
	public static class Hash
	{
		public static string SHA1(string input)
		{
			byte[] hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
		}
		public static string SHA1(byte[] input)
		{
			byte[] hash = (new SHA1Managed()).ComputeHash(input);
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
		}
	}
}
