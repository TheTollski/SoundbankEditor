using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor
{
	public class WwiseShortIdUtility
	{
		public static Dictionary<uint, string> KnownShortIdsMap = new Dictionary<uint, string>();

		const uint FNV1_32_BIT_OFFSET_BASIS = 2166136261;
		const uint FNV1_32_BIT_PRIME = 16777619;

		public static void AddNames(List<string> names)
		{
			foreach (string name in names)
			{
				KnownShortIdsMap[ConvertToShortId(name)] = name;
			}
		}

		public static uint ConvertToShortId(string name)
		{
			// See https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function#FNV-1_hash
			
			uint hash = FNV1_32_BIT_OFFSET_BASIS;
			string sanitizedName = name.Trim().ToLower();

			for (int i = 0; i < sanitizedName.Length; i++)
			{
				hash *= FNV1_32_BIT_PRIME;
				hash ^= sanitizedName[i];
			}

			return hash;
		}

		public static uint ConvertToShortId30Bit(string name)
		{
			// See https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function#FNV-1_hash

			uint hash = ConvertToShortId(name);

			var numBits = 30;
			var mask = ((1 << numBits) - 1);
			var final = ((hash >> numBits)) ^ (hash & mask);
			
			return (uint)final;
		}
	}
}
