using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditorCore.Utility;
using SoundbankEditorCore.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SoundbankEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private List<HircItem>? _hircItems;
		public List<HircItem>? HircItems
		{
			get
			{
				return _hircItems;
			}
			set
			{
				_hircItems = value;
				OnPropertyChanged(nameof(HircItems));
			}
		}

		private string? _selectedHircItemJson;
		public string? SelectedHircItemJson
		{
			get
			{
				return _selectedHircItemJson;
			}
			set
			{
				_selectedHircItemJson = value;
				OnPropertyChanged(nameof(SelectedHircItemJson));
			}
		}

		private string? _selectedHircItemJsonErrorMessage;
		public string? SelectedHircItemJsonErrorMessage
		{
			get
			{
				return _selectedHircItemJsonErrorMessage;
			}
			set
			{
				_selectedHircItemJsonErrorMessage = value;
				OnPropertyChanged(nameof(SelectedHircItemJsonErrorMessage));
			}
		}

		private bool _areChangesPending;
		private bool _isProgrammaticallyChangingSelectedHircItemJson;
		private SoundBank? _openSoundbank;
		private string? _openSoundbankFilePath;
		private int _selectedHircItemIndex = -1;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			cbAddHircItemType.ItemsSource = Enum.GetNames(typeof(HircType)).Order();
		}

		//
		// Event Handlers
		//

		private void BtnOpen_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Filter = $"SoundBank|*.bnk";
			openFileDialog.Title = "Select SoundBank";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			_openSoundbankFilePath = openFileDialog.FileName;
			_openSoundbank = SoundBank.CreateFromBnkFile(_openSoundbankFilePath);
			HircItems = _openSoundbank.HircItems;

			WwiseShortIdUtility.ClearNames();
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList(), false);
			string customNamesPath = GetCustomNamesFilePath();
			if (File.Exists(customNamesPath))
			{
				WwiseShortIdUtility.AddNames(File.ReadAllLines(customNamesPath).ToList(), true);
			}

			_areChangesPending = false;
			UpdateTitle();
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || _openSoundbankFilePath == null)
			{
				return;
			}

			SaveFile();
		}

		private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null)
			{
				return;
			}

			var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			saveFileDialog.Filter = $"SoundBank|*.bnk";
			saveFileDialog.Title = "Save SoundBank";

			if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			_openSoundbankFilePath = saveFileDialog.FileName;
			SaveFile();
		}

		private void BtnAddHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null)
			{
				return;
			}

			string? selectedItemString = cbAddHircItemType.SelectedItem as string;
			if (selectedItemString == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set New HIRC Item ID");
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			HircType hircType = (HircType)Enum.Parse(typeof(HircType), selectedItemString);
			HircItem newHircItem = HircItemFactory.Create(hircType);
			newHircItem.UlID = hircItemIdConverterWindow.Id.Value;
			HircItems.Insert(0, newHircItem);

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnDeleteHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null)
			{
				return;
			}

			HircItem selectedHircItem = (HircItem)dgHircItems.SelectedItem;
			if (MessageBox.Show($"Are you sure you want to delete HIRC item '{selectedHircItem.UlID}'?", "Confirm HIRC Item Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			HircItems.Remove(selectedHircItem);
			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnEditHircItemId_Click(object sender, RoutedEventArgs e)
		{
			if (dgHircItems.SelectedItem == null)
			{
				return;
			}

			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set HIRC Item ID", selectedItem.UlID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			selectedItem.UlID = hircItemIdConverterWindow.Id.Value;
			UpdateSelectedHircItemJson();
			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnInvalidHircItemJson_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedHircItemJsonErrorMessage == null)
			{
				return;
			}

			MessageBox.Show($"{SelectedHircItemJsonErrorMessage}");
		}

		private void DgHircItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (dgHircItems.SelectedIndex < 0)
			{
				UpdateSelectedHircItemJson();

				btnDeleteHircItem.IsEnabled = false;
				tbHircItemJson.IsEnabled = false;
				return;
			}

			if (SelectedHircItemJsonErrorMessage != null && _selectedHircItemIndex != dgHircItems.SelectedIndex &&
					MessageBox.Show($" If you select a different HIRC item, you will lose unpersisted changes in your working HIRC item.", "Confirm HIRC Item Selection", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
			{
				dgHircItems.SelectionChanged -= DgHircItems_SelectionChanged;
				dgHircItems.SelectedIndex = _selectedHircItemIndex;
				dgHircItems.SelectionChanged += DgHircItems_SelectionChanged;
				dgHircItems.Items.Refresh();
				return;
			}

			_selectedHircItemIndex = dgHircItems.SelectedIndex;

			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;

			UpdateSelectedHircItemJson();

			btnDeleteHircItem.IsEnabled = true;
			tbHircItemJson.IsEnabled = true;

			shiev.HircItem = selectedItem;
		}

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private void TbHircItemJson_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (HircItems == null || SelectedHircItemJson == null || dgHircItems.SelectedIndex < 0)
			{
				return;
			}

			if (!_isProgrammaticallyChangingSelectedHircItemJson)
			{
				if (!_areChangesPending)
				{
					_areChangesPending = true;
					UpdateTitle();
				}
			}

			// Debounce logic.
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			Dispatcher.Invoke(async () => // Run task in current thread.
			{
				await Task.Delay(500);
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				try
				{
					HircItem existingHircItem = HircItems[dgHircItems.SelectedIndex];
					HircItem? newHircItem = JsonSerializer.Deserialize<HircItem>(SelectedHircItemJson);
					if (newHircItem == null)
					{
						throw new JsonException();
					}

					if (existingHircItem.GetType() != newHircItem.GetType())
					{
						throw new JsonException($"You cannot change the '$type'.");
					}

					using MemoryStream memoryStream = new MemoryStream();
					using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					newHircItem.WriteToBinary(binaryWriter); // Test to make sure it doesn't fail.

					if (!_isProgrammaticallyChangingSelectedHircItemJson && newHircItem.UlID != existingHircItem.UlID)
					{
						throw new JsonException($"You cannot change the item's ID in the JSON editor.");
					}

					newHircItem.CopyTo(existingHircItem);

					SelectedHircItemJsonErrorMessage = null;
					shiev.HircItem = newHircItem; // This shouldn't be.
				}
				catch (Exception ex)
				{
					SelectedHircItemJsonErrorMessage = ex.Message;
				}
			}, System.Windows.Threading.DispatcherPriority.Normal , cancellationToken);
		}

		HircItem? _rightClickedHircItem;
		private void DgHircItems_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

			var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
			DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
			while (cell != null && !(cell is DataGridCell)) cell = VisualTreeHelper.GetParent(cell);

			DataGridCell? clickedCell = cell as DataGridCell;
			if (clickedCell == null)
			{
				return;
			}

			HircItem? clickedHircItem = clickedCell.DataContext as HircItem;
			if (clickedHircItem == null)
			{
				return;
			}

			_rightClickedHircItem = clickedHircItem;
			dgHircItems.ContextMenu.IsOpen = true;
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (_rightClickedHircItem == null)
			{
				return;
			}

			Clipboard.SetText(_rightClickedHircItem.UlID.ToString());
		}

		private void Shiev_HircItemUpdated(object sender, EventArgs e)
		{
			UpdateSelectedHircItemJson();
			// dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
			{
				SaveFile();
			}
		}

		//
		// Helper Functions
		//

		private string GetCustomNamesFilePath()
		{
			if (_openSoundbankFilePath == null)
			{
				throw new Exception($"No soundbank is open.");
			}

			return $"{Path.GetDirectoryName(_openSoundbankFilePath)}\\{Path.GetFileNameWithoutExtension(_openSoundbankFilePath)}_custom_names.txt";
		}

		private void SaveFile()
		{
			if (_openSoundbank == null || _openSoundbankFilePath == null)
			{
				return;
			}

			_openSoundbank.WriteToBnkFile(_openSoundbankFilePath);
			File.WriteAllLines(GetCustomNamesFilePath(), WwiseShortIdUtility.GetAllNames(true));
			_areChangesPending = false;
			UpdateTitle();
		}

		private void UpdateSelectedHircItemJson()
		{
			string text = dgHircItems.SelectedItem != null
				? JsonSerializer.Serialize((HircItem)dgHircItems.SelectedItem, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
				: string.Empty;

			_isProgrammaticallyChangingSelectedHircItemJson = true;
			SelectedHircItemJson = text;
			_isProgrammaticallyChangingSelectedHircItemJson = false;
		}

		private void UpdateTitle()
		{
			Title = $"{(_areChangesPending ? "*" : string.Empty)}{System.IO.Path.GetFileName(_openSoundbankFilePath)} - Soundbank Editor";
		}
	}
}
