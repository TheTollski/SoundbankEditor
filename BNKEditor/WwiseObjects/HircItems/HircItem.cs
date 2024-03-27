using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	[JsonDerivedType(typeof(CakAction))]
	[JsonDerivedType(typeof(CAkEvent))]
	[JsonDerivedType(typeof(HircItemUnknown))]
	public interface HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }

		public void WriteToBinary(BinaryWriter binaryWriter);
	}

	public enum HircType : byte
	{
		//State = 1,
		Sound = 2,
		Action = 3,
		Event = 4,
		//SequenceContainer = 5,
		//SwitchContainer = 6,
		//ActorMixer = 7,
		//Audio_Bus = 8,
		//LayerContainer = 9,

		//Music_Segment = 10,
		//Music_Track = 11,
		//Music_Switch = 12,
		//Music_Random_Sequence = 13,

		//Attenuation = 14,
		//Dialogue_Event = 15,
		//FxShareSet = 16,
		//FxCustom = 17,
		//AuxiliaryBus = 18,
		//LFO = 19,
		//Envelope = 20,
		//AudioDevice = 21,
		//TimeMod = 22,

		//Didx_Audio = 23
	}
}
