using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
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
				return binaryReader != null ? new CAkSound(binaryReader) : CreateCAkSoundForBattleVoOrders();
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
				return binaryReader != null ? new CAkSwitchCntr(binaryReader) : CreateCAkSwitchCntrForBattleVoOrders();
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

		public static CAkSound CreateCAkSoundForBattleVoOrders()
		{
			return new CAkSound
			{
				AkBankSourceData = new AkBankSourceData
				{
					UlPluginId = 262145,
					StreamType = 2,
					AkMediaInformation = new AkMediaInformation
					{
						SourceId = 0,
						FileId = 0,
						InMemoryMediaSize = 0,
						SourceBits = 1,
					},
				},
				NodeBaseParams = new NodeBaseParams
				{
					NodeInitialFxParams = new NodeInitialFxParams
					{
						IsOverrideParentFX = 0,
						FxChunks = new List<FxChunk> { },
					},
					OverrideAttachmentParams = 0,
					OverrideBusId = 0,
					DirectParentID = 0,
					ByBitVector = 0,
					NodeInitialParams = new NodeInitialParams
					{
						AkPropBundle1 = new AkPropBundle
						{
							Props = new List<AkProp>
							{
								new AkProp
								{
									Id = 6,
									Value = 100
								}
							}
						},
						AkPropBundle2 = new AkPropBundle(),
					},
					PositioningParams = new PositioningParams
					{
						ByVector = 192,
					},
					AuxParams = new AuxParams
					{
						ByBitVector = 0,
					},
					AdvSettingsParams = new AdvSettingsParams
					{
						ByBitVector1 = 0,
						VirtualQueueBehavior = 1,
						MaxNumInstance = 0,
						BelowThresholdBehavior = 0,
						ByBitVector2 = 0,
					},
					StateChunk = new StateChunk
					{
						StateGroups = new List<object>(), 
					},
					InitialRtpc = new InitialRtpc
					{
						RtpcList = new List<Rtpc>(),
					}
				}
			};
		}

		public static CAkSwitchCntr CreateCAkSwitchCntrForBattleVoOrders()
		{
			return new CAkSwitchCntr
			{
				NodeBaseParams = new NodeBaseParams
				{
					NodeInitialFxParams = new NodeInitialFxParams
					{
						IsOverrideParentFX = 0,
						FxChunks = new List<FxChunk> { },
					},
					OverrideAttachmentParams = 0,
					OverrideBusId = 0,
					DirectParentID = 0,
					ByBitVector = 0,
					NodeInitialParams = new NodeInitialParams
					{
						AkPropBundle1 = new AkPropBundle(),
						AkPropBundle2 = new AkPropBundle(),
					},
					PositioningParams = new PositioningParams
					{
						ByVector = 192,
					},
					AuxParams = new AuxParams
					{
						ByBitVector = 0,
					},
					AdvSettingsParams = new AdvSettingsParams
					{
						ByBitVector1 = 0,
						VirtualQueueBehavior = 1,
						MaxNumInstance = 0,
						BelowThresholdBehavior = 0,
						ByBitVector2 = 0,
					},
					StateChunk = new StateChunk
					{
						StateGroups = new List<object>(),
					},
					InitialRtpc = new InitialRtpc
					{
						RtpcList = new List<Rtpc>(),
					}
				},
				GroupType = 0,
				GroupId = 0,
				DefaultSwitch = 0,
				IsContinuousValidation = 0,
				ChildIds = new List<uint>(),
				SwitchPackages = new List<CAkSwitchPackage>(),
				SwitchParams = new List<AkSwitchNodeParams>(),
			};
		}
	}
}
