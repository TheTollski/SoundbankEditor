using SoundbankEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundbankEditor
{
	/// <summary>
	/// Interaction logic for HircItemIdListControl.xaml
	/// </summary>
	public partial class HircItemIdListControl : UserControl
	{
		public event EventHandler? IdsUpdated;

		private string? _header;
		public string? Header
		{
			get { return _header; }
			set
			{
				_header = value;
				lHeader.Content = _header;
			}
		}

		private List<uint>? _ids;
		public List<uint>? Ids
		{
			get { return _ids; }
			set
			{
				_ids = value;

				btnAdd.IsEnabled = _ids != null;
				btnSort.IsEnabled = _ids != null;
				UpdateIdsDataGrid();
			}
		}

		public HircItemIdListControl()
		{
			InitializeComponent();
		}

		//
		// Event Handlers
		//

		private void BtnAdd_Click(object sender, RoutedEventArgs e)
		{
			if (Ids == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set ID");
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			Ids.Insert(0, hircItemIdConverterWindow.Id.Value);

			UpdateIdsDataGrid();
			IdsUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (Ids == null)
			{
				return;
			}

			uint selectedId = (uint)dgIds.SelectedItem;
			if (MessageBox.Show($"Are you sure you want to delete ID '{selectedId}'?", "Confirm ID Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			Ids.RemoveAt(dgIds.SelectedIndex);

			UpdateIdsDataGrid();
			IdsUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
		{
			if (Ids == null || dgIds.SelectedIndex < 0 || dgIds.SelectedIndex > Ids.Count - 2)
			{
				return;
			}

			var temp = Ids[dgIds.SelectedIndex + 1];
			Ids[dgIds.SelectedIndex + 1] = Ids[dgIds.SelectedIndex];
			Ids[dgIds.SelectedIndex] = temp;

			UpdateIdsDataGrid();
			IdsUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
		{
			if (Ids == null || dgIds.SelectedIndex < 1)
			{
				return;
			}

			var temp = Ids[dgIds.SelectedIndex - 1];
			Ids[dgIds.SelectedIndex - 1] = Ids[dgIds.SelectedIndex];
			Ids[dgIds.SelectedIndex] = temp;

			UpdateIdsDataGrid();
			IdsUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void BtnSort_Click(object sender, RoutedEventArgs e)
		{
			if (Ids == null)
			{
				return;
			}

			Ids.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a, b));

			UpdateIdsDataGrid();
			IdsUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void DgSwitches_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool isAnIdSelected = dgIds.SelectedIndex >= 0;
			btnDelete.IsEnabled = isAnIdSelected;
			btnMoveDown.IsEnabled = isAnIdSelected;
			btnMoveUp.IsEnabled = isAnIdSelected;
			ifevId.IsEnabled = isAnIdSelected;

			UpdateIdTextBlock();
		}

		private void IfevEditId_Click(object sender, EventArgs e)
		{
			if (Ids == null)
			{
				return;
			}

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
			IdsUpdated?.Invoke(this, EventArgs.Empty);
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
			ifevId.Value = selectedId != null
				? selectedId.ToString()
				: "";
		}
	}
}
