using BNKEditor.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BNKEditor
{
	public class SoundData
	{
		public List<Tuple<string, uint>> A { get; set; }
		public List<Tuple<string, List<string>>> B { get; set; }
		public List<Tuple<string, List<string>>> C { get; set; }
		public List<Tuple<string, List<uint>>> D { get; set; }
		public List<string> E { get; set; }
		public List<Tuple<uint, List<Tuple<uint, List<uint>>>>> F { get; set; }

		public SoundData() { }

		private SoundData(
			List<Tuple<string, uint>> a,
			List<Tuple<string, List<string>>> b,
			List<Tuple<string, List<string>>> c,
			List<Tuple<string, List<uint>>> d,
			List<string> e,
			List<Tuple<uint, List<Tuple<uint, List<uint>>>>> f
		)
		{
			A = a;
			B = b;
			C = c;
			D = d;
			E = e;
			F = f;
		}

		public static SoundData CreateFromDatFile(string inputDatFilePath)
		{
			using FileStream fileStream = File.OpenRead(inputDatFilePath);
			using BinaryReader binaryReader = new BinaryReader(fileStream);

			uint aSize = binaryReader.ReadUInt32();
			var a = new List<Tuple<string, uint>>();
			for (int i = 0; i < aSize; i++)
			{
				uint stringSize = binaryReader.ReadUInt32();
				string s = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize));
				uint id = binaryReader.ReadUInt32();

				a.Add(Tuple.Create(s, id));
			}

			uint bSize = binaryReader.ReadUInt32();
			var b = new List<Tuple<string, List<string>>>();
			for (int i = 0; i < bSize; i++)
			{
				uint stringSize1 = binaryReader.ReadUInt32();
				string s1 = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize1));
				
				uint arraySize = binaryReader.ReadUInt32();
				var array = new List<string>();
				for (int j = 0; j < arraySize; j++)
				{
					uint stringSize2 = binaryReader.ReadUInt32();
					string s2 = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize2));

					array.Add(s2);
				}

				b.Add(Tuple.Create(s1, array));
			}

			uint cSize = binaryReader.ReadUInt32();
			var c = new List<Tuple<string, List<string>>>();
			for (int i = 0; i < cSize; i++)
			{
				uint stringSize1 = binaryReader.ReadUInt32();
				string s1 = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize1));

				uint arraySize = binaryReader.ReadUInt32();
				var array = new List<string>();
				for (int j = 0; j < arraySize; j++)
				{
					uint stringSize2 = binaryReader.ReadUInt32();
					string s2 = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize2));

					array.Add(s2);
				}

				c.Add(Tuple.Create(s1, array));
			}

			uint dSize = binaryReader.ReadUInt32();
			var d = new List<Tuple<string, List<uint>>>();
			for (int i = 0; i < dSize; i++)
			{
				uint stringSize = binaryReader.ReadUInt32();
				string s = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize));

				uint arraySize = binaryReader.ReadUInt32();
				var array = new List<uint>();
				for (int j = 0; j < arraySize; j++)
				{
					array.Add(binaryReader.ReadUInt32());
				}

				d.Add(Tuple.Create(s, array));
			}

			uint eSize = binaryReader.ReadUInt32();
			var e = new List<string>();
			for (int i = 0; i < eSize; i++)
			{
				uint stringSize = binaryReader.ReadUInt32();
				string s = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize));

				e.Add(s);
			}

			uint fSize = binaryReader.ReadUInt32();
			var f = new List<Tuple<uint, List<Tuple<uint, List<uint>>>>>();
			for (int i = 0; i < fSize; i++)
			{
				uint val1 = binaryReader.ReadUInt32();
				
				uint arraySize1 = binaryReader.ReadUInt32();
				var array1 = new List<Tuple<uint, List<uint>>>();
				for (int j = 0; j < arraySize1; j++)
				{
					uint val2 = binaryReader.ReadUInt32();

					uint arraySize2 = binaryReader.ReadUInt32();
					var array2 = new List<uint>();
					for (int k = 0; k < arraySize2; k++)
					{
						uint val3 = binaryReader.ReadUInt32();

						array2.Add(val3);
					}

					array1.Add(Tuple.Create(val2, array2));
				}

				f.Add(Tuple.Create(val1, array1));
			}

			return new SoundData(a, b, c, d, e, f);
		}

		public static SoundData CreateFromJsonFile(string inputJsonFilePath)
		{
			string fileText = File.ReadAllText(inputJsonFilePath);

			SoundData? soundData = JsonSerializer.Deserialize<SoundData>(fileText);
			if (soundData == null)
			{
				throw new Exception("Unable to parse sound data from JSON file.");
			}

			return soundData;
		}

		private void WriteStringToBinary(BinaryWriter binaryWriter, string s)
		{
			binaryWriter.Write((uint)s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				binaryWriter.Write(s[i]);
			}
		}

		public void WriteToDatFile(string outputDatFilePath)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

			binaryWriter.Write((uint)A.Count);
			for (int i = 0; i < A.Count; i++)
			{
				WriteStringToBinary(binaryWriter, A[i].Item1);
				binaryWriter.Write(A[i].Item2);
			}

			binaryWriter.Write((uint)B.Count);
			for (int i = 0; i < B.Count; i++)
			{
				WriteStringToBinary(binaryWriter, B[i].Item1);
				binaryWriter.Write((uint)B[i].Item2.Count);
				for (int j = 0; j < B[i].Item2.Count; j++)
				{
					WriteStringToBinary(binaryWriter, B[i].Item2[j]);
				}
			}

			binaryWriter.Write((uint)C.Count);
			for (int i = 0; i < C.Count; i++)
			{
				WriteStringToBinary(binaryWriter, C[i].Item1);
				binaryWriter.Write((uint)C[i].Item2.Count);
				for (int j = 0; j < C[i].Item2.Count; j++)
				{
					WriteStringToBinary(binaryWriter, C[i].Item2[j]);
				}
			}

			binaryWriter.Write((uint)D.Count);
			for (int i = 0; i < D.Count; i++)
			{
				WriteStringToBinary(binaryWriter, D[i].Item1);
				binaryWriter.Write((uint)D[i].Item2.Count);
				for (int j = 0; j < D[i].Item2.Count; j++)
				{
					binaryWriter.Write(D[i].Item2[j]);
				}
			}

			binaryWriter.Write((uint)E.Count);
			for (int i = 0; i < E.Count; i++)
			{
				WriteStringToBinary(binaryWriter, E[i]);
			}

			binaryWriter.Write((uint)F.Count);
			for (int i = 0; i < F.Count; i++)
			{
				binaryWriter.Write(F[i].Item1);
				binaryWriter.Write((uint)F[i].Item2.Count);
				for (int j = 0; j < F[i].Item2.Count; j++)
				{
					binaryWriter.Write(F[i].Item2[j].Item1);
					binaryWriter.Write((uint)F[i].Item2[j].Item2.Count);
					for (int k = 0; k < F[i].Item2[j].Item2.Count; k++)
					{
						binaryWriter.Write(F[i].Item2[j].Item2[k]);
					}
				}
			}

			byte[] serializedBytes = memoryStream.ToArray();
			File.WriteAllBytes(outputDatFilePath, serializedBytes);
		}

		public void WriteToJsonFile(string outputJsonFilePath)
		{
			string json = JsonSerializer.Serialize(
				this,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);

			File.WriteAllText(outputJsonFilePath, json);
		}
	}
}
