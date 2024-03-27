using SoundbankEditor.Core.WwiseObjects.HircItems;
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

namespace SoundbankEditor.SpecificHircItemEditorViews
{
	/// <summary>
	/// Interaction logic for SpecificHircItemEditorView.xaml
	/// </summary>
	public partial class SpecificHircItemEditorView : UserControl
	{
		public static readonly DependencyProperty HircItemProperty = DependencyProperty.Register(
			"HircItem", typeof(HircItem), typeof(SpecificHircItemEditorView), new UIPropertyMetadata(null)
		);

		public HircItem HircItem
		{
			get { return (HircItem)GetValue(HircItemProperty); }
			set { SetValue(HircItemProperty, value); }
		}

		public event EventHandler? HircItemUpdated;

		public SpecificHircItemEditorView()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void OnHircItemUpdated(object sender, EventArgs e)
		{
			HircItemUpdated?.Invoke(sender, e);
		}
	}
}
