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
	/// Interaction logic for TextInputWindow.xaml
	/// </summary>
	public partial class TextInputWindow : Window
	{
		Type _valueType;

		public TextInputWindow(string title, Type valueType, string? originalValue = null)
		{
			InitializeComponent();
			Title = title;
			_valueType = valueType;
			
			if (originalValue != null)
			{
				tbValue.Text = originalValue;
			}

			tbValue.Focus();
			tbValue.SelectAll();
		}

		public string? Value
		{
			get
			{
				return tbValue.Text;
			}
			set
			{
				tbValue.Text = value?.ToString();
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				object convertedValue = Convert.ChangeType(tbValue.Text, _valueType);
			}
			catch(Exception ex)
			{
				MessageBox.Show($"Unable to convert '{tbValue.Text}' to type '{_valueType.Name}'. Error: '${ex.Message}'");
				return;
			}

			DialogResult = true;
		}
	}
}
