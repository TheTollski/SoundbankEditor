using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundbankEditor.SpecificHircItemEditorViews
{
	/// <summary>
	/// Interaction logic for DialogueEventHircItemEditorView.xaml
	/// </summary>
	public partial class DialogueEventHircItemEditorView : UserControl
	{
		private CakDialogueEvent? _cakDialogueEvent;

		public event EventHandler? HircItemUpdated;

		public DialogueEventHircItemEditorView()
		{
			InitializeComponent();
			MainWindow.OnHircItemUpdated += UpdateAllFields;
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakDialogueEvent = (CakDialogueEvent)DataContext;

			UpdateAllFields();
		}

		private void BtnAddGameSync_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			_cakDialogueEvent.Arguments.GameSyncs.Insert(0, new AkGameSync());

			UpdateGameSyncsDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnAddNode_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				selectedNode = _cakDialogueEvent.AkDecisionTree.RootNode;
			}

			if (selectedNode.AudioNodeId != 0)
			{
				throw new Exception($"Cannot add children to a node with AudioNodeId set.");
			}

			Node newNode = new Node();
			newNode.Weight = 50;
			newNode.Probability = 100;

			AddChildNode(selectedNode, newNode);

			UpdateDecisionTreeTreeView(false);
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnDeleteGameSync_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			if (selectedGameSync == null)
			{
				return;
			}

			if (MessageBox.Show($"Are you sure you want to delete GameSync '{selectedGameSync.Group}'?", "Confirm GameSync Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			_cakDialogueEvent.Arguments.GameSyncs.RemoveAt(dgGameSyncs.SelectedIndex);

			UpdateGameSyncsDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnDeleteNode_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			if (MessageBox.Show($"Are you sure you want to delete Node '{selectedNode.Key}'?", "Confirm Node Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			Node? parentNode = _cakDialogueEvent.AkDecisionTree.GetParentNode(selectedNode);
			if (parentNode == null)
			{
				throw new Exception($"Cannot find parent node.");
			}

			parentNode.Children.Remove(selectedNode);

			List<Node> nodes = _cakDialogueEvent.AkDecisionTree.FlattenTree();
			foreach (Node node in nodes)
			{
				if (node.ChildrenIdx > parentNode.ChildrenIdx)
				{
					node.ChildrenIdx--;
				}
			}

			UpdateDecisionTreeTreeView(false);
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnDuplicateNode_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			Node? parentNode = _cakDialogueEvent.AkDecisionTree.GetParentNode(selectedNode);
			if (parentNode == null)
			{
				throw new Exception($"Cannot find parent node.");
			}

			Node? newNode = JsonSerializer.Deserialize<Node>(JsonSerializer.Serialize(selectedNode));
			if (newNode == null)
			{
				throw new Exception($"Cannot duplicate node.");
			}

			AddChildNode(parentNode, newNode, parentNode.Children.IndexOf(selectedNode) + 1);

			UpdateDecisionTreeTreeView(false);
			UpdateNodeProbabilityTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveGameSyncDown_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null || dgGameSyncs.SelectedIndex < 0 || dgGameSyncs.SelectedIndex > _cakDialogueEvent.Arguments.GameSyncs.Count - 2)
			{
				return;
			}

			var temp = _cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex + 1];
			_cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex + 1] = _cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex];
			_cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex] = temp;

			UpdateGameSyncsDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveGameSyncUp_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null || dgGameSyncs.SelectedIndex < 1)
			{
				return;
			}

			var temp = _cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex - 1];
			_cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex - 1] = _cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex];
			_cakDialogueEvent.Arguments.GameSyncs[dgGameSyncs.SelectedIndex] = temp;

			UpdateGameSyncsDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveNodeDown_Click(object sender, RoutedEventArgs e)
		{
			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			Node? parentNode = _cakDialogueEvent?.AkDecisionTree.GetParentNode(selectedNode);
			if (_cakDialogueEvent == null || parentNode == null)
			{
				return;
			}

			int selectedNodeIndex = parentNode.Children.IndexOf(selectedNode);
			if (selectedNodeIndex > parentNode.Children.Count - 2)
			{
				return;
			}

			var temp = parentNode.Children[selectedNodeIndex + 1];
			parentNode.Children[selectedNodeIndex + 1] = parentNode.Children[selectedNodeIndex];
			parentNode.Children[selectedNodeIndex] = temp;

			UpdateDecisionTreeTreeView(false);
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveNodeUp_Click(object sender, RoutedEventArgs e)
		{
			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			Node? parentNode = _cakDialogueEvent?.AkDecisionTree.GetParentNode(selectedNode);
			if (_cakDialogueEvent == null || parentNode == null)
			{
				return;
			}

			int selectedNodeIndex = parentNode.Children.IndexOf(selectedNode);
			if (selectedNodeIndex < 1)
			{
				return;
			}

			var temp = parentNode.Children[selectedNodeIndex - 1];
			parentNode.Children[selectedNodeIndex - 1] = parentNode.Children[selectedNodeIndex];
			parentNode.Children[selectedNodeIndex] = temp;

			UpdateDecisionTreeTreeView(false);
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void DgGameSyncs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool isAGameSyncSelected = dgGameSyncs.SelectedIndex >= 0;
			btnDeleteGameSync.IsEnabled = isAGameSyncSelected;
			btnMoveGameSyncDown.IsEnabled = isAGameSyncSelected;
			btnMoveGameSyncUp.IsEnabled = isAGameSyncSelected;
			ifevGroup.IsEnabled = isAGameSyncSelected;
			ifevGroupType.IsEnabled = isAGameSyncSelected;

			UpdateGroupTextBlock();
			UpdateGroupTypeTextBlock();
		}

		private void IfevEditProbability_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			var textInputWindow = new TextInputWindow("Set Probability", typeof(byte), _cakDialogueEvent.Probability.ToString());
			if (textInputWindow.ShowDialog() != true || textInputWindow.Value == null)
			{
				return;
			}

			_cakDialogueEvent.Probability = byte.Parse(textInputWindow.Value);

			UpdateProbabilityTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditGroup_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			if (selectedGameSync == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Group", selectedGameSync.Group);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			selectedGameSync.Group = hircItemIdConverterWindow.Id.Value;

			UpdateGameSyncsDataGrid();
			UpdateGroupTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditGroupType_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			if (selectedGameSync == null)
			{
				return;
			}

			var textInputWindow = new TextInputWindow("Set Group Type", typeof(byte), selectedGameSync.GroupType.ToString());
			if (textInputWindow.ShowDialog() != true || textInputWindow.Value == null)
			{
				return;
			}

			selectedGameSync.GroupType = byte.Parse(textInputWindow.Value);

			UpdateGameSyncsDataGrid();
			UpdateGroupTypeTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditNodeAudioNodeId_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Node AudioNodeId", selectedNode.AudioNodeId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			selectedNode.AudioNodeId = hircItemIdConverterWindow.Id.Value;

			UpdateDecisionTreeTreeView(false);
			UpdateNodeAudioNodeIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditNodeKey_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Node Key", selectedNode.Key);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			selectedNode.Key = hircItemIdConverterWindow.Id.Value;

			UpdateDecisionTreeTreeView(false);
			UpdateNodeKeyTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditNodeProbability_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			var textInputWindow = new TextInputWindow("Set Node Probability", typeof(ushort), selectedNode.Probability.ToString());
			if (textInputWindow.ShowDialog() != true || textInputWindow.Value == null)
			{
				return;
			}

			selectedNode.Probability = ushort.Parse(textInputWindow.Value);

			UpdateDecisionTreeTreeView(false);
			UpdateNodeProbabilityTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditNodeWeight_Click(object sender, EventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			if (selectedNode == null)
			{
				return;
			}

			var textInputWindow = new TextInputWindow("Set Node Weight", typeof(ushort), selectedNode.Weight.ToString());
			if (textInputWindow.ShowDialog() != true || textInputWindow.Value == null)
			{
				return;
			}

			selectedNode.Weight = ushort.Parse(textInputWindow.Value);

			UpdateDecisionTreeTreeView(false);
			UpdateNodeWeightTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void tvDecisionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			bool isANodeSelected = tvDecisionTree.SelectedItem != null;
			btnAddNode.IsEnabled = !isANodeSelected ? true : (tvDecisionTree.SelectedItem as Node)!.AudioNodeId == 0;
			//btnDuplicateNode.IsEnabled = isANodeSelected;
			btnDeleteNode.IsEnabled = isANodeSelected;
			btnMoveNodeDown.IsEnabled = isANodeSelected;
			btnMoveNodeUp.IsEnabled = isANodeSelected;
			ifevNodeAudioNodeId.IsEnabled = isANodeSelected && (tvDecisionTree.SelectedItem as Node)!.Children.Count == 0;
			ifevNodeKey.IsEnabled = isANodeSelected;
			ifevNodeProbability.IsEnabled = isANodeSelected;
			ifevNodeWeight.IsEnabled = isANodeSelected;

			UpdateNodeAudioNodeIdTextBlock();
			UpdateNodeKeyTextBlock();
			UpdateNodeProbabilityTextBlock();
			UpdateNodeWeightTextBlock();
		}

		////
		//// Helpers
		////

		private void UpdateAllFields()
		{
			UpdateDecisionTreeTreeView(true);
			UpdateGameSyncsDataGrid();
			UpdateProbabilityTextBlock();
		}

		private void UpdateDecisionTreeTreeView(bool updateItemSource)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			if (updateItemSource)
			{
				tvDecisionTree.ItemsSource = _cakDialogueEvent.AkDecisionTree.RootNode.Children;
				return;
			}

			// Get tree layout.
			Node? selectedNode = tvDecisionTree.SelectedItem as Node;

			HashSet<Node> expandedNodes = new HashSet<Node>();
			foreach (var item in tvDecisionTree.Items)
			{
				TreeViewItem? treeViewItem = tvDecisionTree.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				if (treeViewItem == null)
				{
					continue;
				}

				expandedNodes.UnionWith(GetExpandedNodes(treeViewItem));
			}

			// Refresh tree data.
			tvDecisionTree.Items.Refresh();

			// Update tree layout.
			foreach (var item in tvDecisionTree.Items)
			{
				TreeViewItem? treeViewItem = tvDecisionTree.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				if (treeViewItem == null)
				{
					continue;
				}

				ExpandNodes(expandedNodes, selectedNode, treeViewItem);
			}
		}

		private HashSet<Node> GetExpandedNodes(TreeViewItem treeViewItem)
		{
			HashSet<Node> expandedNodes = new HashSet<Node>();
			
			if (treeViewItem.IsExpanded)
			{
				Node node = (Node)treeViewItem.Header;
				expandedNodes.Add(node);
			}

			foreach (var childItem in treeViewItem.Items)
			{
				TreeViewItem? childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
				if (childTreeViewItem == null)
				{
					continue;
				}

				expandedNodes.UnionWith(GetExpandedNodes(childTreeViewItem));
			}

			return expandedNodes;
		}

		private void ExpandNodes(HashSet<Node> expandedNodes, Node? selectedNode, TreeViewItem treeViewItem)
		{
			Node node = (Node)treeViewItem.Header;

			if (expandedNodes.Contains(node))
			{
				treeViewItem.IsExpanded = true;
				tvDecisionTree.UpdateLayout();
			}
			if (node == selectedNode)
			{
				treeViewItem.IsSelected = true;
				tvDecisionTree.UpdateLayout();
			}

			foreach (var childItem in treeViewItem.Items)
			{
				TreeViewItem? childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
				if (childTreeViewItem == null)
				{
					continue;
				}

				ExpandNodes(expandedNodes, selectedNode, childTreeViewItem);
			}
		}

		private void UpdateGameSyncsDataGrid()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			int selectedIndex = dgGameSyncs.SelectedIndex;
			dgGameSyncs.ItemsSource = _cakDialogueEvent.Arguments.GameSyncs;
			dgGameSyncs.Items.Refresh();

			if (selectedIndex >= 0)
			{
				dgGameSyncs.SelectedIndex = selectedIndex;
			}
		}

		private void UpdateGroupTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			ifevGroup.Value = selectedGameSync != null
				? selectedGameSync.Group.ToString()
				: "";
		}

		private void UpdateGroupTypeTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			ifevGroupType.Value = selectedGameSync != null
				? selectedGameSync.GroupType.ToString()
				: "";
		}

		private void UpdateNodeAudioNodeIdTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			ifevNodeAudioNodeId.Value = selectedNode != null && selectedNode.Children.Count == 0
				? selectedNode.AudioNodeId.ToString()
				: "";
		}

		private void UpdateNodeKeyTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			ifevNodeKey.Value = selectedNode != null
				? selectedNode.Key.ToString()
				: "";
		}

		private void UpdateNodeProbabilityTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			ifevNodeProbability.Value = selectedNode != null
				? selectedNode.Probability.ToString()
				: "";
		}

		private void UpdateNodeWeightTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			Node? selectedNode = tvDecisionTree.SelectedItem as Node;
			ifevNodeWeight.Value = selectedNode != null
				? selectedNode.Weight.ToString()
				: "";
		}

		private void UpdateProbabilityTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			ifevProbability.Value = _cakDialogueEvent.Probability.ToString(); ;
		}

		private void AddChildNode(Node parentNode, Node childNode, int? indexToInsert = null)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			List<Node> nodes = _cakDialogueEvent.AkDecisionTree.FlattenTree();
			if (parentNode.Children.Count == 0)
			{
				parentNode.ChildrenIdx = (ushort)(nodes.Count);
			}
			else
			{
				foreach (Node node in nodes)
				{
					if (node.ChildrenIdx > parentNode.ChildrenIdx)
					{
						node.ChildrenIdx++;
					}
				}
			}

			if (indexToInsert == null)
			{
				parentNode.Children.Add(childNode);
			}
			else
			{
				parentNode.Children.Insert(indexToInsert.Value, childNode);
			}

			if (childNode.Children.Count == 0)
			{
				return;
			}

			int nextChildIdx = nodes.Count + 1;

			Queue<Node> bfsNodeQueue = new Queue<Node>();
			bfsNodeQueue.Enqueue(childNode);
			while (bfsNodeQueue.Count > 0)
			{
				Node currentNode = bfsNodeQueue.Dequeue();

				if (currentNode.Children.Count > 0)
				{
					currentNode.ChildrenIdx = (ushort)nextChildIdx;
					nextChildIdx += currentNode.Children.Count;

					for (int i = 0; i < currentNode.Children.Count; i++)
					{
						bfsNodeQueue.Enqueue(currentNode.Children[i]);
					}
				}
			}
		}
	}
}
