using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundbankEditorCore.WwiseObjects.HircItems
{
	public class HircItemFactory
	{
		public static HircItem Create(HircType hircType, BinaryReader? binaryReader = null)
		{
			if (hircType == HircType.Sound) // 2
			{
				return binaryReader != null ? new CAkSound(binaryReader) : new CAkSound();
			}
			if (hircType == HircType.Action) // 3
			{
				return binaryReader != null ? new CAkAction(binaryReader) : new CAkAction();
			}
			if (hircType == HircType.Event) // 4
			{
				return binaryReader != null ? new CAkEvent(binaryReader) : new CAkEvent();
			}
			if (hircType == HircType.RandomSequenceContainer) // 5
			{
				return binaryReader != null ? new CAkRanSeqCntr(binaryReader) : new CAkRanSeqCntr();
			}
			if (hircType == HircType.SwitchContainer) // 6
			{
				return binaryReader != null ? new CAkSwitchCntr(binaryReader) : new CAkSwitchCntr();
			}
			if (hircType == HircType.ActorMixer) // 7
			{
				return binaryReader != null ? new CAkActorMixer(binaryReader) : new CAkActorMixer();
			}
			if (hircType == HircType.Attenuation) // 14
			{
				return binaryReader != null ? new CAkAttenuation(binaryReader) : new CAkAttenuation();
			}
			if (hircType == HircType.FxShareSet) // 18
			{
				return binaryReader != null ? new CAkFxShareSet(binaryReader) : new CAkFxShareSet();
			}

			return binaryReader != null ? new HircItemUnknown(binaryReader) : new HircItemUnknown();
		}
	}
}
