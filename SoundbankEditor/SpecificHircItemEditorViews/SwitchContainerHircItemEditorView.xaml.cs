using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
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
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakSwitchCntr = (CAkSwitchCntr)DataContext;

			UpdateDefaultSwitchIdTextBlock();
			UpdateDirectParentIdTextBlock();
			UpdateGroupIdTextBlock();
			UpdateSwitchesDataGrid();
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
				if (!_cakSwitchCntr.SwitchPackages.Any(sp => sp.NodeIds.Contains(nodeId)))
				{
					_cakSwitchCntr.SwitchParams.RemoveAll(sp => sp.NodeId == nodeId);
				}
			}

			UpdateSwitchesDataGrid();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnEditDefaultSwitchId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow(_cakSwitchCntr.DefaultSwitch);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.DefaultSwitch = hircItemIdConverterWindow.Id.Value;
			UpdateDefaultSwitchIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnEditDirectParentId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow(_cakSwitchCntr.NodeBaseParams.DirectParentID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.NodeBaseParams.DirectParentID = hircItemIdConverterWindow.Id.Value;
			UpdateDirectParentIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnEditGroupId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow(_cakSwitchCntr.GroupId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSwitchCntr.GroupId = hircItemIdConverterWindow.Id.Value;
			UpdateGroupIdTextBlock();
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
		}

		//
		// Helpers
		//

		private void UpdateDefaultSwitchIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			tbDefaultSwitchId.Text = $"Default Switch ID: {WwiseShortIdUtility.ConvertShortIdToReadableString(_cakSwitchCntr.DefaultSwitch)}";
		}

		private void UpdateDirectParentIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			tbDirectParentId.Text = $"Parent ID: {WwiseShortIdUtility.ConvertShortIdToReadableString(_cakSwitchCntr.NodeBaseParams.DirectParentID)}";
		}

		private void UpdateGroupIdTextBlock()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			tbGroupId.Text = $"Group ID: {WwiseShortIdUtility.ConvertShortIdToReadableString(_cakSwitchCntr.GroupId)}";
		}

		private void UpdateSwitchesDataGrid()
		{
			if (_cakSwitchCntr == null)
			{
				return;
			}

			CAkSwitchPackage selectedSwitchPackage = (CAkSwitchPackage)dgSwitches.SelectedItem;
			dgSwitches.ItemsSource = _cakSwitchCntr.SwitchPackages;
			dgSwitches.Items.Refresh();

			if (selectedSwitchPackage != null)
			{
				dgSwitches.SelectedIndex = _cakSwitchCntr.SwitchPackages.FindIndex(p => p.SwitchId == selectedSwitchPackage.SwitchId);
			}
		}
	}
}
