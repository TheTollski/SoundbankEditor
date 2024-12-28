using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditorCore.Utility;
using SoundbankEditorCore.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
			openFileDialog.Title = "Select Soundbank to Open";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			SoundBank soundBank = SoundBank.CreateFromBnkFile(openFileDialog.FileName);

			// Verify that the program can fully parse and save the soundbank.
			string soundBankJson = soundBank.ToJson();
			SoundBank copiedSoundBank = SoundBank.CreateFromJson(soundBankJson);

			using FileStream inputBnkFileStream = File.OpenRead(openFileDialog.FileName);
			byte[] inputBnkBytes = new byte[inputBnkFileStream.Length];
			inputBnkFileStream.Position = 0;
			inputBnkFileStream.Read(inputBnkBytes, 0, inputBnkBytes.Length);

			using var outputBnkMemoryStream = new MemoryStream();
			using var outputBnkBinaryWriter = new BinaryWriter(outputBnkMemoryStream);
			copiedSoundBank.WriteToBinary(outputBnkBinaryWriter);
			outputBnkMemoryStream.Position = 0;
			byte[] outputBnkBytes = outputBnkMemoryStream.ToArray();

			if (!inputBnkBytes.SequenceEqual(outputBnkBytes))
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
			btnImportHircItem.IsEnabled = true;
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
			btnImportHircItem.IsEnabled = false;
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

		private void BtnDeleteHircItems_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || dgHircItems.SelectedItems.Count < 1)
			{
				return;
			}

			List<HircItem> selectedHircItems = new List<HircItem>();
			foreach (object selectedItem in dgHircItems.SelectedItems)
			{
				selectedHircItems.Add((HircItem)selectedItem);
			}

			if (MessageBox.Show($"Are you sure you want to delete {selectedHircItems.Count} HIRC items?", "Confirm HIRC Item Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				return;
			}

			foreach (HircItem item in selectedHircItems)
			{
				HircItems.Remove(item);
			}

			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnDuplicateHircItems_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || dgHircItems.SelectedItems.Count < 1)
			{
				return;
			}

			List<HircItem> selectedHircItems = new List<HircItem>();
			foreach (object selectedItem in dgHircItems.SelectedItems)
			{
				selectedHircItems.Add((HircItem)selectedItem);
			}

			var patternTextInputWindow = new TextInputWindow(
				$"Duplicating {selectedHircItems.Count} items - REGEX Pattern",
				$"Input optional REGEX pattern to replace in ID names.\nLeave blank to keep same IDs in duplicated items.\nNote: This only affects Random Sequence Container and Sound items.",
				typeof(string),
				"");
			if (patternTextInputWindow.ShowDialog() != true || patternTextInputWindow.Value == null)
			{
				return;
			}

			string pattern = patternTextInputWindow.Value;
			string replacement = "";
			if (pattern != "")
			{
				var replacementTextInputWindow = new TextInputWindow(
					$"Duplicating {selectedHircItems.Count} items - Replacement Text",
					$"Input text to use to replace matches.",
					typeof(string),
					"");
				if (replacementTextInputWindow.ShowDialog() != true || replacementTextInputWindow.Value == null)
				{
					return;
				}

				replacement = replacementTextInputWindow.Value;
			}

			for (int i = selectedHircItems.Count - 1; i >= 0; i--)
			{
				HircItem? newHircItem = JsonSerializer.Deserialize<HircItem>(JsonSerializer.Serialize(selectedHircItems[i]));
				HircItems.Insert(dgHircItems.SelectedIndex, newHircItem);

				if (pattern == "")
				{
					continue;
				}

				Func<uint, uint> getReplacedId = (uint oldId) =>
				{
					string? oldIdName = WwiseShortIdUtility.GetNameFromShortId(oldId);
					if (oldIdName == null)
					{
						return oldId;
					}

					string newIdName = Regex.Replace(oldIdName, pattern, replacement);
					if (oldIdName != newIdName)
					{
						WwiseShortIdUtility.AddNames(new List<string> { newIdName }, true);
						return WwiseShortIdUtility.ConvertToShortId(newIdName);
					}

					return oldId;
				};

				newHircItem.UlID = getReplacedId(newHircItem.UlID);
				if (newHircItem is CAkRanSeqCntr)
				{
					CAkRanSeqCntr? ranSeqCntrItem = (CAkRanSeqCntr)newHircItem;
					ranSeqCntrItem.NodeBaseParams.DirectParentID = getReplacedId(ranSeqCntrItem.NodeBaseParams.DirectParentID);
					for (int j = 0; j < ranSeqCntrItem.ChildIds.Count; j++)
					{
						ranSeqCntrItem.ChildIds[j] = getReplacedId(ranSeqCntrItem.ChildIds[j]);
					}
					foreach (AkPlaylistItem playlistItem in ranSeqCntrItem.CAkPlayList.PlaylistItems)
					{
						playlistItem.PlayId = getReplacedId(playlistItem.PlayId);
					}
				}
				else if (newHircItem is CAkSound)
				{
					CAkSound? soundItem = (CAkSound)newHircItem;
					soundItem.AkBankSourceData.AkMediaInformation.FileId = getReplacedId(soundItem.AkBankSourceData.AkMediaInformation.FileId);
					soundItem.AkBankSourceData.AkMediaInformation.SourceId = getReplacedId(soundItem.AkBankSourceData.AkMediaInformation.SourceId);
					soundItem.NodeBaseParams.DirectParentID = getReplacedId(soundItem.NodeBaseParams.DirectParentID);
				} 
			}

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

		private void BtnGuideBattleAdvice_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("cmd", "/C start https://github.com/TheTollski/SoundbankEditor/blob/master/SoundbankEditorCore/Guides/Battle_Advice.md");
		}

		private void BtnGuideCampaignVo_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("cmd", "/C start https://github.com/TheTollski/SoundbankEditor/blob/master/SoundbankEditorCore/Guides/Campaign_VO.md");
		}

		private void BtnImportHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundbank == null || HircItems == null)
			{
				return;
			}

			var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			openFileDialog.Filter = $"Soundbank|*.bnk";
			openFileDialog.Title = "Select Soundbank to Import From";
			openFileDialog.Multiselect = false;

			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			SoundBank soundBank = SoundBank.CreateFromBnkFile(openFileDialog.FileName);
			if (soundBank.GeneratorVersion != _openSoundbank.GeneratorVersion)
			{
				MessageBox.Show($"'{openFileDialog.FileName}' has a different bank generator version from the opened soundbank.");
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Select HIRC Item ID to Import");
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			HircItem? hircItem = soundBank.HircItems.Find(hi => hi.UlID == hircItemIdConverterWindow.Id.Value);
			if (hircItem == null)
			{
				MessageBox.Show($"No HIRC item with ID '{hircItemIdConverterWindow.Id.Value}' was found in '{openFileDialog.FileName}'.");
				return;
			}

			HircItems.Insert(dgHircItems.SelectedIndex, hircItem);

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

				btnDeleteHircItems.IsEnabled = false;
				btnDuplicateHircItems.IsEnabled = false;
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

			UpdateKnownValidationErrorsCount();
			UpdateSelectedHircItemJson();

			if (dgHircItems.SelectedItems.Count > 1)
			{
				btnDeleteHircItems.IsEnabled = true;
				btnDeleteHircItems.Content = "Delete Items";
				btnDuplicateHircItems.IsEnabled = true;
				btnDuplicateHircItems.Content = "Duplicate Items";
				btnMoveHircItemDown.IsEnabled = false;
				btnMoveHircItemUp.IsEnabled = false;
				dpHircItemEditor.Visibility = Visibility.Hidden;
				tbHircItemJson.IsEnabled = false;

				shiev.HircItem = null;

				return;
			}

			btnDeleteHircItems.IsEnabled = true;
			btnDeleteHircItems.Content = "Delete Item";
			btnDuplicateHircItems.IsEnabled = true;
			btnDuplicateHircItems.Content = "Duplicate Item";
			btnMoveHircItemDown.IsEnabled = dgHircItems.SelectedIndex < HircItems.Count - 1;
			btnMoveHircItemUp.IsEnabled = dgHircItems.SelectedIndex > 0;
			dpHircItemEditor.Visibility = Visibility.Visible;
			tbHircItemJson.IsEnabled = true;

			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;
			ifevItemId.Value = selectedItem.UlID.ToString();
			shiev.HircItem = selectedItem;
		}

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private void TbHircItemJson_TextChanged(object sender, TextChangedEventArgs e)
		{
			_cancellationTokenSource.Cancel();

			if (HircItems == null || SelectedHircItemJson == null || dgHircItems.SelectedIndex < 0)
			{
				return;
			}

			if (_isProgrammaticallyChangingSelectedHircItemJson)
			{
				_isProgrammaticallyChangingSelectedHircItemJson = false;
				return;
			}

			if (!_areChangesPending)
			{
				_areChangesPending = true;
				UpdateTitle();
			}

			// Debounce logic.
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;

			Dispatcher.Invoke(async () => // Run task in current thread.
			{ 
				await Task.Delay(250);
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

					if (newHircItem.UlID != existingHircItem.UlID)
					{
						throw new JsonException($"You cannot change the item's ID in the JSON editor.");
					}

					newHircItem.CopyTo(existingHircItem);

					SelectedHircItemJsonErrorMessage = null;

					OnHircItemUpdated?.Invoke();
					UpdateKnownValidationErrorsCount();
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

			string outputBnkFilePath = _openSoundbankFilePath;
			string outputJsonFilePath = $"{Path.GetDirectoryName(_openSoundbankFilePath)}\\{Path.GetFileNameWithoutExtension(_openSoundbankFilePath)}.json";
			string outputTxtFilePath = GetCustomNamesFilePath();

			// Backup files.
			string backupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundbankEditor", "Backups");
			if (!Directory.Exists(backupDirectory))
			{
				Directory.CreateDirectory(backupDirectory);
			}

			Action<string> backupFile = (filePath) =>
			{
				const string DATETIME_FORMAT = "yyyy_MM_dd_HHmmss";

				if (File.Exists(filePath))
				{
					string backupFileName = $"{Path.GetFileName(filePath)}.{DateTime.Now.ToString(DATETIME_FORMAT)}.bak";
					string backupFilePath = Path.Combine(backupDirectory, backupFileName);
					File.Copy(filePath, backupFilePath, true);
				}

				List<string> backedUpFilesOfSameType = Directory.GetFiles(backupDirectory).Where(path => path.Contains(Path.GetFileName(filePath))).ToList();
				backedUpFilesOfSameType.Sort();
				backedUpFilesOfSameType.Reverse();
				while (backedUpFilesOfSameType.Count > 50)
				{
					int index = backedUpFilesOfSameType.Count - 1;

					string oldestFilePath = backedUpFilesOfSameType[index];
					Match timestampMatch = Regex.Match(oldestFilePath, "\\d\\d\\d\\d_\\d\\d_\\d\\d_\\d\\d\\d\\d\\d\\d");
					if (!timestampMatch.Success)
					{
						continue;
					}

					DateTime oldestFileAge = DateTime.ParseExact(timestampMatch.Value, DATETIME_FORMAT, CultureInfo.InvariantCulture);
					if (DateTime.Now -  oldestFileAge < TimeSpan.FromDays(7))
					{
						break;
					}

					backedUpFilesOfSameType.RemoveAt(index);
					File.Delete(oldestFilePath);
				}
			};

			backupFile(outputBnkFilePath);
			backupFile(outputJsonFilePath);
			backupFile(outputTxtFilePath);

			// Write files.
			_openSoundbank.WriteToBnkFile(outputBnkFilePath);

			string soundbankJson = _openSoundbank.ToJson();
			List<string> customNames = WwiseShortIdUtility
				.GetAllNames(true)
				.Where(n => soundbankJson.Contains(WwiseShortIdUtility.ConvertToShortId(n).ToString()))
				.ToList();
			File.WriteAllLines(outputTxtFilePath, customNames);
			
			_openSoundbank.WriteToJsonFile(outputJsonFilePath);

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
			string text = dgHircItems.SelectedItems.Count == 1 && dgHircItems.SelectedItem != null
				? JsonSerializer.Serialize((HircItem)dgHircItems.SelectedItem, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
				: string.Empty;

			_isProgrammaticallyChangingSelectedHircItemJson = true;
			SelectedHircItemJson = text;
		}

		private void UpdateTitle()
		{
			Title = $"{(_areChangesPending ? "*" : string.Empty)}{System.IO.Path.GetFileName(_openSoundbankFilePath)} - Soundbank Editor";
		}
	}
}
