using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundbankEditor.Core
{
	public class WwiseShortIdUtility
	{
		private static Dictionary<uint, string> KnownBaseShortIdsMap = new Dictionary<uint, string>();
		private static Dictionary<uint, string> KnownCustomShortIdsMap = new Dictionary<uint, string>();

		const uint FNV1_32_BIT_OFFSET_BASIS = 2166136261;
		const uint FNV1_32_BIT_PRIME = 16777619;

		public static void AddNames(List<string> names, bool areCustom = false)
		{
			var dict = areCustom ? KnownCustomShortIdsMap : KnownBaseShortIdsMap;

			foreach (string name in names)
			{
				dict[ConvertToShortId(name)] = name;
			}
		}

		public static void ClearNames()
		{
			KnownBaseShortIdsMap.Clear();
			KnownCustomShortIdsMap.Clear();
		}

		public static List<string> GetAllNames(bool onlyIncludeCustom)
		{
			List<string> names = KnownCustomShortIdsMap.Values.ToList();
			if (!onlyIncludeCustom)
			{
				names.AddRange(KnownBaseShortIdsMap.Values.ToList());
			}

			return names;
		}

		public static string? GetNameFromShortId(uint shortId)
		{
			if (KnownBaseShortIdsMap.ContainsKey(shortId))
			{
				return KnownBaseShortIdsMap[shortId];
			}

			if (KnownCustomShortIdsMap.ContainsKey(shortId))
			{
				return KnownCustomShortIdsMap[shortId];
			}

			return null;
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
