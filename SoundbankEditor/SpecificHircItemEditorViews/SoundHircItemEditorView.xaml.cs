using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Windows;
using System.Windows.Controls;

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

			UpdateDirectParentIdTextBlock();
			UpdateFileIdTextBlock();
		}

		private void BtnEditDirectParentId_Click(object sender, RoutedEventArgs e)
		{
			if (_cakSound == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Direct Parent ID", _cakSound.NodeBaseParams.DirectParentID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSound.NodeBaseParams.DirectParentID = hircItemIdConverterWindow.Id.Value;
			UpdateDirectParentIdTextBlock();

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

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set File ID", _cakSound.AkBankSourceData.AkMediaInformation.FileId);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakSound.AkBankSourceData.AkMediaInformation.FileId = hircItemIdConverterWindow.Id.Value;
			_cakSound.AkBankSourceData.AkMediaInformation.SourceId = hircItemIdConverterWindow.Id.Value;
			UpdateFileIdTextBlock();

			if (HircItemUpdated != null)
			{
				HircItemUpdated(this, EventArgs.Empty);
			}
		}

		//
		// Helpers
		//

		private void UpdateDirectParentIdTextBlock()
		{
			if (_cakSound == null)
			{
				return;
			}

			tbDirectParentId.Text = $"Parent ID: {WwiseShortIdUtility.ConvertShortIdToReadableString(_cakSound.NodeBaseParams.DirectParentID)}";
		}

		private void UpdateFileIdTextBlock()
		{
			if (_cakSound == null)
			{
				return;
			}

			tbFileId.Text = $"File ID: {WwiseShortIdUtility.ConvertShortIdToReadableString(_cakSound.AkBankSourceData.AkMediaInformation.FileId)}";
		}
	}
}
