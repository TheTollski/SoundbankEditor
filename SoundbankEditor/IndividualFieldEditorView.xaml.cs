using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using SoundbankEditor.SpecificHircItemEditorViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// Interaction logic for IndividualFieldEditorView.xaml
	/// </summary>
	public partial class IndividualFieldEditorView : UserControl
	{
		public bool IsValueAShortId { get; set; }
		public event EventHandler? OnEditClicked;

		private string? _title;
		public string? Title
		{
			get { return _title; }
			set
			{
				_title = value;
				UpdateText();
			}
		}

		private string? _value;
		public string? Value
		{
			get { return _value; }
			set
			{
				_value = value;
				UpdateText();
			}
		}

		public IndividualFieldEditorView()
		{
			InitializeComponent();

			//tbTitleAndValue.MaxWidth = Width - 110;
		}

		private void BtnCopy_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Value);
		}

		private void BtnEdit_Click(object sender, RoutedEventArgs e)
		{
			OnEditClicked?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateText()
		{
			string? value = IsValueAShortId && Value != null
				? WwiseShortIdUtility.ConvertShortIdToReadableString(uint.Parse(Value))
				: Value;

			tbTitleAndValue.Text = $"{Title}: {value}";

			btnCopy.Content = $"Copy {Title}";
			btnEdit.Content = $"Edit {Title}";
		}
	}
}
