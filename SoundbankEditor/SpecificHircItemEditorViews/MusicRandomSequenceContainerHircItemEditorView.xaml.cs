using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditorCore.WwiseObjects.HircItems;
using SoundbankEditorCore.WwiseObjects.HircItems.Common;
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
	/// Interaction logic for MusicRandomSequenceContainerHircItemEditorView.xaml
	/// </summary>
	public partial class MusicRandomSequenceContainerHircItemEditorView : UserControl
	{
		private CAkMusicRanSeqCntr? _cakMusicRanSeqCntr;

		public event EventHandler? HircItemUpdated;

		public MusicRandomSequenceContainerHircItemEditorView()
		{
			InitializeComponent();
			MainWindow.OnHircItemUpdated += UpdateAllFields;
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakMusicRanSeqCntr = (CAkMusicRanSeqCntr)DataContext;

			UpdateAllFields();
		}

		private void tvPlaylistItems_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			bool isANodeSelected = tvPlaylistItems.SelectedItem != null;
		}

		////
		//// Helpers
		////

		private void UpdateAllFields()
		{
			UpdateDecisionTreeTreeView(true);
		}

		private void UpdateDecisionTreeTreeView(bool updateItemSource)
		{
			if (_cakMusicRanSeqCntr == null)
			{
				return;
			}

			if (updateItemSource)
			{
				tvPlaylistItems.ItemsSource = _cakMusicRanSeqCntr.PlaylistItems;
				return;
			}

			//// Get tree layout.
			//Node? selectedNode = tvDecisionTree.SelectedItem as Node;

			//HashSet<Node> expandedNodes = new HashSet<Node>();
			//foreach (var item in tvDecisionTree.Items)
			//{
			//	TreeViewItem? treeViewItem = tvDecisionTree.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
			//	if (treeViewItem == null)
			//	{
			//		continue;
			//	}

			//	expandedNodes.UnionWith(GetExpandedNodes(treeViewItem));
			//}

			//// Refresh tree data.
			//tvDecisionTree.Items.Refresh();

			//// Update tree layout.
			//foreach (var item in tvDecisionTree.Items)
			//{
			//	TreeViewItem? treeViewItem = tvDecisionTree.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
			//	if (treeViewItem == null)
			//	{
			//		continue;
			//	}

			//	ExpandNodes(expandedNodes, selectedNode, treeViewItem);
			//}
		}

		//private HashSet<Node> GetExpandedNodes(TreeViewItem treeViewItem)
		//{
		//	HashSet<Node> expandedNodes = new HashSet<Node>();
			
		//	if (treeViewItem.IsExpanded)
		//	{
		//		Node node = (Node)treeViewItem.Header;
		//		expandedNodes.Add(node);
		//	}

		//	foreach (var childItem in treeViewItem.Items)
		//	{
		//		TreeViewItem? childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
		//		if (childTreeViewItem == null)
		//		{
		//			continue;
		//		}

		//		expandedNodes.UnionWith(GetExpandedNodes(childTreeViewItem));
		//	}

		//	return expandedNodes;
		//}

		//private void ExpandNodes(HashSet<Node> expandedNodes, Node? selectedNode, TreeViewItem treeViewItem)
		//{
		//	Node node = (Node)treeViewItem.Header;

		//	if (expandedNodes.Contains(node))
		//	{
		//		treeViewItem.IsExpanded = true;
		//		tvDecisionTree.UpdateLayout();
		//	}
		//	if (node == selectedNode)
		//	{
		//		treeViewItem.IsSelected = true;
		//		tvDecisionTree.UpdateLayout();
		//	}

		//	foreach (var childItem in treeViewItem.Items)
		//	{
		//		TreeViewItem? childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
		//		if (childTreeViewItem == null)
		//		{
		//			continue;
		//		}

		//		ExpandNodes(expandedNodes, selectedNode, childTreeViewItem);
		//	}
		//}
	}
}
