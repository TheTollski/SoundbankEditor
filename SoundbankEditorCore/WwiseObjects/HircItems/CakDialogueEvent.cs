using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CakDialogueEvent : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Dialogue_Event;

		private HircType _hircType;
		public HircType EHircType
		{
			get
			{
				return _hircType;
			}
			set
			{
				if (value != EXPECTED_HIRC_TYPE)
				{
					throw new Exception($"HIRC item of type '{GetType().Name}' cannot have {nameof(EHircType)} of '{(byte)value}', it must be '{(byte)EXPECTED_HIRC_TYPE}'.");
				}

				_hircType = value;
			}
		}

		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint UlID { get; set; }
		public byte Probability { get; set; }
		public uint TreeDepth { get; set; }
		public DialogueEventArgumentList Arguments { get; set; } = new DialogueEventArgumentList();
		public uint TreeDataSize { get; set; }
		public byte Mode { get; set; }
		public AkDecisionTree AkDecisionTree { get; set; }

		public CakDialogueEvent()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CakDialogueEvent(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			Probability = binaryReader.ReadByte();
			TreeDepth = binaryReader.ReadUInt32();
			Arguments = new DialogueEventArgumentList(binaryReader, TreeDepth);
			TreeDataSize = binaryReader.ReadUInt32();
			Mode = binaryReader.ReadByte();
			AkDecisionTree = new AkDecisionTree(binaryReader, TreeDepth, TreeDataSize);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CakDialogueEvent '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 19 + Arguments.ComputeTotalSize() + AkDecisionTree.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CakDialogueEvent '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate 
			//if (ActionIds.Count == 0)
			//{
			//	knownValidationErrors.Add($"CAkEvent '{UlID}' has no action IDs.");
			//}
			//ActionIds.ForEach(actionId =>
			//{
			//	if (!soundbank.HircItems.Any(hi => hi.UlID == actionId))
			//	{
			//		knownValidationErrors.Add($"CAkEvent '{UlID}' has an ActionId that is '{actionId}', but no HIRC item in the soundbank has that ID.");
			//	}
			//});

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			binaryWriter.Write(Probability);
			binaryWriter.Write(TreeDepth);
			Arguments.WriteToBinary(binaryWriter);
			binaryWriter.Write(TreeDataSize);
			binaryWriter.Write(Mode);
			AkDecisionTree.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CakDialogueEvent '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class DialogueEventArgumentList : WwiseObject
	{
		public List<AkGameSync> GameSyncs { get; set; } = new List<AkGameSync>();

		public DialogueEventArgumentList() { }

		public DialogueEventArgumentList(BinaryReader binaryReader, uint count)
		{
			for (int i = 0; i < count; i++)
			{
				GameSyncs.Add(new AkGameSync { Group = binaryReader.ReadUInt32() });
			}
			for (int i = 0; i < count; i++)
			{
				GameSyncs[i].GroupType = binaryReader.ReadByte();
			}
		}

		public uint ComputeTotalSize()
		{
			return (uint)(5 * GameSyncs.Count);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			for (int i = 0; i < GameSyncs.Count; i++)
			{
				binaryWriter.Write(GameSyncs[i].Group);
			}
			for (int i = 0; i < GameSyncs.Count; i++)
			{
				binaryWriter.Write(GameSyncs[i].GroupType);
			}
		}
	}

	public class AkGameSync
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Group { get; set; }
		public byte GroupType { get; set; }

		public AkGameSync() { }

		// Note: An AkGameSync is not stored as a whole in the binary. For a list, all the AkGameSync Groups are stored and then all the GroupTypes are stored.
	}

	public class AkDecisionTree : WwiseObject
	{
		public Node RootNode { get; set; }

		public AkDecisionTree() { }

		public AkDecisionTree(BinaryReader binaryReader, uint treeDepth, uint treeDataSize)
		{
			long position = binaryReader.BaseStream.Position;

		  var nodes = new List<Node>();
			while (position + treeDataSize > binaryReader.BaseStream.Position)
			{
				nodes.Add(new Node(binaryReader));
			}

			RootNode = nodes[0];

			Dictionary<Node, int> nodeDepthDict = new Dictionary<Node, int>();
			nodeDepthDict.Add(RootNode, 0);
			for (int i = 0; i < nodes.Count; i++)
			{
				Node currentNode = nodes[i];
				int currentDepth = nodeDepthDict[currentNode];

				if (currentDepth == treeDepth)
				{
					// This is an audio node.
					currentNode.ChildrenCount = 0;
					currentNode.ChildrenIdx = 0;

					continue;
				}

				// This is a parent node.
				currentNode.AudioNodeId = 0;

				for (int j = 0; j < currentNode.ChildrenCount; j++)
				{
					Node childNode = nodes[currentNode.ChildrenIdx + j];
					currentNode.Children.Add(childNode);
					nodeDepthDict.Add(childNode, currentDepth + 1);
				}
			}
		}

		public uint ComputeTotalSize()
		{
			Func<Node, uint> func = (node) =>
			{
				return node.ComputeTotalSize();
			};

			List<uint> nodeSizes = TraverseTree(func);
			return (uint)nodeSizes.Sum(x => x);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			Func<Node, bool> func = (node) =>
			{
				node.WriteToBinary(binaryWriter);
				return true;
			};

			TraverseTree(func);
		}

		private List<T> TraverseTree<T>(Func<Node, T> nodeFunc)
		{
			// Nodes are located near their siblings but these groups of sibling nodes can seemingly be stored randomly throughout the section.

			SortedDictionary<int, Node> nodePositionDict = new SortedDictionary<int, Node>
			{
				[0] = RootNode
			};
			Queue<Node> bfsNodeQueue = new Queue<Node>();
			bfsNodeQueue.Enqueue(RootNode);
			while (bfsNodeQueue.Count > 0)
			{
				Node currentNode = bfsNodeQueue.Dequeue();
				for (int i = 0; i < currentNode.Children.Count; i++)
				{
					Node childNode = currentNode.Children[i];

					bfsNodeQueue.Enqueue(childNode);
					nodePositionDict.Add(currentNode.ChildrenIdx + i, childNode);
				}
			}

			List<T> results = new List<T>();
			int expectedIndex = 0;
			foreach (var kvp  in nodePositionDict)
			{
				if (kvp.Key != expectedIndex)
				{
					throw new Exception($"Expected index for node is {expectedIndex} but it's actual index is {kvp.Key}");
				}

				results.Add(nodeFunc(kvp.Value));
				expectedIndex++;
			}

			return results;
		}
	}

	public class Node
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Key { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint AudioNodeId { get; set; }
		public ushort ChildrenIdx { get; set; }
		public ushort ChildrenCount { get; set; }
		public ushort Weight { get; set; }
		public ushort Probability { get; set; }
		public List<Node> Children { get; set; } = new List<Node>();

		public Node() { }

		public Node(BinaryReader binaryReader)
		{
			Key = binaryReader.ReadUInt32();
			AudioNodeId = binaryReader.ReadUInt32();
			binaryReader.BaseStream.Position -= 4; // The AudioNodeId is stored at the same position as the ChildrenIdx and ChildrenCount, depending on the node type.
			ChildrenIdx = binaryReader.ReadUInt16();
			ChildrenCount = binaryReader.ReadUInt16();
			Weight = binaryReader.ReadUInt16();
			Probability = binaryReader.ReadUInt16();
		}

		public uint ComputeTotalSize()
		{
			return 12;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Key);
			if (AudioNodeId != 0)
			{
				binaryWriter.Write(AudioNodeId);
			}
			else
			{
				binaryWriter.Write(ChildrenIdx);
				binaryWriter.Write(ChildrenCount);
			}
			binaryWriter.Write(Weight);
			binaryWriter.Write(Probability);
		}
	}
}
