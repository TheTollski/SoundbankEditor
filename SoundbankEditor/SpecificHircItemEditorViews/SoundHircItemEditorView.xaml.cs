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
	/// Interaction logic for SoundHircItemEditorView.xaml
	/// </summary>
	public partial class SoundHircItemEditorView : UserControl
	{
		private CAkSound? _cakSound;

		public event EventHandler? HircItemUpdated;

		public SoundHircItemEditorView()
		{
			InitializeComponent();
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakSound = (CAkSound)DataContext;

			UpdateDirectParentIdLabel();
			UpdateFileIdLabel();
		}

		private void BtnEditDirectParentId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSound == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow(_cakSound.NodeBaseParams.DirectParentID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSound.NodeBaseParams.DirectParentID = hircItemIdConverterWindow.Id.Value;
			UpdateDirectParentIdLabel();

			if (HircItemUpdated != null)
			{
				HircItemUpdated(this, EventArgs.Empty);
			}
		}

		private void BtnEditFileId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSound == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow(_cakSound.AkBankSourceData.AkMediaInformation.FileId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSound.AkBankSourceData.AkMediaInformation.FileId = hircItemIdConverterWindow.Id.Value;
			_cakSound.AkBankSourceData.AkMediaInformation.SourceId = hircItemIdConverterWindow.Id.Value;
			UpdateFileIdLabel();

			if (HircItemUpdated != null)
			{
				HircItemUpdated(this, EventArgs.Empty);
			}
		}

		//
		// Helpers
		//

		private void UpdateDirectParentIdLabel()
		{
			lDirectParentId.Content = $"Direct Parent ID: {_cakSound?.NodeBaseParams.DirectParentID}";
		}

		private void UpdateFileIdLabel()
		{
			lFileId.Content = $"File ID: {_cakSound?.AkBankSourceData.AkMediaInformation.FileId}";
		}
	}
}
