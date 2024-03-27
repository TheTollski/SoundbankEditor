using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditorCore.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

		private bool _isSelectedHircItemJsonValid;
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

		private bool _isProgrammaticallyChangingSelectedHircItemJson;
		private SoundBank? _openSoundBank;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void OpenFile(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			openFileDialog1.Filter = $"SoundBank|*.bnk";
			openFileDialog1.Title = "Select SoundBank";
			openFileDialog1.Multiselect = false;

			if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			_openSoundBank = SoundBank.CreateFromBnkFile(openFileDialog1.FileName);
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList());

			HircItems = _openSoundBank.HircItems;
		}

		private void dgHircItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HircItem selectedItem = (HircItem)dgHircItems.SelectedItem;

			_isProgrammaticallyChangingSelectedHircItemJson = true;
			SelectedHircItemJson = JsonSerializer.Serialize(
				selectedItem,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);
			_isProgrammaticallyChangingSelectedHircItemJson = false;
		}

		private void tbHircItemJson_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SelectedHircItemJson == null)
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

				bool hasIdChanged = newHircItem.UlID != existingHircItem.UlID;

				newHircItem.CopyTo(existingHircItem);

				if (!_isProgrammaticallyChangingSelectedHircItemJson && hasIdChanged)
				{
					dgHircItems.Items.Refresh();
				}

				IsSelectedHircItemJsonValid = true;
			} catch (JsonException ex)
			{
				IsSelectedHircItemJsonValid = false;
			}
		}
	}
}
