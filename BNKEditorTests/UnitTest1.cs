using BNKEditor;
using BNKEditor.WwiseObjects;

namespace BNKEditorTests
{
	public class UnitTest1
	{
		[Theory]
		[InlineData("Resources\\TW Attila\\battle_vo_orders.bnk")]
		[InlineData("Resources\\TW Attila\\battle_vo_orders_barbarian.bnk")]
		[InlineData("Resources\\TW Attila\\battle_vo_orders_barbarian_inf1.bnk")]
		public void DeserializeThenSerialize(string path)
		{
			using FileStream fileStream = File.OpenRead(path);
			using BinaryReader binaryReader = new BinaryReader(fileStream);

			BnkReader bnkReader = new BnkReader();
			List<WwiseRootObject> wwiseRootObjects = bnkReader.Parse(binaryReader);

			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

			for (int i = 0; i < wwiseRootObjects.Count; i++)
			{
				wwiseRootObjects[i].WriteToBinary(binaryWriter);
			}

			Assert.Equal(fileStream.Length, memoryStream.Length);

			byte[] fileBytes = new byte[fileStream.Length];
			fileStream.Position = 0;
			fileStream.Read(fileBytes, 0, fileBytes.Length);

			byte[] serializedBytes = memoryStream.ToArray();

			Assert.True(fileBytes.SequenceEqual(serializedBytes));
		}
	}
}