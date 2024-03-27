using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
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
	public partial class HircItemIdListConverterWindow : Window
	{
		public HircItemIdListConverterWindow(List<uint> originalIds)
		{
			InitializeComponent();

			var ids = new List<uint>();
			ids.AddRange(originalIds);
			Ids = ids;

			hiilc.Ids = Ids;
		}

		public List<uint> Ids { get; private set; }

		////
		//// Event Handlers
		////

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
