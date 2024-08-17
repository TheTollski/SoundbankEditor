using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
			tvDecisionTree.ItemsSource = _cakDialogueEvent.AkDecisionTree.RootNode.Children;

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

		private void BtnDeleteGameSync_Click(object sender, RoutedEventArgs e)
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			AkGameSync selectedGameSync = (AkGameSync)dgGameSyncs.SelectedItem;
			if (MessageBox.Show($"Are you sure you want to delete GameSync '{selectedGameSync.Group}'?", "Confirm GameSync Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			_cakDialogueEvent.Arguments.GameSyncs.RemoveAt(dgGameSyncs.SelectedIndex);

			UpdateGameSyncsDataGrid();
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
			UpdateDecisionTreeTreeView();
			UpdateNodeAudioNodeIdTextBlock();
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
			UpdateDecisionTreeTreeView();
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
			UpdateDecisionTreeTreeView();
			UpdateNodeWeightTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void tvDecisionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			bool isANodeSelected = tvDecisionTree.SelectedItem != null;
			ifevNodeAudioNodeId.IsEnabled = isANodeSelected && (tvDecisionTree.SelectedItem as Node)!.Children.Count == 0;
			ifevNodeProbability.IsEnabled = isANodeSelected;
			ifevNodeWeight.IsEnabled = isANodeSelected;

			UpdateNodeAudioNodeIdTextBlock();
			UpdateNodeProbabilityTextBlock();
			UpdateNodeWeightTextBlock();
		}

		////
		//// Helpers
		////

		private void UpdateAllFields()
		{
			UpdateDecisionTreeTreeView();
			UpdateGameSyncsDataGrid();
			UpdateProbabilityTextBlock();
		}

		private void UpdateDecisionTreeTreeView()
		{
			if (_cakDialogueEvent == null)
			{
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

			AkGameSync? selectedGameSync = dgGameSyncs.SelectedItem as AkGameSync;
			dgGameSyncs.ItemsSource = _cakDialogueEvent.Arguments.GameSyncs;
			dgGameSyncs.Items.Refresh();

			if (selectedGameSync != null)
			{
				dgGameSyncs.SelectedIndex = _cakDialogueEvent.Arguments.GameSyncs.FindIndex(p => p.Group == selectedGameSync.Group);
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
	}
}
