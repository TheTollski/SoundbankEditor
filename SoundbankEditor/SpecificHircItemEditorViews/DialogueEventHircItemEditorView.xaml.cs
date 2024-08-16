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

		////
		//// Helpers
		////

		private void UpdateAllFields()
		{
			UpdateProbabilityTextBlock();
			UpdateGameSyncsDataGrid();
		}

		private void UpdateProbabilityTextBlock()
		{
			if (_cakDialogueEvent == null)
			{
				return;
			}

			ifevProbability.Value = _cakDialogueEvent.Probability.ToString(); ;
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
	}
}
