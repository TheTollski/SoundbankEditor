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
			MainWindow.OnHircItemUpdated = UpdateAllFields;
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakSound = (CAkSound)DataContext;

			UpdateAllFields();
		}

		private void IfevEditDirectParentId_Click(object sender, EventArgs e)
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

		private void IfevEditFileId_Click(object sender, EventArgs e)
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

		private void UpdateAllFields()
		{
			UpdateDirectParentIdTextBlock();
			UpdateFileIdTextBlock();
		}

		private void UpdateDirectParentIdTextBlock()
		{
			if (_cakSound == null)
			{
				return;
			}

			ifevParentId.Value =_cakSound.NodeBaseParams.DirectParentID.ToString();
		}

		private void UpdateFileIdTextBlock()
		{
			if (_cakSound == null)
			{
				return;
			}

			ifevFileId.Value = _cakSound.AkBankSourceData.AkMediaInformation.FileId.ToString();
		}
	}
}
