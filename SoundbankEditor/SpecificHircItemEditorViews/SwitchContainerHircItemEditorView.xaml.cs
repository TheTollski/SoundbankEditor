using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundbankEditor.SpecificHircItemEditorViews
{
	/// <summary>
	/// Interaction logic for SwitchContainerHircItemEditorView.xaml
	/// </summary>
	public partial class SwitchContainerHircItemEditorView : UserControl
	{
		private CAkSwitchCntr? _cakSwitchCntr;

		public event EventHandler? HircItemUpdated;

		public SwitchContainerHircItemEditorView()
		{
			InitializeComponent();
			MainWindow.OnHircItemUpdated += UpdateAllFields;
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakSwitchCntr = (CAkSwitchCntr)DataContext;

			UpdateAllFields();
		}

		private void BtnAddSwitch_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			_cakSwitchCntr.SwitchPackages.Insert(0, new CAkSwitchPackage());

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnDeleteSwitch_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage selectedSwitchPackage = (CAkSwitchPackage)dgSwitches.SelectedItem;
			if (MessageBox.Show($"Are you sure you want to delete switch '{selectedSwitchPackage.SwitchId}'?", "Confirm Switch Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			_cakSwitchCntr.SwitchPackages.RemoveAt(dgSwitches.SelectedIndex);
			foreach (uint nodeId in selectedSwitchPackage.NodeIds)
			{
				RemoveLinksToNodeIdIfUnused(nodeId);
			}

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveSwitchDown_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null || dgSwitches.SelectedIndex < 0 || dgSwitches.SelectedIndex > _cakSwitchCntr.SwitchPackages.Count - 2)
			{
				return;
			}

			var temp = _cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex + 1];
			_cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex + 1] = _cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex];
			_cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex] = temp;

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveSwitchUp_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null || dgSwitches.SelectedIndex < 1)
			{
				return;
			}

			var temp = _cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex - 1];
			_cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex - 1] = _cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex];
			_cakSwitchCntr.SwitchPackages[dgSwitches.SelectedIndex] = temp;

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnSortSwitches_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			_cakSwitchCntr.ChildIds.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a, b));
			_cakSwitchCntr.SwitchPackages.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a.SwitchId, b.SwitchId));
			_cakSwitchCntr.SwitchParams.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a.NodeId, b.NodeId));

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void DgSwitches_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool isASwitchSelected = dgSwitches.SelectedIndex >= 0;
			btnDeleteSwitch.IsEnabled = isASwitchSelected;
			btnMoveSwitchDown.IsEnabled = isASwitchSelected;
			btnMoveSwitchUp.IsEnabled = isASwitchSelected;
			ifevSwitchId.IsEnabled = isASwitchSelected;
			ifevNodeIds.IsEnabled = isASwitchSelected;

			UpdateNodeIdsTextBlock();
			UpdateSwitchIdTextBlock();
		}

		private void IfevEditDefaultSwitchId_Click(object sender, EventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Default Switch ID", _cakSwitchCntr.DefaultSwitch);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.DefaultSwitch = hircItemIdConverterWindow.Id.Value;
			UpdateDefaultSwitchIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditDirectParentId_Click(object sender, EventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Direct Parent ID", _cakSwitchCntr.NodeBaseParams.DirectParentID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.NodeBaseParams.DirectParentID = hircItemIdConverterWindow.Id.Value;
			UpdateDirectParentIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditGroupId_Click(object sender, EventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Group ID", _cakSwitchCntr.GroupId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.GroupId = hircItemIdConverterWindow.Id.Value;
			UpdateGroupIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditNodeIds_Click(object sender, EventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage? selectedSwitchPackage = dgSwitches.SelectedItem as CAkSwitchPackage;
			if (selectedSwitchPackage == null)
			{
				return;
			}

			var hircItemIdListConverterWindow = new HircItemIdListConverterWindow(selectedSwitchPackage.NodeIds);
			if (hircItemIdListConverterWindow.ShowDialog() != true)
			{
				return;
			}

			List<uint> addedIds = hircItemIdListConverterWindow.Ids.Where(id => !selectedSwitchPackage.NodeIds.Contains(id)).ToList();
			List<uint> removedIds = selectedSwitchPackage.NodeIds.Where(id => !hircItemIdListConverterWindow.Ids.Contains(id)).ToList();
			selectedSwitchPackage.NodeIds = hircItemIdListConverterWindow.Ids;

			foreach (uint nodeId in addedIds)
			{
				AddLinksToNodeIdIfNecessary(nodeId);
			}
			foreach (uint nodeId in removedIds)
			{
				RemoveLinksToNodeIdIfUnused(nodeId);
			}

			UpdateNodeIdsTextBlock();
			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditSwitchId_Click(object sender, EventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage? selectedSwitchPackage = dgSwitches.SelectedItem as CAkSwitchPackage;
			if (selectedSwitchPackage == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Switch ID", selectedSwitchPackage.SwitchId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			selectedSwitchPackage.SwitchId = hircItemIdConverterWindow.Id.Value;
			UpdateSwitchesDataGrid();
			UpdateSwitchIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		//
		// Helpers
		//

		private void AddLinksToNodeIdIfNecessary(uint nodeId)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			if (!_cakSwitchCntr.ChildIds.Any(id => id == nodeId))
			{
				_cakSwitchCntr.ChildIds.Add(nodeId);
				_cakSwitchCntr.ChildIds.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a, b));
			}

			if (!_cakSwitchCntr.SwitchParams.Any(sp => sp.NodeId == nodeId))
			{
				_cakSwitchCntr.SwitchParams.Add(new AkSwitchNodeParams { NodeId = nodeId, ByBitVector2 = 1 });
				_cakSwitchCntr.SwitchParams.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a.NodeId, b.NodeId));
			}
		}

		private void RemoveLinksToNodeIdIfUnused(uint nodeId)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			if (!_cakSwitchCntr.SwitchPackages.Any(sp => sp.NodeIds.Contains(nodeId)))
			{
				_cakSwitchCntr.ChildIds.RemoveAll(id => id == nodeId);
				_cakSwitchCntr.SwitchParams.RemoveAll(sp => sp.NodeId == nodeId);
			}
		}

		private void UpdateAllFields()
		{
			UpdateDefaultSwitchIdTextBlock();
			UpdateDirectParentIdTextBlock();
			UpdateGroupIdTextBlock();
			UpdateSwitchesDataGrid();
		}

		private void UpdateDefaultSwitchIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			ifevDefaultSwitchId.Value = _cakSwitchCntr.DefaultSwitch.ToString();
		}

		private void UpdateDirectParentIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			ifevParentId.Value = _cakSwitchCntr.NodeBaseParams.DirectParentID.ToString();
		}

		private void UpdateGroupIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			ifevGroupId.Value = _cakSwitchCntr.GroupId.ToString();
		}

		private void UpdateNodeIdsTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage? selectedSwitchPackage = dgSwitches.SelectedItem as CAkSwitchPackage;
			string nodeIdsString = selectedSwitchPackage != null
					? string.Join(',', selectedSwitchPackage.NodeIds.Select(id => WwiseShortIdUtility.ConvertShortIdToReadableString(id)))
					: "";

			ifevNodeIds.Value = nodeIdsString;
		}

		private void UpdateSwitchIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage? selectedSwitchPackage = dgSwitches.SelectedItem as CAkSwitchPackage;
			ifevSwitchId.Value = selectedSwitchPackage != null
				? selectedSwitchPackage.SwitchId.ToString()
				: "";
		}

		private void UpdateSwitchesDataGrid()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage? selectedSwitchPackage = dgSwitches.SelectedItem as CAkSwitchPackage;
			dgSwitches.ItemsSource = _cakSwitchCntr.SwitchPackages;
			dgSwitches.Items.Refresh();

			if (selectedSwitchPackage != null)
			{
				dgSwitches.SelectedIndex = _cakSwitchCntr.SwitchPackages.FindIndex(p => p.SwitchId == selectedSwitchPackage.SwitchId);
			}
		}
	}
}
