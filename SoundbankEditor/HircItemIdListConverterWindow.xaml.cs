﻿using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SoundbankEditor
{
	/// <summary>
	/// Interaction logic for HircItemIdConverterWindow.xaml
	/// </summary>
	public partial class HircItemIdListConverterWindow : Window
	{
		public HircItemIdListConverterWindow(List<uint> originalIds)
		{
			InitializeComponent();
			
			var ids = new List<uint>();
			ids.AddRange(originalIds);
			Ids = ids;

			UpdateIdsDataGrid();
		}

		public List<uint> Ids { get; private set; }

		//
		// Event Handlers
		//

		private void BtnAdd_Click(object sender, RoutedEventArgs e)
		{
			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set ID");
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			Ids.Insert(0, hircItemIdConverterWindow.Id.Value);

			UpdateIdsDataGrid();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			uint selectedId = (uint)dgIds.SelectedItem;
			if (MessageBox.Show($"Are you sure you want to delete ID '{selectedId}'?", "Confirm ID Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			Ids.RemoveAt(dgIds.SelectedIndex);

			UpdateIdsDataGrid();
		}

		private void BtnEditId_Click(object sender, RoutedEventArgs e)
		{
			uint? selectedId = dgIds.SelectedItem as uint?;
			if (selectedId == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set ID", selectedId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			Ids[dgIds.SelectedIndex] = hircItemIdConverterWindow.Id.Value;
			UpdateIdsDataGrid();
			UpdateIdTextBlock();
		}

		private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
		{
			if (dgIds.SelectedIndex < 0 || dgIds.SelectedIndex > Ids.Count - 2)
			{
				return;
			}

			var temp = Ids[dgIds.SelectedIndex + 1];
			Ids[dgIds.SelectedIndex + 1] = Ids[dgIds.SelectedIndex];
			Ids[dgIds.SelectedIndex] = temp;

			UpdateIdsDataGrid();
		}

		private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
		{
			if (dgIds.SelectedIndex < 1)
			{
				return;
			}

			var temp = Ids[dgIds.SelectedIndex - 1];
			Ids[dgIds.SelectedIndex - 1] = Ids[dgIds.SelectedIndex];
			Ids[dgIds.SelectedIndex] = temp;

			UpdateIdsDataGrid();
		}

		private void BtnSort_Click(object sender, RoutedEventArgs e)
		{
			Ids.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a, b));

			UpdateIdsDataGrid();
		}

		private void DgSwitches_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool isAnIdSelected = dgIds.SelectedIndex >= 0;
			btnDelete.IsEnabled = isAnIdSelected;
			btnMoveDown.IsEnabled = isAnIdSelected;
			btnMoveUp.IsEnabled = isAnIdSelected;
			btnEditId.IsEnabled = isAnIdSelected;

			UpdateIdTextBlock();
		}

		//
		// Helpers
		//

		private void UpdateIdsDataGrid()
		{
			int selectedIndex = dgIds.SelectedIndex;
			dgIds.ItemsSource = Ids;
			dgIds.Items.Refresh();
			dgIds.SelectedIndex = selectedIndex;
		}

		private void UpdateIdTextBlock()
		{
			uint? selectedId = dgIds.SelectedItem as uint?;
			tbId.Text = $"ID: {(selectedId != null ? WwiseShortIdUtility.ConvertShortIdToReadableString(selectedId.Value) : "")}";
		}
	}
}