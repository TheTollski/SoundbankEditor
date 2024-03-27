using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditorCore.Utility;
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

		private bool _isSelectedHircItemJsonValid = true;
		public bool IsSelectedHircItemJsonValid
		{
			get
			{
				return _isSelectedHircItemJsonValid;
			}
			set 
			{
				_isSelectedHircItemJsonValid = value;
				OnPropertyChanged(nameof(IsSelectedHircItemJsonValid));
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

		private bool _areChangesPending;
		private bool _isProgrammaticallyChangingSelectedHircItemJson;
		private string? _openFilePath;
		private SoundBank? _openSoundBank;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
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

		private void DgHircItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;

			_isProgrammaticallyChangingSelectedHircItemJson = true;
			SelectedHircItemJson = JsonSerializer.Serialize(
				selectedItem,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);
			_isProgrammaticallyChangingSelectedHircItemJson = false;
		}

		private void TbHircItemJson_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (HircItems == null || SelectedHircItemJson == null)
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

				using MemoryStream memoryStream = new MemoryStream();
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				newHircItem.WriteToBinary(binaryWriter); // Test to make sure it doesn't fail.

				bool hasIdChanged = newHircItem.UlID != existingHircItem.UlID;

				newHircItem.CopyTo(existingHircItem);

				if (!_isProgrammaticallyChangingSelectedHircItemJson && hasIdChanged)
				{
					dgHircItems.Items.Refresh();
				}

				IsSelectedHircItemJsonValid = true;
			}
			catch (Exception ex)
			{
				if (ex is JsonException || ex is SerializationException)
				{
					IsSelectedHircItemJsonValid = false;
					return;
				}

				throw;
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
