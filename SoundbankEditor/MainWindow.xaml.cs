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
using System.Text.RegularExpressions;
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
		public static Action? OnHircItemUpdated { get; set; }

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
			openFileDialog.Filter = $"Soundbank|*.bnk";
			openFileDialog.Title = "Select Soundbank";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			SoundBank soundBank = SoundBank.CreateFromBnkFile(openFileDialog.FileName);

			// Verify that the program can fully parse and save the soundbank.
			string soundBankJson = soundBank.ToJson();
			SoundBank copiedSoundBank = SoundBank.CreateFromJson(soundBankJson);

			using var memoryStream1 = new MemoryStream();
			using var memoryStream2 = new MemoryStream();
			using var binaryWriter1 = new BinaryWriter(memoryStream1);
			using var binaryWriter2 = new BinaryWriter(memoryStream2);
			soundBank.WriteToBinary(binaryWriter1);
			copiedSoundBank.WriteToBinary(binaryWriter2);
			memoryStream1.Position = 0;
			memoryStream2.Position = 0;
			if (!memoryStream1.ToArray().SequenceEqual(memoryStream2.ToArray()))
			{
				MessageBox.Show("Failed to open Soundbank. Unable to fully parse the soundbank.");
				return;
			}

			// Continue with opening the Soundbank
			_openSoundbankFilePath = openFileDialog.FileName;
			_openSoundbank = soundBank;
			HircItems = _openSoundbank.HircItems;

			WwiseShortIdUtility.ClearNames();
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList(), false);
			string customNamesPath = GetCustomNamesFilePath();
			if (File.Exists(customNamesPath))
			{
				WwiseShortIdUtility.AddNames(File.ReadAllLines(customNamesPath).ToList(), true);
			}

			btnAddHircItem.IsEnabled = true;
			btnClose.IsEnabled = true;
			btnConvertBnkToJson.IsEnabled = false;
			btnConvertJsonToBnk.IsEnabled = false;
			btnGoToHircItem.IsEnabled = true;
			btnSave.IsEnabled = true;
			btnSaveAs.IsEnabled = true;
			btnViewKnownValidationErrorCounts.IsEnabled = true;
			cbAddHircItemType.IsEnabled = true;
			tbGoToHircItemId.IsEnabled = true;

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
			saveFileDialog.Filter = $"Soundbank|*.bnk";
			saveFileDialog.Title = "Save Soundbank";

			if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			_openSoundbankFilePath = saveFileDialog.FileName;
			SaveFile();
		}
		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || _openSoundbankFilePath == null)
			{
				return;
			}

			if (_areChangesPending &&
					MessageBox.Show($"There are unsaved changes to this soundbank. If you close it now, you will lose these changes.", "Confirm Close", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
			{
				return;
			}

			_openSoundbankFilePath = null;
			_openSoundbank = null;
			HircItems = null;

			WwiseShortIdUtility.ClearNames();

			btnAddHircItem.IsEnabled = false;
			btnClose.IsEnabled = false;
			btnConvertBnkToJson.IsEnabled = true;
			btnConvertJsonToBnk.IsEnabled = true;
			btnGoToHircItem.IsEnabled = false;
			btnSave.IsEnabled = false;
			btnSaveAs.IsEnabled = false;
			btnViewKnownValidationErrorCounts.IsEnabled = false;
			cbAddHircItemType.IsEnabled = false;
			tbGoToHircItemId.IsEnabled = false;

			_areChangesPending = false;
			UpdateTitle();
		}

		private void BtnViewKnownValidationErrorCounts_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || HircItems == null)
			{
				return;
			}

			var hircItemsWithKnownValidationErrors = HircItems
				.Select(hi => new KeyValuePair<uint, int>(hi.UlID, hi.GetKnownValidationErrors(_openSoundbank).Count))
				.Where(kvp => kvp.Value > 0)
				.Select(kvp => $"{kvp.Key}: {kvp.Value}");

			MessageBox.Show(string.Join("\n", hircItemsWithKnownValidationErrors), "Hirc Items With Known Validation Errors");
		}

		private void BtnConvertBnkToJson_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank != null || _openSoundbankFilePath != null)
			{
				return;
			}

			var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Filter = $"Soundbank|*.bnk";
			openFileDialog.Title = "Select Soundbank";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			SoundBank soundBank = SoundBank.CreateFromBnkFile(openFileDialog.FileName);

			WwiseShortIdUtility.ClearNames();
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList(), false);
			string customNamesPath = GetCustomNamesFilePath(openFileDialog.FileName);
			if (File.Exists(customNamesPath))
			{
				WwiseShortIdUtility.AddNames(File.ReadAllLines(customNamesPath).ToList(), true);
			}

			string outputJsonFilePath = $"{Path.GetDirectoryName(openFileDialog.FileName)}\\{Path.GetFileNameWithoutExtension(openFileDialog.FileName)}.json";
			if (File.Exists(outputJsonFilePath) &&
					MessageBox.Show($"A file exists at '{outputJsonFilePath}'. If you proceed, you will overwrite this file.", "Confirm File Overwite", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
			{
				return;
			}

			soundBank.WriteToJsonFile(outputJsonFilePath);
			MessageBox.Show($"File successfully saved: '{outputJsonFilePath}'");
		}

		private void BtnConvertJsonToBnk_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank != null || _openSoundbankFilePath != null)
			{
				return;
			}

			var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Filter = $"JSON|*.json";
			openFileDialog.Title = "Select JSON File";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			string fileText = File.ReadAllText(openFileDialog.FileName);
			SoundBank soundBank = SoundBank.CreateFromJson(fileText);

			List<string> baseNames = File.ReadAllLines("TWA_Names.txt").ToList();
			List<string> customNames = Regex.Matches(fileText, "\\[\\w+\\]")
				.Select(m => m.Value.Substring(1, m.Value.Length - 2))
				.Distinct()
				.Where(s => !baseNames.Contains(s))
				.ToList();

			string outputBnkFilePath = $"{Path.GetDirectoryName(openFileDialog.FileName)}\\{Path.GetFileNameWithoutExtension(openFileDialog.FileName)}.bnk";
			if (File.Exists(outputBnkFilePath) &&
					MessageBox.Show($"A file exists at '{outputBnkFilePath}'. If you proceed, you will overwrite this file.", "Confirm File Overwite", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
			{
				return;
			}

			soundBank.WriteToBnkFile(outputBnkFilePath);
			File.WriteAllLines(GetCustomNamesFilePath(openFileDialog.FileName), customNames);

			MessageBox.Show($"File successfully saved: '{outputBnkFilePath}'");
		}

		private void BtnAbout_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show($"" +
				$"Soundbank Editor v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}\n" +
				$"Created by Tollski for Total War: Attila.",
				"About");
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

			HircItem? selectedHircItem = (HircItem)dgHircItems.SelectedItem;
			if (selectedHircItem == null)
			{
				return;
			}

			if (MessageBox.Show($"Are you sure you want to delete HIRC item '{selectedHircItem.UlID}'?", "Confirm HIRC Item Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			HircItems.Remove(selectedHircItem);

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnDuplicateHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null)
			{
				return;
			}

			HircItem? selectedHircItem = (HircItem)dgHircItems.SelectedItem;
			if (selectedHircItem == null)
			{
				return;
			}

			HircItem? newHircItem = JsonSerializer.Deserialize<HircItem>(JsonSerializer.Serialize(selectedHircItem));
			HircItems.Insert(dgHircItems.SelectedIndex, newHircItem);

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();

			dgHircItems.SelectedIndex = dgHircItems.SelectedIndex - 1;
		}

		private void BtnGotoHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (HircItems == null)
			{
				return;
			}

			uint shortId;
			if (!uint.TryParse(tbGoToHircItemId.Text, out shortId))
			{
				shortId = WwiseShortIdUtility.ConvertToShortId(tbGoToHircItemId.Text);
			}

			int index = HircItems.FindIndex(hi => hi.UlID == shortId);
			if (index < 0)
			{
				return;
			}

			dgHircItems.SelectedIndex = index;
			dgHircItems.ScrollIntoView(HircItems[index]);
		}

		private void BtnInvalidHircItemJson_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedHircItemJsonErrorMessage == null)
			{
				return;
			}

			MessageBox.Show($"{SelectedHircItemJsonErrorMessage}");
		}

		private void BtnMoveHircItemDown_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || dgHircItems.SelectedIndex < 0 || dgHircItems.SelectedIndex > HircItems.Count - 2)
			{
				return;
			}

			var temp = HircItems[dgHircItems.SelectedIndex + 1];
			HircItems[dgHircItems.SelectedIndex + 1] = HircItems[dgHircItems.SelectedIndex];
			HircItems[dgHircItems.SelectedIndex] = temp;

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnMoveHircItemUp_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || dgHircItems.SelectedIndex < 1)
			{
				return;
			}

			var temp = HircItems[dgHircItems.SelectedIndex - 1];
			HircItems[dgHircItems.SelectedIndex - 1] = HircItems[dgHircItems.SelectedIndex];
			HircItems[dgHircItems.SelectedIndex] = temp;

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnViewKnownValidationErrorCount_Click(object sender, RoutedEventArgs e)
		{
			if (dgHircItems.SelectedItem == null || _openSoundbank == null)
			{
				return;
			}

			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;
			MessageBox.Show(string.Join("\n", selectedItem.GetKnownValidationErrors(_openSoundbank)), "Known Validation Errors");
		}

		private void DgHircItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (dgHircItems.SelectedIndex < 0)
			{
				UpdateKnownValidationErrorsCount();
				UpdateSelectedHircItemJson();

				btnDeleteHircItem.IsEnabled = false;
				btnDuplicateHircItem.IsEnabled = false;
				btnMoveHircItemDown.IsEnabled = false;
				btnMoveHircItemUp.IsEnabled = false;
				dpHircItemEditor.Visibility = Visibility.Hidden;
				tbHircItemJson.IsEnabled = false;

				ifevItemId.Value = null;
				shiev.HircItem = null;
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

			UpdateKnownValidationErrorsCount();
			UpdateSelectedHircItemJson();

			btnDeleteHircItem.IsEnabled = true;
			btnDuplicateHircItem.IsEnabled = true;
			btnMoveHircItemDown.IsEnabled = dgHircItems.SelectedIndex < HircItems.Count - 1;
			btnMoveHircItemUp.IsEnabled = dgHircItems.SelectedIndex > 0;
			dpHircItemEditor.Visibility = Visibility.Visible;
			tbHircItemJson.IsEnabled = true;

			ifevItemId.Value = selectedItem.UlID.ToString();
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

					if (!_isProgrammaticallyChangingSelectedHircItemJson)
					{
						OnHircItemUpdated?.Invoke();
						UpdateKnownValidationErrorsCount();
					}
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

		private void IfevItemId_EditClicked(object sender, EventArgs e)
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

			UpdateKnownValidationErrorsCount();
			UpdateSelectedHircItemJson();
			dgHircItems.Items.Refresh();
			ifevItemId.Value = selectedItem.UlID.ToString();
			_areChangesPending = true;
			UpdateTitle();
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
			UpdateKnownValidationErrorsCount();

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

		private string GetCustomNamesFilePath(string? soundbankFilePath = null)
		{
			if (soundbankFilePath == null)
			{
				soundbankFilePath = _openSoundbankFilePath;
			}

			if (soundbankFilePath == null)
			{
				throw new Exception($"Unable to identify soundbank file path.");
			}

			return $"{Path.GetDirectoryName(soundbankFilePath)}\\{Path.GetFileNameWithoutExtension(soundbankFilePath)}_custom_names.txt";
		}

		private void SaveFile()
		{
			if (_openSoundbank == null || _openSoundbankFilePath == null)
			{
				return;
			}

			_openSoundbank.WriteToBnkFile(_openSoundbankFilePath);

			string soundbankJson = _openSoundbank.ToJson();
			List<string> customNames = WwiseShortIdUtility
				.GetAllNames(true)
				.Where(n => soundbankJson.Contains(WwiseShortIdUtility.ConvertToShortId(n).ToString()))
				.ToList();
			File.WriteAllLines(GetCustomNamesFilePath(), customNames);

			_areChangesPending = false;
			UpdateTitle();
		}

		private void UpdateKnownValidationErrorsCount()
		{
			if (dgHircItems.SelectedItem == null || _openSoundbank == null)
			{
				tbKnownValidationErrorCount.Foreground = new SolidColorBrush(Colors.Black);
				tbKnownValidationErrorCount.Text = $"Known Validation Error Count:";
				return;
			}

			int knownValidationErrorsCount = ((HircItem)dgHircItems.SelectedItem).GetKnownValidationErrors(_openSoundbank).Count;

			Color color = knownValidationErrorsCount == 0 ? Colors.Black : Colors.Red;
			tbKnownValidationErrorCount.Foreground = new SolidColorBrush(color);
			tbKnownValidationErrorCount.Text = $"Known Validation Error Count: {knownValidationErrorsCount}";
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
