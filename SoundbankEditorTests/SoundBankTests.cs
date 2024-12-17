using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using System.IO;
using System.IO.Pipes;

namespace SoundbankEditorTests
{
	public class SoundBankTests
	{
		[Theory]
		[InlineData("Resources\\TW Attila\\battle.bnk")]
		[InlineData("Resources\\TW Attila\\battle_advice.bnk")]
		[InlineData("Resources\\TW Attila\\battle_vo_orders.bnk")]
		[InlineData("Resources\\TW Attila\\battle_vo_orders_barbarian.bnk")]
		[InlineData("Resources\\TW Attila\\battle_vo_orders_barbarian_inf1.bnk")]
		[InlineData("Resources\\TW Attila\\campaign_vo.bnk")]
		[InlineData("Resources\\TW Attila\\global_music_det.bnk")]
		public void BnkToJsonToBnk(string inputBnkFilePath)
		{
			if (!Directory.Exists("Temp"))
			{
				Directory.CreateDirectory("Temp");
			}

			SoundBank soundBank1 = SoundBank.CreateFromBnkFile(inputBnkFilePath);

			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputBnkFilePath);

			string outputJsonFilePath = $"Temp\\{fileNameWithoutExtension}_temp.json";
			soundBank1.WriteToJsonFile(outputJsonFilePath);

			SoundBank soundBank2 = SoundBank.CreateFromJsonFile(outputJsonFilePath);

			string outputBnkFilePath = $"Temp\\{fileNameWithoutExtension}_temp.bnk";
			soundBank2.WriteToBnkFile(outputBnkFilePath);

			using FileStream bnk1FileStream = File.OpenRead(inputBnkFilePath);
			using FileStream bnk2FileStream = File.OpenRead(outputBnkFilePath);

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