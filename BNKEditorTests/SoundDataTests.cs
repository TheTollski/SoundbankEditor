using BNKEditor;
using BNKEditor.WwiseObjects;
using System.IO;
using System.IO.Pipes;

namespace BNKEditorTests
{
	public class SoundDataTests
	{
		[Theory]
		[InlineData("Resources\\TW Attila\\event_data.dat")]
		public void DatToJsonToDat(string inputDatFilePath)
		{
			if (!Directory.Exists("Temp"))
			{
				Directory.CreateDirectory("Temp");
			}

			SoundData soundData1 = SoundData.CreateFromDatFile(inputDatFilePath);

			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputDatFilePath);

			string outputJsonFilePath = $"Temp\\{fileNameWithoutExtension}_temp.json";
			soundData1.WriteToJsonFile(outputJsonFilePath);

			SoundData soundData2 = SoundData.CreateFromJsonFile(outputJsonFilePath);

			string outputDatFilePath = $"Temp\\{fileNameWithoutExtension}_temp.dat";
			soundData2.WriteToDatFile(outputDatFilePath);


			using FileStream bnk1FileStream = File.OpenRead(inputDatFilePath);
			using FileStream bnk2FileStream = File.OpenRead(outputDatFilePath);

			Assert.Equal(bnk1FileStream.Length, bnk2FileStream.Length);

			byte[] bnk1Bytes = new byte[bnk1FileStream.Length];
			bnk1FileStream.Position = 0;
			bnk1FileStream.Read(bnk1Bytes, 0, bnk1Bytes.Length);

			byte[] bnk2Bytes = new byte[bnk2FileStream.Length];
			bnk2FileStream.Position = 0;
			bnk2FileStream.Read(bnk2Bytes, 0, bnk2Bytes.Length);

			Assert.True(bnk1Bytes.SequenceEqual(bnk2Bytes));
		}
	}
}