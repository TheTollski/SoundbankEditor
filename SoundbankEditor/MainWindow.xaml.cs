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
		private string? _openFilePath;
		private SoundBank? _openSoundBank;
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

			_openSoundBank = SoundBank.CreateFromBnkFile(openFileDialog.FileName);
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList());

			HircItems = _openSoundBank.HircItems;
			_openFilePath = openFileDialog.FileName;
			_areChangesPending = false;
			UpdateTitle();
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundBank == null || _openFilePath == null)
			{
				return;
			}

			SaveFile();
		}

		private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundBank == null)
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

			_openFilePath = saveFileDialog.FileName;
			SaveFile();
		}

		private void BtnAddHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundBank == null)
			{
				return;
			}

			string? selectedItemString = cbAddHircItemType.SelectedItem as string;
			if (selectedItemString == null)
			{
				return;
			}

			HircType hircType = (HircType)Enum.Parse(typeof(HircType), selectedItemString);
			HircItems.Insert(0, HircItemFactory.Create(hircType));
			dgHircItems.Items.Refresh();
			_areChangesPending = true;
			UpdateTitle();
		}

		private void BtnDeleteHircItem_Click(object sender, RoutedEventArgs e)
		{
			if (_openSoundBank == null)
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
				_isProgrammaticallyChangingSelectedHircItemJson = true;
				SelectedHircItemJson = "";
				_isProgrammaticallyChangingSelectedHircItemJson = false;

				btnDeleteHircItem.IsEnabled = false;
				tbHircItemJson.IsEnabled = false;
				return;
			}

			if (SelectedHircItemJsonErrorMessage != null && _selectedHircItemIndex != dgHircItems.SelectedIndex &&
					MessageBox.Show($"There are unpersisted changes in your working HIRC item. Are you sure you want to select a different HIRC item?", "Confirm HIRC Item Selection", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			{
				dgHircItems.SelectionChanged -= DgHircItems_SelectionChanged;
				dgHircItems.SelectedIndex = _selectedHircItemIndex;
				dgHircItems.SelectionChanged += DgHircItems_SelectionChanged;
				dgHircItems.Items.Refresh();
				return;
			}

			_selectedHircItemIndex = dgHircItems.SelectedIndex;

			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;

			_isProgrammaticallyChangingSelectedHircItemJson = true;
			SelectedHircItemJson = JsonSerializer.Serialize(
				selectedItem,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);
			_isProgrammaticallyChangingSelectedHircItemJson = false;

			btnDeleteHircItem.IsEnabled = true;
			tbHircItemJson.IsEnabled = true;
		}

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

				bool hasIdChanged = newHircItem.UlID != existingHircItem.UlID;

				newHircItem.CopyTo(existingHircItem);

				if (!_isProgrammaticallyChangingSelectedHircItemJson && hasIdChanged)
				{
					dgHircItems.Items.Refresh();
				}

				SelectedHircItemJsonErrorMessage = null;
			}
			catch (Exception ex)
			{
				SelectedHircItemJsonErrorMessage = ex.Message;
			}
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

		private void SaveFile()
		{
			if (_openSoundBank == null || _openFilePath == null)
			{
				return;
			}

			_openSoundBank.WriteToBnkFile(_openFilePath);
			_areChangesPending = false;
			UpdateTitle();
		}

		private void UpdateTitle()
		{
			Title = $"{(_areChangesPending ? "*" : string.Empty)}{System.IO.Path.GetFileName(_openFilePath)} - Soundbank Editor";
		}
	}
}
