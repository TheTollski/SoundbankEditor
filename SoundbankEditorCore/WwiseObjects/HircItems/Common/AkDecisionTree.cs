using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditorCore.WwiseObjects.HircItems.Common
{
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
					currentNode.ChildrenIdx = 0;

					continue;
				}

				// This is a parent node.
				currentNode.AudioNodeId = 0;

				for (int j = 0; j < currentNode.ChildrenCount_ReadFromBinary; j++)
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

		public List<Node> FlattenTree()
		{
			List<Node> nodes = new List<Node>();
			nodes.Add(RootNode);

			Queue<Node> bfsNodeQueue = new Queue<Node>();
			bfsNodeQueue.Enqueue(RootNode);
			while (bfsNodeQueue.Count > 0)
			{
				Node currentNode = bfsNodeQueue.Dequeue();
				for (int i = 0; i < currentNode.Children.Count; i++)
				{
					Node childNode = currentNode.Children[i];

					bfsNodeQueue.Enqueue(childNode);
					nodes.Add(childNode);
				}
			}

			return nodes;
		}

		public Node? GetParentNode(Node childNode)
		{
			List<Node> nodes = FlattenTree();

			foreach (Node node in nodes)
			{
				if (node.Children.Contains(childNode))
				{
					return node;
				}
			}

			return null;
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
			// This function expects that all Nodes in the tree have their ChildrenIdx set correctly.

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
			foreach (var kvp in nodePositionDict)
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
		[JsonIgnore]
		public ushort ChildrenCount_ReadFromBinary { get; set; }
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
			ChildrenCount_ReadFromBinary = binaryReader.ReadUInt16();
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
				binaryWriter.Write((ushort)Children.Count);
			}
			binaryWriter.Write(Weight);
			binaryWriter.Write(Probability);
		}
	}
}
