using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
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

		public string? SelectedHircJson { get; set; }


		private SoundBank? openSoundBank;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		void OpenFile(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			openFileDialog1.Filter = $"SoundBank|*.bnk";
			openFileDialog1.Title = "Select SoundBank";
			openFileDialog1.Multiselect = false;

			if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			openSoundBank = SoundBank.CreateFromBnkFile(openFileDialog1.FileName);
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList());

			dgList.ItemsSource = openSoundBank.HircItems;
			//dgList.SelectionChanged += SelectedItemChanged;
		}

		void SelectedItemChanged(object sender, SelectionChangedEventArgs e)
		{
			HircItem selectedItem = (HircItem)dgList.SelectedItem;
			SelectedHircJson = JsonSerializer.Serialize(
				selectedItem,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);
			OnPropertyChanged(nameof(SelectedHircJson));
		}



		

		
	}
}
