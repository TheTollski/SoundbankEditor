using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	[JsonDerivedType(typeof(CAkAction), "Action")]
	[JsonDerivedType(typeof(CAkActorMixer), "Actor Mixer")]
	[JsonDerivedType(typeof(CAkAttenuation), "Attenuation")]
	[JsonDerivedType(typeof(CAkEvent), "Event")]
	[JsonDerivedType(typeof(CAkFxShareSet), "FxShareSet")]
	[JsonDerivedType(typeof(CAkRanSeqCntr), "Random/Sequence Container")]
	[JsonDerivedType(typeof(CAkSound), "Sound")]
	[JsonDerivedType(typeof(CAkSwitchCntr), "Switch Container")]
	[JsonDerivedType(typeof(HircItemUnknown), "Unknown")]
	public interface HircItem : WwiseObject
	{
		public HircType EHircType { get; set; }
		public uint UlID { get; set; }
	}

	public enum HircType : byte
	{
		//State = 1,
		Sound = 2,
		Action = 3,
		Event = 4,
		RandomSequenceContainer = 5,
		SwitchContainer = 6,
		ActorMixer = 7,
		//Audio_Bus = 8,
		//LayerContainer = 9,

		//Music_Segment = 10,
		//Music_Track = 11,
		//Music_Switch = 12,
		//Music_Random_Sequence = 13,

		Attenuation = 14,
		//Dialogue_Event = 15,
		FxShareSet = 18,
		//FxCustom = 17,
		//AuxiliaryBus = 18,
		//LFO = 19,
		//Envelope = 20,
		//AudioDevice = 21,
		//TimeMod = 22,

		//Didx_Audio = 23
	}
}
