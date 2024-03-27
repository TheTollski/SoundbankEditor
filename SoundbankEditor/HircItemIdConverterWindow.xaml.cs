using SoundbankEditor.Core;
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
	public partial class HircItemIdConverterWindow : Window
	{
		public HircItemIdConverterWindow(string title, uint? originalId = null)
		{
			InitializeComponent();
			Title = title;
			Id = originalId;

			tbIdOrName.Focus();
			tbIdOrName.SelectAll();
		}

		public uint? Id
		{
			get
			{
				if (uint.TryParse(tbIdOrName.Text, out uint value))
				{
					return value;
				}

				return null; 
			}
			set
			{
				tbIdOrName.Text = value?.ToString();
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			if (Id != null)
			{
				DialogResult = true;
				return;
			}

			if (Regex.Match(tbIdOrName.Text, "^\\w*[A-z]\\w*$").Success)
			{
				WwiseShortIdUtility.AddNames(new List<string> { tbIdOrName.Text }, true);
				Id = WwiseShortIdUtility.ConvertToShortId(tbIdOrName.Text);

				DialogResult = true;
				return;
			}

			MessageBox.Show($"Unable to convert '{tbIdOrName.Text}' to a Wwise short ID. The value must be either 1) an unsigned integer or 2) alphanumeric characters or underscores.");
		}
	}
}
